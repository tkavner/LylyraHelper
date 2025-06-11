using Celeste.Mod.LylyraHelper.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Celeste.Mod.LylyraHelper.Entities.Paper;

namespace Celeste.Mod.LylyraHelper.Code.Components.PaperComponents;

public class BlankPresetPaperComponent : PresetPaperComponent
{

    public BlankPresetPaperComponent(string gapTexture, string decalPlacements, Paper Parent, Color wallpaperColor) : base(gapTexture, decalPlacements, Parent, wallpaperColor)
    {
    }

}