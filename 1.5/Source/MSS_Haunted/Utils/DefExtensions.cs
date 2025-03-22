using Verse;

namespace MSS_Haunted.Utils;

public static class DefExtensions
{
    public static bool TryGetDefModExtension<T>(this Def def, out T extension)
        where T : DefModExtension
    {
        extension = def.GetModExtension<T>();
        return extension != null;
    }
}
