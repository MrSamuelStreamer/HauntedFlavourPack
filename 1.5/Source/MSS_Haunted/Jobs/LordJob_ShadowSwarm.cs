using RimWorld;
using Verse;
using Verse.AI.Group;

namespace MSS_Haunted.Jobs;

public class LordJob_ShadowSwarm : LordJob_EntitySwarm
{
    public LordJob_ShadowSwarm() { }

    public LordJob_ShadowSwarm(IntVec3 startPos, IntVec3 destPos)
        : base(startPos, destPos) { }

    protected override LordToil CreateTravelingToil(IntVec3 start, IntVec3 dest)
    {
        return new LordToil_ShadowSwarm(start, dest);
    }
}
