using HarmonyLib;
using UnityEngine;
using Verse;

namespace MSS_Haunted;

public class MSS_HauntedMod : Mod
{
    public static Settings settings;

    public MSS_HauntedMod(ModContentPack content)
        : base(content)
    {
        Log.Message("Hello world from MSS_Haunted");

        // initialize settings
        settings = GetSettings<Settings>();
#if DEBUG
        Harmony.DEBUG = true;
#endif
        Harmony harmony = new Harmony("MrSamuelStreamer.rimworld.MSS_Haunted.main");
        harmony.PatchAll();
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        base.DoSettingsWindowContents(inRect);
        settings.DoWindowContents(inRect);
    }

    public override string SettingsCategory()
    {
        return "MSS_Haunted_SettingsCategory".Translate();
    }
}
