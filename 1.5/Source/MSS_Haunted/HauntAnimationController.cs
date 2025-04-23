using System;
using System.Collections.Generic;
using System.Linq;
using MSS_Haunted.Utils;
using RimWorld;
using UnityEngine;
using Verse;

namespace MSS_Haunted;

public class HauntAnimationController(Map map) : MapComponent(map)
{
    // Things we don't want to haunt
    public List<ThingCategory> ThingsToSkip =
    [
        ThingCategory.Filth,
        ThingCategory.Projectile,
        ThingCategory.Gas,
        ThingCategory.Mote,
        ThingCategory.Attachment,
        ThingCategory.Ethereal,
        ThingCategory.PsychicEmitter,
        ThingCategory.Building,
    ];

    public Map Map => map;
    public bool HauntingActive;
    public Pawn Target;
    public HauntedThingFlyer TargetFlyer;
    public Vector3 StartPos;

    public IntRange LiftPhaseTimeRange = new IntRange(600, 1200);
    public IntRange SpinPhaseTimeRange = new IntRange(1200, 2400);
    public IntRange DropPhaseTimeRange = new IntRange(300, 900);

    /// <summary>
    /// Represents a range of offsets used for determining the starting time of a lift sequence in the haunting animation.
    /// This property returns an `IntRange` based on the total ticks required for the lift phase as defined in the `LiftPhaseTicks` field.
    /// It is utilized for calculating randomized offsets for individual haunted items or target flyers at the start of the lift phase.
    /// </summary>
    public IntRange StartLiftOffsetRange => new IntRange(0, LiftPhaseTicks);

    /// <summary>
    /// Represents the range of vertical movement (lift height) for objects during the haunting
    /// animation's lifting phase.
    /// </summary>
    public FloatRange LiftRange = new(1f, 10f);

    /// <summary>
    /// Represents a range of valid spin counts for objects being manipulated during the haunting spin phase
    /// in the HauntAnimationController. The spin count determines how many rotations an object will complete
    /// during the spin phase of the haunting process.
    /// </summary>
    public IntRange SpinRange = new(1, 30);

    public WeatherDef originalWeather;
    public int ticksSinceLastEmitted;
    protected Mote psychicEffectMote;

    public class HauntingParams : IExposable
    {
        public HauntingParams() { }

        public HauntingParams(float liftHeightHeight, int timesToSpin, int startLiftOffset = 0)
        {
            LiftHeight = liftHeightHeight;
            TimesToSpin = timesToSpin;
            StartLiftOffset = startLiftOffset;
        }

        /// <summary>
        /// Represents the vertical displacement (in units) to which an object is lifted during the haunting animation.
        /// </summary>
        public float LiftHeight;

        /// <summary>
        /// Represents the number of times the target object is spun during the spin phase
        /// of a haunting animation.
        /// This value is utilized to calculate the angular rotation and movement
        /// of the target object during the spinning phase.
        /// </summary>
        public int TimesToSpin;

        /// <summary>
        /// The offset in ticks before the lifting phase starts for a specific haunted flyer object.
        /// Represents the delay relative to the start of the lifting phase for other objects.
        /// This value ensures a staggered lifting effect among haunted objects.
        /// </summary>
        public int StartLiftOffset;

        /// <summary>
        /// Represents the radius of a circular motion during the haunting animation, defining the distance
        /// between the object in motion and the center of its rotation.
        /// </summary>
        public float Radius;

        /// <summary>
        /// Represents the starting position for the rotational movement of a haunted object during the spinning phase.
        /// </summary>
        public Vector3 RotationStart;

        /// <summary>
        /// Represents the final position of a haunted object after the spinning phase of the haunting animation has completed.
        /// </summary>
        public Vector3 EndSpinPosition;

