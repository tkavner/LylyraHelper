using Celeste.Mod.LylyraHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.LylyraHelper.Components;

[Tracked(true)]
public abstract class SliceableComponent : Component
{


    public SliceableComponent(bool active, bool visible) : base(active, visible)
    {

    }

    public abstract void OnSliceStart(Slicer slicer);

    public abstract SlicerCollisionResults Slice(Slicer slicer);

    public abstract void Activate(Slicer slicer, Slicer.NewlySlicedEntityWrapper secondFrameEntityCombo);


    internal void AddParticles(Vector2 position, Vector2 range, Color color)
    {
        int numParticles = (int)(range.X * range.Y) / 10; //proportional to the area to cover
        float percent = GetSettingsAsPercent();
        if (percent * numParticles < 1) return;
        this.Entity.SceneAs<Level>().ParticlesFG.Emit(Cuttable.paperScraps, (int)(numParticles * percent), position + new Vector2(range.X / 2, range.Y / 2), new Vector2(range.X / 2, range.Y / 2), color);
    }

    private float GetSettingsAsPercent()
    {
        var amount = LylyraHelperModule.Settings.SlicerParticles;
        switch (amount)
        {
            case LylyraHelperSettings.ParticleAmount.None: return 0f;
            case LylyraHelperSettings.ParticleAmount.Light: return 0.03f;
            case LylyraHelperSettings.ParticleAmount.Normal: return 0.1f;
            case LylyraHelperSettings.ParticleAmount.More: return 0.2f;
            case LylyraHelperSettings.ParticleAmount.WayTooMany: return 1f;
            case LylyraHelperSettings.ParticleAmount.JustExcessive: return 1.25f;
            default: return 0;
        }
    }
}