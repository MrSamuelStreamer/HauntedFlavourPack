using HarmonyLib;
using RimWorld;
using Verse;

namespace MSS_Haunted.HarmonyPatches;

[HarmonyPatch(typeof(MemoryThoughtHandler))]
public static class MemoryThoughtHandler_Patch
{
    [HarmonyPatch(nameof(MemoryThoughtHandler.TryGainMemory), typeof(ThoughtDef), typeof(Pawn), typeof(Precept))]
    [HarmonyPostfix]
    public static void TryGainMemoryPostfix(MemoryThoughtHandler __instance, ThoughtDef def)
    {
        if (def == ThoughtDefOf.AteWithoutTable)
            InfectionPathwayUtility.AddInfectionPathway(MSS_HauntedDefOf.MSS_Haunted_AteWithoutTable, __instance.pawn);
    }
}
