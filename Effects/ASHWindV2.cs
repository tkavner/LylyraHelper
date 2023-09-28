using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LylyraHelper.Other.Helpers;

namespace LylyraHelper.Effects
{
    public class ASHWindV2 : Backdrop
    {

        private class Wind
        {
            private static Vector2 endPoint;
            private float thicc;
            public float percent = 75F;
            public Vector2 startingPoint;
            public int pointsPerWind;
            public CurveHelper.CubicSpline xSpline;
            public CurveHelper.CubicSpline ySpline;

            public static Wind MakeWind(Vector2 startingPoint, Random rand, float initAngle, float speed, float twist, float bend, int pointsPerWind, float maxBend)
            {
                initAngle *= (float)Math.PI / 180;
                Wind wind = new Wind();
                wind.pointsPerWind = pointsPerWind;
                Vector2[] curve = new Vector2[pointsPerWind];
                Vector2 nextPoint = wind.startingPoint = startingPoint;
                float nextAngle = initAngle;
                float nextBend = bend;
                List<Vector2> points = new List<Vector2>();
                List<Vector2> xOnly = new List<Vector2>();
                List<Vector2> yOnly = new List<Vector2>();
                for (int i = 0; i < pointsPerWind / 2; i++)
                {
                    points.Add(nextPoint);
                    xOnly.Add(new Vector2(pointsPerWind / 2 + i, nextPoint.X));
                    yOnly.Add(new Vector2(pointsPerWind / 2 + i, nextPoint.Y));
                    nextPoint += new Vector2((float)Math.Cos(nextAngle), (float)Math.Sin(nextAngle)) * speed;
                    nextAngle += nextBend;
                    nextBend = Calc.Clamp(nextBend, -maxBend, maxBend);
                    nextBend += rand.NextFloat() * twist - twist / 2;

                }

                nextPoint = startingPoint;
                nextAngle = initAngle;
                nextBend = bend;
                for (int i = 0; i < pointsPerWind / 2; i++)
                {
                    nextPoint -= new Vector2((float)Math.Cos(nextAngle), (float)Math.Sin(nextAngle)) * speed;
                    nextAngle -= nextBend;
                    nextBend -= rand.NextFloat() * twist - twist / 2;
                    nextBend = Calc.Clamp(nextBend, -maxBend, maxBend);
                    points.Insert(0, nextPoint);
                    xOnly.Insert(0, new Vector2(pointsPerWind / 2 - i - 1, nextPoint.X));
                    yOnly.Insert(0, new Vector2(pointsPerWind / 2 - i - 1, nextPoint.Y));
                }
                wind.xSpline = new CurveHelper.CubicSpline(xOnly.ToArray(), 1, -1);
                wind.ySpline = new CurveHelper.CubicSpline(yOnly.ToArray(), 1, -1);

                wind.thicc = rand.Next(10) == 0 ? 2 : 1;
                endPoint = nextPoint;


                return wind;
            }

            internal float GetHeight(int v)
            {
                int indexWeight = 100;
                //percents are out of a 200 point system right?! this actually occurs at 150% progress on our 200 point system
                if (percent - indexWeight + indexWeight * (pointsPerWind - v) / pointsPerWind > 100)
                {
                    var num1 = Calc.Clamp(200 - percent + (indexWeight * v / pointsPerWind), 0, 100) / 100;
                    num1 = num1 < 0.3 ? 0 : num1;
                    num1 = num1 > 0.5 ? 0.5F : num1;
                    return num1 * thicc;
                }
                else
                {
                    var num1 = Calc.Clamp(percent - indexWeight + indexWeight * (pointsPerWind - v) / pointsPerWind, 0, 100) / 100;
                    num1 = num1 < 0.3 ? 0 : num1;
                    num1 = num1 > 0.5 ? 0.5F : num1;
                    return num1 * thicc;
                }
            }

            public float GetPercent()
            {
                return (percent) / 250;
            }
        }

