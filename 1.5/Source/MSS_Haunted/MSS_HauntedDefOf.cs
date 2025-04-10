using RimWorld;
using Verse;
using Verse.AI;

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

    public static readonly NeedDef MSS_Haunted_Paranoia;
    public static readonly StatDef MSS_Haunted_Paranoia_RecoverySpeed;

    public static readonly StatDef MSS_Haunted_MysticalSensitivity;

    public static readonly IncidentCategoryDef MSS_Haunted;

    public static readonly IncidentDef MSS_Haunted_PoltergeistSmall;
    public static readonly HediffDef MSS_Haunted_ApparitionHaunt;
    public static readonly HediffDef MSS_Haunted_ShadowPersonInvisibility;

    public static readonly DutyDef MSS_Haunted_ShadowPersonSwarm;
    public static readonly ThingDef MSS_Haunted_ShadowPerson;
    public static readonly PawnGroupKindDef MSS_Haunted_Shadows;
    public static readonly FactionDef MSSFP_HauntedFaction;

    static MSS_HauntedDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(MSS_HauntedDefOf));
}
