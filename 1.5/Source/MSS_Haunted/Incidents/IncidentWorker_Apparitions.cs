using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace MSS_Haunted.Incidents;

public class IncidentWorker_Apparitions : IncidentWorker
{
    protected override bool CanFireNowSub(IncidentParms parms)
    {
        if (!base.CanFireNowSub(parms) || parms.target is not Map map)
            return false;

        return map.mapPawns.FreeColonistsAndPrisonersSpawned.Any(p => p.health?.hediffSet?.HasHediff(MSS_HauntedDefOf.MSS_Haunted_PossessionHaunt) ?? false);
    }

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        Map target = (Map)parms.target;

        // add apparition hidden hediff to all target.mapPawns.FreeColonistsAndPrisonersSpawned

        foreach (Pawn pawn in target.mapPawns.FreeColonistsAndPrisonersSpawned)
        {
            pawn.health?.AddHediff(MSS_HauntedDefOf.MSS_Haunted_ApparitionHaunt);
        }

        return true;
    }
}
