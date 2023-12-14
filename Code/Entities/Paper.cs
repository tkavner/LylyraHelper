﻿using Celeste;
using Celeste.Mod;
using Celeste.Mod.LylyraHelper.Code.Components.PaperComponents;
using Celeste.Mod.LylyraHelper.Components;
using Celeste.Mod.LylyraHelper.Intefaces;
using LylyraHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.LylyraHelper.Entities
{
    [Tracked(true)]
    public class Paper : Trigger
    {
        private Scene scene;
        private Vector2 groupOrigin;
        private List<Paper> group;
        private bool GroupLeader { get; set; }

        public bool[,] skip;
        public int[,][] holeTiles;
        public int[,][] tiles;

        private enum RenderMode
        {
            TileSet, FromDecals, Preset
        }

        private bool shouldRegenerate;
        private float regenerateTime;

        public float regenerateTimer;
        public RegenerationState regenState;

        public Color WallpaperColor = Calc.HexToColor("cac7e3");
        public enum RegenerationState
        {
            NoRegeneration, Regenerated, Regenerating, Waiting
        }

        public bool ShouldRegenerate()
        {
            return regenerateTimer <= 0 && regenState == RegenerationState.Waiting;
        }

        public IEnumerator Regenerate()
        {
            regenState = RegenerationState.Regenerating;
            float progressPerFrame = 0.17F; //percent of full progress per frame 0.05 = 5%
            while (regenerationProgress < 1)
            {
                regenerationProgress += progressPerFrame;
                yield return null;
            }
            yield return 0.05F;
            regenerationProgress = 0;
            for (int i = 0; i < (int)Width / 8; i++)
            {
                for (int j = 0; j < (int)Height / 8; j++)
                {
                    skip[i, j] = false;
                    holeTiles[i, j] = [-1, -1];
                }
            }
            regenState = RegenerationState.Regenerated;
            regenerateTimer = regenerateTime;
            Remove(regenCoroutine);
            yield break;
        }

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
        internal static int[][] holeRightSide = [[4, 1], [4, 2], [4, 3]];
        internal static int[][] holeRightSideTopEdge = [[1, 2]];
        internal static int[][] holeRightSideBottomEdge = [[1, 3]];

        internal static int[][] holeLeftTopCorner = [[0, 0]];
        internal static int[][] holeRightTopCorner = [[4, 0]];
        internal static int[][] holeRightBottomCorner = [[4, 4]];
        internal static int[][] holeLeftBottomCorner = [[0, 4]];

        public static int[][] holeEmpty = [[2, 2]];

        public Type thisType;
        internal bool noEffects;
        private string flagName;

        public Paper(EntityData data, Vector2 offset, 
            string texture = "objects/LylyraHelper/dashPaper/cloudblocknew", 
            string gapTexture = "objects/LylyraHelper/dashPaper/cloudblockgap")
        : base(data, offset)
        {
            int width = data.Width;
            int height = data.Height;
            thisType = this.GetType();
            base.Collider = new PaperHitbox(this, width, height);
            Collidable = true;
            Visible = true;
            this.noEffects = data.Bool("noEffects");
            this.flagName = data.Attr("flag");
            invert = data.Bool("invertFlag", false);
            Depth = Depths.SolidsBelow + 200;
            skip = new bool[width / 8, height / 8];
            tiles = new int[width / 8, height / 8][];
            holeTiles = new int[width / 8, height / 8][];
            for (int i = 0; i < width / 8; i++)
            {
                for (int j = 0; j < height / 8; j++)
                {
                    tiles[i, j] = new int[2];
                    holeTiles[i, j] = [-1, -1];
                }
            }
            Add(new DashListener(OnDash));
            Add(new PlayerCollider(OnPlayer));

            wallpaper = data.Attr("wallpaperMode", "Preset: Refill Gem");
            if (wallpaper == "Preset: Refill Gem")
            {

                Add(new RefillPresetPaperComponent(gapTexture, this));
            } 
            else if (wallpaper == "From FG Decals")
            {
                string decalPlacements = data.Attr("decalStampData", "");
                Add(new FromDecalsPresetPaperComponent(gapTexture, decalPlacements, this, WallpaperColor));
            } 
            else
            {
                Add(new RefillPresetPaperComponent(gapTexture, this));
            }


            //regeneration intialization

            regenerateTimer = regenerateTime = data.Float("regenerationDelay", 0);
            if (regenerateTimer > 0)
            {
                regenState = RegenerationState.Regenerated;
            }
            else
            {
                regenState = RegenerationState.NoRegeneration;
            }
            Chooser<MTexture> sourceChooser = new Chooser<MTexture>(
                    GFX.Game["particles/LylyraHelper/dashpapershard00"],
                    GFX.Game["particles/LylyraHelper/dashpapershard01"],
                    GFX.Game["particles/LylyraHelper/dashpapershard02"]);
            Cuttable.paperScraps = new ParticleType()
            {
                SourceChooser = sourceChooser,
                Color = Calc.HexToColor("cac7e3"),
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
            MethodInfo mi = this.GetType().GetMethod(nameof(CheckForSame)).MakeGenericMethod(new Type[] { thisType });
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

        bool previousState = false;
        private bool invert = false;
        private float regenerationProgress;
        private Coroutine regenCoroutine;
        private string wallpaper;

        public override void Update()
        {
            base.Update();
            if (regenState == RegenerationState.Waiting)
            {
                regenerateTimer -= Engine.DeltaTime;
                if (regenerateTimer <= 0) Add(regenCoroutine = new Coroutine(Regenerate()));
            }

        }

        public override void OnLeave(Player player)
        {
            if (flagName != "")
            {
                SceneAs<Level>().Session.SetFlag(flagName, invert);
            }
        }

        //method only called when the player enters the player
        public override void OnEnter(Player player)
        {
            if (flagName != "")
            {
                SceneAs<Level>().Session.SetFlag(flagName, !invert);
            }
        }

        public override void OnStay(Player player)
        {
            AddPlayerEffects(player);
        }

        //Add Visual Effects for the player being on the paper
        internal virtual void AddPlayerEffects(Player player)
        {
            
        }

        public override void Render()
        {
            base.Render();

            if (regenState == RegenerationState.Regenerating)
            {
                Draw.Rect(Position, Width, Height, Calc.HexToColor("FFFFFFFF") * regenerationProgress);
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
            public Vector2 renderOffset; //in pixels relative to tile position
            private MTexture[,] texSplice;
            private Paper parent;

            public Decoration(Paper parent, string filePath, Vector2 tilingPosition, Vector2 size)
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

            public Decoration(Paper parent, string filePath, Vector2 relativePos /*in pixels*/)
            {
                this.parent = parent;
                MTexture uncut = GFX.Game[filePath];
                relativePos -= new Vector2(uncut.Width, uncut.Height)/2;
                Vector2 uvStart = -relativePos;
                if (uvStart.X <= 0) uvStart.X = 0;
                if (uvStart.Y <= 0) uvStart.Y = 0;
                this.position = new Vector2((int)(relativePos.X / 8), (int)(relativePos.Y / 8));/*in tiles*/
                this.renderOffset = relativePos - (position * 8);/*in pixels*/

                if (position.X < 0)
                {
                    position.X = 0;
                    renderOffset.X = 0;
                }
                if (position.Y < 0) {
                    position.Y = 0;
                    renderOffset.Y = 0;
                }
                int xTiles = (int) (uncut.Width /*in pixels*/- uvStart.X/*in pixels*/) / 8 + (renderOffset.X != 0 ? 1 : 0);
                int yTiles = (int) (uncut.Height /*in pixels*/- uvStart.Y/*in pixels*/) / 8 + (renderOffset.Y != 0 ? 1 : 0);
                xTiles = (int) Calc.Clamp(xTiles, 0, parent.Width / 8 - position.X);
                yTiles = (int) Calc.Clamp(yTiles, 0, parent.Height / 8 - position.Y);
                this.size = new(xTiles, yTiles);
                texSplice = new MTexture[xTiles, yTiles];

                for (int i = 0; i < xTiles; i++)
                {
                    for (int j = 0; j < yTiles; j++)
                    {
                        int startPixelX = (int)(uvStart.X + i * 8);
                        int startPixelY = (int)(uvStart.Y + j * 8);
                        int textureSizeX = 8;
                        int textureSizeY = 8;
                        if (i == 0) textureSizeX = (int)(8 - (int)renderOffset.X);
                        if (j == 0) textureSizeY = (int)(8 - (int)renderOffset.Y);

                        if (i == xTiles - 1) textureSizeX = (int)Math.Min((uncut.Width - i * 8 - uvStart.X), 8);
                        if (j == yTiles - 1) textureSizeY = (int)Math.Min(((uncut.Height - j * 8 - uvStart.Y)), 8);
                        texSplice[i, j] = uncut.GetSubtexture(new Rectangle(startPixelX, startPixelY, textureSizeX, textureSizeY));
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
                            Vector2 drawPos = parent.TopLeft + renderOffset + position * 8 + new Vector2(i * 8, j * 8);
                            texSplice[i, j].Draw(drawPos);
                        }
                    }
                }
            }
        }

        public virtual void AddDecorations(){}

        internal void ResetRegenTimer()
        {
            if (regenState != RegenerationState.Regenerating && regenState != RegenerationState.NoRegeneration)
            {
                regenerateTimer = regenerateTime;
                regenState = RegenerationState.Waiting;
            }
        }
    }
}
