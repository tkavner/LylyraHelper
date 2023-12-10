using Celeste.Mod.LylyraHelper.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.LylyraHelper.Code.Entities
{
    internal class CustomPaper : Paper
    {

        public CustomPaper(EntityData data, Vector2 offset) :
            base(data, offset,
             texture: "objects/LylyraHelper/dashPaper/dashpaper",
             gapTexture: "objects/LylyraHelper/dashPaper/dashpapergap")
        {

        }
}
