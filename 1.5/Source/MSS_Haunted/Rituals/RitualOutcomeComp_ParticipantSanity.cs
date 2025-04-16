using System;
using System.Collections.Generic;
using MSS_Haunted.Needs;
using RimWorld;
using VAEInsanity;
using Verse;

namespace MSS_Haunted.Rituals;

public class RitualOutcomeComp_ParticipantSanity : RitualOutcomeComp_Quality
{
    public override RitualOutcomeComp_Data MakeData()
    {
        return new RitualOutcomeComp_DataThingPresence();
    }

    public override float Count(LordJob_Ritual ritual, RitualOutcomeComp_Data data)
    {
        RitualOutcomeComp_DataThingPresence dataThingPresence = (RitualOutcomeComp_DataThingPresence)data;

        float totalSanity = 0;
        int sanityHavers = 0;

        foreach (KeyValuePair<Thing, float> presentForTick in dataThingPresence.presentForTicks)
        {
            Pawn key = (Pawn)presentForTick.Key;
            Need_Sanity sanity = key.needs.TryGetNeed<Need_Sanity>();

            if (sanity == null)
                continue;

            sanityHavers++;
            totalSanity += sanity.CurLevel;
        }

        float avgSanity = sanityHavers > 0 ? totalSanity / sanityHavers : 0;

        return curve != null ? (int)Math.Min(avgSanity, curve.Points[curve.PointsCount - 1].x) : avgSanity;
    }
}
