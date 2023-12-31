using Celeste.Mod.LylyraHelper.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Celeste.Mod.LylyraHelper.Entities.Paper;

namespace Celeste.Mod.LylyraHelper.Code.Components.PaperComponents
{
    public class FromDecalsPresetPaperComponent : PresetPaperComponent
    {
        private string decalPlacements;

        public FromDecalsPresetPaperComponent(string gapTexture, string decalPlacements, Paper Parent, Color color) : base(gapTexture, Parent)
        {
            this.WallpaperColor = color;
            this.decalPlacements = decalPlacements;
        }


        public override void Render()
        {
            base.Render();
        }

        public override void AddDecorations()
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
    }
}
