using System;
using System.Collections.Generic;
using System.Linq;
using MSS_Haunted.GameComponents;
using RimWorld;
using Verse;

namespace MSS_Haunted.Hediffs;

public class HediffComp_Ghostification : HediffComp, Triggerable
{
    public int ghostificationTick = -1;
    public Pawn killer = null;

    public HediffCompProperties_Ghostification Props => (HediffCompProperties_Ghostification)props;

    public override void CompPostMake()
    {
        base.CompPostMake();
        ghostificationTick = Find.TickManager.TicksGame + Props.ghostificationTime.RandomInRange;
        Current.Game.GetComponent<GameComponent_TriggerableTracker>().triggerAtTicks.TryAdd(this, ghostificationTick);
    }

    public override void CompPostPostRemoved()
    {
        Current.Game.GetComponent<GameComponent_TriggerableTracker>().triggerAtTicks.TryRemove(this, out _);
        base.CompPostPostRemoved();
    }

    public int Trigger()
    {
        if (ghostificationTick < 0 || Find.TickManager.TicksGame < ghostificationTick || parent.pawn.MapHeld != Find.CurrentMap)
            return ghostificationTick;
        if (Find.TickManager.TicksGame > ghostificationTick + (Props.ghostificationTime.max * 100))
        {
            parent.pawn.health.RemoveHediff(parent);
            return -1;
        }
        Pawn origPawn = parent.pawn;
        if (origPawn is not { Dead: true })
            return -1;

        Pawn pawn = Duplicate();
        if (pawn == null)
            return ghostificationTick;
        Hediff dupeHediff = pawn.health?.hediffSet?.GetFirstHediffOfDef(parent.def);
        if (dupeHediff != null)
        {
            pawn.health.RemoveHediff(dupeHediff);
        }

        GenRadial.RadialCellsAround(origPawn.PositionHeld, 2, true).Where(c => c.Standable(origPawn.MapHeld)).TryRandomElement(out IntVec3 cell);
        if (cell == default)
            return ghostificationTick;
        pawn.health?.SetDead();
        Corpse newThing = (Corpse)ThingMaker.MakeThing(pawn.RaceProps.corpseDef);
        newThing.InnerPawn = pawn;
        GenSpawn.Spawn(newThing, cell, origPawn.MapHeld);
        ResurrectionUtility.TryResurrect(
            pawn,
            new ResurrectionParams
            {
                gettingScarsChance = 0.1f,
                canKidnap = false,
                canTimeoutOrFlee = false,
                useAvoidGridSmart = true,
                canSteal = false,
                invisibleStun = true,
                removeDiedThoughts = false,
            }
        );
        origPawn.health.RemoveHediff(parent);
        return -1;
    }

    public override string CompTipStringExtra => DebugSettings.godMode ? $"Ghostification in: {(ghostificationTick - Find.TickManager.TicksGame).ToStringTicksToPeriod()}" : "";
    public override bool CompShouldRemove => Find.TickManager.TicksGame > ghostificationTick + 10000 || !(parent.pawn?.Dead ?? false);

    public override void CompExposeData()
    {
        Scribe_Values.Look(ref ghostificationTick, "ghostificationTick", -1);
        Scribe_References.Look(ref killer, "ghostificationKiller");
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            Current.Game.GetComponent<GameComponent_TriggerableTracker>().triggerAtTicks.TryAdd(this, ghostificationTick);
        }

