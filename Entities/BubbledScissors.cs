using Celeste;
using Celeste.Mod.Entities;
using Celeste.Mod.LylyraHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Celeste.Mod.LylyraHelper.Entities.DashPaper;

namespace Celeste.Mod.LylyraHelper.Entities
{
    [CustomEntity("LylyraHelper/BubbledScissors")]
    public class BubbledScissors : Actor
    {
        private Level level;

        public BubbledScissors(Vector2 position)
		: base(position)
		{

		}

		public BubbledScissors(EntityData e, Vector2 offset)
			: this(e.Position + offset)
		{

		}

        public override void Added(Scene scene)
        {
            base.Added(scene);
            this.level = SceneAs<Level>();
        }

        internal static void Unload()
        {
            On.Celeste.TheoCrystal.OnCollideH -= OnCollideH;
            On.Celeste.TheoCrystal.OnCollideH -= OnCollideV;
        }

        internal static void Load()
        {
            On.Celeste.TheoCrystal.OnCollideH += OnCollideH;
            On.Celeste.TheoCrystal.OnCollideH += OnCollideV;
        }

        private void SpawnScissors(CollisionData data)
        {
            if (Math.Abs(data.Direction.X) >= Math.Abs(data.Direction.Y))
            {
                Scissors s = new Scissors(Position, new Vector2(Math.Sign(this.Speed.X), 0), false);
                level.Add(s);
            } else
            {
                Scissors s = new Scissors(Position, new Vector2(0, Math.Sign(this.Speed.Y)), false);
                level.Add(s);
            }
        }

        private static void OnCollideH(On.Celeste.TheoCrystal.orig_OnCollideH orig, TheoCrystal self, CollisionData data)
        {
            if(self is BubbledScissors)
            {
                (self as BubbledScissors).SpawnScissors(data);
            } 
            else
            {
                orig.Invoke(self, data);
            }
        }


        private static void OnCollideV(On.Celeste.TheoCrystal.orig_OnCollideH orig, TheoCrystal self, CollisionData data)
        {
            if (self is BubbledScissors)
            {
                (self as BubbledScissors).SpawnScissors(data);
            }
            else
            {
                orig.Invoke(self, data);
            }
        }
    }
}
