using Celeste.Mod.LylyraHelper.Components;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Celeste.GaussianBlur;

namespace Celeste.Mod.LylyraHelper.Components.Sliceables;

public class DashBlockSliceableComponent : SliceableComponent
{
    public DashBlockSliceableComponent(bool active, bool visible) : base(active, visible)
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
        (Entity as DashBlock).Break(Entity.Position, slicer.Direction, true);

        foreach (StaticMover mover in (Entity as DashBlock).staticMovers)
        {
            Scene.Remove(mover.Entity);
        }
        return null;
    }
}