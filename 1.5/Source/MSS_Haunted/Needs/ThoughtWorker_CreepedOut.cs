using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace MSS_Haunted.Needs;

public class ThoughtWorker_CreepedOut : ThoughtWorker
{
    public static SimpleCurve thoughtCurve = new SimpleCurve()
    {
        new CurvePoint(0f, 1f),
        new CurvePoint(1f, 2f),
        new CurvePoint(10f, 3f),
        new CurvePoint(25f, 4f),
        new CurvePoint(50f, 5f),
        new CurvePoint(100f, 6f),
    };

    protected override ThoughtState CurrentStateInternal(Pawn p)
    {
        if (!ModsConfig.AnomalyActive)
            return false;
        if (p.MapHeld == null)
            return false;
        if (!p.IsPlayerControlled && !p.IsPrisonerOfColony)
            return false;
        List<Pawn> shadows = p.MapHeld.mapPawns.AllPawns.Where(pawn => pawn.def == MSS_HauntedDefOf.MSS_Haunted_ShadowPerson).ToList();

        if (shadows.Count <= 0)
            return false;

        int nearby = shadows.Count(pawn => p.Position.InHorDistOf(pawn.Position, 45));

        return ThoughtState.ActiveAtStage(Mathf.RoundToInt(thoughtCurve.Evaluate(nearby)));
    }
}
