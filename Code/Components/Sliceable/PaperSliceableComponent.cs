using Celeste.Mod.LylyraHelper.Components;
using Celeste.Mod.LylyraHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Celeste.GaussianBlur;
using Color = Microsoft.Xna.Framework.Color;

namespace Celeste.Mod.LylyraHelper.Code.Components.Sliceable
{
    public class PaperSliceableComponent : SliceableComponent
    {
        private Color Color = Calc.HexToColor("cac7e3");
        public static ParticleType paperScraps;
        private Dictionary<Slicer, Vector2> slicerStarts = new Dictionary<Slicer, Vector2>();
        public PaperSliceableComponent(bool active, bool visible) : base(active, visible)
        {
            if (paperScraps == null)
            {
                Chooser<MTexture> sourceChooser = new Chooser<MTexture>(
                    GFX.Game["particles/LylyraHelper/dashpapershard00"],
                    GFX.Game["particles/LylyraHelper/dashpapershard01"],
                    GFX.Game["particles/LylyraHelper/dashpapershard02"]);
                paperScraps = new ParticleType()
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
        }

        public override void Activate(Slicer slicer)
        {
            
        }

        public override void OnSliceStart(Slicer slicer)
        {
            if (!slicerStarts.ContainsKey(slicer))
            {
                slicerStarts.Remove(slicer);
                if (slicer.Direction.X > 0)
                {
                    slicerStarts.Add(slicer, slicer.Entity.CenterLeft);
                }
                else if (slicer.Direction.X < 0)
                {
                    slicerStarts.Add(slicer, slicer.Entity.CenterRight);
                }
                else if (slicer.Direction.Y > 0)
                {
                    slicerStarts.Add(slicer, slicer.Entity.TopCenter);
                }
                else
                {
                    slicerStarts.Add(slicer, slicer.Entity.BottomCenter);
                }
            }
        }

        private static Random particleRandom = new Random();
        private float ParticleChance()
        {
            var amount = LylyraHelperModule.Settings.SlicerParticles;
            switch (amount)
            {
                case LylyraHelperSettings.ParticleAmount.None: return 0f;
                case LylyraHelperSettings.ParticleAmount.Light: return 0.05f;
                case LylyraHelperSettings.ParticleAmount.Normal: return 0.15f;
                case LylyraHelperSettings.ParticleAmount.More: return 0.5f;
                case LylyraHelperSettings.ParticleAmount.WayTooMany: return 1f;
                case LylyraHelperSettings.ParticleAmount.JustExcessive: return 1f;
                default: return 0;
            }
        }

        public override Entity[] Slice(Slicer slicer)
        {
            Paper Parent = (Paper)this.Entity;
            Vector2 Position = Parent.Position;
            Vector2 cutPosition = slicer.GetDirectionalPosition();
            float Width = Parent.Width;
            float Height = Parent.Height;
            slicerStarts.TryGetValue(slicer, out Vector2 cutStartPosition);
            slicerStarts.Remove(slicer);

            Vector2[] arrayResults = Slicer.CalcCuts(Position, new Vector2(Width, Height), cutPosition, slicer.Direction, slicer.CutSize); //cuts gives where the new block should exist, we want where it should not
            Vector2 p1, p2; //these points represent the cut area
            if (slicer.Direction.X != 0) //horizontal cut, vertical gaps
            {
                p1 = arrayResults[0] + new Vector2(0, (arrayResults[2].Y > 0 ? arrayResults[2].Y : 0));
                p2 = arrayResults[1] + new Vector2((arrayResults[3].X < Width ? arrayResults[3].X : Width), 0);
            }
            else //vertical cut, horizontal gap
            {
                p1 = arrayResults[0] + new Vector2((arrayResults[2].X > 0 ? arrayResults[2].X : 0), 0);
                p2 = arrayResults[1] + new Vector2(0, (arrayResults[3].Y < Height ? arrayResults[3].Y : Height));
            }

            if (slicer.Direction.X > 0)
            {
                if (cutStartPosition.X > p1.X) p1.X = cutStartPosition.X;
                if (cutPosition.X < p2.X) p2.X = cutPosition.X;
                p1 -= new Vector2(8, 0F);
                p2 += new Vector2(8, 0F);
            }
            else if (slicer.Direction.X < 0)
            {
                if (cutPosition.X > p1.X) p1.X = cutPosition.X;
                if (cutStartPosition.X < p2.X) p2.X = cutStartPosition.X;
                p1 -= new Vector2(8, 0F);
                p2 += new Vector2(8, 0F);
            }
            else if (slicer.Direction.Y > 0)
            {
                if (cutStartPosition.Y > p1.Y) p1.Y = cutStartPosition.Y;
                if (cutPosition.Y < p2.Y) p2.Y = cutPosition.Y;
                p1 -= new Vector2(0, 8F);
                p2 += new Vector2(0, 8F);
            }
            else if (slicer.Direction.Y < 0)
            {
                if (cutPosition.Y > p1.Y) p1.Y = cutPosition.Y;
                if (cutStartPosition.Y < p2.Y) p2.Y = cutStartPosition.Y;
                p1 -= new Vector2(0, 8F);
                p2 += new Vector2(0, 8F);
            }


            p1 -= Position;
            p2 -= Position;
            p1 /= 8;
            p2 /= 8;

            int furthestTop = Int32.MaxValue;
            int furthestDown = -1;
            int furthestLeft = Int32.MaxValue;
            int furthestRight = -1;
            float chanceOfParticle = ParticleChance();

            Parent.ResetRegenTimer();
            for (int i = (int)p1.X; i <= p2.X; i++)
            {
                for (int j = (int)p1.Y; j <= p2.Y; j++)
                {
                    if (i >= 0 && j >= 0 && i < (int)Width / 8 && j < (int)Height / 8)
                    {
                        if (!Parent.skip[i, j])
                        {
                            if (i < furthestLeft) furthestLeft = i;
                            if (i > furthestRight) furthestRight = i;

                            if (j < furthestTop) furthestTop = j;
                            if (j > furthestDown) furthestDown = j;

                            if (particleRandom.NextFloat() < chanceOfParticle) SceneAs<Level>().ParticlesFG.Emit(paperScraps, 1, Position + new Vector2(i * 8 + 4, j * 8 + 4), new Vector2(4), Color);
                        }

                        Parent.skip[i, j] = true;

                        Parent.holeTiles[i, j] = Paper.holeEmpty[0];
                    }
                }
            }

            int counter1 = 0;
            int counter2 = 0;
            for (int i = (int)p1.X - 1; i <= p2.X + 1; i++)
            {
                for (int j = (int)p1.Y - 1; j <= p2.Y + 1; j++)
                {

                    int x = i;
                    int y = j;
                    if (Parent.TileExists(x, y))
                    {
                        bool emptyTop = Parent.TileEmpty(i, y - 1);
                        bool emptyLeft = Parent.TileEmpty(i - 1, y);
                        bool emptyRight = Parent.TileEmpty(i + 1, y);
                        bool emptyBottom = Parent.TileEmpty(i, y + 1);
                        if (emptyTop && emptyLeft && emptyRight && emptyBottom) Parent.holeTiles[i, y] = Paper.holeEmpty[0];
                        else if (!emptyTop && !emptyLeft && emptyRight && emptyBottom) Parent.holeTiles[i, y] = Paper.holeTopSideLeftEdge[0];
                        else if (!emptyTop && emptyLeft && !emptyRight && emptyBottom) Parent.holeTiles[i, y] = Paper.holeTopSideRightEdge[0];
                        else if (emptyTop && !emptyLeft && emptyRight && !emptyBottom) Parent.holeTiles[i, y] = Paper.holeBottomSideLeftEdge[0];
                        else if (emptyTop && emptyLeft && !emptyRight && !emptyBottom) Parent.holeTiles[i, y] = Paper.holeBottomSideLeftEdge[0];
                        else if (!emptyTop) Parent.holeTiles[i, y] = Paper.holeTopSide[x % Paper.holeTopSide.Length];
                        else if (!emptyLeft) Parent.holeTiles[i, y] = Paper.holeLeftSide[y % Paper.holeLeftSide.Length];
                        else if (!emptyRight) Parent.holeTiles[i, y] = Paper.holeRightSide[y % Paper.holeRightSide.Length];
                        else if (!emptyBottom) Parent.holeTiles[i, y] = Paper.holeBottomSide[x % Paper.holeBottomSide.Length];


                        else Parent.holeTiles[i, y] = Paper.holeEmpty[0];
                        if (!emptyTop)
                        {
                            if (i == 0)
                            {
                                Parent.holeTiles[i, y] = Paper.holeTopSideLeftEdge[0];
                            }
                            else if (i == (int)Width / 8 - 1)
                            {
                                Parent.holeTiles[i, y] = Paper.holeTopSideLeftEdge[0];
                            }
                            else
                            {
                                Parent.holeTiles[i, y] = Paper.holeTopSide[(counter1++ % Paper.holeTopSide.Length)];
                            }
                        }
                    }
                }
            }

            return null;
        }
    }
}
