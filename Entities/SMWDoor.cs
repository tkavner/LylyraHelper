using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LylyraHelper.Entities
{
    [Tracked]
    [CustomEntity("LylyraHelper/SMWDoor")]
    public class SMWDoor : Solid
    {
        private MTexture doorTexture;

        public SMWDoor(Vector2 position, float width, float height, bool safe) :
            base(position, width, height, safe)
        {

        }


        public SMWDoor(EntityData data, Vector2 offset) : base(data.Position, 8, data.Height, false)
        {
            doorTexture = GFX.Game["objects/LylyraHelper/smwDoor/smwDoor"];
        }

        public override void Render()
        {
            base.Render();
            for (int i = 0; i< (int) Height / 8; i++)
            {
                doorTexture.Draw(Position + new Vector2(0, i * 8));
            }
        }
    }
}
