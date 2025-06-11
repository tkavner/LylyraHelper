using Celeste.Mod.LylyraHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Celeste.GaussianBlur;

namespace Celeste.Mod.LylyraHelper.Code.Components.Sliceables;

public class FallingBlockSliceableComponent : SliceableComponent
{
    public FallingBlockSliceableComponent(bool active, bool visible) : base(active, visible)
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
        FallingBlock original = Entity as FallingBlock;
        Vector2[] resultArray = Slicer.CalcCuts(original.Position, new Vector2(original.Width, original.Height), slicer.Entity.Center, slicer.Direction, slicer.CutSize);
        Vector2 cb1Pos = resultArray[0];
        Vector2 cb2Pos = resultArray[1];
        int cb1Width = (int)resultArray[2].X;
        int cb1Height = (int)resultArray[2].Y;

        int cb2Width = (int)resultArray[3].X;
        int cb2Height = (int)resultArray[3].Y;

        List<StaticMover> staticMovers = original.staticMovers;
        char tileTypeChar = original.TileType;

        if (tileTypeChar == '1')
        {
            Audio.Play("event:/game/general/wall_break_dirt", Entity.Position);
        }
        else if (tileTypeChar == '3')
        {
            Audio.Play("event:/game/general/wall_break_ice", Entity.Position);
        }
        else if (tileTypeChar == '9')
        {
            Audio.Play("event:/game/general/wall_break_wood", Entity.Position);
        }
        else
        {
            Audio.Play("event:/game/general/wall_break_stone", Entity.Position);
        }
        FallingBlock fb1 = null;
        FallingBlock fb2 = null;
        Scene.Remove(original);
        if (cb1Width >= 8 && cb1Height >= 8)
        {
            fb1 = new FallingBlock(cb1Pos, tileTypeChar, cb1Width, cb1Height, original.finalBoss, false, true);
            Scene.Add(fb1);
            fb1.Triggered = true;
            fb1.FallDelay = 0;
        }
        if (cb2Width >= 8 && cb2Height >= 8)
        {
            fb2 = new FallingBlock(cb2Pos, tileTypeChar, cb2Width, cb2Height, original.finalBoss, false, true);
            Scene.Add(fb2);
            fb2.Triggered = true;
            fb2.FallDelay = 0;
        }
        foreach (StaticMover mover in staticMovers)
        {
            Slicer.HandleStaticMover(Scene, slicer.Direction, fb1, fb2, mover);
        }
        AddParticles(
            original.Position,
            new Vector2(original.Width, original.Height),
            Calc.HexToColor("444444"));

        return null;
    }
}