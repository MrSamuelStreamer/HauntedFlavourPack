using System.Collections.Generic;
using HarmonyLib;
using Verse;

namespace MSS_Haunted.HarmonyPatches;

[HarmonyPatch(typeof(Pawn))]
public static class Pawn_Patches
{
    [HarmonyPatch(nameof(Pawn.GetGizmos))]
    [HarmonyPostfix]
    public static void Pawn_GetGizmos_Postfix(Pawn __instance, ref IEnumerable<Gizmo> __result)
    {
        List<Gizmo> gizmos = new(__result);
        if (__instance.RaceProps.Humanlike)
        {
            gizmos.Add(
                new Command_Action
                {
                    defaultLabel = "MSSFP_TESTHAUNT".Translate(),
                    defaultDesc = "MSSFP_TESTHAUNTDESC".Translate(),
                    // icon = ContentFinder<Texture2D>.Get("UI/MSS_FP_Haunts_Toggle"),
                    action = delegate
                    {
                        __instance.Map.GetComponent<HauntAnimationController>().StartHaunting(__instance);
                    },
                }
            );
        }

        __result = gizmos;
    }
}