        //twist = jerk / acceleration change, bend = starting acceleration
        public ASHWindV2(float initAngle, float angleVariance, float speed, float speedVariance, float twist, float bend, float frequency, float lifespan, float maxBend, int pointsPointWind)
        {
            rand = Calc.Random;
            this.initAngle = initAngle;
            this.angleVarience = angleVariance;
            this.speed = speed;
            this.twist = twist;
            this.bend = bend;
            this.frequency = frequency;
            this.speedVarience = speedVariance;
            this.windLifespan = lifespan;
            this.maxBend = maxBend;
            this.pointsPerWind = pointsPointWind;
            int vertecies = GetVertecies();
            vertices = new VertexPositionColor[vertecies];
        }

        public ASHWindV2(BinaryPacker.Element child)
        {
            rand = Calc.Random;
            initAngle = child.AttrFloat("initAngle");
            angleVarience = child.AttrFloat("angleVariance");
            speed = child.AttrFloat("speed");
            speedVarience = child.AttrFloat("speedVariance");
            twist = child.AttrFloat("twist");
            bend = child.AttrFloat("bend");
            frequency = child.AttrFloat("frequency", 3F);
            windLifespan = child.AttrFloat("windLifespan", 7.5F);
            maxBend = child.AttrFloat("maxBend", 0.01F);
            pointsPerWind = child.AttrInt("pointsPerWind", 10);
            color = Calc.HexToColor(child.Attr("color", "FFFFFF"));
            fadeColor = Calc.HexToColor(child.Attr("fadeColor", "FFFFFF"));
            transparency = child.AttrFloat("transparency", 0.3F);

            hsvBlending = child.AttrBool("hsvBlending", false);
            int vertecies = GetVertecies();
            vertices = new VertexPositionColor[vertecies];
        }

        private VertexPositionColor[] vertices;

        private int GetVertecies()
        {
            return (int)(Math.Ceiling(frequency) * Math.Ceiling(windLifespan) * 3 * (pointsPerWind * interpols * 2 - 2));
        }

        internal int pointsPerWind = 4;
        private Color color;
        private Color fadeColor;
        private float transparency;
        private List<Wind> winds = new List<Wind>();
        private int vertexCount;
        private float windCounter;
        private Random rand;
        private float twist;
        private float bend;
        private float initAngle;
        private float speed;
        private float frequency;

        private float windLifespan = 7.5F;
        private float maxBend;
        private float MAXPERCENT = 250; //percents are out of 500 now
        private float angleVarience = 0.0F;
        private float speedVarience;
        private Vector2 screenDimensions = new Vector2(384, 244);
        private bool hsvBlending;
        private float interpols = 10;

