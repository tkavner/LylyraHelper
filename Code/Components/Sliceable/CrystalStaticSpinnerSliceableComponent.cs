using Celeste.Mod.LylyraHelper.Components;

namespace Celeste.Mod.LylyraHelper.Code.Components.Sliceables;

public class CrystalStaticSpinnerSliceableComponent : SliceableComponent
{
    public CrystalStaticSpinnerSliceableComponent(bool active, bool visible) : base(active, visible)
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
        (Entity as CrystalStaticSpinner).Destroy();
        return null;
    }
}