using RimWorld;
using Verse;

namespace MSS_Haunted.Comps;

public class CompGlowWhileStanding : CompGlower
{
    public JobDef previousJob;
    protected override bool ShouldBeLitNow => parent.Spawned && parent is Pawn pawn && pawn.CurJobDef == JobDefOf.Wait_Wander;

    public override void CompTick()
    {
        if (!parent.Spawned || parent is not Pawn pawn)
            return;

        if (previousJob != pawn.CurJobDef)
        {
            UpdateLit(parent.Map);
        }
        previousJob = pawn.CurJobDef;
    }
}
