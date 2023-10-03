using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.LylyraHelper.Components
{
    //component for static mover entities specified by SlicerController
    public class AttachedSliceableComponent : Component
    {
        public AttachedSliceableComponent(bool active, bool visible) : base(active, visible)
        {

        }
    }
}
