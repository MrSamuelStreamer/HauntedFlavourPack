using RimWorld;
using UnityEngine;
using Verse;

namespace MSS_Haunted.Needs;

public class Needs_Paranoia : Need
{
    public Needs_Paranoia(Pawn pawn)
        : base(pawn) { }

    public override bool ShowOnNeedList
    {
        get { return !Disabled; }
    }

    private bool Disabled
    {
        get { return pawn.Dead; }
    }

    public override void SetInitialLevel()
    {
        CurLevel = 1f;
    }

    public override int GUIChangeArrow
    {
        get
        {
            if (!Mathf.Approximately(CurLevel, 1.0f))
                return 1;
            return pawn.GetStatValue(MSS_HauntedDefOf.MSS_Haunted_Paranoia_RecoverySpeed, true, -1) < 0.0 ? -1 : 0;
        }
    }

    public float GlobalRecoveryMultiplier => 1f; // Get from settings

    public override void NeedInterval()
    {
        if (!IsFrozen)
            CurLevel += 5E-05f * pawn.GetStatValue(MSS_HauntedDefOf.MSS_Haunted_Paranoia_RecoverySpeed, true, -1) * GlobalRecoveryMultiplier;
    }

    public ParanoiaStage CurCategory
    {
        get
        {
            if ((double)this.CurLevel > 0.800000011920929)
                return ParanoiaStage.None;
            if ((double)this.CurLevel > 0.64999997615814209)
                return ParanoiaStage.Initial;
            if ((double)this.CurLevel > 0.5)
                return ParanoiaStage.Minor;
            if ((double)this.CurLevel > 0.34999999403953552)
                return ParanoiaStage.Moderate;
            return (double)this.CurLevel > 0.20000000298023224 ? ParanoiaStage.Serious : ParanoiaStage.Extreme;
        }
    }
}
