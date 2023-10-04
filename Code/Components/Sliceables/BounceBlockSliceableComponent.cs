using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Celeste.GaussianBlur;

namespace Celeste.Mod.LylyraHelper.Components.Sliceables
{
    public class BounceBlockSliceableComponent : SliceableComponent
    {
        public BounceBlockSliceableComponent(bool active, bool visible) : base(active, visible)
        {
        }

        public override void Activate(Slicer slicer)
        {

        }

        public override void OnSliceStart(Slicer slicer)
        {

        }

        public override Entity[] Slice(Slicer slicer)
        {
            BounceBlock original = Entity as BounceBlock;
            Vector2[] resultArray = Slicer.CalcCuts(original.Position, new Vector2(original.Width, original.Height), slicer.Entity.Center, slicer.Direction, slicer.CutSize);

            Vector2 b1Pos = resultArray[0];
            Vector2 b2Pos = resultArray[1];
            int b1Width = (int)resultArray[2].X;
            int b1Height = (int)resultArray[2].Y;

            int b2Width = (int)resultArray[3].X;
            int b2Height = (int)resultArray[3].Y;

            BounceBlock sjb1 = null;
            BounceBlock sjb2 = null;

            float respawnTimer = original.respawnTimer;
            BounceBlock.States state = original.state;

            Scene.Remove(original);
            if (respawnTimer > 0 || state == BounceBlock.States.Broken)
            {
                return null;
            }
            if (b1Width >= 16 && b1Height >= 16 && original.CollideRect(new Rectangle((int)b1Pos.X, (int)b1Pos.Y, b1Width, b1Height))) Scene.Add(sjb1 = new BounceBlock(b1Pos, b1Width, b1Height));
            if (b2Width >= 16 && b2Height >= 16 && original.CollideRect(new Rectangle((int)b2Pos.X, (int)b2Pos.Y, b2Width, b2Height))) Scene.Add(sjb2 = new BounceBlock(b2Pos, b2Width, b2Height));

            if (Session.CoreModes.Cold == SceneAs<Level>().CoreMode)
            {
                AddParticles(original.Position, new Vector2(original.Width, original.Height), Calc.HexToColor("53cee6"));
            }
            else
            {
                Vector2 range = new Vector2(original.Width, original.Height);
                int numParticles = (int)(range.X * range.Y) / 10; //proportional to the area to cover
                SceneAs<Level>().ParticlesFG.Emit(Cuttable.paperScraps, numParticles / 4, original.Position + new Vector2(range.X / 2, range.Y / 2), new Vector2(range.X / 2, range.Y / 2), Calc.HexToColor("f3570e"));
                SceneAs<Level>().ParticlesFG.Emit(Cuttable.paperScraps, 3 * numParticles / 4, original.Position + new Vector2(range.X / 2, range.Y / 2), new Vector2(range.X / 2, range.Y / 2), Calc.HexToColor("16152b"));
            }
            return null;
        }
    }
}
