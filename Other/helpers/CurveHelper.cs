using Celeste.Mod;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LylyraHelper.Other.Helpers
{
    public static class CurveHelper
    {
        public  class SpaceCurve
        {

            public SpaceCurve(Vector2[] points)
            {

            }
        }

        //https://en.wikipedia.org/wiki/Spline_interpolation
        //idk if ill ever use this but it is fun
        public class CubicSpline
        {
            public float[] a;
            public float[] b;
            public Vector2[] points;
            public float[] k; //slopes at points
            public CubicSpline(Vector2[] points, float startingSlop, float endingSlope) //k1 = starting slope, k2 = ending
            {
                if (points == null) throw new ArgumentNullException(nameof(points));
                this.points = points;
                a = new float[points.Length - 1];
                b = new float[points.Length - 1];
                k = new float[points.Length - 1];
                k[0] = startingSlop;
                for (int i = 1; i < points.Length - 1; i++)
                {
                    a[i - 1] = k[i - 1] * (points[i].X - points[i - 1].X) - (points[i].Y - points[i - 1].Y);
                    if (i != 1) k[i - 1] = (points[i + 1].Y - points[i].Y) / (points[i + 1].X - points[i].X) + (1 - 2*(0)) * a[i] / (points[i + 1].X - points[i].X);
                    b[i - 1] = -k[i - 1] * (points[i + 1].X - points[i - 1].X) + (points[i].Y - points[i - 1].Y);
                }
                k[k.Length - 1] = endingSlope;

            }

            //t from zero to 1
            public float Evaluate(float x)
            {
                if (x < points[0].X)
                {
                    return points[0].X - k[0] * (x - points[0].X); //temp return value
                }
                if (points[points.Length - 1].X < x)
                {
                    return points[points.Length - 1].X - k[k.Length - 1] * (x - points[points.Length - 1].X); //temp return value
                }
                float fi = 0; //binary serach goes here (int)(t * a.Length);
                int counter = 0;
                while (counter < Math.Log(points.Length) + 2)
                {
                    if (points[(int)((fi + 1F / (2 * (counter + 2))) * (points.Length))].X < x)
                    {
                        fi += 1F / (2 * (counter + 2));
                    }
                    counter++;
                }
                int i = (int)(fi * (points.Length));
                Logger.Log(LogLevel.Error, "LylyraHelper", "" + fi + "|" + i);
                float t = (x - points[i].X) / (points[i + 1].X - points[i].X);
                return (1 - t) * points[i].Y + points[i + 1].Y + t * (1 - t) * ((1 - t) * a[i] + t * b[i]);
            }

        }
    }
}
