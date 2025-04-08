using System;
using System.Collections.Generic;
using System.Linq;
using MSS_Haunted.Hediffs;
using MSS_Haunted.ModExtensions;
using RimWorld;
using Verse;

namespace MSS_Haunted.Storyteller;

public class StorytellerComp_Possession : StorytellerComp
{
    public StorytellerCompProperties_Possession Props => (StorytellerCompProperties_Possession)props;
    public static List<Pawn> tmpPawns = [];
    public static float tmpIntensity = 1;

    public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
    {
        Map map = (Map)target;
        tmpPawns.Clear();
        tmpIntensity = 1;

        tmpPawns = map.mapPawns.AllHumanlike.Where(p => p.health?.hediffSet?.HasHediff(MSS_HauntedDefOf.MSS_Haunted_PossessionHaunt) ?? false).ToList();

        if (tmpPawns.Any<Thing>())
        {
            int maxInfectionTicks = tmpPawns.Max(p =>
                p.health.hediffSet.GetFirstHediffOfDef(MSS_HauntedDefOf.MSS_Haunted_PossessionHaunt).TryGetComp<HediffComp_TimeTracker>()?.TicksSinceStart ?? 0
            );

            int maxInfectionDays = maxInfectionTicks / GenDate.TicksPerDay;

            tmpIntensity = Props.IntensityScaleByDays.Evaluate(maxInfectionDays);

            float mtb = Props.baseMtbDaysPerPossessed;

            if (mtb >= 0.0)
            {
                for (int i = 0; i < tmpPawns.Count; ++i)
                {
                    if (Rand.MTBEventOccurs(mtb, GenDate.TicksPerDay, 1000f))
                    {
                        IncidentParms parms = GenerateParms(MSS_HauntedDefOf.MSS_Haunted, target);
                        if (UsableIncidentsInCategory(MSS_HauntedDefOf.MSS_Haunted, parms).TryRandomElement(out IncidentDef result))
                            yield return new FiringIncident(result, this, parms);
                    }
                }
            }
        }
    }

    public override IncidentParms GenerateParms(IncidentCategoryDef incCat, IIncidentTarget target)
    {
        IncidentParms defs = StorytellerUtility.DefaultParmsNow(incCat, target);
        defs.points *= tmpIntensity;
        return defs;
    }

    protected override IEnumerable<IncidentDef> UsableIncidentsInCategory(IncidentCategoryDef cat, Func<IncidentDef, IncidentParms> parmsGetter)
    {
        IEnumerable<IncidentDef> extras = DefDatabase<IncidentDef>.AllDefsListForReading.Where(def => def.HasModExtension<AdditionalHauntedIncidentsModExtension>());
        return extras.Concat(base.UsableIncidentsInCategory(cat, parmsGetter));
    }
}
