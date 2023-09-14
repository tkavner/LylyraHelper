using Celeste;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static Celeste.TrackSpinner;

namespace LylyraHelper.Effects
{
    public class ASHWind : Backdrop
    {
        private class Wind
        {
            internal static int Points = 300;
            private static Vector2 endPoint;
            private float thicc;
            public Vector2[] curve;
            public float percent;
            public Vector2 startingPoint;

            public static Wind MakeWind(Vector2 startingPoint, Random rand, float initAngle, float speed, float twist, float bend)
            {
                initAngle *= (float)Math.PI / 180;
                Wind wind = new Wind();
                Vector2[] curve = wind.curve = new Vector2[Points];
                Vector2 nextPoint = wind.startingPoint = startingPoint;
                float nextAngle = initAngle;
                List<Vector2> points = new List<Vector2>();
                for (int i = 0; i < Points; i++)
                {
                    points.Add(nextPoint);
                    nextPoint += new Vector2((float)Math.Cos(nextAngle), (float)Math.Sin(nextAngle)) * speed;
                    nextAngle += bend;
                    bend += rand.NextFloat() * twist - twist / 2;

                }
                wind.curve = points.ToArray();

                wind.thicc = rand.Next(10) == 0 ? 2 : 1;
                endPoint = nextPoint;


                return wind;
            }

            internal float GetHeight(int v)
            {
                int indexWeight = 100;
                //percents are out of a 200 point system right?! this actually occurs at 150% progress on our 200 point system
                if (percent - indexWeight + indexWeight * (Points - v) / Points > 100)
                {
                    var num1 = Calc.Clamp(200 - percent + (indexWeight * v / Points), 0, 100) / 100;
                    num1 = num1 < 0.3 ? 0 : num1;
                    num1 = num1 > 0.5 ? 0.5F : num1;
                    return num1 * thicc;
                } 
                else
                {
                    var num1 = Calc.Clamp(percent - indexWeight + indexWeight * (Points - v) / Points, 0, 100) / 100;
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
        public ASHWind(Vector2 startingPoint, int numWinds, float initAngle, float angleVariance, float speed, float speedVariance, float twist, float bend, float frequency, float lifespan)
        {
            rand = new Random();
            this.numWinds = numWinds;
            this.initAngle = initAngle;
            this.angleVarience = angleVariance;
            this.speed = speed;
            this.twist = twist;
            this.bend = bend;
            this.frequency = frequency;
            this.speedVarience = speedVariance;
            this.windLifespan = lifespan;
            int vertecies = GetVertecies();
            vertices = new VertexPositionColor[vertecies];
        }



        private VertexPositionColor[] vertices;

        private int GetVertecies()
        {
            return (int) (Math.Ceiling(frequency) * Math.Ceiling(windLifespan) * 3 * (Wind.Points * 2 - 2));
        }

        private List<Wind> winds = new List<Wind>();
        private int vertexCount;
        private float windCounter;
        private Random rand;
        private int numWinds;
        private float twist;
        private float bend;
        private float initAngle;
        private float speed;
        private float frequency;

        private float windLifespan = 7.5F;
        private float MAXPERCENT = 250; //percents are out of 500 now
        private float angleVarience = 0.3F;
        private float speedVarience;
        private Vector2 screenDimensions = new Vector2(384, 244);

        public override void Update(Scene scene)
        {
            Level level = scene as Level;
            bool flag = IsVisible(level);

            Player entity = level.Tracker.GetEntity<Player>();
            Vector2 vector = Calc.AngleToVector(-1.67079639f, 1f); //probably supposed to be -pi/2. but isn't that isn't that just <0,-1>?
            Vector2 vector2 = new Vector2(0f - vector.Y, vector.X);
            int vertexCounter = 0;
            List<Wind> oldWinds = new List<Wind>();
            foreach (Wind wind in winds)
            {
                wind.percent += Engine.DeltaTime * 75 * 6.5F / windLifespan;
                Vector2 startingPoint = wind.startingPoint;
                float xRenderPosBase = level.Camera.X; //this gun need to change
                float yRenderPosBase = level.Camera.Y;
                Vector2 vector3 = new Vector2(0, 0);


                Vector2 prevPoint = wind.curve[0];
                Vector2 prevTangentPoint = wind.curve[1] - wind.curve[0];
                Vector2 prevV1 = prevPoint + wind.GetHeight(0) * prevTangentPoint.Perpendicular().SafeNormalize();
                Vector2 prevV2 = prevPoint - wind.GetHeight(0) * prevTangentPoint.Perpendicular().SafeNormalize();
                for (int i = 1; i < wind.curve.Length - 1; i++)
                {
                    Vector2 nextPoint = wind.curve[i];
                    Vector2 nextTangentPoint = wind.curve[i] - wind.curve[i  - 1];
                    Vector2 nextV1 = nextPoint + wind.GetHeight(i) * nextTangentPoint.Perpendicular().SafeNormalize();
                    Vector2 nextV2 = nextPoint - wind.GetHeight(i) * nextTangentPoint.Perpendicular().SafeNormalize();
                    float transparency = 0.3F;
                    if (wind.GetPercent() > 0.5F)
                    {
                        transparency = transparency - (wind.GetPercent() - 0.5F) * transparency / (1 - 0.5F);
                    }
                    
                    VertexPositionColor vertexPositionColor = new VertexPositionColor(new Vector3(prevV1 + vector3, 0f), Color.White * transparency);
                    VertexPositionColor vertexPositionColor2 = new VertexPositionColor(new Vector3(prevV2 + vector3, 0f), Color.White * transparency);
                    VertexPositionColor vertexPositionColor3 = new VertexPositionColor(new Vector3(nextV1 + vector3, 0f), Color.White * transparency);
                    VertexPositionColor vertexPositionColor4 = new VertexPositionColor(new Vector3(nextV2 + vector3, 0f), Color.White * transparency);

                    vertices[vertexCounter++] = vertexPositionColor;
                    vertices[vertexCounter++] = vertexPositionColor2;
                    vertices[vertexCounter++] = vertexPositionColor3;

                    vertices[vertexCounter++] = vertexPositionColor2;
                    vertices[vertexCounter++] = vertexPositionColor3;
                    vertices[vertexCounter++] = vertexPositionColor4;

                    prevV1 = nextV1;
                    prevV2 = nextV2;
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
                Vector2 startPoint = screenDimensions / 2+ num1 + num2;
                winds.Add(Wind.MakeWind(startPoint, rand, angle + 180F, speed + rand.NextFloat() * speedVarience - speedVarience / 2, twist, bend));
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
                angle *= (float) Math.PI / 180;
                return (new Vector2((float)Math.Tan(angle) * ellipseDims.X, -1 * ellipseDims.Y)).SafeNormalize();
            }
        }

        public static Vector2 PointOnEllipseFromAngle(Vector2 ellipseDims, float angle)
        {
            angle *= (float)Math.PI / 180;
            return new Vector2(ellipseDims.X * (float)Math.Cos(angle),  ellipseDims.Y * (float) Math.Sin(angle));
        }
    }
}
