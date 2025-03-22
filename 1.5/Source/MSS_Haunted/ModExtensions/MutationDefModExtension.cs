using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MSS_Haunted.ModExtensions;

public class MutationDefModExtension : DefModExtension
{
    public float? mutationalStabilityOffset;

    public float GetInitialOffset(Hediff parent)
    {
        if (mutationalStabilityOffset.HasValue)
            return mutationalStabilityOffset.Value;

        return !Mathf.Approximately(parent.CurStage?.painOffset ?? 0, 0) ? parent.CurStage!.painOffset : 0.2f;
    }
}
