using HarmonyLib;
using Verse;

namespace MSS_Haunted.HarmonyPatches;

[HarmonyPatch(typeof(Verb))]
public static class Verb_Patch
{
    [HarmonyPatch(nameof(Verb.CanHitTargetFrom))]
    [HarmonyPostfix]
    public static void CanHitTargetFrom_Postfix(LocalTargetInfo targ, ref bool __result)
    {
        if (targ.Pawn != null && (targ.Pawn.health?.hediffSet?.HasHediff(MSS_HauntedDefOf.MSS_Haunted_ShadowPersonInvisibility) ?? false))
            __result = false;
    }
}
