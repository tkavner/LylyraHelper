using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.LylyraHelper.Entities
{
    class CloudBlock : JumpThru
    {
        public CloudBlock(Vector2 position, int width, int height, bool safe)
        : base(position, width, safe)
        {
            base.Collider = new Hitbox(width, height);
        }

        public CloudBlock(EntityData data, Vector2 vector2) : this(data.Position + vector2, data.Int("width"), data.Int("height"), true)
        {

        }

        public override void Update()
        {
            base.Update();

        }
    }
}
