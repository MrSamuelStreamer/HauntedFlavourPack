using System;
using System.Collections.Generic;
using MSS_Haunted.Needs;
using RimWorld;
using Verse;

namespace MSS_Haunted.Rituals;

public class RitualOutcomeComp_ParticipantParanoia : RitualOutcomeComp_Quality
{
    public override RitualOutcomeComp_Data MakeData()
    {
        return new RitualOutcomeComp_DataThingPresence();
    }

    public override float Count(LordJob_Ritual ritual, RitualOutcomeComp_Data data)
    {
        RitualOutcomeComp_DataThingPresence dataThingPresence = (RitualOutcomeComp_DataThingPresence)data;

        float totalParanoia = 0;
        int paranoiaHavers = 0;

        foreach (KeyValuePair<Thing, float> presentForTick in dataThingPresence.presentForTicks)
        {
            Pawn key = (Pawn)presentForTick.Key;
            Needs_Paranoia paranoia = key.needs.TryGetNeed<Needs_Paranoia>();

            if (paranoia == null)
                continue;

            paranoiaHavers++;
            totalParanoia += paranoia.CurLevel;
        }

        float avgParanoia = paranoiaHavers > 0 ? totalParanoia / paranoiaHavers : 0;

        return curve != null ? (int)Math.Min(avgParanoia, curve.Points[curve.PointsCount - 1].x) : avgParanoia;
    }
}
