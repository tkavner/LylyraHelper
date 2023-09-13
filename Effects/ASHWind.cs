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
            internal static int Points = 50;
            private static Vector2 endPoint;
            public Vector2[] curve;
            public float percent;
            public Vector2 startingPoint;

            public static Wind MakeWind(Vector2 startingPoint, Random rand, float initAngle, float speed, float twist, float bend)
            {
                Wind wind = new Wind();
                Vector2[] curve = wind.curve = new Vector2[Points];
                Vector2 nextPoint = wind.startingPoint = startingPoint;
                float nextAngle = initAngle;
                for (int i = 0; i < Points; i++)
                {
                    curve[i] = new Vector2(nextPoint.X, nextPoint.Y);
                    nextPoint += new Vector2((float)Math.Cos(nextAngle), (float)Math.Sin(nextAngle)) * speed;
                    nextAngle += bend;
                    bend += rand.NextFloat() * twist - twist / 2;

                }
                endPoint = nextPoint;


                return wind;
            }

            internal float GetHeight(int v)
            {

                if (percent - 100 + (50 - v) > 100)
                {
                    var num1 = Calc.Clamp(150 - percent + (50 + v), 0, 100) / 100;
                    num1 = num1 < 0.3 ? 0 : num1;
                    num1 = num1 > 0.5 ? 0.5F : num1;
                    return num1;
                } 
                else
                {
                    var num1 = Calc.Clamp(percent - 100 + (50 - v), 0, 100) / 100;
                    num1 = num1 < 0.3 ? 0 : num1;
                    num1 = num1 > 0.5 ? 0.5F : num1;
                    return num1;
                }
            }
        }

        //twist = jerk / acceleration change, bend = starting acceleration
        public ASHWind(Vector2 startingPoint, int numWinds, float initAngle, float speed, float twist, float bend, float frequency)
        {
            rand = new Random();
            this.numWinds = numWinds;
            this.initAngle = initAngle;
            this.speed = speed;
            this.twist = twist;
            this.bend = bend;
            this.frequency = frequency;
            for (int i = 0; i <= 2; i++)
            {
                winds.Add(Wind.MakeWind(new Vector2(380, rand.NextFloat() * 250), rand, initAngle, speed, twist, bend));
            }
        }



        private VertexPositionColor[] vertices;

        private int GetVertecies()
        {
            if (winds.Count == 0)
            {
                return 0;
            }
            return winds.Count * 3 * (Wind.Points * 2 - 2);
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

        public override void Update(Scene scene)
        {
            Level level = scene as Level;
            bool flag = IsVisible(level);

            Player entity = level.Tracker.GetEntity<Player>();
            Vector2 vector = Calc.AngleToVector(-1.67079639f, 1f); //probably supposed to be -pi/2. but isn't that isn't that just <0,-1>?
            Vector2 vector2 = new Vector2(0f - vector.Y, vector.X);
            int vertexCounter = 0;
            int vertecies = GetVertecies();
            vertices = new VertexPositionColor[vertecies];
            List<Wind> oldWinds = new List<Wind>();
            foreach (Wind wind in winds)
            {
                wind.percent += Engine.DeltaTime * 75;
                Vector2 startingPoint = wind.startingPoint;
                float xRenderPosBase = -32f + Mod(startingPoint.X - level.Camera.X * 0.9f, 384f); //this gun need to change
                float yRenderPosBase = -32f + Mod(startingPoint.Y - level.Camera.Y * 0.9f, 244f);
                Vector2 vector3 = new Vector2((int)xRenderPosBase, (int)yRenderPosBase);


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

                    VertexPositionColor vertexPositionColor = new VertexPositionColor(new Vector3(prevV1, 0f), Color.White);
                    VertexPositionColor vertexPositionColor2 = new VertexPositionColor(new Vector3(prevV2, 0f), Color.White);
                    VertexPositionColor vertexPositionColor3 = new VertexPositionColor(new Vector3(nextV1, 0f), Color.White);
                    VertexPositionColor vertexPositionColor4 = new VertexPositionColor(new Vector3(nextV2, 0f), Color.White);

                    vertices[vertexCounter++] = vertexPositionColor;
                    vertices[vertexCounter++] = vertexPositionColor2;
                    vertices[vertexCounter++] = vertexPositionColor3;

                    vertices[vertexCounter++] = vertexPositionColor2;
                    vertices[vertexCounter++] = vertexPositionColor3;
                    vertices[vertexCounter++] = vertexPositionColor4;

                    prevV1 = nextV1;
                    prevV2 = nextV2;
                }
                if (wind.percent > 500)
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
                winds.Add(Wind.MakeWind(new Vector2(380, rand.NextFloat() * 250), rand, initAngle, speed, twist, bend));
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
    }
}
