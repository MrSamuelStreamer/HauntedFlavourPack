using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace MSS_Haunted;

public class PawnGroupKindWorker_Shadows : PawnGroupKindWorker_Normal
{
    private static readonly FloatRange ChildrenDisabledExcludedAgeRange = new(0.0f, 18f);

    protected override void GeneratePawns(PawnGroupMakerParms parms, PawnGroupMaker groupMaker, List<Pawn> outPawns, bool errorOnZeroResults = true)
    {
        if (!CanGenerateFrom(parms, groupMaker))
        {
            if (!errorOnZeroResults)
                return;
            Log.Error("Cannot generate pawns for " + parms.faction + " with " + parms.points + ". Defaulting to a single random cheap group.");
        }
        else
        {
            float points = parms.points;
            float minCost = groupMaker.options.Min(opt => opt.Cost);
            int generationAttempts = 0;
            while (points > (double)minCost && generationAttempts < 200)
            {
                ++generationAttempts;
                groupMaker.options.TryRandomElementByWeight(gr => gr.selectionWeight, out PawnGenOption result);
                if (!(result.Cost <= (double)points))
                    continue;

                points -= result.Cost;

                Pawn pawn = PawnGenerator.GeneratePawn(
                    new PawnGenerationRequest(
                        result.kind,
                        parms.faction,
                        developmentalStages: DevelopmentalStage.Adult,
                        excludeBiologicalAgeRange: ChildrenDisabledExcludedAgeRange
                    )
                );
                outPawns.Add(pawn);
            }
        }
    }
}
