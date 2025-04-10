using System;
using RimWorld;
using Verse;

namespace MSS_Haunted.Needs;

public class ThoughtWorker_NeedMutationalStability : ThoughtWorker
{
    public ThoughtWorker_NeedMutationalStability()
        : base() { }

    protected override ThoughtState CurrentStateInternal(Pawn p)
    {
        Needs_MutationalStability need = p.needs.TryGetNeed<Needs_MutationalStability>();
        return need == null ? ThoughtState.Inactive : ThoughtState.ActiveAtStage((int)need.CurCategory);
    }
}
