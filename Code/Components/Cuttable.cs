using Celeste.Mod.LylyraHelper.Code.Components;
using Celeste.Mod.LylyraHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.LylyraHelper.Components
{
    public class Cuttable : PaperComponent
    {
        private Paper Parent;
        private Color Color = Calc.HexToColor("cac7e3");
        public static ParticleType paperScraps;

        public Cuttable(Paper parent, Color color) : base()
        {
            Parent = parent;
            Color = color;
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

        public bool Cut(Vector2 cutPosition, Vector2 direction, int gapWidth, Vector2 cutStartPosition)
        {
            Vector2 Position = Parent.Position;
            float Width = Parent.Width;
            float Height = Parent.Height;
            if (direction.Y < 0)
            {

            }
            if (direction.X < 0)
            {

            }
            Vector2[] arrayResults = Slicer.CalcCuts(Position, new Vector2(Width, Height), cutPosition, direction, gapWidth); //cuts gives where the new block should exist, we want where it should not
            Vector2 p1, p2; //these points represent the cut area
            if (direction.X != 0) //horizontal cut, vertical gap
            {
                p1 = arrayResults[0] + new Vector2(0, (arrayResults[2].Y > 0 ? arrayResults[2].Y : 0));
                p2 = arrayResults[1] + new Vector2((arrayResults[3].X < Width ? arrayResults[3].X : Width), 0);
            }
            else //vertical cut, horizontal gap
            {
                p1 = arrayResults[0] + new Vector2((arrayResults[2].X > 0 ? arrayResults[2].X : 0), 0);
                p2 = arrayResults[1] + new Vector2(0, (arrayResults[3].Y < Height ? arrayResults[3].Y : Height));
            }

            if (direction.X > 0)
            {
                if (cutStartPosition.X > p1.X) p1.X = (int)cutStartPosition.X;
                if (cutPosition.X < p2.X) p2.X = (int)cutPosition.X;
            }
            else if (direction.X < 0)
            {
                if (cutPosition.X > p1.X) p1.X = (int)cutPosition.X;
                if (cutStartPosition.X < p2.X) p2.X = (int)cutStartPosition.X;
            }
            else if (direction.Y > 0)
            {
                if (cutStartPosition.Y > p1.Y) p1.Y = (int)cutStartPosition.Y;
                if (cutPosition.Y < p2.Y) p2.Y = (int)cutPosition.Y;
            }
            else if (direction.Y < 0)
            {
                if (cutPosition.Y > p1.Y) p1.Y = (int)cutPosition.Y;
                if (cutStartPosition.Y < p2.Y) p2.Y = (int)cutStartPosition.Y;
            }


            p1 -= Position;
            p2 -= Position;
            p1 /= 8;
            p2 /= 8;

            int furthestTop = Int32.MaxValue;
            int furthestDown = -1;
            int furthestLeft = Int32.MaxValue;
            int furthestRight = -1;

            for (int i = (int)p1.X; i < p2.X; i++)
            {
                for (int j = (int)p1.Y; j < p2.Y; j++)
                {
                    if (i >= 0 && j >= 0 && i < (int)Width / 8 && j < (int)Height / 8)
                    {
                        if (!Parent.skip[i, j])
                        {
                            if (i < furthestLeft) furthestLeft = i;
                            if (i > furthestRight) furthestRight = i;

                            if (j < furthestTop) furthestTop = j;
                            if (j > furthestDown) furthestDown = j;

                            SceneAs<Level>().ParticlesFG.Emit(paperScraps, 1, Position + new Vector2(i * 8 + 4, j * 8 + 4), new Vector2(4), Color);
                        }

                        Parent.skip[i, j] = true;
                        Parent.holeTiles[i, j] = Paper.holeEmpty[0];
                    }
                }
            }

            int counter1 = 0;
            int counter2 = 0;
            //fix top and bottom holes
            if (direction.X != 0) for (int i = (int)p1.X - 1; i < p2.X + 1; i++)
            {
                     for (int j = 0; j <= 1; j++)
                     {
                        int x = i;
                        int y1 = (int)p1.Y - j;
                        int y2 = (int)p2.Y + j - 1;
                        if (Parent.TileExists(x, y1))
                        {
                            bool emptyTop = Parent.TileEmpty(i, y1 - 1);
                            bool emptyLeft = Parent.TileEmpty(i - 1, y1);
                            bool emptyRight = Parent.TileEmpty(i + 1, y1);

                            if (!emptyTop)
                            {
                                if (i == 0)
                                {
                                    Parent.holeTiles[i, y1] = Paper.holeTopSideLeftEdge[0];
                                }
                                else if (i == (int)Width / 8 - 1)
                                {
                                    Parent.holeTiles[i, y1] = Paper.holeTopSideLeftEdge[0];
                                }
                                else
                                {
                                    Parent.holeTiles[i, y1] = Paper.holeTopSide[(counter1++ % Paper.holeTopSide.Length)];
                                }
                            }
                            else if (emptyTop && emptyLeft && emptyRight) Parent.holeTiles[i, y1] = Paper.holeEmpty[0];
                        }

                        if (Parent.TileExists(x, y2))
                        {
                            bool emptyTop = Parent.TileEmpty(i, y2 + 1);
                            bool emptyLeft = Parent.TileEmpty(i - 1, y2);
                            bool emptyRight = Parent.TileEmpty(i + 1, y2);

                            if (!emptyTop)
                            {
                                if (i == 0)
                                {
                                    Parent.holeTiles[i, y2] = Paper.holeBottomSideLeftEdge[0];
                                }
                                else if (i == (int)Width / 8 - 1)
                                {
                                    Parent.holeTiles[i, y2] = Paper.holeBottomSideRightEdge[0];
                                }
                                else
                                {
                                    Parent.holeTiles[i, y2] = Paper.holeBottomSide[(counter2++ % Paper.holeBottomSide.Length)];
                                }
                            }
                            else if (emptyTop && emptyLeft && emptyRight) Parent.holeTiles[i, y2] = Paper.holeEmpty[0];
                        }
                    }
                }
            else
            {
                //left and right
                for (int i = (int)p1.Y - 1; i < p2.Y + 1; i++)
                {
                    for (int j = 0; j <= 1; j++)
                    {
                        //left
                        int y = i;
                        int x1 = (int)p1.X - j;
                        int x2 = (int)p2.X + j - 1;
                        if (Parent.TileExists(x1, y))
                        {
                            bool emptyTop = Parent.TileEmpty(x1 - 1, y);

                            bool emptyLeft = Parent.TileEmpty(x1, y - 1);

                            bool emptyRight = Parent.TileEmpty(x1, y + 1);
                            if (!emptyTop)
                            {
                                if (i == 0)
                                {
                                    Parent.holeTiles[x1, y] = Paper.holeLeftSideTopEdge[0];
                                }
                                else if (i == (int)Height / 8 - 1)
                                {
                                    Parent.holeTiles[x1, y] = Paper.holeLeftSideBottomEdge[0];
                                }
                                else
                                {
                                    Parent.holeTiles[x1, y] = Paper.holeLeftSide[(counter1++ % Paper.holeLeftSide.Length)];
                                }
                            }
                            else if (emptyTop && emptyLeft && emptyRight) Parent.holeTiles[x1, y] = Paper.holeEmpty[0];
                        }
                        //right
                        if (Parent.TileExists(x2, y))
                        {
                            bool emptyTop = Parent.TileEmpty(x2 + 1, y);
                            bool emptyLeft = Parent.TileEmpty(x2, y - 1);

                            bool emptyRight = Parent.TileEmpty(x2, y + 1);
                            if (!emptyTop)
                            {
                                if (i == 0)
                                {
                                    Parent.holeTiles[x2, y] = Paper.holeRightSideTopEdge[0];
                                }
                                else if (i == (int)Height / 8 - 1)
                                {
                                    Parent.holeTiles[x2, y] = Paper.holeRightSideBottomEdge[0];
                                }
                                else
                                {
                                    Parent.holeTiles[x2, y] = Paper.holeRightSide[(counter2++ % Paper.holeRightSide.Length)];
                                }
                            }
                            else if (emptyTop && emptyLeft && emptyRight) Parent.holeTiles[x2, y] = Paper.holeEmpty[0];
                        }
                    }
                }
            }
            return true;
        }

    }
}
