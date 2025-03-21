using System;
using System.Collections.Generic;
using System.Linq;
using MSS_Haunted.Utils;
using RimWorld;
using UnityEngine;
using Verse;

namespace MSS_Haunted;

/// <summary>
/// The HauntAnimationController class manages animations and logic for a haunting event within
/// a map. It controls phases of a haunting including lifting, spinning, and dropping objects or targets.
/// </summary>
public class HauntAnimationController(Map map) : MapComponent(map)
{
    /// <summary>
    /// Represents the collection of thing categories that the haunt animation controller
    /// should ignore while performing its operations. Items in this list will not be
    /// considered for haunting or other related effects.
    /// </summary>
    public List<ThingCategory> ThingsToSkip =
    [
        ThingCategory.Filth,
        ThingCategory.Projectile,
        ThingCategory.Gas,
        ThingCategory.Mote,
        ThingCategory.Attachment,
        ThingCategory.Ethereal,
        ThingCategory.PsychicEmitter,
    ];

    /// <summary>
    /// Represents the map associated with the current instance of the HauntAnimationController.
    /// This property provides the Map object that the animation controller operates on.
    /// </summary>
    public Map Map => map;

    /// <summary>
    /// Indicates whether a haunting sequence is currently active.
    /// When set to true, various phases of the haunting process, such as lifting, spinning, and dropping objects, occur.
    /// When set to false, the haunting process is inactive, and the controller remains idle.
    /// </summary>
    public bool HauntingActive = false;

    /// <summary>
    /// Represents the target pawn for the haunting sequence in the HauntAnimationController.
    /// This pawn is the primary subject of the haunting effects such as lifting, spinning, and dropping phases,
    /// and is affected by the animations, stuns, and other haunting interactions during the process.
    /// </summary>
    public Pawn Target;

    /// <summary>
    /// Represents the instance of a <see cref="HauntedThingFlyer"/> used to perform and manage
    /// animations or logic associated with a target during the haunting sequence.
    /// </summary>
    /// <remarks>
    /// This variable is specifically responsible for handling the haunting effects targeted at an object.
    /// It gets spawned and manipulated during the haunting process, including phases like lifting,
    /// spinning, and dropping the target.
    /// </remarks>
    public HauntedThingFlyer TargetFlyer;

    /// Represents the starting position of the haunting effect in a 3D space.
    /// This position is initialized during the start of a haunting and is used as a reference
    /// point for various movements and animations throughout the haunting phases.
    /// It is primarily used in calculations, including determining the epicenter of the haunting
    /// or interpolating positions during the lift and drop phases.
    public Vector3 StartPos;

    /// <summary>
    /// Represents the range of time, in ticks, for the lift phase of a haunting animation.
    /// This variable determines the minimum and maximum duration of the lifting phase,
    /// which is randomly selected within this range when the haunting begins.
    /// </summary>
    public IntRange LiftPhaseTimeRange = new IntRange(600, 1200);

    /// <summary>
    /// Represents the time range, in ticks, for the spinning phase during a haunting animation.
    /// This phase occurs after the lifting phase and before the dropping phase.
    /// </summary>
    public IntRange SpinPhaseTimeRange = new IntRange(1200, 2400);

    /// <summary>
    /// Specifies the time range for the Drop phase of the haunting animation.
    /// This represents the duration in ticks that the target or objects
    /// will remain in the dropping phase during the haunting sequence.
    /// </summary>
    public IntRange DropPhaseTimeRange = new IntRange(300, 900);

    /// Represents a range of offsets used for determining the starting time of a lift sequence in the haunting animation.
    /// This property returns an `IntRange` based on the total ticks required for the lift phase as defined in the `LiftPhaseTicks` field.
    /// It is utilized for calculating randomized offsets for individual haunted items or target flyers at the start of the lift phase.
    public IntRange StartLiftOffsetRange => new IntRange(0, LiftPhaseTicks);

    /// <summary>
    /// Represents the range of vertical movement (lift height) for objects during the haunting
    /// animation's lifting phase.
    /// </summary>
    /// <remarks>
    /// The lift range determines how high objects are lifted off the ground when the haunting
    /// animation is in the Lifting phase. This range is used to randomize the height for each object
    /// involved in the animation, contributing to the dynamic visual effects.
    /// </remarks>
    public FloatRange LiftRange = new(1f, 10f);

