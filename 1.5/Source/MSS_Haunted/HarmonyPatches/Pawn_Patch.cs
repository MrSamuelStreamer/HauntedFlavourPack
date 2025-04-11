using HarmonyLib;
using Verse;

namespace MSS_Haunted.HarmonyPatches;

[HarmonyPatch(typeof(Pawn))]
public static class Pawn_Patch
{
    [HarmonyPatch(nameof(Pawn.SpawnSetup))]
    [HarmonyPostfix]
    public static void SpawnSetup_Postfix(Pawn __instance)
    {
        if (__instance.RaceProps.Humanlike)
            __instance.abilities.GainAbility(MSS_HauntedDefOf.MSS_Haunted_Accuse);
    }
}
