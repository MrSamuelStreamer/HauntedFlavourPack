﻿using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace MSS_Haunted.Rituals;

public class RitualOutcomeEffectWorker_WitchTrial : RitualOutcomeEffectWorker_FromQuality
{
    public const int ConvictGuiltyForDays = 15;

    public override bool SupportsAttachableOutcomeEffect => false;

    public RitualOutcomeEffectWorker_WitchTrial() { }

    public RitualOutcomeEffectWorker_WitchTrial(RitualOutcomeEffectDef def)
        : base(def) { }

    public Pawn judge;
    public Pawn accused;

    public override void Apply(float progress, Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual)
    {
        float quality = GetQuality(jobRitual, progress);
        RitualOutcomePossibility outcome = GetOutcome(quality, jobRitual);

        judge = jobRitual.PawnWithRole("leader");
        accused = jobRitual.PawnWithRole("convict");

        if (outcome.positivityIndex == 2)
        {
            // swap for the flipped accusations
            judge = jobRitual.PawnWithRole("convict");
            accused = jobRitual.PawnWithRole("leader");
        }

        LookTargets letterLookTargets = accused;

        string extraLetterText = null;
        if (jobRitual.Ritual != null)
            ApplyAttachableOutcome(totalPresence, jobRitual, outcome, out extraLetterText, ref letterLookTargets);

        string label = accused.LabelShort + " " + outcome.label;
        TaggedString taggedString = outcome.description.Formatted(accused.Named("PAWN"), judge.Named("PROSECUTOR"));
        string str = def.OutcomeMoodBreakdown(outcome);
        if (!str.NullOrEmpty())
            taggedString += "\n\n" + str;
        TaggedString text = taggedString + ("\n\n" + OutcomeQualityBreakdownDesc(quality, progress, jobRitual));
        if (extraLetterText != null)
            text += "\n\n" + extraLetterText;

        if (outcome.Positive)
        {
            accused.guilt.Notify_Guilty(900000);
            accused.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.TrialConvicted);
            if (accused.health.hediffSet.TryGetHediff(MSS_HauntedDefOf.MSS_Haunted_PossessionHaunt, out Hediff hediff))
            {
                accused.health.RemoveHediff(hediff);
                text += "\n\n" + "MSS_Haunted_PossessionHaunt_Removed".Translate(accused.Named("PAWN"));
            }
            Find.LetterStack.ReceiveLetter(LetterMaker.MakeLetter((TaggedString)label, text, LetterDefOf.RitualOutcomePositive, letterLookTargets));
        }
        else
        {
            Find.LetterStack.ReceiveLetter((TaggedString)label, text, LetterDefOf.RitualOutcomeNegative, letterLookTargets);
            judge.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.TrialFailed);
            accused.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.TrialExonerated);

            if (Rand.Chance(0.5f))
            {
                Pawn p = jobRitual.assignments.Participants.Where(p => p != accused && p != judge).RandomElementWithFallback();
                if (p != null)
                {
                    if (!accused.health.hediffSet.TryGetHediff(MSS_HauntedDefOf.MSS_Haunted_PossessionHaunt, out Hediff hediff))
                    {
                        accused.health.AddHediff(MSS_HauntedDefOf.MSS_Haunted_PossessionHaunt);
                    }
                }
            }
        }
    }

    public override RitualOutcomePossibility GetOutcome(float quality, LordJob_Ritual ritual)
    {
        // Define a scaling factor for biasing, adjust as per requirements.
        const float biasFactor = 2f; // Higher value increases influence of quality

        // Step 1: Calculate biased weights
        List<float> weightedChances = def
            .outcomeChances.Select(e =>
            {
                // Adjust the weight of each element based on its positivity and quality
                float positivityBias = 1 + biasFactor * quality * e.positivityIndex / 2; // Bias factor smooths influence
                return e.chance * positivityBias;
            })
            .ToList();

        // Step 2: Normalize weights to ensure they sum to 1 (preserving probabilities)
        float totalWeight = weightedChances.Sum();
        List<float> normalizedWeights = weightedChances.Select(w => w / totalWeight).ToList();

        // Step 3: Randomly select an element based on weighted probabilities
        float randomValue = UnityEngine.Random.value; // Random value between 0 and 1
        float cumulative = 0;

        for (int i = 0; i < def.outcomeChances.Count; i++)
        {
            cumulative += normalizedWeights[i];
            if (randomValue <= cumulative)
                return def.outcomeChances[i];
        }

        // Default fallback (shouldn't happen if normalized weights are correct)
        return def.outcomeChances.Last();
    }

    public override string ExpectedQualityLabel()
    {
        return "ExpectedConvictionChance".Translate();
    }
}
