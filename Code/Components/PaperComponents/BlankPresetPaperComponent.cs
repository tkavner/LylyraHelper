using Celeste.Mod.LylyraHelper.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.LylyraHelper.Code.Components.PaperComponents;

public class BlankPresetPaperComponent : PresetPaperComponent
{

    public BlankPresetPaperComponent(string gapTexture, string decalPlacements, Paper Parent, Color wallpaperColor) : base(gapTexture, decalPlacements, Parent, wallpaperColor)
    {
    }

}