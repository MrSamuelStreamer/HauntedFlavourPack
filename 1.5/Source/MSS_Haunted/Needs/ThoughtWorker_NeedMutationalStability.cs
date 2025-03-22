using System;
using RimWorld;
using Verse;

namespace MSS_Haunted.Needs;

public class ThoughtWorker_NeedMutationalStabilityStage : ThoughtWorker
{
    public ThoughtWorker_NeedMutationalStabilityStage()
        : base() { }

    protected override ThoughtState CurrentStateInternal(Pawn p)
    {
        Needs_MutationalStability need = p.needs.TryGetNeed<Needs_MutationalStability>();
        return need == null ? ThoughtState.Inactive : ThoughtState.ActiveAtStage((int)need.CurCategory);
    }
}
