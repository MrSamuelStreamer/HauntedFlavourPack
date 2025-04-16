using MSS_Haunted.Needs;
using VAEInsanity;
using Verse;

namespace MSS_Haunted.Hediffs;

public class HediffComp_SanityTicker : HediffComp
{
    public HediffCompProperties_SanityTicker Props => (HediffCompProperties_SanityTicker)props;

    public override void CompPostTick(ref float severityAdjustment)
    {
        if (!parent.pawn.IsHashIntervalTick(Props.sanityTickInterval))
            return;

        Need_Sanity need = parent.pawn.needs.TryGetNeed<Need_Sanity>();
        if (need != null)
        {
            need.CurLevel -= Props.dropPerInterval.RandomInRange;
        }
    }
}
