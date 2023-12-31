using Celeste.Mod.LylyraHelper.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Celeste.Mod.LylyraHelper.Entities.Paper;

namespace Celeste.Mod.LylyraHelper.Code.Components.PaperComponents
{
    public class PresetPaperComponent : PaperComponent
    {
        private MTexture[,] holeTexSplice;
        internal Color WallpaperColor;
        internal List<Decoration> decorations = [];

        public PresetPaperComponent(string gapTexture, Paper Parent) : base(Parent)
        {
            MTexture holeTexturesUnsliced = GFX.Game[gapTexture];

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
            
        }

        public override void Render()
        {
            base.Render();
            for (int i = 0; i < (int)Entity.Width / 8; i++)
            {
                /*for (int j = 0; j < (int)Entity.Height / 8; j++)
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
                }*/
            }
            for (int i = 0; i < (int)Parent.Width / 8; i++)
            {
                for (int j = 0; j < (int)Parent.Height / 8; j++)
                {
                    if (!Parent.skip[i, j])
                    {
                        Draw.Rect(Parent.Position + new Vector2(i * 8, j * 8), 8, 8, WallpaperColor);
                    }
                    else
                    {
                        if (Parent.holeTiles[i, j] != holeEmpty[0])
                        {
                            holeTexSplice[Parent.holeTiles[i, j][0], Parent.holeTiles[i, j][1]].Draw(Parent.Position + new Vector2(i * 8, j * 8));
                        }
                    }
                }
            }
            foreach (Decoration deco in decorations)
            {
                deco.Render();
            }
        }

    }
}
