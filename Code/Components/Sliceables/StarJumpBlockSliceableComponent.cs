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
    public class StarJumpBlockSliceableComponent : SliceableComponent
    {
        public StarJumpBlockSliceableComponent(bool active, bool visible) : base(active, visible)
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
            StarJumpBlock original = Entity as StarJumpBlock;
            Vector2[] resultArray = Slicer.CalcCuts(original.Position, new Vector2(original.Width, original.Height), Entity.Center, slicer.Direction, slicer.CutSize);

            Vector2 b1Pos = resultArray[0];
            Vector2 b2Pos = resultArray[1];
            int b1Width = (int)resultArray[2].X;
            int b1Height = (int)resultArray[2].Y;

            int b2Width = (int)resultArray[3].X;
            int b2Height = (int)resultArray[3].Y;

            StarJumpBlock sjb1 = null;
            StarJumpBlock sjb2 = null;

            bool sinks = original.sinks;

            Scene.Remove(original);

            AddParticles(original.Position, new Vector2(original.Width, original.Height), Calc.HexToColor("FFFFFF"));
            if (b1Width >= 8 && b1Height >= 8 && original.CollideRect(new Rectangle((int)b1Pos.X, (int)b1Pos.Y, b1Width, b1Height))) Scene.Add(sjb1 = new StarJumpBlock(b1Pos, b1Width, b1Height, sinks));
            if (b2Width >= 8 && b2Height >= 8 && original.CollideRect(new Rectangle((int)b2Pos.X, (int)b2Pos.Y, b2Width, b2Height))) Scene.Add(sjb2 = new StarJumpBlock(b2Pos, b2Width, b2Height, sinks));

            return null;
        }
    }
}
