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
            Scene scene = slicer.Scene;
            (Entity as CassetteBlock).SetActivatedSilently(scene.Tracker.GetEntity<CassetteBlockManager>().currentIndex == (Entity as CassetteBlock).Index);
        }

        public override void OnSliceStart(Slicer slicer)
        {
        }

        public override Entity[] Slice(Slicer slicer)
        {
            CassetteBlock original = Entity as CassetteBlock;
            if (original.Mode != CassetteBlock.Modes.Solid) return null;
            if (!original.Activated) return null;
            Vector2[] resultArray = Slicer.CalcCuts(original.Position, new Vector2(original.Width, original.Height), slicer.Entity.Center, slicer.Direction, slicer.CutSize);

            original.RemoveSelf();
            original.side.RemoveSelf();
            Slicer.masterRemovedList.Add(original.side);
            slicer.Scene.Remove(original);
            slicer.Scene.Remove(original.side);
            Vector2 b1Pos = resultArray[0];
            Vector2 b2Pos = resultArray[1];
            int b1Width = (int)resultArray[2].X;
            int b1Height = (int)resultArray[2].Y;

            int b2Width = (int)resultArray[3].X;
            int b2Height = (int)resultArray[3].Y;

            AddParticles(
                original.Position,
                new Vector2(original.Width, original.Height),
                Calc.HexToColor(GetColor()), 2.5F/6.4F);
            CassetteBlock b1 = null;
            CassetteBlock b2 = null;

            if (b1Width >= 16 && b1Height >= 16)
            {
                b1 = new CassetteBlock(b1Pos, new EntityID(slicer.SceneAs<Level>().Session.Level, -1), b1Width, b1Height, original.Index, original.Tempo);
                b1.Scene = slicer.Scene;
                b1.Activated = b1.Collidable = true;
                b1.blockHeight = 0;
                Scene.Add(b1);
            }

            if (b2Width >= 16 && b2Height >= 16)
            {
                b2 = new CassetteBlock(b2Pos, new EntityID(slicer.SceneAs<Level>().Session.Level, -1), b2Width, b2Height, original.Index, original.Tempo);
                b2.Scene = slicer.Scene;
                b2.Activated = b2.Collidable = true;
                b2.blockHeight = 0;
                Scene.Add(b2);
            }
            List<CassetteBlock> group = original.group;

            foreach (StaticMover mover in original.staticMovers)
            {
                Slicer.HandleStaticMover(Scene, slicer.Direction, b1, b2, mover);
            }
            //disassemble group
            if (group.Count > 1)
            {
                foreach (CassetteBlock block in group)
                {
                    if (block == original) continue;
                    if (Slicer.masterRemovedList.Contains(block)) continue;

                    CassetteBlock newBlock = new(block.Position, new EntityID(slicer.SceneAs<Level>().Session.Level, -1), block.Width, block.Height, block.Index, block.Tempo);
                    Scene.Remove(block);
                    Scene.Remove(block.side);
                    Scene.Add(newBlock);

                    newBlock.Activated = newBlock.Collidable = true;
                    newBlock.blockHeight = 0;
                    block.RemoveSelf();
                    block.side.RemoveSelf();
                    Slicer.masterRemovedList.Add(block);
                    Slicer.masterRemovedList.Add(block.side);
                    foreach (StaticMover mover in block.staticMovers)
                    {
                        Slicer.HandleStaticMover(Scene, slicer.Direction, newBlock, null, mover);
                    }
                }

            } 
            else
            {

            }

            return new Entity[] { b1, b2};

        }

        public string GetColor()
        {
            switch ((Entity as CassetteBlock).Index)
            {
                case 0:
                    return "3da8f3";
                case 1:
                    return "e441be";
                case 2:
                    return "f0de31";
                case 3:
                    return "2ce246";
                default:
                    return "FFFFFF";
            }
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
