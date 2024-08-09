using Celeste.Mod.LylyraHelper.Code.Entities.SS2024;
using Celeste.Mod.LylyraHelper.Components;
using Celeste.Mod.LylyraHelper.Other;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.LylyraHelper.Code.Components.Sliceable
{
    public class ARFBSlicableComponent : SliceableComponent
    {
        public ARFBSlicableComponent(bool active, bool visible) : base(active, visible)
        {
        }

        public override void OnSliceStart(Slicer slicer)
        {
        }

        public override void Activate(Slicer slicer)
        {
            AutoReturnFallingBlock block = Entity as AutoReturnFallingBlock;
            block.Get<Coroutine>().enumerators.Peek().MoveNext();
            block.Triggered = true;
        }

        public override Entity[] Slice(Slicer slicer)
        {
            AutoReturnFallingBlock original = Entity as AutoReturnFallingBlock;

            Vector2[] resultArray = Slicer.CalcCuts(original.Position, new Vector2(original.Width, original.Height), slicer.Entity.Center, slicer.Direction, slicer.CutSize);
            Vector2 b1Pos = resultArray[0];
            Vector2 b2Pos = resultArray[1];
            int b1Width = (int)resultArray[2].X;
            int b1Height = (int)resultArray[2].Y;

            int b2Width = (int)resultArray[3].X;
            int b2Height = (int)resultArray[3].Y;

            AutoReturnFallingBlock b1 = null;
            AutoReturnFallingBlock b2 = null;
            List<StaticMover> staticMovers = original.staticMovers;
            Audio.Play("event:/game/general/wall_break_stone", original.Position);

            bool vertical = slicer.Direction.Y != 0;

            Scene.Remove(original);

            if (b1Width >= 16 && b1Height >= 16)
            {
                b1 = new AutoReturnFallingBlock(LyraUtils.CloneEntityData(original.originalData, b1Pos, b1Width, b1Height), Vector2.Zero);
                Scene.Add(b1);
                b1.initialPos = vertical ? new Vector2(b1Pos.X, original.initialPos.Y + b1Pos.Y - original.Position.Y) : new Vector2(original.initialPos.X + b1Pos.X - original.Position.X, b1Pos.Y);
                b1.impacted = original.impacted;
                b1.returning = original.returning;
            }
            if (b2Width >= 16 && b2Height >= 16)
            {
                b2 = new AutoReturnFallingBlock(LyraUtils.CloneEntityData(original.originalData, b2Pos, b2Width, b2Height), Vector2.Zero);
                Scene.Add(b2);
                b2.initialPos = vertical ? new Vector2(b2Pos.X, original.initialPos.Y + b2Pos.Y - original.Position.Y) : new Vector2(original.initialPos.X + b2Pos.X - original.Position.X, b2Pos.Y);
                b2.impacted = original.impacted;
                b2.returning = original.returning;
            }

            foreach (StaticMover mover in staticMovers)
            {
                Slicer.HandleStaticMover(Scene, slicer.Direction, b1, b2, mover);
            }

            return [b1, b2];
        }

    }
}
