using Celeste.Mod.LylyraHelper.Components;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.LylyraHelper.Components.Sliceables
{
    public class ModItemSliceableComponent : SliceableComponent
    {
        public Func<Entity, DynamicData, Entity[]> firstFrameSlicing;
        public Action<Entity, DynamicData> secondFrameSlicing;
        public Action<Entity, DynamicData> onSliceStart;

        public ModItemSliceableComponent(bool active, bool visible) : base(active, visible)
        {

        }

        public ModItemSliceableComponent(Slicer.CustomSlicingActionHolder action) : this(true, true)
        {
            this.firstFrameSlicing = action.firstFrameSlice;
            this.secondFrameSlicing= action.secondFrameSlice;
            this.onSliceStart = action.onSliceStart;
        }

        public override Entity[] Slice(Slicer slicer) => firstFrameSlicing(Entity, new DynamicData(slicer));

        public override void Activate(Slicer slicer) => secondFrameSlicing(Entity, new DynamicData(slicer));

        public override void OnSliceStart(Slicer slicer) => onSliceStart(Entity, new DynamicData(slicer));
    }
}
