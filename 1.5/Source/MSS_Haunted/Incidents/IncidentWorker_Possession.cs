using MSS_Haunted.Needs;
using RimWorld;
using Verse;

namespace MSS_Haunted.Incidents;

public class IncidentWorker_Possession : IncidentWorker
{
    protected override bool CanFireNowSub(IncidentParms parms)
    {
        return base.CanFireNowSub(parms) && parms.target is Map;
    }

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        Map target = (Map)parms.target;

        Pawn targetPawn = target.mapPawns.FreeColonistsAndPrisonersSpawned.RandomElementWithFallback();

        if (targetPawn == null)
            return false;

        targetPawn.health.AddHediff(MSS_HauntedDefOf.MSS_Haunted_PossessionHaunt);

        foreach (Pawn pawn in target.mapPawns.FreeColonistsSpawned)
        {
            Need need = pawn.needs.TryGetNeed<Needs_Paranoia>();
            if (need != null)
                need.CurLevel -= 0.2f;
        }

        SendStandardLetter(parms, null);

        return true;
    }
}
