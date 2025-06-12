using Celeste.Mod.LylyraHelper.Entities;
using Microsoft.Xna.Framework;
using System;
using static Celeste.Mod.LylyraHelper.Entities.Paper;

namespace Celeste.Mod.LylyraHelper.Code.Components.PaperComponents;

public class RefillPresetPaperComponent : PresetPaperComponent
{

    public RefillPresetPaperComponent(string gapTexture, string decalPlacements, Paper Parent, Color wallpaperColor) : base(gapTexture, decalPlacements, Parent, wallpaperColor)
    {
        WallpaperColor = wallpaperColor;
    }

    public override void Render()
    {
        base.Render();
    }


    public override void AddDecorations()
    {
        int width = (int)Parent.Width;
        int height = (int)Parent.Height;
        if (width >= 32)
        {
            int borderSize = width / 8 % 2 == 0 ? 32 : 24;
            int offsetBorder = width / 8 % 4 == 1 ? -1 : -2;
            decorations.Add(new Decoration(Parent, string.Format("objects/LylyraHelper/dashPaper/dash_paper_decoration_bottom_{0}", borderSize),
                new Vector2((int)Math.Round(width / 16F) + offsetBorder, height / 8 - 2), new Vector2(borderSize / 8, 2)));
            decorations.Add(new Decoration(Parent, string.Format("objects/LylyraHelper/dashPaper/dash_paper_decoration_up_{0}", borderSize),
                new Vector2((int)Math.Round(width / 16F) + offsetBorder, 0), new Vector2(borderSize / 8, 2)));

            if (height >= 48)
            {
                string xSize = "32";
                string ySize = "32";
                if (width / 8 % 2 == 1)
                {
                    xSize = "40";
                }
                if (height / 8 % 2 == 1)
                {
                    ySize = "40";
                }
                int xOffset = width / 8 % 4 == 3 ? -3 : -2;
                int yOffset = height / 8 % 4 == 3 ? -3 : -2;
                decorations.Add(new Decoration(Parent, string.Format("objects/LylyraHelper/dashPaper/dash_paper_decoration_center_{0}_{1}", xSize, ySize),
                    new Vector2((int)Math.Round(width / 16F) + xOffset, (int)Math.Round(height / 16F) + yOffset), new Vector2(4, 4)));
            }
        }
        for (int i = 0; i < width / 8; i++)
        {
            decorations.Add(new Decoration(Parent, string.Format("objects/LylyraHelper/dashPaper/dash_paper_decoration_top_small", 8, 8),
                new Vector2(i, 0), new Vector2(1, 1)));
            decorations.Add(new Decoration(Parent, string.Format("objects/LylyraHelper/dashPaper/dash_paper_decoration_bottom_small", 8, 8),
                new Vector2(i, height / 8 - 1), new Vector2(1, 1)));
        }

        base.AddDecorations();
    }
}