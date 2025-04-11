using System.Collections.Generic;
using System.Linq;
using MSS_Haunted.Jobs;
using MSS_Haunted.Needs;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace MSS_Haunted.Incidents;

public class IncidentWorker_ShadowPeople : IncidentWorker_EntitySwarm
{
    protected override LordJob GenerateLordJob(IntVec3 entry, IntVec3 dest)
    {
        return new LordJob_ShadowSwarm(entry, dest);
    }

    protected override PawnGroupKindDef GroupKindDef => MSS_HauntedDefOf.MSS_Haunted_Shadows;

    protected override List<Pawn> GenerateEntities(IncidentParms parms, float points)
    {
        Map target = (Map)parms.target;
        PawnGroupMakerParms parms1 = new()
        {
            groupKind = GroupKindDef,
            tile = target.Tile,
            faction = Find.FactionManager.FirstFactionOfDef(MSS_HauntedDefOf.MSSFP_HauntedFaction),
            points = points * SwarmSizeVariance.RandomInRange,
        };
        parms1.points = Mathf.Max(parms1.points, MSS_HauntedDefOf.MSSFP_HauntedFaction.MinPointsToGeneratePawnGroup(parms1.groupKind) * 1.05f);
        return PawnGroupMakerUtility.GeneratePawns(parms1).ToList();
    }

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        bool ret = base.TryExecuteWorker(parms);
        if (parms.target is Map map)
        {
            foreach (Pawn pawn in map.mapPawns.FreeColonistsSpawned)
            {
                Need need = pawn.needs.TryGetNeed<Needs_Paranoia>();
                if (need != null)
                    need.CurLevel -= 0.4f;
            }
        }

        return ret;
    }
}
