using Celeste.Mod.LylyraHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.LylyraHelper.Code.Components.Sliceable
{
    public class SwapBlockSliceableComponent : SliceableComponent
    {
        public SwapBlockSliceableComponent(bool active, bool visible) : base(active, visible)
        {

        }

        public override void Activate(Slicer slicer, Slicer.NewlySlicedEntityWrapper secondFrameEntityCombo)
        {
            (secondFrameEntityCombo.child as SwapBlock).OnDash(Vector2.UnitX); //parameter passed is irrelevant in this case, but we do need one
        }

        public override void OnSliceStart(Slicer slicer)
        {
        }

        public override SlicerCollisionResults Slice(Slicer slicer)
        {
            SwapBlock original = Entity as SwapBlock;
            Vector2[] resultArray = Slicer.CalcCuts(original.Position, new Vector2(original.Width, original.Height), slicer.Entity.Center, slicer.Direction, slicer.CutSize);

            Vector2 b1Pos = resultArray[0];
            Vector2 b2Pos = resultArray[1];
            int b1Width = (int)resultArray[2].X;
            int b1Height = (int)resultArray[2].Y;

            int b2Width = (int)resultArray[3].X;
            int b2Height = (int)resultArray[3].Y;

            SwapBlock b1 = null;
            SwapBlock b2 = null;

            Scene.Remove(original.path);
            Scene.Remove(original);
            if (b1Width >= 16 && b1Height >= 16)
            {
                Vector2 b1origdiff = b1Pos - original.Position;
                b1 = new SwapBlock(b1Pos, b1Width, b1Height, original.end, original.Theme);
                b1.start = original.start + b1origdiff;
                b1.end = original.end + b1origdiff;
                b1.maxBackwardSpeed = original.maxBackwardSpeed;
                b1.maxForwardSpeed = original.maxForwardSpeed;
                b1.moveRect = original.moveRect;
                b1.returnTimer = original.returnTimer;
                Scene.Add(b1);
            }
            if (b2Width >= 16 && b2Height >= 16)
            {
                Vector2 b2origdiff = original.start + (b2Pos - original.Position);
                b2 = new SwapBlock(b2Pos, b2Width, b2Height, original.end, original.Theme);
                b2.start = original.start + b2origdiff;
                b2.end = original.end + b2origdiff;
                b2.maxBackwardSpeed = original.maxBackwardSpeed;
                b2.maxForwardSpeed = original.maxForwardSpeed;
                b2.moveRect = original.moveRect;
                b2.returnTimer = original.returnTimer;
                Scene.Add(b2);
            }
            AddParticles(original.Position, new Vector2(original.Width, original.Height), Calc.HexToColor("FFFFFF"));
            foreach (StaticMover mover in original.staticMovers)
            {
                Slicer.HandleStaticMover(Scene, slicer.Direction, b1, b2, mover);
            }
            return new SlicerCollisionResults([b1, b2], original, resultArray);
        }
    }
}
