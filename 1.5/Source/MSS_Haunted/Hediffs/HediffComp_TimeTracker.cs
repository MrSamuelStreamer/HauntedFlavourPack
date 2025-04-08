using Verse;

namespace MSS_Haunted.Hediffs;

public class HediffComp_TimeTracker : HediffComp
{
    public HediffCompProperties_TimeTracker Props => (HediffCompProperties_TimeTracker)props;

    protected int startTick;

    public int TicksSinceStart => Find.TickManager.TicksGame - startTick;
    public int StartTick => startTick;

    public override void CompExposeData()
    {
        base.CompExposeData();
        Scribe_Values.Look(ref startTick, "startTick", Find.TickManager.TicksGame);
    }

    public override void CompPostMake()
    {
        base.CompPostMake();
        startTick = Find.TickManager.TicksGame;
    }

    public override void CopyFrom(HediffComp other)
    {
        base.CopyFrom(other);
        if (other is HediffComp_TimeTracker otherTracker)
        {
            startTick = otherTracker.startTick;
        }
    }
}
