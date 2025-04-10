using HarmonyLib;
using RimWorld;
using Verse;

namespace MSS_Haunted.HarmonyPatches;

[HarmonyPatch(typeof(InvisibilityUtility))]
public static class InvisibilityUtility_Patch
{
    // [HarmonyPatch(nameof(InvisibilityUtility.IsHiddenFromPlayer))]
    // [HarmonyPostfix]
    // public static void IsHiddenFromPlayer_Patch(Pawn pawn, ref bool __result)
    // {
    //     if (pawn.health?.hediffSet.HasHediff(MSS_HauntedDefOf.MSS_Haunted_ShadowPersonInvisibility) ?? false)
    //         __result = true;
    // }
    //
    // [HarmonyPatch(nameof(InvisibilityUtility.IsPsychologicallyInvisible))]
    // [HarmonyPostfix]
    // public static void IsPsychologicallyInvisible_Patch(Pawn pawn, ref bool __result)
    // {
    //     if (pawn.health?.hediffSet.HasHediff(MSS_HauntedDefOf.MSS_Haunted_ShadowPersonInvisibility) ?? false)
    //         __result = true;
    // }
}
