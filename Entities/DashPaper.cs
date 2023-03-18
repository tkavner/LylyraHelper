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
    public class DashPaper : Paper
    {
        public class Scissors : Entity
        {
            private List<Paper> Cutting = new List<Paper>();
            private Vector2 CutDirection;
            private Vector2 initialPosition;
            private float timeElapsed;
            private float lerp;
            private float lerpTime = 1F; //time to lerp over in seconds

            private double rampUpTime = 0.2F;
            private Vector2 moveStartPos;
            private Vector2 targetPos;
            private float speed;
            private Sprite sprite;
            private bool Moving = true;
            private bool playedAudio;
            private string directionPath;
            private List<DreamBlock> DreamCutting = new List<DreamBlock>();
            private List<FallingBlock> FallCutting = new List<FallingBlock>();



            public Scissors(Vector2[] nodes, int amount, int index, float offset, float speedMult, Vector2 direction, Vector2 initialPosition) : base(nodes[0])
            {
                this.CutDirection = direction;
                if (nodes[1].X - nodes[0].X > 0)
                {
                    directionPath = "right";
                }
                else if (nodes[1].X - nodes[0].X < 0)
                {
                    directionPath = "left";
                }
                else if (nodes[1].Y - nodes[0].Y > 0)
                {
                    directionPath = "down";
                }
                else
                {
                    directionPath = "up";
                }
                this.initialPosition = initialPosition;
                moveStartPos = nodes[0];
                targetPos = nodes[1];
                Position = moveStartPos;

                sprite = new Sprite(GFX.Game, "objects/LylyraHelper/scissors/");
                sprite.AddLoop("spawn", "cut" + directionPath, 0.1F, new int[] { 0 });
                sprite.AddLoop("idle", "cut" + directionPath, 0.1F);
                sprite.Play("spawn");
                Add(sprite);
                sprite.CenterOrigin();
                sprite.Visible = true;
                base.Collider = new ColliderList(new Circle(12f), new Hitbox(30F, 8f, -15f, -4f));
                Add(new PlayerCollider(OnPlayer));
            }


            private void OnPlayer(Player player)
            {
                if (timeElapsed > 1)
                {
                    player.Die((player.Position - Position).SafeNormalize());
                    Moving = false;
                }
            }

            public override void Update()
            {
                base.Update();
                float oldElapsed = timeElapsed;
                timeElapsed += Engine.DeltaTime;

                if (timeElapsed != oldElapsed && Moving) //check for frame advacement
                {
                    if (oldElapsed == 0)
                    {
                        Level level = SceneAs<Level>();
                        level.Displacement.AddBurst(Position, 0.4f, 12f, 36f, 0.5f);
                        level.Displacement.AddBurst(Position, 0.4f, 24f, 48f, 0.5f);
                        level.Displacement.AddBurst(Position, 0.4f, 36f, 60f, 0.5f);
                        Audio.Play("event:/game/05_mirror_temple/bladespinner_spin", Position);
                    }
                    if (timeElapsed > 1)
                    {
                        this.Position += (targetPos - moveStartPos).SafeNormalize() * 3;
                        sprite.CenterOrigin();
                        sprite.Visible = true;
                        sprite.Play("idle");
                        if (!playedAudio)
                        {
                            playedAudio = true;
                        }
                    }

                    //get dash paper, check if colliding, if so add to list (we need to check each type of DashPaper manually apparently for sppeed)
                    foreach (Paper d in base.Scene.Tracker.GetEntities<DashPaper>())
                    {
                        if (!Cutting.Contains(d)) if (this.CollideCheck(d)) Cutting.Add(d);
                    }

                    foreach (Paper d in base.Scene.Tracker.GetEntities<DeathNote>())
                    {
                        if (!Cutting.Contains(d)) if (this.CollideCheck(d)) Cutting.Add(d);
                    }

                    foreach (FallingBlock d in base.Scene.Tracker.GetEntities<FallingBlock>())
                    {
                        int x1 = (int)d.Position.X;
                        int x2 = (int)(Position.X);

                        int y1 = (int)d.Position.Y;
                        int y2 = (int)(Position.Y);
                        if (!FallCutting.Contains(d) && this.CollideCheck(d))
                        {
                            if (!(x1 == x2 ||
                                y1 == y2 ||
                                x1 + d.Width <= x2 ||
                                y1 + d.Height <= y2))
                            {
                                FallCutting.Add(d);
                            }
                        }
                    }
                    foreach (DreamBlock d in base.Scene.Tracker.GetEntities<DreamBlock>())
                    {
                        int x1 = (int)d.Position.X;
                        int x2 = (int)(Position.X);

                        int y1 = (int)d.Position.Y;
                        int y2 = (int)(Position.Y);
                        if (!DreamCutting.Contains(d) && this.CollideCheck(d))
                        {
                            if (!(x1 == x2 ||
                                y1 == y2 ||
                                x1 + d.Width <= x2 ||
                                y1 + d.Height <= y2))
                            {
                                DreamCutting.Add(d);
                            }
                        }
                    }
                    foreach (DreamBlock d in base.Scene.Tracker.GetEntities<DreamBlock>())
                    {
                        int x1 = (int)d.Position.X;
                        int x2 = (int)(Position.X);

                        int y1 = (int)d.Position.Y;
                        int y2 = (int)(Position.Y);
                        if (!DreamCutting.Contains(d) && this.CollideCheck(d))
                        {
                            if (!(x1 == x2 ||
                                y1 == y2 ||
                                x1 + d.Width <= x2 ||
                                y1 + d.Height <= y2))
                            {
                                DreamCutting.Add(d);
                            }
                        }
                    }

                    //check list for not colliding if so call Cut(X/Y)()
                    Cutting.RemoveAll(d =>
                    {
                        if (!d.CollideCheck(this))
                        {
                            if (CutDirection.Y != 0)
                            {
                                d.CutY(new Hole(initialPosition), CutDirection);
                            }
                            if (CutDirection.X != 0)
                            {
                                d.CutX(new Hole(initialPosition), CutDirection);
                            }
                            return true;
                        }
                        return false;
                    });

                    DreamCutting.RemoveAll(d =>
                    {
                        if (!d.CollideCheck(this))
                        {
                            Vector2 d1Position = d.Position;
                            float d1Width = d.Width;
                            float d1Height = d.Height;
                            if (CutDirection.Y != 0)
                            {
                                d1Height = d.Height;
                                //check for larger side
                                if (Math.Abs(d.Position.X - this.Position.X) > Math.Abs(d.Position.X + d.Width - this.Position.X))
                                {
                                    d1Width = Math.Abs(d.Position.X - this.Position.X);
                                }
                                else
                                {
                                    d1Width = Math.Abs(d.Position.X + d.Width - this.Position.X);
                                    d1Position.X = this.Position.X;
                                }
                            }
                            if (CutDirection.X != 0)
                            {
                                d1Height = d.Width;
                                //check for larger side
                                if (Math.Abs(d.Position.Y - this.Position.Y) > Math.Abs(d.Position.Y + d.Height - this.Position.Y))
                                {
                                    d1Height = Math.Abs(d.Position.Y - this.Position.Y);
                                }
                                else
                                {
                                    d1Height = Math.Abs(d.Position.Y + d.Height - this.Position.Y);
                                    d1Position.Y = this.Position.Y;
                                }
                            }
                            Logger.Log("Scissors", "test");
                            DreamBlock d1 = new DreamBlock(d1Position, d1Width, d1Height, null, false, false);

                            Scene.Add(d1);
                            Scene.Remove(d);
                            Audio.Play("event:/game/05_mirror_temple/bladespinner_spin", Position);
                            return true;
                        }
                        return false;
                    });
                    FallCutting.RemoveAll(d =>
                    {
                        if (!d.CollideCheck(this))
                        {
                            Vector2 d1Position = d.Position;
                            float d1Width = d.Width;
                            float d1Height = d.Height;
                            if (CutDirection.Y != 0)
                            {
                                d1Height = d.Height;
                                //check for larger side
                                if (Math.Abs(d.Position.X - this.Position.X) > Math.Abs(d.Position.X + d.Width - this.Position.X))
                                {
                                    d1Width = Math.Abs(d.Position.X - this.Position.X);
                                }
                                else
                                {
                                    d1Width = Math.Abs(d.Position.X + d.Width - this.Position.X);
                                    d1Position.X = this.Position.X;
                                }
                            }
                            if (CutDirection.X != 0)
                            {
                                d1Height = d.Width;
                                //check for larger side
                                if (Math.Abs(d.Position.Y - this.Position.Y) > Math.Abs(d.Position.Y + d.Height - this.Position.Y))
                                {
                                    d1Height = Math.Abs(d.Position.Y - this.Position.Y);
                                }
                                else
                                {
                                    d1Height = Math.Abs(d.Position.Y + d.Height - this.Position.Y);
                                    d1Position.Y = this.Position.Y;
                                }
                            }
                            Logger.Log("Scissors", "test");
                            


                            d.Collider = new Hitbox(d1Width, d1Height);


                            var tiles = d.GetType().GetField("tiles", BindingFlags.NonPublic | BindingFlags.Instance);
                            var tileType = d.GetType().GetField("TileType", BindingFlags.NonPublic | BindingFlags.Instance);
                            char tileTypeChar = (char)tileType.GetValue(d);

                            if (tileTypeChar == '1')
                            {
                                Audio.Play("event:/game/general/wall_break_dirt", Position);
                            }
                            else if (tileTypeChar == '3')
                            {
                                Audio.Play("event:/game/general/wall_break_ice", Position);
                            }
                            else if (tileTypeChar == '9')
                            {
                                Audio.Play("event:/game/general/wall_break_wood", Position);
                            }
                            else
                            {
                                Audio.Play("event:/game/general/wall_break_stone", Position);
                            }
                            TileGrid t = GFX.FGAutotiler.GenerateBox(tileTypeChar, (int)d1Width / 8, (int)d1Height / 8).TileGrid;
                            d.Remove((Component)tiles.GetValue(d));
                            d.Position = d1Position;
                            tiles.SetValue(d, t);
                            d.Add(t);
                            
                            return true;
                        }
                        return false;
                    });
                }
            }
    }

    public DashPaper(Vector2 position, int width, int height, bool safe, string texture = "objects/LylyraHelper/dashPaper/cloudblocknew")
    : base(position, width, height, safe, texture)
    {
        thisType = this.GetType();
    }

    public DashPaper(EntityData data, Vector2 vector2) : this(data.Position + vector2, data.Width, data.Height, false)
    {

    }

    public override void Update()
    {
        base.Update();
    }

    internal override void OnDash(Vector2 direction)
    {
        if (CanCloudSpawn())
        {
            Audio.Play("event:/char/madeline/jump");
            SpawnCloud(direction);
        }
    }

    private void SpawnCloud(Vector2 direction)
    {
        Audio.Play("event:/game/04_cliffside/cloud_pink_boost", Position);
        Player p = base.Scene.Tracker.GetEntity<Player>();
        var session = SceneAs<Level>().Session;
        session.Inventory.Dashes = p.MaxDashes;
        p.Dashes = p.MaxDashes;
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
                s = new Scissors(new Vector2[] { v1, v2 }, 1, 1, 0, 1, yOnly, gridPosition);
            }
            else
            {
                s = new Scissors(new Vector2[] { v2, v1 }, 1, 1, 0, 1, yOnly, gridPosition);
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
                s = new Scissors(new Vector2[] { v1, v2 }, 1, 1, 0, 1, xOnly, gridPosition);
            }
            else
            {
                s = new Scissors(new Vector2[] { v2, v1 }, 1, 1, 0, 1, xOnly, gridPosition);
            }
            base.Scene.Add(s);
        }
        Logger.Log("DashPaper", "Spawning Scissors!");
    }

    public bool CanCloudSpawn()
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
                Logger.Log("CloudBlock", string.Format("x: {0}, y: {1} player: {2}, {3}", x, y, p.Width, p.Height));
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