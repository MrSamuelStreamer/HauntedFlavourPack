using System.Collections.Generic;
using RimWorld;
using Verse;

namespace MSS_Haunted.Hediffs;

public class HediffCompProperties_Ghostification : HediffCompProperties
{
    public List<XenotypeDef> xenoTypes = [];
    public List<TraitDef> possibleTraits = [];
    public IntRange ghostificationTime = new(600, 6000);

    public HediffCompProperties_Ghostification() => compClass = typeof(HediffComp_Ghostification);
}
