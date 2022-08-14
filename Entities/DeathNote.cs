using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.LylyraHelper.Entities
{
    [Tracked]
    [CustomEntity("LylyraHelper/DeathNote")]
    class DeathNote : DashPaper
    {
        public DeathNote(EntityData data, Vector2 vector2) : base(data.Position + vector2, data.Width, data.Height, false, "objects/LylyraHelper/dashPaper/deathnote")
        {
            Add(new PlayerCollider(OnPlayer));
            thisType = this.GetType();
        }


        private void OnPlayer(Player player)
        {
            if (CollidePaper(player))
            {
                player.Die((player.Position - Position).SafeNormalize());
            }
        }

        internal override void OnDash(Vector2 direction)
        {

        }


    }
}
