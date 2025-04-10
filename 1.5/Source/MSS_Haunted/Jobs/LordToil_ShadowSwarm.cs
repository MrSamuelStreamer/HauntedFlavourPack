using RimWorld;
using Verse;
using Verse.AI;

namespace MSS_Haunted.Jobs;

public class LordToil_ShadowSwarm(IntVec3 start, IntVec3 dest) : LordToil_EntitySwarm(start, dest)
{
    protected override DutyDef GetDutyDef() => MSS_HauntedDefOf.MSS_Haunted_ShadowPersonSwarm;

    public override bool CanAddPawn(Pawn p) => p.def == MSS_HauntedDefOf.MSS_Haunted_ShadowPerson;
}
