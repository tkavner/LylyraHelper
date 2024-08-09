using Celeste.Mod.Backdrops;
using Celeste.Mod.LylyraHelper.Other.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// Adapted from Celeste.Godrays, most base code taken from said file
namespace Celeste.Mod.LylyraHelper.Effects
{
    [CustomBackdrop("LylyraHelper/StarGodray")]
    public class StarGodray : Backdrop
    {
        private struct StarRay
        {
            public float X;

            public float Y;

            public float Percent;

            public float Duration;

            public float angle0;

            public float angle1;

            public int length;

            public Vector2[] points;

            public void Reset(float minRotation, float maxRotation)
            {
                Percent = 0f;
                X = Calc.Random.NextFloat(384f);
                Y = Calc.Random.NextFloat(244f);
                Duration = 4f + Calc.Random.NextFloat() * 8f;
                float randRotate = minRotation - maxRotation + (Calc.Random.NextFloat() * maxRotation) * 2;
                angle0 = (float)(Math.PI / 180F * (randRotate));
                angle1 = (float)(Math.PI / 180F * (randRotate + 360 / 10));
                length = Calc.Random.Next(15, 22);

                if (points == null)
                    points = new Vector2[10];

                for (int i = 0; i < 5; i++)
                {
                    float num = 2 * i * (float) Math.PI / 5F;
                    points[0 + 2 * i] = new Vector2((float)Math.Cos(angle0 + num), (float)Math.Sin(angle0 + num));
                    points[1 + 2 * i] = 0.4F * new Vector2((float)Math.Cos(angle1 + num), (float)Math.Sin(angle1 + num));
                }
            }
        }


        private VertexPositionColor[] vertices;

        private int vertexCount;

        private Color rayColor = Calc.HexToColor("f52b63") * 0.5f;
        private StarRay[] rays;

        private float fade;

        private Color fadeColor;

        public float speedX;
        public float speedY;
        private bool hsvBlending;
        private float minRotation;
        private float maxRotation;
        
        public StarGodray(BinaryPacker.Element child) : this(child.Attr("color"), child.Attr("fadeColor"), child.AttrInt("numberOfRays"), child.AttrFloat("speedX"), child.AttrFloat("speedY"), 
            child.AttrFloat("rotation"), child.AttrFloat("rotationRandomness"), child.Attr("blendingMode", "HSV"))
        {

        }

        public StarGodray(string color, string fadeToColor, int numRays, float speedx, float speedy, float minRotation, float maxRotation, string blendingMode)
        {
            vertices = new VertexPositionColor[3 * 10 * numRays];
            rays = new StarRay[numRays];

            if (string.IsNullOrEmpty(fadeToColor))
            { //we could add an exception case for optimization.
                fadeToColor = color;
            }
            rayColor = Calc.HexToColor(color) * 0.5f;
            fadeColor = Calc.HexToColor(fadeToColor) * 0.5f;
            UseSpritebatch = false;
            speedX = speedx;
            speedY = speedy;
            this.hsvBlending = blendingMode == "HSV";

            this.minRotation = minRotation;
            this.maxRotation = Math.Max(0, maxRotation);

            for (int i = 0; i < rays.Length; i++)
            {
                rays[i] = new StarRay();
                rays[i].Reset(minRotation, maxRotation);
                rays[i].Percent = Calc.Random.NextFloat();
            }
        }

        public override void Update(Scene scene)
        {
            base.Update(scene);
            Level level = scene as Level;
            bool flag = IsVisible(level);
            fade = Calc.Approach(fade, flag ? 1 : 0, Engine.DeltaTime);
            Visible = fade > 0f;
            if (!Visible)
            {
                return;
            }
            Player entity = level.Tracker.GetEntity<Player>();
            Vector2 vector = Calc.AngleToVector(-1.67079639f, 1f); //probably supposed to be -pi/2. but isn't that isn't that just <0,-1>?
            Vector2 vector2 = new Vector2(0f - vector.Y, vector.X);
            int num = 0;
            for (int i = 0; i < rays.Length; i++)
            {
                if (rays[i].Percent >= 1f)
                {
                    rays[i].Reset(minRotation, maxRotation);
                }


                rays[i].Percent += Engine.DeltaTime / rays[i].Duration;

                rays[i].X += speedX * Engine.DeltaTime;
                rays[i].Y += speedY * Engine.DeltaTime;

                float percent = rays[i].Percent;
                float num2 = -32f + Mod(rays[i].X - level.Camera.X * 0.9f, 384f);
                float num3 = -32f + Mod(rays[i].Y - level.Camera.Y * 0.9f, 244f);
                int length = rays[i].length;
                Vector2 vector3 = new Vector2((int)num2, (int)num3);

                Color color = Color.White;
                if (hsvBlending)
                {
                    color = ColorHelper.HSVLerp(rayColor, fadeColor, percent) * Ease.CubeInOut(Calc.Clamp(((percent < 0.5f) ? percent : (1f - percent)) * 2f, 0f, 1f)) * fade;
                }
                else
                {
                    color = Color.Lerp(rayColor, fadeColor, percent) * Ease.CubeInOut(Calc.Clamp(((percent < 0.5f) ? percent : (1f - percent)) * 2f, 0f, 1f)) * fade;
                }
                if (entity != null)
                {
                    float num4 = (vector3 + level.Camera.Position - entity.Position).Length();
                    if (num4 < 64f)
                    {
                        color *= 0.25f + 0.75f * (num4 / 64f);
                    }
                }
                for (int j = 0; j < 5; j++)
                {
                    Vector2 center = new Vector2(0);
                    Vector2 v1 = rays[i].points[(0 + 2 * j) % 10];
                    Vector2 v2 = rays[i].points[(1 + 2 * j) % 10];
                    Vector2 v3 = rays[i].points[(2 + 2 * j) % 10];

                    //points of the vertecies of the star, in fifths
                    VertexPositionColor vertexPositionColor0 = new VertexPositionColor(new Vector3(vector3 + center * length, 0f), color);
                    VertexPositionColor vertexPositionColor1 = new VertexPositionColor(new Vector3(vector3 + v1 * length, 0f), color);
                    VertexPositionColor vertexPositionColor2 = new VertexPositionColor(new Vector3(vector3 + v2 * length, 0f), color);
                    VertexPositionColor vertexPositionColor3 = new VertexPositionColor(new Vector3(vector3 + v3 * length, 0f), color);


                    vertices[num++] = vertexPositionColor0;
                    vertices[num++] = vertexPositionColor1;
                    vertices[num++] = vertexPositionColor2;

                    vertices[num++] = vertexPositionColor0;
                    vertices[num++] = vertexPositionColor2;
                    vertices[num++] = vertexPositionColor3;
                }
            }
            vertexCount = num;
        }

        private static float Mod(float x, float m)
        {
            return (x % m + m) % m;
        }


        public override void Render(Scene scene)
        {
            if (vertexCount > 0 && fade > 0f)
            {
                GFX.DrawVertices(Matrix.Identity, vertices, vertexCount);
            }
        }
    }

}

