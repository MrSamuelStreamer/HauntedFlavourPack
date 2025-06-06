﻿using MSS_Haunted.Needs;
using RimWorld;
using VAEInsanity;
using Verse;

namespace MSS_Haunted.Comps;

public class CompAbilityEffect_Accuse : CompAbilityEffect
{
    public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
    {
        base.Apply(target, dest);
        Pawn pawn = target.Pawn;
        if (pawn == null)
            return;

        pawn.story.traits.GainTrait(new Trait(MSS_HauntedDefOf.MSS_Haunted_Accused));

        pawn.style.FaceTattoo = MSS_HauntedDefOf.MSS_AccusedMark;
        pawn.style.Notify_StyleItemChanged();

        LetterMaker.MakeLetter(
            "MSS_Haunted_AccusedLetterLabel".Translate(pawn.Named("PAWN")),
            "MSS_Haunted_AccusedLetterText".Translate(pawn.Named("PAWN"), parent.pawn.Named("ACCUSER")),
            LetterDefOf.PositiveEvent,
            new LookTargets(pawn)
        );

        Need need = parent.pawn.needs.TryGetNeed<Need_Sanity>();
        if (need != null)
            need.CurLevel += 0.25f;

        need = pawn.needs.TryGetNeed<Need_Sanity>();
        if (need != null)
            need.CurLevel -= 0.25f;
    }

    public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
    {
        Pawn pawn = target.Pawn;
        if (pawn == null)
            return false;

        if (!base.CanApplyOn(target, dest))
            return false;

        return !pawn.story.traits.HasTrait(MSS_HauntedDefOf.MSS_Haunted_Accused);
    }
}
