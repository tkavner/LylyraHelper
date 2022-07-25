using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.LylyraHelper.Entities
{
    [Tracked]
    [CustomEntity("LylyraHelper/CloudBlock")]
    class CloudBlock : Entity
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
        //calling them trampolines to give them a unique name for sanity purposes
        class MiniTrampoline : JumpThru
        {

            Vector2 jumpPosition; //different from the cloud's position oddly enough
            private float timer;
            private float respawnTimer;
			private float endY;
            private float speed;
            private CloudBlock groupParent;
            private bool canRumble;
            private State state;
            private float startY;
            private MTexture sprite;

            public enum State
            {
                descending, springUp, returning
            }

            public MiniTrampoline(Vector2 position, int width, bool safe, CloudBlock cb)
                : base(position, width, safe)
            {
                groupParent = cb;
                state = State.descending;
                startY = Position.Y;
                Depth = cb.Depth - 10;
                sprite = GFX.Game["objects/LylyraHelper/cloudblock/minicloud"];
                Visible = true;
                
            }

			public override void Added(Scene scene)
			{
				base.Added(scene);
			}

            public override void Update()
            {
                base.Update();
                Player p = GetPlayerRider();

                if (p != null)
                {
                    //calc speed
                    switch (state)
                    {
                        case State.descending:
                            speed = 180f;
                            Audio.Play("event:/game/04_cliffside/cloud_pink_boost", Position);
                            state = State.springUp;
                            return;
                        case State.springUp:
                            if (base.Y >= startY)
                            {
                                speed -= 1200f * Engine.DeltaTime;
                            }
                            else
                            {
                                speed += 1200f * Engine.DeltaTime;
                                if (speed >= -100f)
                                {
                                    Player playerRider2 = GetPlayerRider();
                                    if (playerRider2 != null && playerRider2.Speed.Y >= 0f)
                                    {
                                        playerRider2.Speed.Y = -200f;
                                    }
                                    Collidable = false;
                                    groupParent.RemoveTrampoline(this);
                                }
                            }
                            break;
                        case State.returning:
                            speed = Calc.Approach(speed, 180f, 600f * Engine.DeltaTime);
                            MoveTowardsY(startY, speed * Engine.DeltaTime);
                            if (base.ExactPosition.Y == startY)
                            {
                                speed = 0f;
                            }
                            return;

                    }
                }

                float num = speed;
                if (num < 0f)
                {
                    num = -220f;
                }
                MoveV(speed * Engine.DeltaTime, num);
            }

            public override void Render()
            {
                sprite.Draw(Position);
                Logger.Log("Cloud Block", "Rendering mini cloud");
            }
        }

        private List<MiniTrampoline> trampolines;
        private Scene scene;
        private Vector2 groupOrigin;
        private List<CloudBlock> group;
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
        private static int[][] leftTopCorners = new int[][] { new int[] { 0, 0}, new int[] { 7, 3 },  new int[] { 7, 4 } };
        private static int[][] leftBottomCorners = new int[][] { new int[] { 0, 5 }, new int[] { 6, 3 },  new int[] { 6, 4 } };
        private static int[][] rightTopCorners = new int[][] { new int[] { 5, 0 }, new int[] { 6, 2},  new int[] { 7, 2 } };
        private static int[][] rightBottomCorners = new int[][] { new int[] { 5, 5 }, new int[] { 6, 5 },  new int[] { 7, 5 } };

        private static int[][] rightBottomCornersInvert = new int[][] { new int[] { 6, 0 }};
        private static int[][] leftBottomCornersInvert = new int[][] { new int[] { 7, 0 } };
        private static int[][] rightTopCornersInvert = new int[][] { new int[] { 6, 1 } };
        private static int[][] leftTopCornersInvert = new int[][] { new int[] { 7, 1 } };

        private static int[][] topSide = new int[][] { new int[] { 1, 0 }, new int[] { 2, 0 }, new int[] { 3, 0 }, new int[] { 4, 0 } };

        private static int[][] bottomSide = new int[][] { new int[] { 1, 5 }, new int[] { 2, 5 }, new int[] { 3, 5 }, new int[] { 4, 5 } };

        private static int[][] leftSide = new int[][] { new int[] { 0, 1 }, new int[] { 0, 2 }, new int[] { 0, 3 }, new int[] { 0, 4 } };

        private static int[][] rightSide = new int[][] { new int[] { 5, 1 }, new int[] { 5, 2 }, new int[] { 5, 3 }, new int[] { 5, 4 } };

        private static int[][] middle =  new int[][] { 
            new int[] { 1, 1 }, new int[] { 1, 2 }, new int[] { 1, 3 }, new int[] { 1, 4 },
            new int[] { 2, 1 }, new int[] { 2, 2 }, new int[] { 2, 3 }, new int[] { 2, 4 },
            new int[] { 3, 1 }, new int[] { 3, 2 }, new int[] { 3, 3 }, new int[] { 3, 4 },
            new int[] { 4, 1 }, new int[] { 4, 2 }, new int[] { 4, 3 }, new int[] { 4, 4 }};

        private int tileCounter;

        private static int[][] holeTopSide = new int[][] { new int[] { 1, 0 }, new int[] { 2, 0 }, new int[] { 3, 0 }};
        private static int[][] holeBottomSide = new int[][] { new int[] { 1, 4 }, new int[] { 2, 4 }, new int[] { 3, 4 }};
        private static int[][] holeLeftSide = new int[][] { new int[] { 0, 1 }, new int[] { 0, 2 }, new int[] { 0, 3 }};
        private static int[][] holeRightSide = new int[][] { new int[] { 4, 1 }, new int[] { 4, 2 }, new int[] { 4, 3 }};

        private static int[][] holeLeftTopCorner = new int[][] { new int[] {0, 0}};
        private static int[][] holeRightTopCorner = new int[][] { new int[] {4, 0}};
        private static int[][] holeRightBottomCorner = new int[][] { new int[] {4, 4}};
        private static int[][] holeLeftBottomCorner = new int[][] { new int[] {0, 4}};

        private static int[][] holeEmpty = new int[][] { new int[] { 1, 1 } };

        public CloudBlock(Vector2 position, int width, int height, bool safe)
        : base(position)
        {
            base.Collider = new Hitbox(width, height);
            Collidable = true;
            Visible = true;
            Logger.Log("CloudBlock", "Initialized");
            Depth = Depths.Below;
            skip = new bool[width / 8, height / 8];
            tiles = new int[width / 8, height / 8][];
            holeTiles = new int[width / 8, height / 8][];
            for (int i = 0; i < width / 8; i++)
            {
                for (int j = 0; j < height / 8; j++)
                {
                    tiles[i, j] = new int[2];
                    holeTiles[i, j] = new int[2] { -1, -1};
                }
            }
            MTexture cloudTexturesUnsliced = GFX.Game["objects/LylyraHelper/cloudBlock/cloudblocknew"];
            MTexture holeTexturesUnsliced = GFX.Game["objects/LylyraHelper/cloudBlock/cloudblockgap"];
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

        public CloudBlock(EntityData data, Vector2 vector2) : this(data.Position + vector2, data.Width, data.Height, false)
        {

        }

        

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            this.scene = scene;
            if (group == null)
            {
                groupLeader = true;
                group = new List<CloudBlock>();
                trampolines = new List<MiniTrampoline>();
                Add(new DashListener(OnDash));
                group.Add(this);
                FindInGroup(this);

                float num = float.MaxValue;
                float num2 = float.MinValue;
                float num3 = float.MaxValue;
                float num4 = float.MinValue;
                foreach (CloudBlock item in group)
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

                foreach (CloudBlock item2 in group)
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

        private void OnDash(Vector2 direction)
        {
            if (CanCloudSpawn())
            {
                if (cooldownTimer <= 0)
                {

                    cooldownTimer = cloudCooldown;
                    Audio.Play("event:/char/madeline/jump");
                    SpawnCloud();
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
            foreach (CloudBlock entity in base.Scene.Tracker.GetEntities<CloudBlock>())
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


            if (!groupLeader)
            {
                return;
            }
            if (cooldownTimer > 0)
            {
                cooldownTimer -= Engine.DeltaTime;
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
                        int x = (int) Math.Round(v.X / 8);
                        int y = (int)Math.Round(v.Y / 8);
                        int numx = x + i;
                        int numy = y + j;
                        
                        if (numx >= 0 && numy >= 0 && numx < (int) Width / 8 && numy < (int)Height / 8)
                        {
                            if (skip[numx, numy]) { //thing exist there then we need to delete it instead
                                holeTiles[numx, numy] = holeEmpty[0];
                            } else {
                                skip[numx, numy] = true;
                                if (numx == 0 || numy == 0 || numx + 1 == (int)Width / 8 || numy + 1 == (int)Height / 8) {
                                    holeTiles[numx, numy] = holeEmpty[0];
                                }
                                else if(i == -2 && j == -2) //top left
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

        private void CalcHole(Hole h)
        {
            holes.Add(h);
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
                                    holeTiles[numx, numy] = holeRightTopCorner[tileCounter++ % holeRightTopCorner.Length];
                                }
                                else if (i == 2 && j == 2) //bottom right
                                {
                                    holeTiles[numx, numy] = holeRightBottomCorner[tileCounter++ % holeRightBottomCorner.Length];
                                }
                                else if (i == -2 && j == -2) //bottom left
                                {
                                    holeTiles[numx, numy] = holeLeftBottomCorner[tileCounter++ % holeLeftBottomCorner.Length];
                                }
                                else if (j == -2) //top side
                                {
                                    holeTiles[numx, numy] = holeTopSide[tileCounter++ % holeTopSide.Length];
                                }
                                else if (j == 2) //bottom side
                                {
                                    holeTiles[numx, numy] = holeBottomSide[tileCounter++ % holeBottomSide.Length];
                                }
                                else if (i == -2) //left side
                                {
                                    holeTiles[numx, numy] = holeLeftSide[tileCounter++ % holeLeftSide.Length];
                                }
                                else if (i == 2) //right side
                                {
                                    holeTiles[numx, numy] = holeRightSide[tileCounter++ % holeRightSide.Length];
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

        public override void Render()
        {
            base.Render();
            
            for (int i = 0; i < (int) Width / 8; i++)
            {
                for (int j = 0; j < (int) Height / 8; j++)
                {
                    if (!skip[i,j])
                    {
                        texSplice[tiles[i, j][0], tiles[i, j][1]].Draw(Position + new Vector2(i * 8, j * 8));
                    } else
                    {
                        if (holeTiles[i, j] != holeEmpty[0]) {
                            holeTexSplice[holeTiles[i, j][0], holeTiles[i, j][1]].Draw(Position + new Vector2(i * 8, j * 8));
                        }
                    }
                }
            }
        }

        private void SpawnCloud()
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
            foreach (CloudBlock cb in group)
            {
                cb.CalcHole(new Hole(p.Position));
            }
            //trampolines.Add(m);
            //scene.Add(m);
            Logger.Log("CloudBlock", "Spawning Cloud!");
        }

        //
        private void FindInGroup(CloudBlock block)
        {
            foreach (CloudBlock entity in base.Scene.Tracker.GetEntities<CloudBlock>())
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
            foreach (CloudBlock item in group)
            {
                Player p = base.Scene.Tracker.GetEntity<Player>();
                if (item.CollideCheck<Player>())
                {
                    Vector2 v = (p.Position - item.Position) / 8;
                    int x = (int)Math.Round(v.X);
                    int y = (int)Math.Round(v.Y);
                    if (x >= 0 && y >= 0 && x < (int) item.Width / 8 && y < (int) item.Height / 8)
                    {
                        Logger.Log("CloudBlock", string.Format("x: {0}, y: {1} player: {2}, {3}", x, y, p.Position.X, p.Position.Y));
                        if (!item.skip[x, y])
                        {
                            return true;
                        }
                    }
                    
                }
            }

            return false;
        }

        private void RemoveTrampoline(MiniTrampoline miniTrampoline)
        {
            if (groupLeader) {
                trampolines.Remove(miniTrampoline);
            }
        }

        public static void Load()
        {
            
        }

        public static void Unload()
        {

        }
    }
}
