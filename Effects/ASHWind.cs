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
        private struct Wind
        {
            internal static int Points = 25;
            private static Vector2 endPoint;
            public Vector2[] curve;
            public float percent;
            public Vector2 startingPoint;

            public static Wind MakeWind(Vector2 startingPoint, float initAngle, float speed, float twist, float bend)
            {
                Wind wind = new Wind();
                Vector2[] curve = wind.curve = new Vector2[Points];
                Vector2 nextPoint = wind.startingPoint = startingPoint;
                float nextAngle = initAngle;
                Random rand = new Random();
                for (int i = 0; i < Points; i++)
                {
                    curve[i] = new Vector2(startingPoint.X, startingPoint.Y);
                    nextPoint += new Vector2((float)Math.Cos(nextAngle), (float)Math.Sin(nextAngle)) * speed;
                    nextAngle += bend;
                    bend += rand.NextFloat() * twist - twist / 2;

                }
                endPoint = nextPoint;


                return wind;
            }

            internal float GetHeight(int v)
            {
                return 1;
            }
        }

        //twist = jerk / acceleration change, bend = starting acceleration
        public ASHWind(int numWinds, float initAngle, float speed, float twist, float bend)
        {
            for (int i = 0; i <= numWinds; i++)
            {
                winds.Add(Wind.MakeWind(new Vector2(), initAngle, speed, twist, bend));
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
            foreach (Wind wind in winds)
            {
                Vector2 startingPoint = wind.startingPoint;
                float xRenderPosBase = -32f + Mod(startingPoint.X - level.Camera.X * 0.9f, 384f); //this gun need to change
                float yRenderPosBase = -32f + Mod(startingPoint.Y - level.Camera.Y * 0.9f, 244f);
                Vector2 vector3 = new Vector2((int)xRenderPosBase, (int)yRenderPosBase);


                Vector2 prevPoint = wind.curve[0];
                Vector2 prevTangentPoint = wind.curve[1] - wind.curve[0];
                Vector2 prevV1 = prevPoint + wind.GetHeight(0) * prevTangentPoint.Perpendicular();
                Vector2 prevV2 = prevPoint + wind.GetHeight(0) * prevTangentPoint.Perpendicular();
                for (int i = 1; i < wind.curve.Length - 1; i++)
                {
                    Vector2 nextPoint = wind.curve[i];
                    Vector2 nextTangentPoint = wind.curve[1] - wind.curve[0];
                    Vector2 nextV1 = prevPoint + wind.GetHeight(i) * prevTangentPoint.Perpendicular();
                    Vector2 nextV2 = prevPoint + wind.GetHeight(i) * prevTangentPoint.Perpendicular();

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
                }
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
