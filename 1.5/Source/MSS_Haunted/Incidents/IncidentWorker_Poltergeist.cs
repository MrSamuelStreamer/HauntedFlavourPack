using System.Collections.Generic;
using System.Linq;
using MSS_Haunted.Needs;
using RimWorld;
using VAEInsanity;
using Verse;

namespace MSS_Haunted.Incidents;

public class IncidentWorker_Poltergeist : IncidentWorker
{
    public bool IsSmall => def == MSS_HauntedDefOf.MSS_Haunted_PoltergeistSmall;

    protected override bool CanFireNowSub(IncidentParms parms)
    {
        if (!base.CanFireNowSub(parms) || parms.target is not Map map)
            return false;

        return map.mapPawns.FreeColonistsAndPrisonersSpawned.Any(p => p.health?.hediffSet?.HasHediff(MSS_HauntedDefOf.MSS_Haunted_PossessionHaunt) ?? false);
    }

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        Map target = (Map)parms.target;

        List<Pawn> pawns = target
            .mapPawns.FreeColonistsAndPrisonersSpawned.Where(p => p.health?.hediffSet?.HasHediff(MSS_HauntedDefOf.MSS_Haunted_PossessionHaunt) ?? false)
            .ToList();

        if (pawns.Count <= 0)
            return false;

        Pawn targetPawn = pawns.RandomElementWithFallback();

        if (targetPawn == null)
            return false;

        List<Pawn> potentialTargets = target.mapPawns.FreeColonistsAndPrisonersSpawned.Where(p => p.Position.InHorDistOf(targetPawn.Position, 15)).ToList();

        int radius = IsSmall ? 5 : 15;
        float hauntChance = IsSmall ? 0.35f : 0.65f;
        int minItems = IsSmall ? 5 : 20;

        float sanityOffset = IsSmall ? 0.2f : 0.4f;

        Pawn chosenTarget = potentialTargets.RandomElementWithFallback(targetPawn);

        target.GetComponent<HauntAnimationController>().StartHaunting(chosenTarget, radius, hauntChance, minItems);

        foreach (Pawn pawn in target.mapPawns.FreeColonistsSpawned)
        {
            Need need = pawn.needs.TryGetNeed<Need_Sanity>();
            if (need != null)
                need.CurLevel -= sanityOffset;
        }

        TaggedString label = def.letterLabel.Translate(chosenTarget.Named("PAWN")).CapitalizeFirst();
        TaggedString text = def.letterText.Translate(chosenTarget.Named("PAWN")).CapitalizeFirst();

        SendStandardLetter(label, text, def.letterDef, parms, (LookTargets)chosenTarget);

        return true;
    }
}
