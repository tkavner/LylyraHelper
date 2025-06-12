using Celeste.Mod.LylyraHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste.Mod.LylyraHelper.Code.Components.Sliceables;

internal class FloatySpaceBlockSliceableComponent : SliceableComponent
{
    public FloatySpaceBlockSliceableComponent(bool active, bool visible) : base(active, visible)
    {
    }

    public override void Activate(Slicer slicer, Slicer.NewlySlicedEntityWrapper secondFrameEntityCombo)
    {
    }

    public override void OnSliceStart(Slicer slicer)
    {
    }

    public override SlicerCollisionResults Slice(Slicer slicer)
    {
        FloatySpaceBlock original = Entity as FloatySpaceBlock;
        Vector2[] resultArray = Slicer.CalcCuts(original.Position, new Vector2(original.Width, original.Height), slicer.Entity.Center, slicer.Direction, slicer.CutSize);

        Vector2 b1Pos = resultArray[0];
        Vector2 b2Pos = resultArray[1];
        int b1Width = (int)resultArray[2].X;
        int b1Height = (int)resultArray[2].Y;

        int b2Width = (int)resultArray[3].X;
        int b2Height = (int)resultArray[3].Y;

        FloatySpaceBlock b1 = null;
        FloatySpaceBlock b2 = null;

        List<StaticMover> staticMovers = original.staticMovers;
        char tileType = original.tileType;

        if (tileType == '1')
        {
            Audio.Play("event:/game/general/wall_break_dirt", Entity.Position);
        }
        else if (tileType == '3')
        {
            Audio.Play("event:/game/general/wall_break_ice", Entity.Position);
        }
        else if (tileType == '9')
        {
            Audio.Play("event:/game/general/wall_break_wood", Entity.Position);
        }
        else
        {
            Audio.Play("event:/game/general/wall_break_stone", Entity.Position);
        }
        if (b1Width >= 8 && b1Height >= 8)
        {
            b1 = new FloatySpaceBlock(b1Pos, b1Width, b1Height, tileType, false);
            Scene.Add(b1);
        }

        if (b2Width >= 8 && b2Height >= 8)
        {
            b2 = new FloatySpaceBlock(b2Pos, b2Width, b2Height, tileType, false);
            Scene.Add(b2);
        }
        List<FloatySpaceBlock> group = original.Group;
        if (!original.MasterOfGroup)
        {
            FloatySpaceBlock master = original.master;
            group = master.Group;
        }


        //disassemble group
        if (group.Count > 1)
        {
            foreach (FloatySpaceBlock block in group)
            {
                if (block == original) continue;
                if (Slicer.masterRemovedList.Contains(block)) continue;

                FloatySpaceBlock newBlock = new FloatySpaceBlock(block.Position, block.Width, block.Height, tileType, false);
                Scene.Add(newBlock);
                Scene.Remove(block);
                Slicer.masterRemovedList.Add(block);
                foreach (StaticMover mover in block.staticMovers)
                {
                    Slicer.HandleStaticMover(Scene, slicer.Direction, newBlock, null, mover);
                }
            }

        }

        AddParticles(
            original.Position,
            new Vector2(original.Width, original.Height),
            Calc.HexToColor("444444"));
        Slicer.masterRemovedList.Add(original);
        Scene.Remove(original);
        foreach (StaticMover mover in staticMovers)
        {
            Slicer.HandleStaticMover(Scene, slicer.Direction, b1, b2, mover);
        }
        return null;
    }
}