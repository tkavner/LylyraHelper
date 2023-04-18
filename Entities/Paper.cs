using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.LylyraHelper.Entities
{

    [Tracked(true)]
    public abstract class Paper : Entity
    {

        private Scene scene;
        private Vector2 groupOrigin;
        private List<Paper> group;
        private bool GroupLeader { get; set; }
        private MTexture[,] texSplice;
        private MTexture[,] holeTexSplice;

        public bool[,] skip;
        public int[,][] holeTiles;
        public int[,][] tiles;

        public List<Decoration> decorations = new List<Decoration>();

        internal static int[][] leftTopCorners = new int[][] { new int[] { 0, 0 }, new int[] { 7, 3 }, new int[] { 7, 4 } };
        internal static int[][] leftBottomCorners = new int[][] { new int[] { 0, 5 }, new int[] { 6, 3 }, new int[] { 6, 4 } };
        internal static int[][] rightTopCorners = new int[][] { new int[] { 5, 0 }, new int[] { 6, 2 }, new int[] { 7, 2 } };
        internal static int[][] rightBottomCorners = new int[][] { new int[] { 5, 5 }, new int[] { 6, 5 }, new int[] { 7, 5 } };

        internal static int[][] rightBottomCornersInvert = new int[][] { new int[] { 6, 0 } };
        internal static int[][] leftBottomCornersInvert = new int[][] { new int[] { 7, 0 } };
        internal static int[][] rightTopCornersInvert = new int[][] { new int[] { 6, 1 } };
        internal static int[][] leftTopCornersInvert = new int[][] { new int[] { 7, 1 } };

        internal static int[][] topSide = new int[][] { new int[] { 1, 0 }, new int[] { 2, 0 }, new int[] { 3, 0 }, new int[] { 4, 0 } };

        internal static int[][] bottomSide = new int[][] { new int[] { 1, 5 }, new int[] { 2, 5 }, new int[] { 3, 5 }, new int[] { 4, 5 } };

        internal static int[][] leftSide = new int[][] { new int[] { 0, 1 }, new int[] { 0, 2 }, new int[] { 0, 3 }, new int[] { 0, 4 } };

        internal static int[][] rightSide = new int[][] { new int[] { 5, 1 }, new int[] { 5, 2 }, new int[] { 5, 3 }, new int[] { 5, 4 } };

        internal static int[][] middle = new int[][] {
            new int[] { 1, 1 }, new int[] { 1, 2 }, new int[] { 1, 3 }, new int[] { 1, 4 },
            new int[] { 2, 1 }, new int[] { 2, 2 }, new int[] { 2, 3 }, new int[] { 2, 4 },
            new int[] { 3, 1 }, new int[] { 3, 2 }, new int[] { 3, 3 }, new int[] { 3, 4 },
            new int[] { 4, 1 }, new int[] { 4, 2 }, new int[] { 4, 3 }, new int[] { 4, 4 }};

        internal static int[][] holeTopSide = new int[][] { new int[] { 1, 0 }, new int[] { 2, 0 }, new int[] { 3, 0 } };

        internal static int[][] holeTopSideLeftEdge = new int[][] { new int[] { 1, 1 } };
        internal static int[][] holeTopSideRightEdge = new int[][] { new int[] { 1, 2 } };
        internal static int[][] holeBottomSide = new int[][] { new int[] { 1, 4 }, new int[] { 2, 4 }, new int[] { 3, 4 } };
        internal static int[][] holeBottomSideLeftEdge = new int[][] { new int[] { 2, 3 } };
        internal static int[][] holeBottomSideRightEdge = new int[][] { new int[] { 3, 3 } };
        internal static int[][] holeLeftSide = new int[][] { new int[] { 0, 1 }, new int[] { 0, 2 }, new int[] { 0, 3 } };
        internal static int[][] holeLeftSideTopEdge = new int[][] { new int[] { 3, 1 } };
        internal static int[][] holeLeftSideBottomEdge = new int[][] { new int[] { 3, 2 } };
        internal static int[][] holeRightSide = new int[][] { new int[] { 4, 1 }, new int[] { 4, 2 }, new int[] { 4, 3 } };
        internal static int[][] holeRightSideTopEdge = new int[][] { new int[] { 1, 2 } };
        internal static int[][] holeRightSideBottomEdge = new int[][] { new int[] { 1, 3 } };

        internal static int[][] holeLeftTopCorner = new int[][] { new int[] { 0, 0 } };
        internal static int[][] holeRightTopCorner = new int[][] { new int[] { 4, 0 } };
        internal static int[][] holeRightBottomCorner = new int[][] { new int[] { 4, 4 } };
        internal static int[][] holeLeftBottomCorner = new int[][] { new int[] { 0, 4 } };



        internal static int[][] holeEmpty = new int[][] { new int[] { 2, 2 } };

        public Type thisType;

        public Paper(Vector2 position, int width, int height, bool safe, string texture = "objects/LylyraHelper/dashPaper/cloudblocknew", string gapTexture = "objects/LylyraHelper/dashPaper/cloudblockgap")
        : base(position)
        {
            thisType = this.GetType();
            base.Collider = new Hitbox(width, height);
            Collidable = true;
            Visible = true;

            Depth = Depths.SolidsBelow + 200;
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
            MTexture holeTexturesUnsliced = GFX.Game[gapTexture];

            Add(new DashListener(OnDash));
            Add(new PlayerCollider(OnPlayer));

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
            AddDecorations();
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            this.scene = scene;

            //GROUP BUILDING CODE
            if (group == null)
            {
                GroupLeader = true;
                group = new List<Paper>();
                group.Add(this);
                FindInGroup(this);

                float num = float.MaxValue;
                float num2 = float.MinValue;
                float num3 = float.MaxValue;
                float num4 = float.MinValue;
                foreach (Paper item in group)
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

                foreach (Paper item2 in group)
                {
                    item2.groupOrigin = groupOrigin;
                }
            }
            int i = 0;
            MethodInfo mi = this.GetType().GetMethod(nameof(Paper.CheckForSame)).MakeGenericMethod(new Type[] { thisType });
            for (float num5 = base.Left; num5 < base.Right; num5 += 8f)
            {
                int j = 0;
                for (float num6 = base.Top; num6 < base.Bottom; num6 += 8f)
                {
                    bool flag = (bool)mi.Invoke(this, new object[] { num5 - 8f, num6 });
                    bool flag2 = (bool)mi.Invoke(this, new object[] { num5 + 8f, num6 });
                    bool flag3 = (bool)mi.Invoke(this, new object[] { num5, num6 - 8f });
                    bool flag4 = (bool)mi.Invoke(this, new object[] { num5, num6 + 8f });

                    if (flag && flag2 && flag3 && flag4)
                    {
                        //edge cases
                        if (!(bool)mi.Invoke(this, new object[] { num5 + 8f, num6 - 8f })) //inverted corner (right top)
                        {

                            SetImage(num5, num6, rightTopCornersInvert, i, j);
                        }
                        else if (!(bool)mi.Invoke(this, new object[] { num5 - 8f, num6 - 8f })) //inverted corner (left top)
                        {
                            SetImage(num5, num6, leftTopCornersInvert, i, j);
                        }
                        else if (!(bool)mi.Invoke(this, new object[] { num5 + 8f, num6 + 8f })) //inverted corner (right bottom)
                        {
                            SetImage(num5, num6, rightBottomCornersInvert, i, j);
                        }
                        else if (!(bool)mi.Invoke(this, new object[] { num5 - 8f, num6 + 8f })) //inverted corner (left bottom)
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

        private void FindInGroup(Paper paper)
        {
            foreach (Paper entity in base.Scene.Tracker.GetEntities<Paper>())
            {
                if (entity != this && entity != paper && (entity.CollideRect(new Rectangle((int)paper.X - 1, (int)paper.Y, (int)paper.Width + 2, (int)paper.Height)) || entity.CollideRect(new Rectangle((int)paper.X, (int)paper.Y - 1, (int)paper.Width, (int)paper.Height + 2))) && !group.Contains(entity))
                {
                    group.Add(entity);
                    FindInGroup(entity);
                    entity.group = group;
                }
            }
        }

        private void SetImage(float x, float y, int[][] tileChoices, int i, int j)
        {
            int[] selectedChoice = GetTileFromArray(tileChoices);
            tiles[i, j][0] = selectedChoice[0];
            tiles[i, j][1] = selectedChoice[1];
        }

        internal virtual void OnDash(Vector2 direction)
        {

        }
        internal virtual void OnPlayer(Player player)
        {

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
            foreach (Decoration deco in decorations)
            {
                deco.Render();
            }
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            this.scene = null;
        }

        public bool CheckForSame<T>(float x, float y) where T : Paper
        {
            foreach (Paper entity in base.Scene.Tracker.GetEntities<T>())
            {
                if (entity.Collider.Collide(new Rectangle((int)x, (int)y, 8, 8)))
                {
                    return true;
                }
            }
            return false;
        }

        //really only used with the player so this should work?
        public bool CollidePaper(Entity e)
        {
            return CollidePaper(e.Collider);
        }

        public bool CollidePaper(Collider c)
        {

            if (c is Circle)
            {

                return (CollidePaper(c as Circle));
            }
            else
            if (c is Hitbox)
            {

                return (CollidePaper(c as Hitbox));
            }
            else if (c is ColliderList)
            {

                foreach (Collider collider in (c as ColliderList).colliders)
                {
                    bool toReturn = false;

                    toReturn = CollidePaper(collider) || toReturn;
                    return toReturn;
                }
            }
            return false;
        }

        public bool CollidePaper(Hitbox c)
        {
            List<Vector2> pointsToCheck = new List<Vector2>();

            for (float f1 = c.AbsoluteLeft - this.Position.X; f1 < c.AbsoluteRight - this.Position.X; f1 += 8)
            {
                for (float f2 = c.AbsoluteTop - this.Position.Y; f2 < c.AbsoluteBottom - this.Position.Y; f2 += 8)
                {
                    pointsToCheck.Add(new Vector2(f1, f2));
                }
            }

            foreach (Vector2 v in pointsToCheck)
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

            return false;
        }

        public bool CollidePaper(Circle c)
        {
            for (float f1 = c.AbsoluteLeft - this.Position.X; f1 < c.AbsoluteRight - this.Position.X; f1 += 8)
            {
                for (float f2 = c.AbsoluteTop - this.Position.Y; f2 < c.AbsoluteBottom - this.Position.Y; f2 += 8)
                {

                    int x = (int)f1;
                    int y = (int)f2;
                    if (x >= 0 && y >= 0 && (int)x < (int)Width && (int)y < (int)Height)
                    {
                        if (!this.skip[(int)(x / 8), (int)(y / 8)])
                        {
                            if (c.Collide(new Rectangle((int)(f1 + Position.X), (int)(f2 + Position.Y), (int)(8), (int)(8)))) return true;
                        }
                    }
                }
            }
            return false;
        }

        /**
         * returns a set of tile coordinates from an array of choices, can be overriden to specify selection logic.
         */
        public virtual int[] GetTileFromArray(int[][] arr)
        {
            return arr[0];
        }

        public bool TileEmpty(int x, int y)
        {
            if (x >= 0 && x < (int)Width / 8 && y >= 0 && y < (int)Height / 8) return skip[x, y];
            else return true;
        }

        public bool TileEmpty(Vector2 pos)
        {
            return TileEmpty((int)pos.X, (int)pos.Y);
        }

        public bool TileExists(int x, int y)
        {
            return x >= 0 && x < (int)Width / 8 && y >= 0 && y < (int)Height / 8;
        }

        public bool TileExists(Vector2 pos)
        {
            return TileExists((int)pos.X, (int)pos.Y);
        }


        public class Decoration
        {
            public Vector2 position; //in tiles relative to the paper
            public Vector2 size;
            private MTexture[,] texSplice;
            private Paper parent;

            public Decoration(Paper parent, String filePath, Vector2 tilingPosition, Vector2 size)
            {
                this.parent = parent;
                this.size = size;
                this.position = tilingPosition;
                texSplice = new MTexture[(int)size.X, (int)size.Y];
                MTexture uncut = GFX.Game[filePath];
                for (int i = 0; i < (int)size.X; i++)
                {
                    for (int j = 0; j < (int)size.Y; j++)
                    {
                        texSplice[i, j] = uncut.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
                    }
                }
            }

            public void Render()
            {
                for (int i = 0; i < (int)size.X; i++)
                {
                    for (int j = 0; j < (int)size.Y; j++)
                    {
                        if (!parent.TileEmpty((position + new Vector2(i, j))))
                        {
                            texSplice[i, j].Draw(parent.Position + position * 8 + new Vector2(i * 8, j * 8));
                        }
                    }
                }
            }
        }

        internal virtual void AddDecorations()
        {
        }
    }
}