        public override void Update(Scene scene)
        {
            Level level = scene as Level;
            bool flag = IsVisible(level);

            Player entity = level.Tracker.GetEntity<Player>();
            Vector2 vector = Calc.AngleToVector(-1.67079639f, 1f); //probably supposed to be -pi/2. but isn't that isn't that just <0,-1>?
            Vector2 vector2 = new Vector2(0f - vector.Y, vector.X);
            int vertexCounter = 0;
            List<Wind> oldWinds = new List<Wind>();
            Vector2 vector3 = -level.Camera.Position;
            foreach (Wind wind in winds)
            {
                wind.percent += Engine.DeltaTime * 75 * 6.5F / windLifespan;
                Vector2 startingPoint = wind.startingPoint;
                float xRenderPosBase = level.Camera.X; //this gun need to change
                float yRenderPosBase = level.Camera.Y;


                Vector2 prevPoint = new Vector2(wind.xSpline.Evaluate(0), wind.ySpline.Evaluate(0));
                Vector2 prevTangentPoint = new Vector2(wind.ySpline.Evaluate(1), wind.ySpline.Evaluate(1)) - prevPoint;
                Vector2 prevV1 = prevPoint + wind.GetHeight(0) * prevTangentPoint.Perpendicular().SafeNormalize();
                Vector2 prevV2 = prevPoint - wind.GetHeight(0) * prevTangentPoint.Perpendicular().SafeNormalize();
                for (int i = 1; i < wind.pointsPerWind * interpols; i++)
                {
                    Vector2 nextPoint = new Vector2(wind.xSpline.Evaluate(i / interpols + 1), wind.ySpline.Evaluate(i / interpols + 1));
                    Vector2 nextTangentPoint = nextPoint - prevPoint;
                    Vector2 nextV1 = nextPoint + 1 * nextTangentPoint.Perpendicular().SafeNormalize();
                    Vector2 nextV2 = nextPoint - 1 * nextTangentPoint.Perpendicular().SafeNormalize();
                    float transparency = this.transparency;
                    if (wind.GetPercent() / MAXPERCENT > 0.5F)
                    {
                        transparency = transparency - (wind.GetPercent() / MAXPERCENT - 0.5F) * transparency / (1 - 0.5F);
                    }

                    Color windColor = Color.White;
                    if (hsvBlending)
                    {
                        //windColor = HSVLerp(color, fadeColor, wind.percent / MAXPERCENT) * Ease.CubeInOut(Calc.Clamp(((wind.percent / MAXPERCENT < 0.5f) ? wind.percent / MAXPERCENT : (1f - wind.percent / MAXPERCENT)) * 2f, 0f, 1f));
                    }
                    else
                    {
                        //windColor = Color.Lerp(color, fadeColor, wind.percent / MAXPERCENT) * Ease.CubeInOut(Calc.Clamp(((wind.percent / MAXPERCENT < 0.5f) ? wind.percent / MAXPERCENT : (1f - wind.percent / MAXPERCENT)) * 2f, 0f, 1f));
                    }

                    VertexPositionColor vertexPositionColor = new VertexPositionColor(new Vector3(prevV1 + vector3, 0f), windColor * 1);
                    VertexPositionColor vertexPositionColor2 = new VertexPositionColor(new Vector3(prevV2 + vector3, 0f), windColor * 1);
                    VertexPositionColor vertexPositionColor3 = new VertexPositionColor(new Vector3(nextV1 + vector3, 0f), windColor * 1);
                    VertexPositionColor vertexPositionColor4 = new VertexPositionColor(new Vector3(nextV2 + vector3, 0f), windColor * 1);

                    vertices[vertexCounter++] = vertexPositionColor;
                    vertices[vertexCounter++] = vertexPositionColor2;
                    vertices[vertexCounter++] = vertexPositionColor3;

                    vertices[vertexCounter++] = vertexPositionColor2;
                    vertices[vertexCounter++] = vertexPositionColor3;
                    vertices[vertexCounter++] = vertexPositionColor4;

                    prevV1 = nextV1;
                    prevV2 = nextV2;
                    prevPoint = nextPoint;
                }
                if (wind.percent > MAXPERCENT)
                {
                    oldWinds.Add(wind);
                }
            }
            winds.RemoveAll(w =>
            {
                return oldWinds.Contains(w);
            }
            );
            windCounter += Engine.DeltaTime;
            if (windCounter > 1 / frequency)
            {
                windCounter = 0;
                float angle = initAngle + rand.NextFloat() * angleVarience - angleVarience / 2F;
                var num1 = PointOnEllipseFromAngle(screenDimensions, angle);
                var num2 = TangentToEllipseAtPoint(screenDimensions, angle) * (rand.NextFloat() * 180 - 90);
                var normal = NormalToEllipseAtPoint(screenDimensions, angle);
                var num4 = -normal * (rand.NextFloat() * screenDimensions.X / 2);
                var num3 = level.Camera.Position;
                Vector2 startPoint = screenDimensions / 2 + num1 + num2 + num3 + num4;
                Player player = scene.Tracker.GetEntity<Player>();
                Vector2 playerSpeed = player == null ? Vector2.Zero : player.Speed;

                float cosineInBase = (playerSpeed.X * normal.X + playerSpeed.Y * normal.Y);//normal.Length() = 1, guarenteed.

                float playerSpeedAdjustment = -cosineInBase;
                if (playerSpeedAdjustment < 0) { playerSpeedAdjustment = 0; }
                winds.Add(Wind.MakeWind(startPoint, rand, angle + 180F, speed + rand.NextFloat() * speedVarience - speedVarience / 2 + playerSpeedAdjustment / 60, twist, bend, pointsPerWind, maxBend));
            }
            vertexCount = vertexCounter;

        }



