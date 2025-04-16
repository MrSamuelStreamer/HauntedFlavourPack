using Verse;

namespace MSS_Haunted.Hediffs;

public class HediffCompProperties_SanityTicker : HediffCompProperties
{
    public int sanityTickInterval = 600;
    public FloatRange dropPerInterval = new FloatRange(0.01f, 0.05f);

    public HediffCompProperties_SanityTicker()
        : base()
    {
        compClass = typeof(HediffComp_SanityTicker);
    }
}
