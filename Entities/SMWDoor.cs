using Celeste;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LylyraHelper.Entities
{
    internal class SMWDoor : Solid
    {
        public SMWDoor(Vector2 position, float width, float height, bool safe) : 
            base(position, width, height, safe)
        {

        }

        public SMWDoor(EntityData data, Vector2 offset) : base(data.Position, 8, data.Height, false)
        {
            
        }

        
    }
}
