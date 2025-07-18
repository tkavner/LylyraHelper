﻿using Monocle;
using MonoMod.Utils;
using System;

namespace Celeste.Mod.LylyraHelper.Components.Sliceables;

public class ModItemSliceableComponent : SliceableComponent
{
    public Func<Entity, DynamicData, Entity[]> firstFrameSlicing;
    public Action<Entity, Entity, DynamicData> postSlice;
    public Action<Entity, DynamicData> secondFrameSlicing;
    public Action<Entity, DynamicData> onSliceStart;

    public ModItemSliceableComponent(bool active, bool visible) : base(active, visible)
    {

    }

    public ModItemSliceableComponent(Slicer.CustomSlicingActionHolder action) : this(true, true)
    {
        this.firstFrameSlicing = action.firstFrameSlice;
        this.secondFrameSlicing = action.secondFrameSlice;
        this.onSliceStart = action.onSliceStart;
        this.postSlice = action.postSlice;

    }

    public override SlicerCollisionResults Slice(Slicer slicer) {
        Entity[] children = firstFrameSlicing?.Invoke(Entity, new DynamicData(slicer));
        return children == null ? null : new SlicerCollisionResults(children, Entity);
    }

    public override void Activate(Slicer slicer, Slicer.NewlySlicedEntityWrapper secondFrameEntityCombo)
    {
        secondFrameSlicing?.Invoke(Entity, new DynamicData(slicer));
    }

    public override void OnSliceStart(Slicer slicer)
    {
        onSliceStart?.Invoke(Entity, new DynamicData(slicer));
    }
}