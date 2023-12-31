using Celeste.Mod.Entities;
using Celeste.Mod.LylyraHelper.Components;
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
    class DeathNote : Paper
    {
        public DeathNote(EntityData data, Vector2 offset) : base(data, offset, "objects/LylyraHelper/dashPaper/deathnote", "objects/LylyraHelper/dashPaper/deathnotegap")
        {
            thisType = this.GetType();
            //Add(new Cuttable(this, Calc.HexToColor("8f0020")));
        }


        internal override void OnPlayer(Player player)
        {
            if (CollideCheck(player))
            {
                player.Die((player.Position - Position).SafeNormalize());
            }
        }

        internal override void OnDash(Vector2 direction)
        {

        }


    }
}
