using Celeste.Mod.LylyraHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Celeste.GaussianBlur;

namespace Celeste.Mod.LylyraHelper.Code.Components.Sliceables
{
    public class CrushBlockSliceableComponent : SliceableComponent
    {
        public Vector2 gridOffset = Vector2.Zero;

        public CrushBlockSliceableComponent(bool active, bool visible) : base(active, visible)
        {

        }


        public override Entity[] Slice(Slicer slicer)
        {

            CrushBlock original = this.Entity as CrushBlock;
            if (!Scene.Contains(original))
            {
                return null;
            }

            //get private fields
            bool canMoveVertically = original.canMoveVertically;
            bool canMoveHorizontally = original.canMoveHorizontally;
            bool chillOut = original.chillOut;

            var returnStack = original.returnStack;
            List<StaticMover> staticMovers = original.staticMovers;
            SoundSource moveLoopSound = original.currentMoveLoopSfx;

            moveLoopSound?.Stop();

            SoundSource grumbleSound = original.returnLoopSfx;

            grumbleSound?.Stop();

            //Process private fields
            CrushBlock.Axes axii = canMoveVertically && canMoveHorizontally ? CrushBlock.Axes.Both : canMoveVertically ? CrushBlock.Axes.Vertical : CrushBlock.Axes.Horizontal;
            Vector2[] resultArray = Slicer.CalcCuts(original.Position, new Vector2(original.Width, original.Height), slicer.Entity.Center, slicer.Direction, slicer.CutSize);
            Vector2 cb1Pos = Slicer.Vector2Int(resultArray[0]);
            Vector2 cb2Pos = Slicer.Vector2Int(resultArray[1]);
            int cb1Width = (int)resultArray[2].X;
            int cb1Height = (int)resultArray[2].Y;

            int cb2Width = (int)resultArray[3].X;
            int cb2Height = (int)resultArray[3].Y;

            //create cloned crushblocks + set data

            CrushBlock cb1 = null;
            bool cb1Added;
            if (cb1Added = cb1Width >= 24 && cb1Height >= 24 && original.CollideRect(new Rectangle((int)cb1Pos.X, (int)cb1Pos.Y, cb1Width, cb1Height)))
            {
                Vector2 cb1Offset = original.Position - cb1Pos;

                Vector2 offset = cb1Pos - original.Position;
                List<CrushBlock.MoveState> newReturnStack = new();
                foreach (CrushBlock.MoveState state in returnStack)
                {
                    newReturnStack.Add(new CrushBlock.MoveState(state.From + offset, state.Direction));
                }
                cb1 = new CrushBlock(cb1Pos, cb1Width, cb1Height, axii, chillOut)
                {
                    returnStack = newReturnStack
                };
                Scene.Add(cb1);
            }

            CrushBlock cb2 = null;
            bool cb2Added;
            if (cb2Added = cb2Width >= 24 && cb2Height >= 24 && original.CollideRect(new Rectangle((int)cb2Pos.X, (int)cb2Pos.Y, cb2Width, cb2Height)))
            {
                Vector2 offset = cb2Pos - original.Position;
                List<CrushBlock.MoveState> newReturnStack = new();
                foreach (CrushBlock.MoveState state in returnStack)
                {
                    newReturnStack.Add(new CrushBlock.MoveState(state.From + offset, state.Direction));
                }
                cb2 = new CrushBlock(cb2Pos, cb2Width, cb2Height, axii, chillOut)
                {
                    returnStack = newReturnStack
                };
                Scene.Add(cb2);
            }
            foreach (StaticMover mover in staticMovers)
            {
                Slicer.HandleStaticMover(Scene, slicer.Direction, cb1, cb2, mover);
            }
            Slicer.masterRemovedList.Add(original);
            Scene.Remove(original);
            AddParticles(original.Position, new Vector2(original.Width, original.Height), Calc.HexToColor("62222b"));

            return new Entity[] {cb1, cb2};
        }

        public override void Activate(Slicer slicer)
        {

            CrushBlock crushBlock = this.Entity as CrushBlock;
            crushBlock.crushDir = -slicer.Direction;
            crushBlock.Attack(-slicer.Direction);
        }

        public override void OnSliceStart(Slicer slicer)
        {

        }
    }
}
