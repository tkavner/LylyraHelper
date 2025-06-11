using Celeste.Mod.Backdrops;
using Celeste.Mod.LylyraHelper.Code.Entities.SS2024;
using Monocle;

namespace Celeste.Mod.LylyraHelper.Code.Effects.SS2024;

//legit the only fast way to get the shockwaves to render on top
[CustomBackdrop("LylyraHelper/SS2024/ShockwaveRenderer")]
public class ShockwaveRenderer : Backdrop
{
    public ShockwaveRenderer(BinaryPacker.Element child)
    {

    }
    public ShockwaveRenderer()
    {

    }

    public override void Render(Scene scene)
    {
        base.Render(scene);
        if (!Visible) return;
        foreach (EllipticalShockwave shockwave in scene.Tracker.GetEntities<EllipticalShockwave>())
        {
            shockwave.RenderWave();
            //this makes the debug hitbox more visible in debug
            if (Engine.Commands.Open)
            {
                shockwave.DebugRenderWave(((Level)scene).Camera);
            }
        }

    }
}