using HarmonyLib;
using Verse;

namespace MSS_Haunted.HarmonyPatches;

[HarmonyPatch(typeof(Hediff_MetalhorrorImplant))]
public static class Hediff_MetalhorrorImplant_Patch
{
    [HarmonyPatch(nameof(Hediff_MetalhorrorImplant.Emerge))]
    [HarmonyPostfix]
    public static void Emerge_Patch(Hediff_MetalhorrorImplant __instance)
    {
        if (__instance.pawn.Dead)
            return;

        __instance.pawn.health.AddHediff(MSS_HauntedDefOf.MSS_MHAgony);
    }
}
