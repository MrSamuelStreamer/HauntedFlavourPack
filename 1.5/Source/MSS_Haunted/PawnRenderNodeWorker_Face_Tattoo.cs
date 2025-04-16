using Verse;

namespace MSS_Haunted;

public class PawnRenderNodeWorker_Face_Tattoo : PawnRenderNodeWorker_FlipWhenCrawling
{
    public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
    {
        return base.CanDrawNow(node, parms) && (MSS_HauntedDefOf.MSS_AccusedMark.visibleNorth || !(parms.facing == Rot4.North));
    }
}
