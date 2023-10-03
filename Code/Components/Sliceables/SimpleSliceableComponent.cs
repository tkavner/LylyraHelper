using Celeste.Mod.LylyraHelper.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.LylyraHelper.Code.Components.Sliceables
{
    public class SimpleSliceableComponent : SliceableComponent
    {
        public SimpleSliceableComponent(bool active, bool visible) : base(active, visible)
        {

        }

        public override bool Slice(Slicer slicer)
        {
            Entity.RemoveSelf();
            return true;
        }
    }
}
