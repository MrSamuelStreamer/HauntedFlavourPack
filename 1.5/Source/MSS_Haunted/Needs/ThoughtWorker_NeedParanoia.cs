using System;
using RimWorld;
using Verse;

namespace MSS_Haunted.Needs;

public class ThoughtWorker_NeedParanoia : ThoughtWorker
{
    public ThoughtWorker_NeedParanoia()
        : base() { }

    protected override ThoughtState CurrentStateInternal(Pawn p)
    {
        Needs_Paranoia need = p.needs.TryGetNeed<Needs_Paranoia>();
        return need == null ? ThoughtState.Inactive : ThoughtState.ActiveAtStage((int)need.CurCategory);
    }
}
