using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Verse;

namespace MSS_Haunted.GameComponents;

public interface Triggerable
{
    /**
     * Triggers the object. Returns the next trigger tick or -1 to remove.
     */
    int Trigger();
}

public class GameComponent_TriggerableTracker : GameComponent
{
    public ConcurrentDictionary<Triggerable, int> triggerAtTicks = new();

    public GameComponent_TriggerableTracker(Game game) { }

    public override void GameComponentTick()
    {
        base.GameComponentTick();
        int ticksGame = Find.TickManager.TicksGame;
        if (triggerAtTicks.Count == 0 || ticksGame % 60 != 0)
            return;

        List<KeyValuePair<Triggerable, int>> ordered = triggerAtTicks.Where(p => p.Value < ticksGame).OrderBy(p => p.Value).ToList();
        if (ordered.Count == 0)
            return;
        (Triggerable triggerable, int triggerTick) = ordered.First();
        int nextTick = triggerable.Trigger();
        if (nextTick == -1)
        {
            triggerAtTicks.TryRemove(triggerable, out _);
        }
        else
        {
            triggerAtTicks.TryUpdate(triggerable, nextTick, triggerTick);
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();

        if (Scribe.mode == LoadSaveMode.LoadingVars)
        {
            // Clear the dictionary before the PostLoadInit phase
            triggerAtTicks.Clear();
        }
    }
}
