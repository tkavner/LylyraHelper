using Celeste.Mod.LylyraHelper.Components;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.LylyraHelper.Code.Components.Sliceables
{
    public class BoosterSliceableComponent : SliceableComponent
    {
        public BoosterSliceableComponent(bool active, bool visible) : base(active, visible)
        {
        }

        public override void Activate(Slicer slicer)
        {
            
        }

        public override void OnSliceStart(Slicer slicer)
        {
            Booster booster = Entity as Booster;
            bool respawning = booster.respawnTimer > 0;
            if (!respawning) booster.PlayerReleased();
        }

        public override Entity[] Slice(Slicer slicer)
        {
            return null;
        }
    }
}