        private static float Mod(float x, float m)
        {
            return (x % m + m) % m;
        }

        public override void Render(Scene scene)
        {
            if (vertexCount > 0)
            {
                GFX.DrawVertices(Matrix.Identity, vertices, vertexCount);
            }
        }

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

        private static Color HSVLerp(Color c1, Color c2, float amount)
        {
            amount = MathHelper.Clamp(amount, 0f, 1f);
            Vector3 hsv1 = ColortoHSV(c1);
            Vector3 hsv2 = ColortoHSV(c2);
            if (Math.Abs(hsv2.X - hsv1.X) > 3)
            {
                if (hsv2.X > hsv1.X)
                {
                    hsv1.X += 6;
                }
                else
                {
                    hsv2.X += 6;
                }
            }
            Vector3 lerped = new Vector3(
                hsv1.X * amount + (1 - amount) * hsv2.X,
                hsv1.Y * amount + (1 - amount) * hsv2.Y,
                hsv1.Z * amount + (1 - amount) * hsv2.Z);
            Vector3 vector3 = HSVToRGB(lerped);
            vector3.X = Mod(vector3.X, 6);
            return new Color(vector3.X, vector3.Y, vector3.Z, 0.5F);
        }

        private static Vector3 HSVToRGB(Vector3 vector)
        {
            return HSVToRGB(vector.X, vector.Y, vector.Z);
        }

        //adapted from https://mattlockyer.github.io/iat455/documents/rgb-hsv.pdf
        private static Vector3 HSVToRGB(float h, float s, float v)
        {
            if (h > 6) h -= 6; if (s > 1) s = 1; if (v > 1) v = 1;
            if (h < 0) h += 6;
            float alpha = v * (1 - s);
            float beta = v * (1 - (Mod(h, 1) * s));
            float gamma = v * (1 - (1 - Mod(h, 1)) * s);
            if (0 <= h && h < 1) return new Vector3(v, gamma, alpha);
            else if (1 <= h && h < 2) return new Vector3(beta, v, alpha);
            else if (2 <= h && h < 3) return new Vector3(alpha, v, gamma);
            else if (3 <= h && h < 4) return new Vector3(alpha, beta, v);
            else if (4 <= h && h < 5) return new Vector3(gamma, alpha, v);
            else if (5 <= h && h < 6) return new Vector3(v, alpha, beta);
            return new Vector3(v, v, v);
        }
        private static Vector3 ColortoHSV(Color color)
        {
            return RGBToHSV(color.R, color.G, color.B, 256);
        }

        //returns h: value between 0 and 6, s (value between 0 and 1, and v 
        private static Vector3 RGBToHSV(float r, float g, float b, float valueRange = 1)
        {
            r /= valueRange;
            g /= valueRange;
            b /= valueRange;
            float max = Math.Max(r, Math.Max(g, b));
            float min = Math.Min(r, Math.Min(g, b));
            float h = 0;
            float delta = max - min;
            if (Math.Abs(delta) < float.Epsilon)
            {

            }
            else if (r == max)
            {
                h = Mod((g - b) / delta, 6F);
            }
            else if (g == max)
            {
                h = Mod((b - r) / delta + 2, 6F);
            }
            else if (b == max)
            {
                h = Mod((r - g) / delta + 4, 6F);
            }
            float v = max;
            float s = 0;
            if (v != 0)
            {
                s = delta / v;
            }

            return new Vector3(h, s, v);
        }
    }
}
