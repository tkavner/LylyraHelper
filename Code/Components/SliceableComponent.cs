using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.LylyraHelper.Components
{
    [Tracked(true)]
    public abstract class SliceableComponent : Component
    {


        public SliceableComponent(bool active, bool visible) : base(active, visible)
        {

        }

        public abstract void OnSliceStart(Slicer slicer);

        public abstract Entity[] Slice(Slicer slicer);

        public abstract void Activate(Slicer slicer);


        internal void AddParticles(Vector2 position, Vector2 range, Color color, float percent = 0.03F)
        {
            int numParticles = (int)(range.X * range.Y) / 10; //proportional to the area to cover
            this.Entity.SceneAs<Level>().ParticlesFG.Emit(Cuttable.paperScraps, (int)(numParticles * percent), position + new Vector2(range.X / 2, range.Y / 2), new Vector2(range.X / 2, range.Y / 2), color);
        }

    }
}
