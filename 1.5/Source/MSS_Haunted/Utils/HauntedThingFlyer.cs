using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MSS_Haunted.Utils;

public class HauntedThingFlyer : ThingWithComps, IThingHolder
{
    private ThingOwner<Thing> innerContainer;
    private Vector3 effectivePos;
    private Vector3 startPos = Vector3.zero;

    public Vector3 StartPosition => startPos;
    public IntVec3 IntStartPosition;

    public Vector3 LiftedPosition = Vector3.zero;

    private string shadowTexPath = "Things/Skyfaller/SkyfallerShadowCircle";

    private Material cachedShadowMaterial;
    public Material ShadowMaterial
    {
        get
        {
            if (cachedShadowMaterial == null && !shadowTexPath.NullOrEmpty())
                cachedShadowMaterial = MaterialPool.MatFrom(shadowTexPath, ShaderDatabase.Transparent);
            return cachedShadowMaterial;
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref IntStartPosition, "IntStartPosition");
        Scribe_Values.Look(ref effectivePos, "effectivePos");
        Scribe_Values.Look(ref startPos, "startPos");
        Scribe_Values.Look(ref LiftedPosition, "LiftedPosition");
        Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
    }

    public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
    {
        foreach (Thing thing in innerContainer.InnerListForReading.ToList())
        {
            innerContainer.TryDrop(thing, IntStartPosition, MapHeld, ThingPlaceMode.Direct, out Thing _, playDropSound: false);
        }
        base.Destroy(mode);
    }

    public bool AddThing(Thing thing)
    {
        if (CarriedThing != null)
            return false;

        if (CarriedThing?.def.graphic is Graphic_Cluster)
            return false;

        IntStartPosition = thing.Position;
        thing.DeSpawn();
        if (!innerContainer.TryAdd(thing, true))
        {
            GenSpawn.Spawn(thing, IntStartPosition, Map);
            return false;
        }

        effectivePos = thing.DrawPos;
        startPos = thing.DrawPos;

        return true;
    }

    public void SetPositionDirectly(Vector3 pos)
    {
        effectivePos = pos;
    }

    protected Thing CarriedThing
    {
        get { return innerContainer.InnerListForReading.Count <= 0 ? null : innerContainer.InnerListForReading[0]; }
    }

    public override Vector3 DrawPos
    {
        get { return effectivePos; }
    }

    public ThingOwner GetDirectlyHeldThings() => innerContainer;

    public HauntedThingFlyer() => innerContainer = new ThingOwner<Thing>(this);

    public void GetChildHolders(List<IThingHolder> outChildren)
    {
        ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
    }

    public override void Tick()
    {
        innerContainer.ThingOwnerTick();
    }

    public override void DynamicDrawPhaseAt(DrawPhase phase, Vector3 drawLoc, bool flip = false)
    {
        try
        {
            if (CarriedThing != null)
                CarriedThing.DynamicDrawPhaseAt(phase, effectivePos, false);
            else
                CarriedThing?.DynamicDrawPhaseAt(phase, effectivePos);
            base.DynamicDrawPhaseAt(phase, drawLoc, flip);
        }
        catch (Exception e)
        {
            ModLog.Error($"Error drawing {CarriedThing}, removing it.", e);
            Destroy();
        }
    }

    protected override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
        DrawShadow(effectivePos);
        CarriedThing?.DrawNowAt(effectivePos, false);
    }

    private void DrawShadow(Vector3 drawLoc)
    {
        Material shadowMaterial = CarriedThing.def.pawnFlyer?.ShadowMaterial ?? ShadowMaterial;
        if (ShadowMaterial == null)
            return;

        float shadowHeight = 1;

        if (LiftedPosition != Vector3.zero)
        {
            shadowHeight = LiftedPosition.z - drawLoc.z;
        }
        else
        {
            shadowHeight = drawLoc.z - startPos.z;
        }

        Vector3 shadowOffset = new Vector3(0, 0, shadowHeight);

        float num = Mathf.Lerp(1f, 0.6f, shadowHeight);
        Vector3 s = new(num, 1f, num);
        Matrix4x4 matrix = new();
        matrix.SetTRS(drawLoc - shadowOffset, Quaternion.identity, s);
        Graphics.DrawMesh(MeshPool.plane10, matrix, shadowMaterial, 0);
    }
}
