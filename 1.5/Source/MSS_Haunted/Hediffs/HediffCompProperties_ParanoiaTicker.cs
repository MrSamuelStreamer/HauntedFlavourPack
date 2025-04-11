using Verse;

namespace MSS_Haunted.Hediffs;

public class HediffCompProperties_ParanoiaTicker : HediffCompProperties
{
    public int paranoiaTickInterval = 600;
    public FloatRange dropPerInterval = new FloatRange(0.01f, 0.05f);

    public HediffCompProperties_ParanoiaTicker()
        : base()
    {
        compClass = typeof(HediffComp_ParanoiaTicker);
    }
}
