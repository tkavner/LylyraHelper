using Celeste.Mod.LylyraHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using static Celeste.Mod.LylyraHelper.Entities.Paper;

namespace Celeste.Mod.LylyraHelper.Code.Components.PaperComponents;

public class PresetPaperComponent : PaperComponent
{
    private string decalPlacements;
    private MTexture[,] holeTexSplice;
    internal Color WallpaperColor;
    internal List<Decoration> decorations = [];
    private int VisualExtend = 0;

    public PresetPaperComponent(string gapTexture, string decalPlacements, Paper Parent, Color wallpaperColor) : base(Parent)
    {
        MTexture holeTexturesUnsliced = GFX.Game[gapTexture];
        this.decalPlacements = decalPlacements;
        this.WallpaperColor = wallpaperColor;
        holeTexSplice = new MTexture[5, 5];
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                holeTexSplice[i, j] = holeTexturesUnsliced.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
            }
        }
    }

    public override void Added(Entity entity)
    {
        base.Added(entity);
        AddDecorations();
    }

    public virtual void AddDecorations()
    {

        if (decalPlacements.Trim() != "")
        {
            string[] decalPlacementArr = decalPlacements.Split(';');
            foreach (string strDecal in decalPlacementArr)
            {
                string[] data = strDecal.Split(',');
                decorations.Add(new Decoration(Parent, data[0], Vector2.One + new Vector2(float.Parse(data[1]), float.Parse(data[2]))));
            }
        }
    }
    //adapted from Monocole.TileGrid
    public Rectangle GetClippedRenderTiles()
    {
        Vector2 entityPos = base.Entity.Position;
        Camera clipCamera = SceneAs<Level>().Camera;
        if (clipCamera == null) return new Rectangle(0, 0, 0, 0);
        int val = (int)Math.Max(0.0, Math.Floor((clipCamera.Left - Parent.Right) / (float)Parent.TileWidth) - (double)VisualExtend);
        int val2 = (int)Math.Max(0.0, Math.Floor((clipCamera.Top - Parent.Bottom) / (float)Parent.TileHeight) - (double)VisualExtend);
        int val3 = (int)Math.Min(Parent.TilesX, Math.Ceiling((clipCamera.Right - Parent.X) / (float)Parent.TileWidth) + (double)VisualExtend);
        int val4 = (int)Math.Min(Parent.TilesY, Math.Ceiling((clipCamera.Bottom - Parent.Y) / (float)Parent.TileHeight) + (double)VisualExtend);

        return new Rectangle(val, val2, val3 - val, val4 - val2);
    }

    public override void Render()
    {
        base.Render();
        Rectangle visibleTiles = GetClippedRenderTiles();
        for (int i = visibleTiles.X; i < visibleTiles.Width; i++)
        {
            for (int j = visibleTiles.Y; j < visibleTiles.Height; j++)
            {
                if (!Parent.skip[i, j])
                {
                    Draw.Rect(Parent.Position + new Vector2(i * 8, j * 8) - Vector2.One, 10, 10, Color.Black);
                }
                else
                {
                    if (Parent.holeTiles[i, j] != holeEmpty[0])
                    {
                        holeTexSplice[Parent.holeTiles[i, j][0], Parent.holeTiles[i, j][1]].DrawOutline(Parent.Position + new Vector2(i * 8, j * 8));
                    }
                }
            }
        }
        for (int i = visibleTiles.X; i < visibleTiles.Width; i++)
        {
            for (int j = visibleTiles.Y; j < visibleTiles.Height; j++)
            {
                if (!Parent.skip[i, j])
                {
                    Draw.Rect(Parent.Position + new Vector2(i * 8, j * 8), 8, 8, WallpaperColor);
                }
                else
                {
                    if (Parent.holeTiles[i, j] != holeEmpty[0])
                    {
                        holeTexSplice[Parent.holeTiles[i, j][0], Parent.holeTiles[i, j][1]].Draw(Parent.Position + new Vector2(i * 8, j * 8), Vector2.Zero, WallpaperColor);
                    }
                }
            }
        }
        foreach (Decoration deco in decorations)
        {
            deco.Render(visibleTiles);
        }
    }

}