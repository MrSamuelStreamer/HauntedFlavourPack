using System.Reflection.Emit;
using HarmonyLib;
using MSS_Haunted.ModExtensions;
using MSS_Haunted.Utils;
using Verse;

namespace MSS_Haunted.HarmonyPatches;

[HarmonyPatch(typeof(DamageWorker_AddInjury))]
public static class DamageWorker_AddInjury_Patch
{
    [HarmonyPatch(nameof(DamageWorker_AddInjury.Apply))]
    [HarmonyPostfix]
    public static void Apply_Postfix(DamageInfo dinfo, Thing thing)
    {
        if (thing is not Pawn target)
            return;

        if (dinfo.Instigator is not Pawn pawn || !pawn.def.TryGetDefModExtension(out GiveHediffOnAttackDefModExtension extension))
            return;

        if (target.health.hediffSet.HasHediff(extension.hediff))
            return;

        if (!Rand.Chance(extension.chance))
            return;

        target.health.AddHediff(extension.hediff);
        if (!extension.message.NullOrEmpty())
        {
            Messages.Message(
                extension.message.Formatted(target.Named("TARGET"), pawn.Named("INSTIGATOR"), extension.hediff.LabelCap.Named("HEDIFF")),
                new LookTargets(target),
                extension.messageType
            );
        }
    }
}
