using System.Linq;
using MSS_Haunted.ModExtensions;
using MSS_Haunted.Utils;
using RimWorld;
using UnityEngine;
using Verse;

namespace MSS_Haunted.Needs;

public class Needs_MutationalStability(Pawn pawn) : Need(pawn)
{
    public override bool ShowOnNeedList
    {
        get { return !Disabled; }
    }

    protected int lastInstantCheckTick = -9999;
    protected float lastInstant = -1f;

    public override float CurInstantLevel
    {
        get
        {
            if (pawn.IsHashIntervalTick(60))
            {
                float stabilitySum = Mathf.Clamp01(
                    pawn.health.hediffSet.hediffs.Sum(h => h.def.TryGetDefModExtension(out MutationDefModExtension ext) ? ext.GetInitialOffset(h) : 0f)
                );
                stabilitySum *= Mathf.Clamp(pawn.GetStatValue(MSS_HauntedDefOf.MSS_Haunted_MysticalSensitivity), 0, 2);

                CurLevel = lastInstant = 1 - stabilitySum;
            }
            return lastInstant;
        }
    }

    private bool Disabled
    {
        get { return pawn.Dead; }
    }

    public override void NeedInterval() { }

    public override void SetInitialLevel()
    {
        CurLevel = 1f;
    }

    public override int GUIChangeArrow => 0;

    public float GlobalRecoveryMultiplier => 1f; // Get from settings

    public MutationalStabilityStage CurCategory
    {
        get
        {
            if (CurLevel > 0.800000011920929)
                return MutationalStabilityStage.None;
            if (CurLevel > 0.64999997615814209)
                return MutationalStabilityStage.Initial;
            if (CurLevel > 0.5)
                return MutationalStabilityStage.Minor;
            if (CurLevel > 0.34999999403953552)
                return MutationalStabilityStage.Moderate;
            return CurLevel > 0.20000000298023224 ? MutationalStabilityStage.Serious : MutationalStabilityStage.Extreme;
        }
    }
}
