using System.Linq;
using RimWorld;
using VAEInsanity;
using Verse;

namespace MSS_Haunted.Incidents;

public class IncidentWorker_Possession : IncidentWorker
{
    protected override bool CanFireNowSub(IncidentParms parms)
    {
        return base.CanFireNowSub(parms) && parms.target is Map && GetTargetPawn(parms) != null;
    }

    public virtual Pawn GetTargetPawn(IncidentParms parms)
    {
        Map target = (Map)parms.target;

        return target.mapPawns.FreeColonistsAndPrisonersSpawned.Where(p => !p.health.hediffSet.HasHediff(MSS_HauntedDefOf.MSS_Haunted_PossessionHaunt)).RandomElementWithFallback();
    }

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        Map target = (Map)parms.target;

        Pawn targetPawn = GetTargetPawn(parms);

        if (targetPawn == null)
            return false;

        targetPawn.health.AddHediff(MSS_HauntedDefOf.MSS_Haunted_PossessionHaunt);

        foreach (Pawn pawn in target.mapPawns.FreeColonistsSpawned)
        {
            Need need = pawn.needs.TryGetNeed<Need_Sanity>();
            if (need != null)
                need.CurLevel -= 0.2f;
        }

        SendStandardLetter(parms, null);

        return true;
    }
}
