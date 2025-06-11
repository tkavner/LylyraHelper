using Celeste.Mod.LylyraHelper.Components;

namespace Celeste.Mod.LylyraHelper.Code.Components.Sliceables;

public class BoosterSliceableComponent : SliceableComponent
{
    public BoosterSliceableComponent(bool active, bool visible) : base(active, visible)
    {
    }

    public override void Activate(Slicer slicer, Slicer.NewlySlicedEntityWrapper secondFrameEntityCombo)
    {
            
    }

    public override void OnSliceStart(Slicer slicer)
    {
        Booster booster = Entity as Booster;
        bool respawning = booster.respawnTimer > 0;
        if (!respawning) booster.PlayerReleased();
    }

    public override SlicerCollisionResults Slice(Slicer slicer)
    {
        return null;
    }
}