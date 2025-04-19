using UnityEngine;
using Verse;

namespace MSS_Haunted;

public class Settings : ModSettings
{
    //Use Mod.settings.setting to refer to this setting.
    public float ghostificationChance = 0.01f;

    public void DoWindowContents(Rect wrect)
    {
        Listing_Standard options = new();
        options.Begin(wrect);

        ghostificationChance = options.SliderLabeled(
            "MSS_Haunted_GhostificationChance".Translate(ghostificationChance.ToStringPercent()),
            ghostificationChance,
            0f,
            1f,
            0.3f,
            "MSS_Haunted_GhostificationChanceDesc".Translate()
        );
        options.Gap();

        options.End();
    }

    public override void ExposeData()
    {
        Scribe_Values.Look(ref ghostificationChance, "ghostificationChance", 0.01f);
        base.ExposeData();
    }
}