        /// <summary>
        /// Calculates the current rotated position of a moving object during the spinning phase
        /// of a haunting event based on the progress of the phase.
        /// </summary>
        public Vector3 CurrentRotatedPos(int PhaseLength, int currentPhaseTick)
        {
            int ticksPerRotation = PhaseLength / TimesToSpin;
            float anglePerTick = Mathf.Deg2Rad * (360f / ticksPerRotation);
            int ticksIntoCurrentRotation = currentPhaseTick % ticksPerRotation;

            float currentAngle = anglePerTick * ticksIntoCurrentRotation;

            return RotationStart + new Vector3(Mathf.Sin(currentAngle) * Radius, 0, Mathf.Cos(currentAngle) * Radius);
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref LiftHeight, "LiftHeight");
            Scribe_Values.Look(ref TimesToSpin, "TimesToSpin");
            Scribe_Values.Look(ref Radius, "Radius");
            Scribe_Values.Look(ref RotationStart, "RotationStart");
            Scribe_Values.Look(ref EndSpinPosition, "EndSpinPosition");
            Scribe_Values.Look(ref StartLiftOffset, "StartLiftOffset");
        }
    }

    /// <summary>
    /// A dictionary that keeps track of currently haunted items and their respective animation parameters.
    /// Each entry associates a <see cref="HauntedThingFlyer"/>'s thing ID (representing the haunted object) with
    /// its corresponding <see cref="HauntAnimationController.HauntingParams"/>, which define the
    /// behavior and animation details for the haunting process.
    /// </summary>
    public Dictionary<int, HauntingParams> HauntedThings;

    public HashSet<HauntedThingFlyer> flyerCache = new();

    public Vector3 EpicenterOffset = new Vector3(0, 0, 5);
    public Vector3 Epicenter => StartPos + EpicenterOffset;

    public enum HauntPhase : byte
    {
        Lifting,
        Spinning,
        Dropping,
    }

    public HauntPhase CurrentPhase;
    public int LiftPhaseTicks;
    public int SpinPhaseTicks;
    public int DropPhaseTicks;
    public int StartPhaseTick;

    public bool shouldResetNextTick = false;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref HauntingActive, "HauntingActive", false);
        Scribe_References.Look(ref Target, "Target");
        Scribe_Values.Look(ref LiftPhaseTicks, "LiftPhaseTicks", 0);
        Scribe_Values.Look(ref SpinPhaseTicks, "SpinPhaseTicks", 0);
        Scribe_Values.Look(ref DropPhaseTicks, "DropPhaseTicks", 0);
        Scribe_Values.Look(ref StartPhaseTick, "StartPhaseTick", 0);
        Scribe_Defs.Look(ref originalWeather, "originalWeather");
        Scribe_Values.Look(ref CurrentPhase, "CurrentPhase", HauntPhase.Lifting);
        Scribe_References.Look(ref TargetFlyer, "TargetFlyer");
        Scribe_Values.Look(ref ticksSinceLastEmitted, "ticksSinceLastEmitted", 0);

        Scribe_Collections.Look(ref HauntedThings, "HauntedThings", LookMode.Value, LookMode.Deep);

        if (Scribe.mode == LoadSaveMode.PostLoadInit || Scribe.mode == LoadSaveMode.LoadingVars)
        {
            HauntedThings ??= new Dictionary<int, HauntingParams>();
        }

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            //Loading from save mid haunt isn't working, so for now, lets just reset it so we don't break maps
            shouldResetNextTick = true;
        }
    }

    /// Retrieves a list of valid Thing options within a specified radius around the target flyer.
    /// <param name="radius">The search radius within which to find valid objects.</param>
    /// <returns>A list of `Thing` objects that meet the criteria for haunting activity.</returns>
    public List<Thing> GetValidThingOptions(float radius)
    {
        IEnumerable<IntVec3> cells = GenRadial.RadialCellsAround(TargetFlyer.Position, radius, true);
        return cells.SelectMany(c => Map.thingGrid.ThingsAt(c)).Where(t => !ThingsToSkip.Any(c => c == t.def.category)).Except([Target]).ToList();
    }

    /// Initiates a haunting sequence centered around a target pawn and affects objects in a specified radius.
    /// This method sets up the haunting event by including weather transitions,
    /// object lifting, and the progression through various haunting phases such as lifting, spinning, and dropping.
    /// <param name="target">The target pawn around which the haunting will occur.</param>
    /// <param name="radius">The radius around the target pawn within which objects will be affected by the haunting. Defaults to 10.</param>
    /// <param name="hauntChance">The probability of selecting additional non-priority items for haunting. Defaults to 0.45.</param>
    /// <param name="minItems">The minimum number of items to haunt. Defaults to 5.</param>
    public void StartHaunting(Pawn target, int radius = 10, float hauntChance = 0.45f, int minItems = 5)
    {
        if (HauntingActive)
            return;
        Target = target;

        TargetFlyer = ThingMaker.MakeThing(MSS_HauntedDefOf.MSS_Haunted_HauntedThingTargetFlyer, null) as HauntedThingFlyer;
        GenSpawn.Spawn(TargetFlyer, Target.Position, map);
        if (!(TargetFlyer?.AddThing(Target) ?? false))
        {
            // failed to add the target to the flyer
            return;
        }
        ;

        ticksSinceLastEmitted = 0;

        LiftPhaseTicks = LiftPhaseTimeRange.RandomInRange;
        SpinPhaseTicks = SpinPhaseTimeRange.RandomInRange;
        DropPhaseTicks = DropPhaseTimeRange.RandomInRange;

        originalWeather = map.weatherManager.curWeather;
        map.weatherManager.TransitionTo(MSS_HauntedDefOf.MSS_Haunted_Thunderstorm);

        StartPhaseTick = Find.TickManager.TicksGame;

        List<Thing> things = GetValidThingOptions(radius);
        List<Thing> selectedThings = things.Take(minItems).ToList();

        selectedThings.AddRange(things.Except(selectedThings).Where(t => Rand.Chance(hauntChance)));

        foreach (Thing selectedThing in selectedThings)
        {
            HauntedThingFlyer flyer = ThingMaker.MakeThing(MSS_HauntedDefOf.MSS_Haunted_HauntedThingFlyer, null) as HauntedThingFlyer;
            GenSpawn.Spawn(flyer, selectedThing.Position, map);
            if (flyer?.AddThing(selectedThing) ?? false)
            {
                AddHauntedThing(flyer, new HauntingParams(LiftRange.RandomInRange, SpinRange.RandomInRange, StartLiftOffsetRange.RandomInRange));
            }
        }
        StartPos = TargetFlyer.DrawPos;

        HauntingActive = true;
    }

    public void AddHauntedThing(HauntedThingFlyer flyer, HauntingParams params_)
    {
        HauntedThings ??= new Dictionary<int, HauntingParams>();
        HauntedThings[flyer.thingIDNumber] = params_;
        flyerCache ??= [];
        flyerCache.Add(flyer);
        PopulateHTCache();
        htCache[flyer] = params_;
    }

    // we don't save this, this just saves us having to look through all map things constantly
    public Dictionary<HauntedThingFlyer, HauntingParams> htCache;

    public void PopulateHTCache(bool force = false)
    {
        if (htCache == null || force)
            htCache = HauntedThings
                .Select(kv => new { Key = flyerCache.FirstOrDefault(t => t.thingIDNumber == kv.Key), kv.Value })
                .Where(kv => kv.Key != null)
                .ToDictionary(k => k.Key, v => v.Value);
    }

    public IEnumerable<KeyValuePair<HauntedThingFlyer, HauntingParams>> HauntedThingsEnumerable
    {
        get
        {
            PopulateHTCache();
            return htCache;
        }
    }

    public void LiftPhaseTick()
    {
        if (StartPhaseTick + LiftPhaseTicks < Find.TickManager.TicksGame)
        {
            CurrentPhase++;
            foreach (KeyValuePair<HauntedThingFlyer, HauntingParams> hauntedThing in HauntedThingsEnumerable)
            {
                // get the distance from the epicenter to the thing to use as the rotational radius
                hauntedThing.Value.Radius = Vector3.Distance(Epicenter, hauntedThing.Key.DrawPos);
                hauntedThing.Value.RotationStart = hauntedThing.Key.DrawPos;
                hauntedThing.Key.LiftedPosition = hauntedThing.Key.DrawPos;
            }
            TargetFlyer.LiftedPosition = TargetFlyer.DrawPos;
            return;
        }

        int currentPhaseTick = Find.TickManager.TicksGame - StartPhaseTick;

        foreach (KeyValuePair<HauntedThingFlyer, HauntingParams> hauntedThing in HauntedThingsEnumerable)
        {
            // Don't start until our offset
            if (hauntedThing.Value.StartLiftOffset > currentPhaseTick)
                continue;

            // Calculate the ratio of the current phase to the total phase duration to use in the lerp
            float ratio = (float)(currentPhaseTick - hauntedThing.Value.StartLiftOffset) / (LiftPhaseTicks - hauntedThing.Value.StartLiftOffset);

            // Calculate the position of the thing based on the ratio of the current phase to the total phase duration
            Vector3 EndPos = new(0, 0, hauntedThing.Value.LiftHeight);
            hauntedThing.Key.SetPositionDirectly(Vector3.Lerp(hauntedThing.Key.StartPosition, hauntedThing.Key.StartPosition + EndPos, ratio));
        }

        TargetFlyer.SetPositionDirectly(Vector3.Lerp(StartPos, Epicenter, (float)currentPhaseTick / LiftPhaseTicks));
    }

    public void SpinPhaseTick()
    {
        if (StartPhaseTick + LiftPhaseTicks + SpinPhaseTicks < Find.TickManager.TicksGame)
        {
            CurrentPhase++;
            foreach (KeyValuePair<HauntedThingFlyer, HauntingParams> hauntedThing in HauntedThingsEnumerable)
            {
                hauntedThing.Value.EndSpinPosition = hauntedThing.Key.DrawPos;
                hauntedThing.Key.LiftedPosition = Vector3.zero;
            }

            TargetFlyer.LiftedPosition = Vector3.zero;
            return;
        }

        int currentPhaseTick = Find.TickManager.TicksGame - StartPhaseTick;

        foreach (KeyValuePair<HauntedThingFlyer, HauntingParams> hauntedThing in HauntedThingsEnumerable)
        {
            hauntedThing.Key.SetPositionDirectly(hauntedThing.Value.CurrentRotatedPos(SpinPhaseTicks, currentPhaseTick));
        }
    }

    public void Reset()
    {
        ModLog.Debug("Resetting haunting animation controller");
        HauntingActive = false;
        PopulateHTCache(true);
        foreach (HauntedThingFlyer flyer in map.listerThings.AllThings.OfType<HauntedThingFlyer>().ToList())
        {
            try
            {
                if (htCache.ContainsKey(flyer))
                    htCache.Remove(flyer);
                flyer.Destroy();
            }
            catch (Exception e)
            {
                ModLog.Error("Couldn't destroy flyer", e);
            }
        }

        HauntedThings.Clear();

        TargetFlyer.Destroy();
        TargetFlyer = null;

        flyerCache.Clear();
        htCache.Clear();

        CurrentPhase = HauntPhase.Lifting;
        LiftPhaseTicks = 0;
        SpinPhaseTicks = 0;
        DropPhaseTicks = 0;
        StartPhaseTick = 0;

        Target = null;
        StartPos = Vector3.zero;

        map.weatherManager.TransitionTo(originalWeather);
        // map.weatherManager.curWeatherAge = 4000 - DropPhaseTicks;

        originalWeather = null;

        psychicEffectMote?.Destroy();
        psychicEffectMote = null;
    }

    public void DropPhaseTick()
    {
        if (StartPhaseTick + LiftPhaseTicks + SpinPhaseTicks + DropPhaseTicks < Find.TickManager.TicksGame)
        {
            Reset();
            return;
        }

        float ratio = (float)(Find.TickManager.TicksGame - (StartPhaseTick + LiftPhaseTicks + SpinPhaseTicks)) / DropPhaseTicks;

        foreach (KeyValuePair<HauntedThingFlyer, HauntingParams> hauntedThing in HauntedThingsEnumerable)
        {
            hauntedThing.Key.SetPositionDirectly(Vector3.Lerp(hauntedThing.Value.EndSpinPosition, hauntedThing.Key.StartPosition, ratio));
        }

        TargetFlyer.SetPositionDirectly(Vector3.Lerp(Epicenter, StartPos, ratio));
    }

    public override void MapComponentTick()
    {
        if (shouldResetNextTick)
        {
            shouldResetNextTick = false;
            Reset();
        }
        if (HauntingActive)
        {
            switch (CurrentPhase)
            {
                case HauntPhase.Lifting:
                    LiftPhaseTick();
                    break;
                case HauntPhase.Spinning:
                    SpinPhaseTick();
                    break;
                case HauntPhase.Dropping:
                    DropPhaseTick();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (TargetFlyer == null)
            {
                return;
            }

            if (psychicEffectMote != null)
                psychicEffectMote.exactPosition = TargetFlyer.DrawPos;

            if (psychicEffectMote is null or { Destroyed: true })
            {
                psychicEffectMote = MoteMaker.MakeAttachedOverlay(TargetFlyer, DefDatabase<ThingDef>.GetNamed("Mote_PsychicConditionCauserEffect"), Vector3.zero);
            }
            else
            {
                psychicEffectMote.Maintain();
            }
            ticksSinceLastEmitted++;
        }
    }
}
