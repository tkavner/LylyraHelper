using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.LylyraHelper.Code.Other.Helpers
{
    public static class EllipseHelper
    {

        //tangent of the tangent, technically
        //angle degrees
        public static Vector2 TangentToEllipseAtPoint(Vector2 ellipseDims, float angle)
        {
            if (Math.Abs(Mod(angle, 180) - 90) < float.Epsilon)
            {
                return new Vector2(1, 0);
            }
            else
            {
                angle *= (float)Math.PI / 180;
                return (new Vector2((float)Math.Tan(angle) * ellipseDims.X, -1 * ellipseDims.Y)).SafeNormalize();
            }
        }

        public static Vector2 NormalToEllipseAtPoint(Vector2 ellipseDims, float angle)
        {
            if (Math.Abs(Mod(angle, 180) - 90) < float.Epsilon)
            {
                return new Vector2(0, 1);
            }
            else
            {
                angle *= (float)Math.PI / 180;
                return (new Vector2((float)1 * ellipseDims.X, (float)Math.Tan(angle) * ellipseDims.Y)).SafeNormalize();
            }
        }

        public static Vector2 PointOnEllipseFromAngle(Vector2 ellipseDims, float angle)
        {
            angle *= (float)Math.PI / 180;
            return new Vector2(ellipseDims.X * (float)Math.Cos(angle), ellipseDims.Y * (float)Math.Sin(angle));
        }

        private static float Mod(float x, float m)
        {
            return (x % m + m) % m;
        }
    }
}
