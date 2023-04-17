using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using global::Celeste;
using global::Celeste.Mod;
using global::Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.LylyraHelper.Entities
{
    [Tracked]
    [CustomEntity("LylyraHelper/DashPaper")]
    public class DashPaper : CuttablePaper
    {
        private bool spawnScissors;
        private bool fragileScissors;
        private Random r = new Random(); //for particle gen only
        private int particleEmitPoints;
        private static ParticleType scissorScraps;

        public DashPaper(Vector2 position, int width, int height, bool safe, bool spawnScissors = true, bool fragileScissors = false, string texture = "objects/LylyraHelper/dashPaper/dashpaper", string gapTexture = "objects/LylyraHelper/dashPaper/cloudblockgap")
        : base(position, width, height, safe, texture, gapTexture)
        {
            thisType = this.GetType();
            this.spawnScissors = spawnScissors;
            this.fragileScissors = fragileScissors; 
            if (scissorScraps == null)
            {
                Chooser<MTexture> sourceChooser = new Chooser<MTexture>(
                    GFX.Game["particles/LylyraHelper/dashsymbol"],
                    GFX.Game["particles/LylyraHelper/dashsymbol"],
                    GFX.Game["particles/LylyraHelper/refillsymbol"],
                    GFX.Game["particles/LylyraHelper/doublerefillsymbol"]);
                scissorScraps = new ParticleType()
                {
                    SourceChooser = sourceChooser,
                    Color = color,
                    Acceleration = new Vector2(0f, 4f),
                    LifeMin = 0.8f,
                    LifeMax = 1.6f,
                    Size = .8f,
                    SizeRange = 0.2f,
                    Direction = (float)Math.PI / 2f,
                    DirectionRange = (float)Math.PI * 2F,
                    SpeedMin = 5f,
                    SpeedMax = 7F,
                    RotationMode = ParticleType.RotationModes.Random,
                    ScaleOut = true,
                    UseActualDeltaTime = true
                };
            }
        }

        public DashPaper(EntityData data, Vector2 vector2) : this(data.Position + vector2, data.Width, data.Height, false, data.Bool("spawnScissors", true), data.Bool("fragileScissors", false))
        {

        }

        public override void Update()
        {
            base.Update();
            if (spawnScissors)
            {
                int i = r.Next(0, (int) Width / 8);
                int j = r.Next(0, (int) Height / 8);
                particleEmitPoints += (int)(Width * Height);
                if (!skip[i, j] && particleEmitPoints > 40000)
                {
                    particleEmitPoints -= 40000;
                    SceneAs<Level>().ParticlesFG.Emit(scissorScraps, 1, Position + new Vector2(i * 8, j * 8), Vector2.Zero, Color.White);
                }

            }
        }

        internal override void OnDash(Vector2 direction)
        {
            if (CanActivate())
            {
                Audio.Play("event:/char/madeline/jump");
                Activate(direction);
            }
        }

        internal virtual void Activate(Vector2 direction)
        {
            Audio.Play("event:/game/04_cliffside/cloud_pink_boost", Position);
            Player p = base.Scene.Tracker.GetEntity<Player>();
            var session = SceneAs<Level>().Session;
            session.Inventory.Dashes = p.MaxDashes;
            p.Dashes = p.MaxDashes;
            
            if (spawnScissors)
            {
                Vector2 gridPosition = new Vector2(8 * (int)(p.Position.X / 8), 8 * (int)(p.Position.Y / 8));

                Vector2 xOnly = new Vector2(direction.X, 0);
                Vector2 yOnly = new Vector2(0, direction.Y);
                if (direction.Y != 0)
                {
                    var v1 = new Vector2(gridPosition.X, Position.Y + Height);
                    var v2 = new Vector2(gridPosition.X, Position.Y + 0);
                    Scissors s;
                    if (direction.Y < 0)
                    {
                        s = new Scissors(new Vector2[] { v1, v2 }, yOnly, fragileScissors);
                    }
                    else
                    {
                        s = new Scissors(new Vector2[] { v2, v1 }, yOnly, fragileScissors);
                    }
                    base.Scene.Add(s);
                }
                if (direction.X != 0)
                {
                    var v1 = new Vector2(Position.X + Width, gridPosition.Y);
                    var v2 = new Vector2(Position.X, gridPosition.Y);
                    Scissors s;
                    if (direction.X < 0)
                    {
                        s = new Scissors(new Vector2[] { v1, v2 }, xOnly, fragileScissors);
                    }
                    else
                    {
                        s = new Scissors(new Vector2[] { v2, v1 }, xOnly, fragileScissors);
                    }
                    base.Scene.Add(s);
                }
            }
        }

        public bool CanActivate()
        {
            Player p = base.Scene.Tracker.GetEntity<Player>();
            if (this.CollideCheck<Player>())
            {

                Vector2[] playerPointsToCheck = new Vector2[] {
                    (p.ExactPosition - this.Position),
                    (p.ExactPosition + new Vector2(p.Width, 0) - this.Position),
                    (p.ExactPosition + new Vector2(p.Width, -p.Height) - this.Position),
                    (p.ExactPosition + new Vector2(0, -p.Height) - this.Position) };

                foreach (Vector2 v in playerPointsToCheck)
                {
                    int x = (int)v.X;
                    int y = (int)v.Y;
                    if (x >= 0 && y >= 0 && x < (int)Width && y < (int)Height)
                    {
                        if (!this.skip[x / 8, y / 8])
                        {
                            return true;
                        }
                    }
                }

            }
            return false;
        }

    }
}