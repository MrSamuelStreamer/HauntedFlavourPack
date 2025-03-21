﻿using RimWorld;
using Verse;

namespace MSS_Haunted;

[DefOf]
public static class MSS_HauntedDefOf
{
    // Remember to annotate any Defs that require a DLC as needed e.g.
    // [MayRequireBiotech]
    // public static GeneDef YourPrefix_YourGeneDefName;

    public static readonly HediffDef MSS_Haunted_PossessionHaunt;
    public static readonly ThingDef MSS_Haunted_HauntedThingFlyer;
    public static readonly ThingDef MSS_Haunted_HauntedThingTargetFlyer;
    public static readonly WeatherDef MSS_Haunted_Thunderstorm;

    static MSS_HauntedDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(MSS_HauntedDefOf));
}
