using RimWorld;

namespace MSS_Haunted.Comps;

public class CompProperties_AbilityAccuse : CompProperties_AbilityStartRitualOnPawn
{
    public CompProperties_AbilityAccuse() => compClass = typeof(CompAbilityEffect_Accuse);
}