    /// <summary>
    /// Represents a range of valid spin counts for objects being manipulated during the haunting spin phase
    /// in the HauntAnimationController. The spin count determines how many rotations an object will complete
    /// during the spin phase of the haunting process.
    /// </summary>
    public IntRange SpinRange = new(1, 30);

    /// <summary>
    /// Represents the original weather state of the map before the haunting event begins.
    /// This field is used to store the current weather at the start of a haunting sequence
    /// and revert back to it once the haunting concludes.
    /// </summary>
    public WeatherDef originalWeather;

    /// <summary>
    /// Tracks the number of ticks that have passed since the last emission event
    /// related to the haunting process in the HauntAnimationController.
    /// </summary>
    public int ticksSinceLastEmitted;

    /// <summary>
    /// A field representing a visual effect associated with the haunting activity.
    /// The mote is used to create and manage an attached overlay effect on the target flyer during
    /// the haunting process. Its position is updated dynamically based on the target flyer's position.
    /// </summary>
    protected Mote psychicEffectMote;

    /// <summary>
    /// Represents the parameters that define the behavior of a haunting animation.
    /// </summary>
    public class HauntingParams(float liftHeightHeight, int timesToSpin, int startLiftOffset = 0) : IExposable
    {
        /// <summary>
        /// Represents the vertical displacement (in units) to which an object is lifted during the haunting animation.
        /// </summary>
        /// <remarks>
        /// This value determines the maximum height to which an object will be raised during the "LiftPhase" of the animation.
        /// It is utilized to calculate the position and motion of objects being haunted.
        /// </remarks>
        public float LiftHeight = liftHeightHeight;

        /// Represents the number of times the target object is spun during the spin phase
        /// of a haunting animation.
        /// This value is utilized to calculate the angular rotation and movement
        /// of the target object during the spinning phase.
        public int TimesToSpin = timesToSpin;

        /// <summary>
        /// The offset in ticks before the lifting phase starts for a specific haunted flyer object.
        /// Represents the delay relative to the start of the lifting phase for other objects.
        /// This value ensures a staggered lifting effect among haunted objects.
        /// </summary>
        public int StartLiftOffset = startLiftOffset;

        /// <summary>
        /// Represents the radius of a circular motion during the haunting animation, defining the distance
        /// between the object in motion and the center of its rotation.
        /// </summary>
        public float Radius;

        /// <summary>
        /// Represents the starting position for the rotational movement of a haunted object during the spinning phase.
        /// </summary>
        /// <remarks>
        /// This vector serves as the reference point for calculating the positional transformations
        /// based on rotational dynamics during the spinning phase of the haunting animation.
        /// </remarks>
        public Vector3 RotationStart;

        /// <summary>
        /// Represents the final position of a haunted object after the spinning phase of the haunting animation has completed.
        /// </summary>
        /// <remarks>
        /// This variable is updated during the SpinPhaseTick method within the HauntAnimationController class.
        /// It stores the calculated position of the affected object as it transitions from spinning to the dropping phase.
        /// </remarks>
        public Vector3 EndSpinPosition;

        /// Calculates the current rotated position of a moving object during the spinning phase
        /// of a haunting event based on the progress of the phase.
        /// The position is determined by the object's rotation around an epicenter, with the
        /// rotation angle changing over time. The rotation is distributed evenly across the
        /// specified number of spins.
        /// Parameters:
        /// - `PhaseLength`: The total duration (in ticks) of the spinning phase.
        /// - `currentPhaseTick`: The current elapsed time (in ticks) in the spinning phase.
        /// Returns:
        /// - A `Vector3` representing the position of the rotating object at the given point
        /// in time within the spinning phase.
        public Vector3 CurrentRotatedPos(int PhaseLength, int currentPhaseTick)
        {
            int ticksPerRotation = PhaseLength / TimesToSpin;
            float anglePerTick = Mathf.Deg2Rad * (360f / ticksPerRotation);
            int ticksIntoCurrentRotation = currentPhaseTick % ticksPerRotation;

            float currentAngle = anglePerTick * ticksIntoCurrentRotation;

            return RotationStart + new Vector3(Mathf.Sin(currentAngle) * Radius, 0, Mathf.Cos(currentAngle) * Radius);
        }

