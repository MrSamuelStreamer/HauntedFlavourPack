using MSS_Haunted.Needs;
using Verse;

namespace MSS_Haunted.Hediffs;

public class HediffComp_ParanoiaTicker : HediffComp
{
    public HediffCompProperties_ParanoiaTicker Props => (HediffCompProperties_ParanoiaTicker)props;

    public override void CompPostTick(ref float severityAdjustment)
    {
        if (!parent.pawn.IsHashIntervalTick(Props.paranoiaTickInterval))
            return;

        Needs_Paranoia need = parent.pawn.needs.TryGetNeed<Needs_Paranoia>();
        if (need != null)
        {
            need.CurLevel -= Props.dropPerInterval.RandomInRange;
        }
    }
}
