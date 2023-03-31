using Celeste.Mod.LylyraHelper.Intefaces;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.LylyraHelper.Entities
{
    public class CuttablePaper : Paper, ICuttable
    {
        public CuttablePaper(Vector2 position, int width, int height, bool safe, string texture = "objects/LylyraHelper/dashPaper/cloudblocknew")
        : base(position, width, height, safe, texture)
        {

        }

        public bool Cut(Vector2 cutPosition, Vector2 direction, int gapWidth)
        {
            Vector2[] arrayResults = Scissors.CalcCuts(Position, new Vector2(Width, Height), cutPosition, direction, gapWidth); //cuts gives where the new block should exist, we want where it should not
            Vector2 p1, p2; //these points represent the cut area
            if (direction.X != 0) //horizontal cut, vertical gap
            {
                p1 = arrayResults[0] + new Vector2(0, (arrayResults[2].Y > 0 ? arrayResults[2].Y : 0));
                p2 = arrayResults[1] + new Vector2((arrayResults[3].X < Width ? arrayResults[3].X : Width), 0);
            } else //vertical cut, horizontal gap
            {
                p1 = arrayResults[0] + new Vector2((arrayResults[2].X > 0 ? arrayResults[2].X : 0), 0);
                p2 = arrayResults[1] + new Vector2(0, (arrayResults[3].Y < Height ? arrayResults[3].Y : Height));
            }
            p1 -= Position;
            p2 -= Position;
            p1 /= 8;
            p2 /= 8;
            Logger.Log(LogLevel.Error, "LylyraHelper", String.Format("arrayresults pos1({0} {1}) arrayresults pos2({2} {3}) arrayresults size1({4} {5}) arrayresultssize2({6} {7}) position({8} {9}) p1({10} {11}) p2({12} {13}) Width/height: ({14} {15})", arrayResults[0].X, arrayResults[0].Y, arrayResults[1].X, arrayResults[1].Y, arrayResults[2].X, arrayResults[2].Y, arrayResults[3].X, arrayResults[3].Y, Position.X, Position.Y, p1.X, p1.Y, p2.X, p2.Y, Width, Height));
            for(int i = (int) p1.X; i < p2.X; i++)
            {
                for (int j = (int)p1.Y; j < p2.Y; j++)
                {
                    if (i >= 0 && j >= 0 && i < (int)Width / 8 && j < (int)Height / 8)
                    {
                        skip[i, j] = true;
                        holeTiles[i, j] = holeEmpty[0];
                    }
                }
            }

            return true;
        }
    }
}
