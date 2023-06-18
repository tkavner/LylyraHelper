using Celeste.Mod.Backdrops;
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
    [CustomBackdrop("LylyraHelper/HexagonalGodray")]
    public class HexagonalGodray : Backdrop
    {
        private struct HexRay
        {
            public float X;

            public float Y;

            public float Percent;

            public float Duration;

            public float angle;

            public int length;

            public Vector2[] points;

            public void Reset(float minRotation, float maxRotation)
            {
                Percent = 0f;
                X = Calc.Random.NextFloat(384f);
                Y = Calc.Random.NextFloat(244f);
                Duration = 4f + Calc.Random.NextFloat() * 8f;
                float randRotate = minRotation - maxRotation + (Calc.Random.NextFloat() * maxRotation) * 2;
                angle = (float)(Math.PI / 180F * (randRotate));
                length = Calc.Random.Next(15, 22);

                if (points == null)
                    points = new Vector2[3];

                points[0] = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                points[1] = new Vector2((float)Math.Cos(angle + Math.PI / 3), (float)Math.Sin(angle + Math.PI / 3));
                points[2] = new Vector2((float)Math.Cos(angle + 2 * Math.PI / 3), (float)Math.Sin(angle + 2 * Math.PI / 3));
            }
        }


        private VertexPositionColor[] vertices;

        private int vertexCount;

        private Color rayColor = Calc.HexToColor("f52b63") * 0.5f;
        private HexRay[] rays;

        private float fade;

        private Color fadeColor;

        public float speedX;
        public float speedY;
        private bool hexLerp;
        private float minRotation;
        private float maxRotation;

        public HexagonalGodray(string color, string fadeToColor, int numRays, float speedx, float speedy, float minRotation, float maxRotation, bool hexLerp)
        {
            vertices = new VertexPositionColor[12 * numRays];
            rays = new HexRay[numRays];


            if (string.IsNullOrEmpty(fadeToColor))
            { //we could add an exception case for optimization.
                fadeToColor = color;
            }
            rayColor = Calc.HexToColor(color) * 0.5f;
            fadeColor = Calc.HexToColor(fadeToColor) * 0.5f;
            UseSpritebatch = false;
            speedX = speedx;
            speedY = speedy;
            this.hexLerp = hexLerp;

            this.minRotation = minRotation;
            this.maxRotation = Math.Max(0, maxRotation);

            for (int i = 0; i < rays.Length; i++)
            {
                rays[i] = new HexRay();
                rays[i].Reset(minRotation, maxRotation);
                rays[i].Percent = Calc.Random.NextFloat();
            }
        }

        public override void Update(Scene scene)
        {
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
                float angle = rays[i].angle;
                int length = rays[i].length;
                Vector2 vector3 = new Vector2((int)num2, (int)num3);

                Color color = Color.White;
                if (hexLerp)
                {
                    color = HSVLerp(rayColor, fadeColor, percent) * Ease.CubeInOut(Calc.Clamp(((percent < 0.5f) ? percent : (1f - percent)) * 2f, 0f, 1f)) * fade;
                } else
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

                //points of the vertecies of the hexagon
                Vector2 v0 = rays[i].points[0];
                Vector2 v1 = rays[i].points[1];
                Vector2 v2 = rays[i].points[2];

                VertexPositionColor vertexPositionColor = new VertexPositionColor(new Vector3(vector3 + v0 * length, 0f), color);
                VertexPositionColor vertexPositionColor2 = new VertexPositionColor(new Vector3(vector3 + v1 * length, 0f), color);
                VertexPositionColor vertexPositionColor3 = new VertexPositionColor(new Vector3(vector3 + v2 * length, 0f), color);
                VertexPositionColor vertexPositionColor4 = new VertexPositionColor(new Vector3(vector3 - v0 * length, 0f), color);
                VertexPositionColor vertexPositionColor5 = new VertexPositionColor(new Vector3(vector3 - v1 * length, 0f), color);
                VertexPositionColor vertexPositionColor6 = new VertexPositionColor(new Vector3(vector3 - v2 * length, 0f), color);

                vertices[num++] = vertexPositionColor;
                vertices[num++] = vertexPositionColor2;
                vertices[num++] = vertexPositionColor3;

                vertices[num++] = vertexPositionColor;
                vertices[num++] = vertexPositionColor3;
                vertices[num++] = vertexPositionColor4;

                vertices[num++] = vertexPositionColor;
                vertices[num++] = vertexPositionColor4;
                vertices[num++] = vertexPositionColor5;

                vertices[num++] = vertexPositionColor;
                vertices[num++] = vertexPositionColor5;
                vertices[num++] = vertexPositionColor6;
            }
            vertexCount = num;
        }

        private static float Mod(float x, float m)
        {
            return (x % m + m) % m;
        }

        private static Color HSVLerp(Color c1, Color c2, float amount)
        {
            Vector3 hsv1 = ColortoHSV(c1);
            Vector3 hsv2 = ColortoHSV(c2);
            
            Vector3 lerped = new Vector3(
                hsv1.X * amount + (1 - amount) * hsv2.X,
                hsv1.Y * amount + (1 - amount) * hsv2.Y,
                hsv1.Z * amount + (1 - amount) * hsv2.Z);
            Vector3 vector3 = HSVToRGB(lerped) * 2;
            Logger.Log("LylyraHelper", "" + vector3);
            return new Color(vector3) * 0.5F;
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
            float beta = v * (1 - (h - (float) Math.Floor((float) h) * s));
            float gamma = v * (1 - (1 - (h - (float)Math.Floor((float)h))) * s);
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
            return RGBToHSV(color.R, color.G, color.B, 255);
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
                h = (int)Mod((g - b) / delta, 6F);
            }
            else if (g == max) 
            {
                h = (int)Mod((b - r) / delta + 2, 6F);
            }
            else if (b == max)
            {
                h = (int)Mod((r - g) / delta + 4, 6F);
            }
            int hPrime = (int) (60 * h);
            float v = max;
            int vPrime = (int) (100 * v);
            float s = 0;
            if (v != 0)
            {
                s = delta / v;
            }
            int sPrime = (int)(s * 100);

            Logger.Log("LylyraHelper", "h: " + h + "\ts: " + s + "\tv: " + v);
            return new Vector3(h, s, v);
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