        /// Saves and loads the state of the object for serialization.
        /// Ensures that important fields such as `HauntingActive`, `Target`, `HauntedThings`,
        /// `LiftPhaseTicks`, `SpinPhaseTicks`, `DropPhaseTicks`, `StartPhaseTick`, `originalWeather`,
        /// `CurrentPhase`, `TargetFlyer`, and `ticksSinceLastEmitted` are preserved across game saves.
        /// This method is automatically called during the save/load process of the game.
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
    /// Each entry associates a <see cref="HauntedThingFlyer"/> (representing the haunted object) with
    /// its corresponding <see cref="HauntAnimationController.HauntingParams"/>, which define the
    /// behavior and animation details for the haunting process.
    /// </summary>
    public Dictionary<HauntedThingFlyer, HauntingParams> HauntedThings;

    /// <summary>
    /// Represents an offset vector affecting the epicenter position
    /// used in the haunting animation process.
    /// </summary>
    /// <remarks>
    /// This variable is combined with the StartPos vector to calculate
    /// the effective epicenter (see the Epicenter property). It is used
    /// to modify the location where haunting effects are centered or
    /// initiated in the environment.
    /// </remarks>
    public Vector3 EpicenterOffset = new Vector3(0, 0, 5);

    /// <summary>
    /// Gets the position of the epicenter for the haunting animation. The epicenter is calculated
    /// as the sum of the starting position and the pre-defined offset.
    /// </summary>
    public Vector3 Epicenter => StartPos + EpicenterOffset;

    /// <summary>
    /// Represents the different phases of a haunting sequence in the <c>HauntAnimationController</c>.
    /// These phases dictate the sequence of animation actions for haunted objects.
    /// </summary>
    public enum HauntPhase : byte
    {
        /// <summary>
        /// Represents the initial phase of a haunting event where items or entities
        /// are gradually lifted into the air. During this phase, objects begin to
        /// defy gravity and ascend to a predetermined height, signifying the start
        /// of a supernatural occurrence.
        /// </summary>
        Lifting,

        /// <summary>
        /// Represents the spinning phase of the haunting sequence in the HauntAnimationController.
        /// During the spinning phase, the haunted object or entity rotates around an epicenter.
        /// The duration and speed of the spinning phase are controlled by configurable parameters
        /// such as <see cref="HauntAnimationController.SpinPhaseTimeRange"/> and <see cref="HauntAnimationController.SpinRange"/>.
        /// This phase immediately follows the lifting phase and precedes the dropping phase in the haunting process.
        /// </summary>
        Spinning,

        /// <summary>
        /// Represents the "Dropping" phase of a haunting animation, where the objects being haunted
        /// are returned to their starting positions or destroyed, marking the end of the haunting process.
        /// </summary>
        /// <remarks>
        /// During this phase, the system calculates the interpolation between the epicenter of the haunting
        /// and the original positions of the haunted objects. Atmospheric conditions are restored, and
        /// resources used during the haunting are cleaned up (e.g., destruction of associated items like motes).
        /// This phase follows the "Spinning" phase and concludes the haunting process.
        /// </remarks>
        Dropping,
    }

    /// <summary>
    /// Represents the current phase of the haunting animation sequence.
    /// This phase determines the current step in the haunting process,
    /// such as lifting, spinning, or dropping the target.
    /// </summary>
    public HauntPhase CurrentPhase = HauntPhase.Lifting;

    /// <summary>
    /// Represents the total number of ticks allocated for the lifting phase in the haunting animation sequence.
    /// During this phase, the target object or pawn starts to rise from its initial position.
    /// </summary>
    public int LiftPhaseTicks = 0;

    /// <summary>
    /// Represents the duration in ticks for the spin phase of the haunting animation sequence.
    /// </summary>
    /// <remarks>
    /// The spin phase occurs after the lifting phase and is characterized by a rotational effect
    /// applied to haunted objects during the haunting animation. The value of this variable is
    /// determined randomly within the range specified by <see cref="SpinPhaseTimeRange"/> when the
    /// haunting sequence starts. It is used to control the timing and behavior of the spin phase.
    /// </remarks>
    public int SpinPhaseTicks = 0;

    /// <summary>
    /// Tracks the duration, in ticks, for the drop phase during the haunting sequence.
    /// The drop phase is the final stage where haunted objects, including the main target,
    /// move back towards their original positions or settle into their concluding movements.
    /// This value is dynamically determined at the start of the haunting sequence by
    /// randomizing within the <see cref="DropPhaseTimeRange"/>.
    /// </summary>
    public int DropPhaseTicks = 0;

    /// <summary>
    /// The tick at which the current haunting phase begins.
    /// Used to track the starting point of the phase within the haunting sequence,
    /// allowing calculations for phase-specific durations and transitions.
    /// </summary>
    public int StartPhaseTick = 0;

