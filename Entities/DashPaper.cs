using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using global::Celeste;
using global::Celeste.Mod;
using global::Celeste.Mod.Entities;
using LylyraHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.LylyraHelper.Entities
{
    [Tracked]
    [CustomEntity("LylyraHelper/DashPaper")]
    public class DashPaper : Entity
    {
        class Hole
        {
            public float timer;
            public Vector2 position;

            public Hole(Vector2 v2)
            {
                timer = 5F;
                position = v2;
            }
        }

        public class Scissors : Entity
        {
            private List<DashPaper> Cutting = new List<DashPaper>();
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

                sprite = LylyraHelperModule.SpriteBank.Create("scissors" + directionPath);
                Add(sprite);
                sprite.CenterOrigin();
                sprite.Visible = true;
                sprite.Play("spawn" + directionPath);
                base.Collider = new ColliderList(new Circle(12f), new Hitbox(30F, 8f, -15f, -4f));
                Add(new PlayerCollider(OnPlayer));
            }


            private void OnPlayer(Player player)
            {
                player.Die((player.Position - Position).SafeNormalize());
                Moving = false;
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
                        sprite.Play("idle" + directionPath);
                        if (!playedAudio)
                        {
                            Audio.Play("event:/game/05_mirror_temple/bladespinner_spin", Position);
                            playedAudio = true;
                        }
                    }

                    //get dash paper, check if colliding, if so add to list
                    foreach (DashPaper d in base.Scene.Tracker.GetEntities<DashPaper>())
                    {
                        if (this.CollideCheck(d)) Cutting.Add(d);
                    }
                    //check list for not colliding if so call Cut(X/Y)()
                    Cutting.RemoveAll(d =>
                    {
                        if (!d.CollideCheck(this))
                        {
                            
                            
                            if (CutDirection.X != 0)
                            {
                                d.CutY(new Hole(initialPosition), CutDirection);
                            }
                            if (CutDirection.Y != 0)
                            {
                                d.CutX(new Hole(initialPosition), CutDirection);
                            }
                            return true;
                        }
                        return false;
                    });
                }


            }


            private Vector2 GetPositionLerp()
            {
                var D = targetPos - moveStartPos;
                var t = lerp / lerpTime;
                var alpha = rampUpTime;
                if (rampUpTime == 0)
                {
                    return (Vector2.Distance(targetPos, moveStartPos) / lerpTime) * D + moveStartPos;
                }
                else
                {
                    if (t >= alpha)
                    {
                        return (float)((2 * t - alpha) / (2 - alpha)) * D + moveStartPos;
                    }
                    else
                    {
                        return (float)(t * t / (alpha) / (2 - alpha)) * D + moveStartPos;
                    }
                }
            }
        }




        private Scene scene;
        private Vector2 groupOrigin;
        private List<DashPaper> group;
        private bool groupLeader;


        private MTexture[,] texSplice;
        private MTexture[,] holeTexSplice;

        private List<Hole> holes = new List<Hole>();

        private float cloudCooldown = 0.1F;
        private float cooldownTimer = 0F;
        private bool[,] skip;
        private int[,][] holeTiles;

        private int[,][] tiles;

        //arrays describing locations for different types of tiles in cloudblock.png
        private static int[][] leftTopCorners = new int[][] { new int[] { 0, 0 }, new int[] { 7, 3 }, new int[] { 7, 4 } };
        private static int[][] leftBottomCorners = new int[][] { new int[] { 0, 5 }, new int[] { 6, 3 }, new int[] { 6, 4 } };
        private static int[][] rightTopCorners = new int[][] { new int[] { 5, 0 }, new int[] { 6, 2 }, new int[] { 7, 2 } };
        private static int[][] rightBottomCorners = new int[][] { new int[] { 5, 5 }, new int[] { 6, 5 }, new int[] { 7, 5 } };

        private static int[][] rightBottomCornersInvert = new int[][] { new int[] { 6, 0 } };
        private static int[][] leftBottomCornersInvert = new int[][] { new int[] { 7, 0 } };
        private static int[][] rightTopCornersInvert = new int[][] { new int[] { 6, 1 } };
        private static int[][] leftTopCornersInvert = new int[][] { new int[] { 7, 1 } };

        private static int[][] topSide = new int[][] { new int[] { 1, 0 }, new int[] { 2, 0 }, new int[] { 3, 0 }, new int[] { 4, 0 } };

        private static int[][] bottomSide = new int[][] { new int[] { 1, 5 }, new int[] { 2, 5 }, new int[] { 3, 5 }, new int[] { 4, 5 } };

        private static int[][] leftSide = new int[][] { new int[] { 0, 1 }, new int[] { 0, 2 }, new int[] { 0, 3 }, new int[] { 0, 4 } };

        private static int[][] rightSide = new int[][] { new int[] { 5, 1 }, new int[] { 5, 2 }, new int[] { 5, 3 }, new int[] { 5, 4 } };

        private static int[][] middle = new int[][] {
            new int[] { 1, 1 }, new int[] { 1, 2 }, new int[] { 1, 3 }, new int[] { 1, 4 },
            new int[] { 2, 1 }, new int[] { 2, 2 }, new int[] { 2, 3 }, new int[] { 2, 4 },
            new int[] { 3, 1 }, new int[] { 3, 2 }, new int[] { 3, 3 }, new int[] { 3, 4 },
            new int[] { 4, 1 }, new int[] { 4, 2 }, new int[] { 4, 3 }, new int[] { 4, 4 }};

        private int tileCounter;

        private static int[][] holeTopSide = new int[][] { new int[] { 1, 0 }, new int[] { 2, 0 }, new int[] { 3, 0 } };
        private static int[][] holeBottomSide = new int[][] { new int[] { 1, 4 }, new int[] { 2, 4 }, new int[] { 3, 4 } };
        private static int[][] holeLeftSide = new int[][] { new int[] { 0, 1 }, new int[] { 0, 2 }, new int[] { 0, 3 } };
        private static int[][] holeRightSide = new int[][] { new int[] { 4, 1 }, new int[] { 4, 2 }, new int[] { 4, 3 } };

        private static int[][] holeLeftTopCorner = new int[][] { new int[] { 0, 0 } };
        private static int[][] holeRightTopCorner = new int[][] { new int[] { 4, 0 } };
        private static int[][] holeRightBottomCorner = new int[][] { new int[] { 4, 4 } };
        private static int[][] holeLeftBottomCorner = new int[][] { new int[] { 0, 4 } };

        private static int[][] holeEmpty = new int[][] { new int[] { 1, 1 } };

        public DashPaper(Vector2 position, int width, int height, bool safe, string texture = "objects/LylyraHelper/cloudBlock/cloudblocknew")
        : base(position)
        {
            base.Collider = new Hitbox(width, height);
            Collidable = true;
            Visible = true;
            Logger.Log("CloudBlock", "Initialized");
            Depth = Depths.BGDecals;
            skip = new bool[width / 8, height / 8];
            tiles = new int[width / 8, height / 8][];
            holeTiles = new int[width / 8, height / 8][];
            for (int i = 0; i < width / 8; i++)
            {
                for (int j = 0; j < height / 8; j++)
                {
                    tiles[i, j] = new int[2];
                    holeTiles[i, j] = new int[2] { -1, -1 };
                }
            }
            MTexture cloudTexturesUnsliced = GFX.Game[texture];
            MTexture holeTexturesUnsliced = GFX.Game["objects/LylyraHelper/cloudBlock/cloudblockgap"];

            Add(new DashListener(OnDash));
            texSplice = new MTexture[8, 6];
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    texSplice[i, j] = cloudTexturesUnsliced.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
                }
            }

            holeTexSplice = new MTexture[5, 5];
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    holeTexSplice[i, j] = holeTexturesUnsliced.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
                }
            }
        }

        public DashPaper(EntityData data, Vector2 vector2) : this(data.Position + vector2, data.Width, data.Height, false)
        {

        }



        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            this.scene = scene;
            if (group == null)
            {
                groupLeader = true;
                group = new List<DashPaper>();
                group.Add(this);
                FindInGroup(this);

                float num = float.MaxValue;
                float num2 = float.MinValue;
                float num3 = float.MaxValue;
                float num4 = float.MinValue;
                foreach (DashPaper item in group)
                {
                    if (item.Left < num)
                    {
                        num = item.Left;
                    }
                    if (item.Right > num2)
                    {
                        num2 = item.Right;
                    }
                    if (item.Bottom > num4)
                    {
                        num4 = item.Bottom;
                    }
                    if (item.Top < num3)
                    {
                        num3 = item.Top;
                    }
                }

                groupOrigin = new Vector2((int)(num + (num2 - num) / 2f), (int)num4);

                foreach (DashPaper item2 in group)
                {
                    item2.groupOrigin = groupOrigin;
                }
            }
            int i = 0;
            for (float num5 = base.Left; num5 < base.Right; num5 += 8f)
            {
                int j = 0;
                for (float num6 = base.Top; num6 < base.Bottom; num6 += 8f)
                {
                    bool flag = CheckForSame(num5 - 8f, num6);
                    bool flag2 = CheckForSame(num5 + 8f, num6);
                    bool flag3 = CheckForSame(num5, num6 - 8f);
                    bool flag4 = CheckForSame(num5, num6 + 8f);
                    if (flag && flag2 && flag3 && flag4)
                    {
                        //edge cases
                        if (!CheckForSame(num5 + 8f, num6 - 8f)) //inverted corner (right top)
                        {
                            SetImage(num5, num6, rightTopCornersInvert, i, j);
                        }
                        else if (!CheckForSame(num5 - 8f, num6 - 8f)) //inverted corner (left top)
                        {
                            SetImage(num5, num6, leftTopCornersInvert, i, j);
                        }
                        else if (!CheckForSame(num5 + 8f, num6 + 8f)) //inverted corner (right bottom)
                        {
                            SetImage(num5, num6, rightBottomCornersInvert, i, j);
                        }
                        else if (!CheckForSame(num5 - 8f, num6 + 8f)) //inverted corner (left bottom)
                        {
                            SetImage(num5, num6, leftBottomCornersInvert, i, j);
                        }
                        else //center
                        {
                            SetImage(num5, num6, middle, i, j);
                        }
                    }
                    else if (flag && flag2 && !flag3 && flag4) //left side
                    {
                        SetImage(num5, num6, topSide, i, j); //actually top
                    }
                    else if (flag && flag2 && flag3 && !flag4) //right side
                    {
                        SetImage(num5, num6, bottomSide, i, j); //actually bottom
                    }
                    else if (flag && !flag2 && flag3 && flag4) //bottom side
                    {
                        SetImage(num5, num6, rightSide, i, j); //actually right
                    }
                    else if (!flag && flag2 && flag3 && flag4) //top side
                    {
                        SetImage(num5, num6, leftSide, i, j);
                    }
                    else if (flag && !flag2 && !flag3 && flag4) //right top corner
                    {
                        SetImage(num5, num6, rightTopCorners, i, j);
                    }
                    else if (!flag && flag2 && !flag3 && flag4) //left top corner
                    {
                        SetImage(num5, num6, leftTopCorners, i, j);
                    }
                    else if (flag && !flag2 && flag3 && !flag4) //right bottom corner
                    {
                        SetImage(num5, num6, rightBottomCorners, i, j);
                    }
                    else if (!flag && flag2 && flag3 && !flag4) //left bottom corner
                    {
                        SetImage(num5, num6, leftBottomCorners, i, j);
                    }
                    j++;
                }
                i++;
            }
        }

        internal virtual void OnDash(Vector2 direction)
        {
            if (CanCloudSpawn())
            {
                if (cooldownTimer <= 0)
                {

                    cooldownTimer = cloudCooldown;
                    Audio.Play("event:/char/madeline/jump");
                    SpawnCloud(direction);
                }
            }
        }

        private void SetImage(float x, float y, int[][] tileChoices, int i, int j)
        {
            int[] selectedChoice = tileChoices[tileCounter++ % tileChoices.Length];
            tiles[i, j][0] = selectedChoice[0];
            tiles[i, j][1] = selectedChoice[1];
        }

        private bool CheckForSame(float x, float y)
        {
            foreach (DashPaper entity in base.Scene.Tracker.GetEntities<DashPaper>())
            {
                if (entity.Collider.Collide(new Rectangle((int)x, (int)y, 8, 8)))
                {
                    return true;
                }
            }
            return false;
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            this.scene = null;
        }

        public override void Update()
        {
            base.Update();

            /*if (holes.RemoveAll(h => {
                h.timer -= Engine.DeltaTime;
                return h.timer < 0; 
            }) > 0)
            {
                RecalcHoles();
            }*/


            if (cooldownTimer > 0)
            {
                cooldownTimer -= Engine.DeltaTime;
            }
            if (!groupLeader)
            {
                return;
            }


        }

        private void RecalcHoles()
        {
            for (int i = 0; i < Width / 8; i++)
            {
                for (int j = 0; j < Height / 8; j++)
                {
                    skip[i, j] = false;
                    holeTiles[i, j] = holeEmpty[0];
                }
            }
            foreach (Hole h in holes)
            {
                for (int i = -2; i <= 2; i++)
                {
                    for (int j = -2; j <= 2; j++)
                    {
                        Vector2 v = h.position - Position;
                        int x = (int)Math.Round(v.X / 8);
                        int y = (int)Math.Round(v.Y / 8);
                        int numx = x + i;
                        int numy = y + j;

                        if (numx >= 0 && numy >= 0 && numx < (int)Width / 8 && numy < (int)Height / 8)
                        {
                            if (skip[numx, numy])
                            { //thing exist there then we need to delete it instead
                                holeTiles[numx, numy] = holeEmpty[0];
                            }
                            else
                            {
                                skip[numx, numy] = true;
                                if (numx == 0 || numy == 0 || numx + 1 == (int)Width / 8 || numy + 1 == (int)Height / 8)
                                {
                                    holeTiles[numx, numy] = holeEmpty[0];
                                }
                                else if (i == -2 && j == -2) //top left
                                {
                                    holeTiles[numx, numy] = holeLeftTopCorner[tileCounter++ % holeLeftTopCorner.Length];
                                }
                                else if (i == 2 && j == -2) //top right
                                {
                                    holeTiles[numx, numy] = holeRightTopCorner[tileCounter++ % holeLeftTopCorner.Length];
                                }
                                else if (i == 2 && j == 2) //bottom right
                                {
                                    holeTiles[numx, numy] = holeRightBottomCorner[tileCounter++ % holeLeftTopCorner.Length];
                                }
                                else if (i == -2 && j == -2) //bottom left
                                {
                                    holeTiles[numx, numy] = holeLeftBottomCorner[tileCounter++ % holeLeftTopCorner.Length];
                                }
                                else if (j == -2) //top side
                                {
                                    holeTiles[numx, numy] = holeTopSide[tileCounter++ % holeLeftTopCorner.Length];
                                }
                                else if (j == 2) //bottom side
                                {
                                    holeTiles[numx, numy] = holeBottomSide[tileCounter++ % holeLeftTopCorner.Length];
                                }
                                else if (i == -2) //left side
                                {
                                    holeTiles[numx, numy] = holeLeftSide[tileCounter++ % holeLeftTopCorner.Length];
                                }
                                else if (i == 2) //right side
                                {
                                    holeTiles[numx, numy] = holeRightSide[tileCounter++ % holeLeftTopCorner.Length];
                                }
                                else
                                {
                                    holeTiles[numx, numy] = holeEmpty[0];
                                }
                            }
                        }

                    }
                }
            }
        }

        private void CutY(Hole h, Vector2 direction)
        {
            holes.Add(h);
            Vector2 v = h.position - Position;

            int x = (int)Math.Round(v.X / 8);
            int y = (int)Math.Round(v.Y / 8);

            int startX = direction.X > 0 ? 0 : x;
            int endX = direction.X > 0 ? x : (int)Width / 8;

            for (int i = startX; i <= endX; i++)
            {
                for (int j = 0; j <= (int)Height / 8; j++)
                {

                    int numx = i;
                    int numy = j;

                    if (numx >= 0 && numy >= 0 && numx < (int)Width / 8 && numy < (int)Height / 8)
                    {
                        skip[numx, numy] = true;
                        holeTiles[numx, numy] = holeEmpty[0];
                    }
                }
            }

        }

        private void CutX(Hole h, Vector2 direction)
        {
            holes.Add(h);
            Vector2 v = h.position - Position;
            int x = (int)Math.Round(v.X / 8);
            int y = (int)Math.Round(v.Y / 8);

            int startY = direction.Y > 0 ? 0 : y;
            int endY = direction.Y > 0 ? y : (int)Height / 8;

            for (int i = 0; i <= (int)Width / 8; i++)
            {
                for (int j = startY; j <= endY; j++)
                {

                    int numx = i;
                    int numy = j;

                    if (numx >= 0 && numy >= 0 && numx < (int)Width / 8 && numy < (int)Height / 8)
                    {
                        holeTiles[numx, numy] = holeEmpty[0];
                        skip[numx, numy] = true;
                    }
                }
            }

        }

        public override void Render()
        {
            base.Render();

            for (int i = 0; i < (int)Width / 8; i++)
            {
                for (int j = 0; j < (int)Height / 8; j++)
                {
                    if (!skip[i, j])
                    {
                        texSplice[tiles[i, j][0], tiles[i, j][1]].Draw(Position + new Vector2(i * 8, j * 8));
                    }
                    else
                    {
                        if (holeTiles[i, j] != holeEmpty[0])
                        {
                            holeTexSplice[holeTiles[i, j][0], holeTiles[i, j][1]].Draw(Position + new Vector2(i * 8, j * 8));
                        }
                    }
                }
            }
        }

        private void SpawnCloud(Vector2 direction)
        {
            //var m = new MiniTrampoline(base.Scene.Tracker.GetEntity<Player>().Position + new Vector2(0, 1), 32, false, this);
            Audio.Play("event:/game/04_cliffside/cloud_pink_boost", Position);
            Player p = base.Scene.Tracker.GetEntity<Player>();
            //Vector2 speed = new Vector2(p.Speed.X, p.Speed.Y);
            //if (speed.LengthSquared() <= 1e-6) speed = - Vector2.UnitY;
            //Vector2 launchFrom = p.Position - 10 * Vector2.Normalize(speed + new Vector2(0, 0));
            //p.ExplodeLaunch(launchFrom, true, false);
            var session = SceneAs<Level>().Session;
            session.Inventory.Dashes = p.MaxDashes;
            p.Dashes = p.MaxDashes;
            if (direction.X != 0)
            {
                Vector2 xOnly = new Vector2(direction.X, 0);
                var v1 = new Vector2(p.Position.X, Position.Y + Height);
                var v2 = new Vector2(p.Position.X, Position.Y + 0);
                Scissors s;
                if (Vector2.DistanceSquared(v1, p.Position) > Vector2.DistanceSquared(v2, p.Position)) {
                    s = new Scissors(new Vector2[] { v1, v2 }, 1, 1, 0, 1, xOnly, p.Position);
                }
                else
                {
                    s = new Scissors(new Vector2[] { v2, v1 }, 1, 1, 0, 1, xOnly, p.Position);
                }
                base.Scene.Add(s);
            }
            if (direction.Y != 0)
            {
                Vector2 yOnly = new Vector2(0, direction.Y);
                var v1 = new Vector2(Position.X + Width, p.Position.Y);
                var v2 = new Vector2(Position.X, p.Position.Y);
                Scissors s;
                if (Vector2.DistanceSquared(v1, p.Position) > Vector2.DistanceSquared(v2, p.Position))
                {
                    s = new Scissors(new Vector2[] { v1, v2 }, 1, 1, 0, 1, yOnly, p.Position);
                }
                else
                {
                    s = new Scissors(new Vector2[] { v2, v1 }, 1, 1, 0, 1, yOnly, p.Position);
                }
                base.Scene.Add(s);
            }
            Logger.Log("DashPaper", "Spawning Scissors!");
        }

        //
        private void FindInGroup(DashPaper block)
        {
            foreach (DashPaper entity in base.Scene.Tracker.GetEntities<DashPaper>())
            {
                if (entity != this && entity != block && (entity.CollideRect(new Rectangle((int)block.X - 1, (int)block.Y, (int)block.Width + 2, (int)block.Height)) || entity.CollideRect(new Rectangle((int)block.X, (int)block.Y - 1, (int)block.Width, (int)block.Height + 2))) && !group.Contains(entity))
                {
                    group.Add(entity);
                    FindInGroup(entity);
                    entity.group = group;
                }
            }
        }

        public bool CanCloudSpawn()
        {
            Player p = base.Scene.Tracker.GetEntity<Player>();
            if (this.CollideCheck<Player>())
            {
                Vector2 v = (p.Position - this.Position) / 8;
                int x = (int)Math.Round(v.X);
                int y = (int)Math.Round(v.Y);
                if (x >= 0 && y >= 0 && x < (int)Width / 8 && y < (int)Height / 8)
                {
                    Logger.Log("CloudBlock", string.Format("x: {0}, y: {1} player: {2}, {3}", x, y, p.Position.X, p.Position.Y));
                    if (!this.skip[x, y])
                    {
                        return true;
                    }
                }

            }

            return false;
        }

    }
}