        base.CompExposeData();
    }

    public Pawn PostGenPawnSetup(Pawn pawn, Pawn newPawn)
    {
        CopyStoryAndTraits(pawn, newPawn);
        CopyGenes(pawn, newPawn);
        CopyApperance(pawn, newPawn);
        CopyStyle(pawn, newPawn);
        CopySkills(pawn, newPawn);
        CopyHediffs(pawn, newPawn);
        CopyNeeds(pawn, newPawn);
        if (pawn.mutant != null)
            MutantUtility.SetPawnAsMutantInstantly(newPawn, pawn.mutant.Def, RotStage.Fresh);
        CopyAbilities(pawn, newPawn);
        if (pawn.guest != null)
            newPawn.guest.Recruitable = pawn.guest.Recruitable;
        newPawn.Drawer.renderer.SetAllGraphicsDirty();
        newPawn.Notify_DisabledWorkTypesChanged();
        return newPawn;
    }

    /**
     * Almost a copy of the original GameComponent_PawnDuplicator.Duplicate method, but that dies due to weird nulls
     * Seems to be an issue with the duplicate tracker for dead pawns
     */
    public Pawn Duplicate()
    {
        Pawn pawn = parent.pawn;
        float biologicalYearsFloat = pawn.ageTracker.AgeBiologicalYearsFloat;
        float chronoAge = pawn.ageTracker.AgeChronologicalYearsFloat;
        if (chronoAge > (double)biologicalYearsFloat)
            chronoAge = biologicalYearsFloat;
        float? fixedBiologicalAge = biologicalYearsFloat;
        float? fixedChronologicalAge = chronoAge;
        XenotypeDef xenotype = pawn.genes?.Xenotype;
        CustomXenotype customXenotype = pawn.genes?.CustomXenotype;
        Pawn newPawn = PawnGenerator.GeneratePawn(
            new PawnGenerationRequest(
                pawn.kindDef,
                pawn.Faction,
                forceGenerateNewPawn: true,
                canGeneratePawnRelations: false,
                colonistRelationChanceFactor: 0.0f,
                relationWithExtraPawnChanceFactor: 0.0f,
                fixedBiologicalAge: fixedBiologicalAge,
                fixedChronologicalAge: fixedChronologicalAge,
                fixedGender: pawn.gender,
                fixedIdeo: pawn.Ideo,
                forbidAnyTitle: true,
                forcedXenotype: xenotype,
                forcedCustomXenotype: customXenotype,
                forceNoGear: true
            )
            {
                IsCreepJoiner = pawn.IsCreepJoiner,
                ForceNoIdeoGear = true,
                CanGeneratePawnRelations = false,
                DontGivePreArrivalPathway = true,
            }
        );
        newPawn.Name = NameTriple.FromString(pawn.Name.ToString());
        if (ModsConfig.BiotechActive)
        {
            if (newPawn.ageTracker != null)
            {
                newPawn.ageTracker.growthPoints = pawn.ageTracker.growthPoints;
                newPawn.ageTracker.vatGrowTicks = pawn.ageTracker.vatGrowTicks;
            }

            if (newPawn.genes != null)
            {
                newPawn.genes.xenotypeName = pawn.genes?.xenotypeName;
                newPawn.genes.iconDef = pawn.genes?.iconDef;
            }
        }
        return PostGenPawnSetup(pawn, newPawn);
    }

    private void CopyAbilities(Pawn pawn, Pawn newPawn)
    {
        foreach (Ability ability in pawn.abilities?.abilities ?? [])
        {
            if (newPawn.abilities.GetAbility(ability.def) == null)
                newPawn.abilities.GainAbility(ability.def);
        }

        List<Ability> abilities = newPawn.abilities?.abilities ?? [];
        for (int index = abilities.Count - 1; index >= 0; --index)
        {
            Ability ability = abilities[index];
            if (pawn.abilities?.GetAbility(ability.def) == null)
                newPawn.abilities?.RemoveAbility(ability.def);
        }

        if (pawn.royalty == null)
            return;
        foreach (RoyalTitle royalTitle in pawn.royalty.AllTitlesForReading ?? [])
        {
            foreach (AbilityDef grantedAbility in royalTitle.def.grantedAbilities ?? [])
            {
                if (newPawn.abilities?.GetAbility(grantedAbility) != null)
                    newPawn.abilities?.RemoveAbility(grantedAbility);
            }
        }
    }

    private void CopyStoryAndTraits(Pawn pawn, Pawn newPawn)
    {
        if (pawn.story == null)
            return;
        newPawn.story.favoriteColor = pawn.story.favoriteColor;
        newPawn.story.Childhood = pawn.story.Childhood;
        newPawn.story.Adulthood = pawn.story.Adulthood;
        if (newPawn.story.traits == null)
            return;
        newPawn.story.traits.allTraits.Clear();
        if (!Props.possibleTraits.NullOrEmpty() && pawn.story?.traits != null)
        {
            Props.possibleTraits.Where(t => !pawn.story.traits.HasTrait(t)).TryRandomElement(out TraitDef trait);
            if (trait != null)
                newPawn.story.traits.GainTrait(new Trait(trait, forced: true));
        }
        foreach (Trait allTrait in pawn.story?.traits?.allTraits ?? [])
        {
            if (!ModsConfig.BiotechActive || allTrait.sourceGene == null)
                newPawn.story.traits.GainTrait(new Trait(allTrait.def, allTrait.Degree, allTrait.ScenForced));
        }
    }

    private void CopyGenes(Pawn pawn, Pawn newPawn)
    {
        if (!ModsConfig.BiotechActive || pawn.genes == null)
            return;
        newPawn.genes.Endogenes.Clear();
        List<Gene> sourceEndogenes = pawn.genes.Endogenes;
        foreach (Gene gene in sourceEndogenes)
            newPawn.genes.AddGene(gene.def, false);
        for (int i = 0; i < sourceEndogenes.Count; i++)
            newPawn.genes.Endogenes[i].overriddenByGene = !sourceEndogenes[i].Overridden
                ? null
                : newPawn.genes.GenesListForReading.First(e => e.def == sourceEndogenes[i].overriddenByGene.def);

        if (Props.xenoTypes.NullOrEmpty())
            return;

        Props.xenoTypes.Except(pawn.genes.Xenotype).TryRandomElement(out XenotypeDef newXenotype);
        if (newXenotype == null)
            return;

        newPawn.genes.SetXenotype(newXenotype);
    }

    private void CopyApperance(Pawn pawn, Pawn newPawn)
    {
        if (pawn.story == null)
            return;
        newPawn.story.headType = pawn.story.headType;
        newPawn.story.bodyType = pawn.story.bodyType;
        newPawn.story.hairDef = pawn.story.hairDef;
        newPawn.story.HairColor = pawn.story.HairColor;
        newPawn.story.SkinColorBase = pawn.story.SkinColorBase;
        newPawn.story.skinColorOverride = pawn.story.skinColorOverride;
        newPawn.story.furDef = pawn.story.furDef;
    }

    private void CopyStyle(Pawn pawn, Pawn newPawn)
    {
        if (pawn.style == null)
            return;
        newPawn.style.beardDef = pawn.style.beardDef;
        if (!ModsConfig.IdeologyActive)
            return;
        newPawn.style.BodyTattoo = pawn.style.BodyTattoo;
        newPawn.style.FaceTattoo = pawn.style.FaceTattoo;
    }

    private void CopySkills(Pawn pawn, Pawn newPawn)
    {
        if (pawn.skills == null)
            return;
        newPawn.skills.skills.Clear();
        foreach (SkillRecord skill in pawn.skills.skills)
        {
            SkillRecord skillRecord = new SkillRecord(newPawn, skill.def)
            {
                levelInt = skill.levelInt,
                passion = skill.passion,
                xpSinceLastLevel = skill.xpSinceLastLevel,
                xpSinceMidnight = skill.xpSinceMidnight,
            };
            newPawn.skills.skills.Add(skillRecord);
        }
    }

    private void CopyHediffs(Pawn pawn, Pawn newPawn)
    {
        if (pawn.health?.hediffSet == null)
            return;
        newPawn.health.hediffSet.hediffs.Clear();
        List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
        foreach (Hediff other in hediffs)
        {
            if (
                !other.def.duplicationAllowed
                || (other.Part != null && !newPawn.health.hediffSet.HasBodyPart(other.Part))
                || (other is Hediff_AddedPart && !other.def.organicAddedBodypart)
                || (other is Hediff_Implant && !other.def.organicAddedBodypart)
                || other.def == MSS_HauntedDefOf.MSS_Haunted_UnfinishedBusiness
            )
                continue;

            Hediff hediff = HediffMaker.MakeHediff(other.def, newPawn, other.Part);
            hediff.CopyFrom(other);
            newPawn.health.hediffSet.AddDirect(hediff);
        }

        foreach (Hediff hediff in hediffs)
        {
            if (hediff is Hediff_AddedPart && !hediff.def.organicAddedBodypart)
                newPawn.health.RestorePart(hediff.Part, checkStateChange: false);
        }
    }

    private void CopyNeeds(Pawn pawn, Pawn newPawn)
    {
        List<Thought_Memory> memories = newPawn.needs?.mood?.thoughts?.memories?.Memories;
        if (memories == null)
            return;
        memories.Clear();
        if (killer != null)
        {
            newPawn.needs?.mood?.thoughts?.memories?.TryGainMemory(MSS_HauntedDefOf.MSS_Haunted_Killed, killer);
            newPawn.relations?.AddDirectRelation(MSS_HauntedDefOf.MSS_Haunted_Killer, killer);
        }
        pawn.relations?.DirectRelations?.ForEach(r => newPawn.relations.AddDirectRelation(r.def, r.otherPawn));

        if (pawn.needs == null || newPawn.needs == null)
        {
            return;
        }
        newPawn.needs.AllNeeds.Clear();
        foreach (Need allNeed in pawn.needs.AllNeeds)
        {
            Need instance = (Need)Activator.CreateInstance(allNeed.def.needClass, newPawn);
            instance.def = allNeed.def;
            newPawn.needs.AllNeeds.Add(instance);
            instance.SetInitialLevel();
            instance.CurLevel = allNeed.CurLevel;
            newPawn.needs.BindDirectNeedFields();
        }

        if (pawn.needs.mood?.thoughts?.memories?.Memories == null)
        {
            return;
        }

        foreach (Thought_Memory memory in pawn.needs.mood.thoughts.memories.Memories)
        {
            Thought_Memory thoughtMemory = (Thought_Memory)ThoughtMaker.MakeThought(memory.def);
            thoughtMemory.CopyFrom(memory);
            thoughtMemory.pawn = newPawn;
            memories.Add(thoughtMemory);
        }
    }
}
