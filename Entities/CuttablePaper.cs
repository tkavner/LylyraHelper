using Celeste.Mod.LylyraHelper.Intefaces;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.LylyraHelper.Entities
{
    public class CuttablePaper : Paper, ICuttable
    {
        public static ParticleType paperScraps;
        internal Color color = Calc.HexToColor("cac7e3");

        public CuttablePaper(Vector2 position, int width, int height, bool safe, 
            string texture = "objects/LylyraHelper/dashPaper/cloudblocknew", 
            string gapTexture = "objects/LylyraHelper/dashPaper/cloudblockgap", 
            string flagName = "",
            bool noEffects = false)
        : base(position, width, height, safe, texture, gapTexture, flagName, noEffects)
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

        //TODO: Add ability to accomodate half cut (eg: scissors start on paper)
        public bool Cut(Vector2 cutPosition, Vector2 direction, int gapWidth, Vector2 cutStartPosition)
        {
            Vector2[] arrayResults = Scissors.CalcCuts(Position, new Vector2(Width, Height), cutPosition, direction, gapWidth); //cuts gives where the new block should exist, we want where it should not
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
                if (cutPosition.X < p2.X) p2.X = (int)cutPosition.X;
            }
            else if (direction.X < 0)
            {
                if (cutPosition.X > p1.X) p1.X = (int)cutPosition.X;
            }

            if (direction.Y > 0)
            {
                if (cutPosition.Y < p2.Y) p2.Y = (int)cutPosition.Y;
            }
            else if (direction.Y < 0)
            {
                if (cutPosition.Y > p1.Y) p1.Y = (int)cutPosition.Y;
            }

            if (direction.X < 0)
            {
                if (cutStartPosition.X < p2.X) p2.X = (int)cutStartPosition.X;
            }
            else if (direction.X > 0)
            {
                if (cutStartPosition.X > p1.X) p1.X = (int)cutStartPosition.X;
            }

            if (direction.Y < 0)
            {
                if (cutStartPosition.Y < p2.Y) p2.Y = (int)cutStartPosition.Y;
            }
            else if (direction.Y > 0)
            {
                if (cutStartPosition.Y > p1.Y) p1.Y = (int)cutStartPosition.Y;
            }


            p1 -= Position;
            p2 -= Position;
            p1 /= 8;
            p2 /= 8;

            int furthestTop = 10000000;
            int furthestDown = -1;
            int furthestLeft = 1000000;
            int furthestRight = -1;

            for (int i = (int)p1.X; i < p2.X; i++)
            {
                for (int j = (int)p1.Y; j < p2.Y; j++)
                {
                    if (i >= 0 && j >= 0 && i < (int)Width / 8 && j < (int)Height / 8)
                    {
                        if (!skip[i, j])
                        {
                            if (i < furthestLeft) furthestLeft = i;
                            if (i > furthestRight) furthestRight = i;

                            if (j < furthestTop) furthestTop = j;
                            if (j > furthestDown) furthestDown = j;

                            SceneAs<Level>().ParticlesFG.Emit(paperScraps, 1, Position + new Vector2(i * 8 + 4, j * 8 + 4), new Vector2(4), color);
                        }
                        
                        skip[i, j] = true;
                        holeTiles[i, j] = holeEmpty[0];
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
                        if (TileExists(x, y1))
                        {
                            bool emptyTop = TileEmpty(i, y1 - 1);
                            bool emptyLeft = TileEmpty(i - 1, y1);
                            bool emptyRight = TileEmpty(i + 1, y1);

                            if (!emptyTop)
                            {
                                if (i == 0)
                                {
                                    holeTiles[i, y1] = holeTopSideLeftEdge[0];
                                }
                                else if (i == (int)Width / 8 - 1)
                                {
                                    holeTiles[i, y1] = holeTopSideLeftEdge[0];
                                }
                                else
                                {
                                    holeTiles[i, y1] = holeTopSide[(counter1++ % holeTopSide.Length)];
                                }
                            }
                            else if (emptyTop && emptyLeft && emptyRight) holeTiles[i, y1] = holeEmpty[0];
                        }

                        if (TileExists(x, y2))
                        {
                            bool emptyTop = TileEmpty(i, y2 + 1);
                            bool emptyLeft = TileEmpty(i - 1, y2);
                            bool emptyRight = TileEmpty(i + 1, y2);

                            if (!emptyTop)
                            {
                                if (i == 0)
                                {
                                    holeTiles[i, y2] = holeBottomSideLeftEdge[0];
                                }
                                else if (i == (int) Width / 8 - 1)
                                {
                                    holeTiles[i, y2] = holeBottomSideRightEdge[0];
                                } else
                                {
                                    holeTiles[i, y2] = holeBottomSide[(counter2++ % holeBottomSide.Length)];
                                }
                            }
                            else if (emptyTop && emptyLeft && emptyRight) holeTiles[i, y2] = holeEmpty[0];
                        }
                    }
                }
            else 
                //left and right
                for (int i = (int)p1.Y - 1; i < p2.Y + 1; i++)
                {
                    for (int j = 0; j <= 1; j++)
                    {
                        //left
                        int y = i;
                        int x1 = (int)p1.X - j;
                        int x2 = (int)p2.X + j - 1;
                        if (TileExists(x1, y))
                        {
                            bool emptyTop = TileEmpty(x1 - 1, y);

                            bool emptyLeft = TileEmpty(x1, y - 1);

                            bool emptyRight = TileEmpty(x1, y + 1);
                            if (!emptyTop) {
                                if (i == 0)
                                {
                                    holeTiles[x1, y] = holeLeftSideTopEdge[0];
                                }
                                else if (i == (int)Height / 8 - 1)
                                {
                                    holeTiles[x1, y] = holeLeftSideBottomEdge[0];
                                }
                                else
                                {
                                    holeTiles[x1, y] = holeLeftSide[(counter1++ % holeLeftSide.Length)];
                                }
                            } 
                            else if (emptyTop && emptyLeft && emptyRight) holeTiles[x1, y] = holeEmpty[0];
                        }
                        //right
                        if (TileExists(x2, y))
                        {
                            bool emptyTop = TileEmpty(x2 + 1, y);
                            bool emptyLeft = TileEmpty(x2, y - 1);

                            bool emptyRight = TileEmpty(x2, y + 1);
                            if (!emptyTop)
                            {
                                if (i == 0)
                                {
                                    holeTiles[x2, y] = holeRightSideTopEdge[0];
                                }
                                else if (i == (int)Height / 8 - 1)
                                {
                                    holeTiles[x2, y] = holeRightSideBottomEdge[0];
                                }
                                else
                                {
                                    holeTiles[x2, y] = holeRightSide[(counter2++ % holeRightSide.Length)];
                                }
                            }
                            else if (emptyTop && emptyLeft && emptyRight) holeTiles[x2, y] = holeEmpty[0];
                        }
                    }
                }

            return true;
        }
        public void AddParticles(Vector2 position, Vector2 range)
        {
            int numParticles = (int)(range.X * range.Y) / 10; //proportional to the area to cover
            SceneAs<Level>().ParticlesFG.Emit(paperScraps, numParticles, position + new Vector2(range.X / 2, range.Y / 2), new Vector2(range.X / 2, range.Y / 2), color);

        }
    }
}