    /// <summary>
    /// Saves and loads the state of the HauntAnimationController instance, including its internal state,
    /// active haunting properties, current phase, and affected entities or objects in the map.
    /// </summary>
    /// <remarks>
    /// This method ensures that the HauntAnimationController's data is persisted across game saves and loads.
    /// It serializes and deserializes values that define the haunting mechanics, such as phase timing,
    /// current phase, target objects, and other relevant attributes.
    /// </remarks>
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref HauntingActive, "HauntingActive", HauntingActive);
        Scribe_References.Look(ref Target, "Target");
        Scribe_Collections.Look(ref HauntedThings, "HauntedThings", LookMode.Reference, LookMode.Deep);
        Scribe_Values.Look(ref LiftPhaseTicks, "LiftPhaseTicks", LiftPhaseTicks);
        Scribe_Values.Look(ref SpinPhaseTicks, "SpinPhaseTicks", SpinPhaseTicks);
        Scribe_Values.Look(ref DropPhaseTicks, "DropPhaseTicks", DropPhaseTicks);
        Scribe_Values.Look(ref StartPhaseTick, "StartPhaseTick", StartPhaseTick);
        Scribe_Defs.Look(ref originalWeather, "originalWeather");
        Scribe_Values.Look(ref CurrentPhase, "CurrentPhase", CurrentPhase);
        Scribe_References.Look(ref TargetFlyer, "TargetFlyer");
        Scribe_Values.Look(ref ticksSinceLastEmitted, "ticksSinceLastEmitted", 0);
    }

    /// Retrieves a list of valid `Thing` options within a specified radius around the target flyer.
    /// These options are determined based on certain exclusion criteria, such as specific
    /// `ThingCategory` values or other attributes of the `Thing`.
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

        // Target.stances.stunner.StunFor(LiftPhaseTicks + SpinPhaseTicks + DropPhaseTicks, Target, true, true, false);

        List<Thing> things = GetValidThingOptions(radius);
        List<Thing> selectedThings = things.Take(minItems).ToList();

        selectedThings.AddRange(things.Except(selectedThings).Where(t => Rand.Chance(hauntChance)));

        HauntedThings = new Dictionary<HauntedThingFlyer, HauntingParams>();

        foreach (Thing selectedThing in selectedThings)
        {
            HauntedThingFlyer flyer = ThingMaker.MakeThing(MSS_HauntedDefOf.MSS_Haunted_HauntedThingFlyer, null) as HauntedThingFlyer;
            GenSpawn.Spawn(flyer, selectedThing.Position, map);
            if (flyer?.AddThing(selectedThing) ?? false)
            {
                HauntedThings[flyer] = new HauntingParams(LiftRange.RandomInRange, SpinRange.RandomInRange, StartLiftOffsetRange.RandomInRange);
            }
        }
        StartPos = TargetFlyer.DrawPos;

        HauntingActive = true;
    }

    /// <summary>
    /// Handles the "Lifting" phase of the haunting animation. This method is called each tick during the
    /// "Lifting" phase to progressively move haunted objects upwards, calculating their position based
    /// on the elapsed time since the start of the phase and their specific offset values. It also moves
    /// the target flyer towards the epicenter during this phase.
    /// </summary>
    /// <remarks>
    /// - Transitions the animation state to the next phase ("Spinning") if the current time surpasses
    /// the duration of the lifting phase.
    /// - Updates the position and height of each haunted object incrementally to provide a smooth
    /// lifting effect.
    /// - Calculates movement using linear interpolation between starting and target positions for
    /// both haunted objects and the player-targeted flyer.
    /// - Ignores objects that have a "start lift offset" greater than the current elapsed phase time.
    /// </remarks>
    public void LiftPhaseTick()
    {
        HauntedThings.RemoveAll(kv => kv.Key.Destroyed);

        if (StartPhaseTick + LiftPhaseTicks < Find.TickManager.TicksGame)
        {
            CurrentPhase++;
            foreach (KeyValuePair<HauntedThingFlyer, HauntingParams> hauntedThing in HauntedThings)
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

        foreach (KeyValuePair<HauntedThingFlyer, HauntingParams> hauntedThing in HauntedThings)
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

    /// <summary>
    /// Updates the positions of all haunted objects during the spinning phase of the haunting animation.
    /// Each haunted object is rotated around a central epicenter for the duration of the spinning phase.
    /// Changes the phase to Dropping once the spinning phase duration is complete.
    /// </summary>
    /// <remarks>
    /// This method calculates the position of each haunted object based on the current tick and the spin phase duration.
    /// It ensures that haunted objects are continuously rotated during this phase.
    /// If the spinning phase time has elapsed, the CurrentPhase is incremented, transitioning to the next phase (Dropping).
    /// </remarks>
    public void SpinPhaseTick()
    {
        HauntedThings.RemoveAll(kv => kv.Key.Destroyed);
        if (StartPhaseTick + LiftPhaseTicks + SpinPhaseTicks < Find.TickManager.TicksGame)
        {
            CurrentPhase++;
            foreach (KeyValuePair<HauntedThingFlyer, HauntingParams> hauntedThing in HauntedThings)
            {
                hauntedThing.Value.EndSpinPosition = hauntedThing.Key.DrawPos;
                hauntedThing.Key.LiftedPosition = Vector3.zero;
            }

            TargetFlyer.LiftedPosition = Vector3.zero;
            return;
        }

        int currentPhaseTick = Find.TickManager.TicksGame - StartPhaseTick;

        foreach (KeyValuePair<HauntedThingFlyer, HauntingParams> hauntedThing in HauntedThings)
        {
            hauntedThing.Key.SetPositionDirectly(hauntedThing.Value.CurrentRotatedPos(SpinPhaseTicks, currentPhaseTick));
        }
    }

    /// <summary>
    /// Executes the logic for the "dropping" phase of the haunting animation.
    /// </summary>
    /// <remarks>
    /// During the dropping phase, all participating objects, including the target flyer and haunted objects, are transitioned
    /// from their current positions to their final resting positions. This is achieved through linear interpolation based on
    /// the ratio of elapsed time during the drop phase.
    /// </remarks>
    /// <remarks>
    /// At the end of this phase, the haunting process is concluded. All affected entities and objects are cleaned up, including
    /// the destruction of motes, haunted flyers, and resetting weather to its original state. Haunting-related data is also reset
    /// within the map component.
    /// </remarks>
    /// <remarks>
    /// The method ensures the graceful termination of the haunting process by safely managing cleanup and restoring the environment
    /// to its pre-haunting state.
    /// </remarks>
    public void DropPhaseTick()
    {
        HauntedThings.RemoveAll(kv => kv.Key.Destroyed);

        if (StartPhaseTick + LiftPhaseTicks + SpinPhaseTicks + DropPhaseTicks < Find.TickManager.TicksGame)
        {
            HauntingActive = false;
            foreach (HauntedThingFlyer hauntedThingFlyer in HauntedThings.Keys.ToList())
            {
                HauntedThings.Remove(hauntedThingFlyer);
                hauntedThingFlyer.Destroy();
            }

            TargetFlyer.Destroy();
            TargetFlyer = null;

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

            psychicEffectMote.Destroy();
            psychicEffectMote = null;

            return;
        }

        float ratio = (float)(Find.TickManager.TicksGame - (StartPhaseTick + LiftPhaseTicks + SpinPhaseTicks)) / DropPhaseTicks;

        foreach (KeyValuePair<HauntedThingFlyer, HauntingParams> hauntedThing in HauntedThings)
        {
            hauntedThing.Key.SetPositionDirectly(Vector3.Lerp(hauntedThing.Value.EndSpinPosition, hauntedThing.Key.StartPosition, ratio));
        }

        TargetFlyer.SetPositionDirectly(Vector3.Lerp(Epicenter, StartPos, ratio));
    }

    /// <summary>
    /// Handles the per-tick logic for the haunting animation behavior in the map component.
    /// This includes transitioning between different phases of the haunting process
    /// (Lifting, Spinning, Dropping) and maintaining visual effects like motes
    /// attached to the targeted flyer object.
    /// </summary>
    /// <remarks>
    /// The method checks the current active phase of the haunting and delegates
    /// control to specific phase-handling methods such as <see cref="LiftPhaseTick"/>,
    /// <see cref="SpinPhaseTick"/>, or <see cref="DropPhaseTick"/>. It also ensures
    /// that a visual effect (mote) is created and updated in sync with the position
    /// of the <see cref="TargetFlyer"/> object. If the haunting is not active,
    /// the method performs no operations.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if the current haunt phase is not recognized.
    /// </exception>
    public override void MapComponentTick()
    {
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
                return;
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
