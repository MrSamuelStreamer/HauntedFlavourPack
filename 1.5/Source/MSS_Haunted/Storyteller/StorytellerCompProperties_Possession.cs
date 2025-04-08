using RimWorld;
using Verse;

namespace MSS_Haunted.Storyteller;

public class StorytellerCompProperties_Possession : StorytellerCompProperties
{
    public float baseMtbDaysPerPossessed = 20;

    public SimpleCurve IntensityScaleByDays = new SimpleCurve();

    public StorytellerCompProperties_Possession()
        : base()
    {
        compClass = typeof(StorytellerComp_Possession);
    }
}
