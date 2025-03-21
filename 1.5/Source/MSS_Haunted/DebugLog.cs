﻿using System;
using System.Diagnostics;

namespace MSS_Haunted;

static class ModLog
{
    [Conditional("DEBUG")]
    public static void Debug(string x)
    {
        Verse.Log.Message(x);
    }

    public static void Log(string msg)
    {
        Verse.Log.Message($"<color=#1c6beb>[MSS_Haunted]</color> {msg ?? "<null>"}");
    }

    public static void Warn(string msg)
    {
        Verse.Log.Warning($"<color=#1c6beb>[MSS_Haunted]</color> {msg ?? "<null>"}");
    }

    public static void Error(string msg, Exception e = null)
    {
        Verse.Log.Error($"<color=#1c6beb>[MSS_Haunted]</color> {msg ?? "<null>"}");
        if (e != null)
            Verse.Log.Error(e.ToString());
    }
}
