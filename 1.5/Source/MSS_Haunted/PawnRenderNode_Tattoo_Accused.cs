using Verse;

namespace MSS_Haunted;

[StaticConstructorOnStartup]
public class PawnRenderNode_Tattoo_Accused(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree) : PawnRenderNode_Tattoo(pawn, props, tree)
{
    public override GraphicMeshSet MeshSetFor(Pawn pawn)
    {
        return HumanlikeMeshPoolUtility.GetHumanlikeHeadSetForPawn(pawn);
    }

    public override Graphic GraphicFor(Pawn pawn)
    {
        return MSS_HauntedDefOf.MSS_AccusedMark.GraphicFor(pawn, ColorFor(pawn));
    }
}
