using HarmonyLib;
using MSS_Haunted.Hediffs;
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

    [HarmonyPatch(typeof(Pawn), "DoKillSideEffects")]
    [HarmonyPostfix]
    public static void DoKillSideEffects_Postfix(Pawn __instance, DamageInfo? dinfo, bool spawned)
    {
        if (!spawned || !Rand.Chance(MSS_HauntedMod.settings.ghostificationChance))
            return;

        Hediff hediff = HediffMaker.MakeHediff(MSS_HauntedDefOf.MSS_Haunted_UnfinishedBusiness, __instance);
        if (dinfo?.Instigator is Pawn instigator && instigator.RaceProps.Humanlike)
        {
            HediffComp_Ghostification comp = hediff.TryGetComp<HediffComp_Ghostification>();
            if (comp != null)
                comp.killer = instigator;
        }

        __instance.health.AddHediff(hediff);
    }
}
