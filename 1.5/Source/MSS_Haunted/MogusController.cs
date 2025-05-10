using System.Collections.Generic;
using System.Linq;
using MSSFP;
using RimWorld;
using Verse;

namespace MSS_Haunted;

public class MogusController(Map map) : MapComponent(map)
{
    public static HashSet<PawnKindDef> _ValidPawnKindDef;

    public static HashSet<PawnKindDef> ValidPawnKindDef
    {
        get
        {
            if (_ValidPawnKindDef.NullOrEmpty())
            {
                _ValidPawnKindDef = [MSSFPDefOf.MSSFP_MogusKind_Blue, MSSFPDefOf.MSSFP_MogusKind_Red, MSSFPDefOf.MSSFP_MogusKind_Green, MSSFPDefOf.MSSFP_MogusKind_Yellow];
            }
            return _ValidPawnKindDef;
            ;
        }
    }

    public override void MapComponentTick()
    {
        if (!map.IsPlayerHome)
            return;
        if (!map.IsHashIntervalTick(GenDate.TicksPerDay))
        {
            return;
        }

        List<Pawn> moguses = map.mapPawns.AllPawns.Where(p => ValidPawnKindDef.Contains(p.kindDef)).ToList();

        if ((!moguses.NullOrEmpty() && Rand.Chance(0.95f)) || Rand.Bool) // 5% chance to spawn a mogus if there's already moguses, 50% if there aren't any
            return;

        Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(ValidPawnKindDef.RandomElement(), Faction.OfPlayer, forceGenerateNewPawn: true));

        CellFinder.TryFindRandomEdgeCellWith(c => map.reachability.CanReachColony(c) && !c.Fogged(map), map, CellFinder.EdgeRoadChance_Neutral, out IntVec3 cell);
        GenSpawn.Spawn(pawn, cell, map);

        ModLog.Debug("Spawned a mogus as there weren't any on the map");
    }
}
