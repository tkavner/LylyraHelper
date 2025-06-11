using Celeste;
using Celeste.Mod.Entities;
using Celeste.Mod.LylyraHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;

namespace LylyraHelper.Entities;

[CustomEntity("LylyraHelper/SlicerController")]
public class SlicerController : Entity
{
    private string settings;
    private bool limitToController;

    public SlicerController(EntityData data, Vector2 offset) {
        settings = data.Attr("sliceableEntityTypes", "");


    }

    public override void Added(Scene scene)
    {
        Slicer.SlicerSettings.DefaultSettings = new Slicer.SlicerSettings(settings);
    }

    public override void Update()
    {
        base.Update();
        if (Scene!=null)Scene.Remove(this);
    }
}