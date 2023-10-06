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
    internal class CassetteBlockSliceableComponent : SliceableComponent
    {
        public CassetteBlockSliceableComponent(bool active, bool visible) : base(active, visible)
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
            CassetteBlock original = Entity as CassetteBlock;

            original.ShiftSize(original.blockHeight);
            Vector2[] resultArray = Slicer.CalcCuts(original.Position, new Vector2(original.Width, original.Height), slicer.Entity.Center, slicer.Direction, slicer.CutSize);

            original.RemoveSelf();
            original.side.RemoveSelf();
            original.side.Visible = false;
            Slicer.masterRemovedList.Add(original.side);
            slicer.Scene.Remove(original);
            slicer.Scene.Remove(original.side);
            Vector2 b1Pos = resultArray[0];
            Vector2 b2Pos = resultArray[1];
            int b1Width = (int)resultArray[2].X;
            int b1Height = (int)resultArray[2].Y;

            int b2Width = (int)resultArray[3].X;
            int b2Height = (int)resultArray[3].Y;

            CassetteBlock b1 = null;
            CassetteBlock b2 = null;

            if (b1Width >= 16 && b1Height >= 16)
            {
                b1 = new CassetteBlock(b1Pos, new EntityID(slicer.SceneAs<Level>().Session.Level, -1), b1Width, b1Height, original.Index, original.Tempo);
                b1.Scene = slicer.Scene;
                b1.blockHeight = 0;
                Scene.Add(b1); 
            }

            if (b2Width >= 16 && b2Height >= 16)
            {
                b2 = new CassetteBlock(b2Pos, new EntityID(slicer.SceneAs<Level>().Session.Level, -1), b2Width, b2Height, original.Index, original.Tempo);
                b2.Scene = slicer.Scene;
                b2.blockHeight = 0;
                Scene.Add(b2);
            }
            List<CassetteBlock> group = original.group;

            //disassemble group
            if (group.Count > 1)
            {
                foreach (CassetteBlock block in group)
                {
                    if (block == original) continue;
                    if (Slicer.masterRemovedList.Contains(block)) continue;

                    original.ShiftSize(block.blockHeight);

                    Scene.Remove(block);
                    Scene.Remove(block.side);
                    Scene.Add(new CassetteBlock(block.Position, block.ID, block.Width, block.Height, block.Index, block.Tempo));
                    block.blockHeight = 0;
                    Slicer.masterRemovedList.Add(block);
                    Slicer.masterRemovedList.Add(block.side);
                    foreach (StaticMover mover in block.staticMovers)
                    {
                        Slicer.HandleStaticMover(Scene, slicer.Direction, b1, b2, mover);
                    }
                }

            }

            foreach (StaticMover mover in original.staticMovers)
            {
                Slicer.HandleStaticMover(Scene, slicer.Direction, b1, b2, mover);
            }
            return new Entity[] { b1, b2};

        }

        public static void Load()
        {
            On.Monocle.Entity.Removed += Entity_Removed;
        }

        public static void Unload()
        {
            On.Monocle.Entity.Removed -= Entity_Removed;
        }

        private static void Entity_Removed(On.Monocle.Entity.orig_Removed orig, Entity self, Scene scene)
        {
            if (self is CassetteBlock cb)
            {
                scene.Remove(cb.side);
            }
            orig(self, scene);
        }
    }
}
