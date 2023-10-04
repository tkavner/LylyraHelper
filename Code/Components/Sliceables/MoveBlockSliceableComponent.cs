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
    public class MoveBlockSliceableComponent : SliceableComponent
    {
        public MoveBlockSliceableComponent(bool active, bool visible) : base(active, visible)
        {
        }

        public override Entity[] Slice(Slicer slicer)
        {
            MoveBlock original = Entity as MoveBlock;
            if (original.state == MoveBlock.MovementState.Breaking)
            {
                return null;
            }

            Vector2[] resultArray = Slicer.CalcCuts(original.Position, new Vector2(original.Width, original.Height), slicer.Entity.Center, slicer.Direction, slicer.CutSize);
            Vector2 b1Pos = resultArray[0];
            Vector2 b2Pos = resultArray[1];
            int b1Width = (int)resultArray[2].X;
            int b1Height = (int)resultArray[2].Y;

            int b2Width = (int)resultArray[3].X;
            int b2Height = (int)resultArray[3].Y;

            MoveBlock mb1 = null;
            MoveBlock mb2 = null;
            List<StaticMover> staticMovers = original.staticMovers;
            AddParticles(
            original.Position,
                new Vector2(original.Width, original.Height),
                Calc.HexToColor("111111"));
            Audio.Play("event:/game/general/wall_break_stone", original.Position);
            MoveBlock.Directions direction = original.direction;
            bool canSteer = original.canSteer;
            bool fast = original.fast;
            Vector2 startPosition = original.startPosition;

            bool vertical = direction == MoveBlock.Directions.Up || direction == MoveBlock.Directions.Down;

            Scene.Remove(original);
            
            if (b1Width >= 16 && b1Height >= 16)
            {
                mb1 = new MoveBlock(b1Pos, b1Width, b1Height, direction, canSteer, fast);
                Scene.Add(mb1);
                mb1.startPosition = vertical ? new Vector2(b1Pos.X, startPosition.Y) : new Vector2(startPosition.X, b1Pos.Y);
            }
            if (b2Width >= 16 && b2Height >= 16)
            {
                mb2 = new MoveBlock(b2Pos, b2Width, b2Height, direction, canSteer, fast);
                Scene.Add(mb2);
                mb2.startPosition = vertical ? new Vector2(b2Pos.X, startPosition.Y) : new Vector2(startPosition.X, b2Pos.Y);
            }

            foreach (StaticMover mover in staticMovers)
            {
                Slicer.HandleStaticMover(Scene, slicer.Direction, mb1, mb2, mover);
            }

            return new Entity[] {mb1, mb2 };
        }

        public override void Activate(Slicer slicer)
        {
            MoveBlock moveBlock = Entity as MoveBlock;
            moveBlock.triggered = true;
            moveBlock.border.Visible = false;
        }

        public override void OnSliceStart(Slicer slicer)
        {
        }
    }
}
