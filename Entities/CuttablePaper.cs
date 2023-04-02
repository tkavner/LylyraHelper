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

            p1 -= Position;
            p2 -= Position;
            p1 /= 8;
            p2 /= 8;

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
