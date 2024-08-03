using Celeste.Mod.Backdrops;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.LylyraHelper.Other.Helpers;
using Celeste.Mod.LylyraHelper.Code.Other.Helpers;

namespace Celeste.Mod.LylyraHelper.Code.Effects.SecretSanta
{
    [CustomBackdrop("LylyraHelper/SS2024/AtmosphericWind")]
    public class AtmosphericWind : Backdrop
    {
        private class Wind
        {
            private static Vector2 endPoint;
            private float thicc;
            public Vector2[] curve;
            public float percent = 75F;
            public Vector2 startingPoint;
            public int pointsPerWind;
            public Vector2 startingCamera;

            public static Wind MakeWind(Vector2 startingPoint, Vector2 startingCamera, Random rand, float initAngle, float speed, float twist, float bend, int pointsPerWind, float maxBend)
            {
                initAngle *= (float) Math.PI / 180F;
                Wind wind = new Wind();
                wind.pointsPerWind = pointsPerWind;
                Vector2[] curve = wind.curve = new Vector2[pointsPerWind];
                Vector2 nextPoint = wind.startingPoint = startingPoint;
                wind.startingCamera = startingCamera;
                float nextAngle = initAngle;
                float nextBend = bend;
                List<Vector2> points = new List<Vector2>();
                for (int i = 0; i < pointsPerWind / 2; i++)
                {
                    points.Add(nextPoint);
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
        public AtmosphericWind(float initAngle, float angleVariance, float speed, float speedVariance, float twist, float bend, float frequency, float lifespan, float maxBend, int pointsPointWind)
        {
            rand = Calc.Random;
            this.initAngle = initAngle;
            this.angleVariance = angleVariance;
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

        public AtmosphericWind(BinaryPacker.Element child)
        {
            rand = Calc.Random;
            initAngle = child.AttrFloat("initAngle"); //in degrees
            angleVariance = child.AttrFloat("angleVariance"); // in degrees
            speed = child.AttrFloat("speed");
            speedVarience = child.AttrFloat("speedVariance");
            twist = Calc.ToRad(child.AttrFloat("angularJerk")); //in radians
            bend = Calc.ToRad(child.AttrFloat("startingAngularAcceleration")); //in radians
            frequency = child.AttrFloat("frequency", 3F);
            windLifespan = child.AttrFloat("windLifespan", 7.5F);
            maxBend = Calc.ToRad(child.AttrFloat("maxAngularAcceleration", 0.01F)); //in radians
            pointsPerWind = child.AttrInt("pointsPerWind", 600);
            color = Calc.HexToColor(child.Attr("color", "FFFFFF"));
            fadeColor = Calc.HexToColor(child.Attr("fadeColor", "FFFFFF"));
            transparency = child.AttrFloat("transparency", 0.3F);
            hsvBlending = child.AttrBool("hsvBlending", true);
            scrollX = child.AttrFloat("scrollX", 0.0F);
            scrollY = child.AttrFloat("scrollY", 0.0F);
            int vertecies = GetVertecies();
            vertices = new VertexPositionColor[vertecies];
        }

        private VertexPositionColor[] vertices;

        private int GetVertecies()
        {
            return (int)(Math.Ceiling(frequency) * Math.Ceiling(windLifespan) * 3 * (pointsPerWind * 2 - 2));
        }

        internal int pointsPerWind = 300;
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
        private float angleVariance = 0.0F;
        private float speedVarience;
        private Vector2 screenDimensions = new Vector2(384, 244);
        private bool hsvBlending;
        private float scrollX;
        private float scrollY;

        public override void Update(Scene scene)
        {

            Level level = scene as Level;

            Player player = level.Tracker.GetEntity<Player>();
            if (player == null) return;
            Vector2 vector = Calc.AngleToVector(-1.67079639f, 1f); //probably supposed to be -pi/2. but isn't that isn't that just <0,-1>?
            int vertexCounter = 0;
            List<Wind> oldWinds = new List<Wind>();
            Vector2 cameraPosition = -level.Camera.Position;
            foreach (Wind wind in winds)
            {
                Vector2 parallax = -new Vector2((1 - scrollX) * (cameraPosition.X + wind.startingCamera.X), (1 - scrollY) * (cameraPosition.Y + wind.startingCamera.Y));
                wind.percent += Engine.DeltaTime * 75 * 6.5F / windLifespan;
                Vector2 startingPoint = wind.startingPoint;
                float xRenderPosBase = level.Camera.X; //this gun need to change
                float yRenderPosBase = level.Camera.Y;


                Vector2 prevPoint = wind.curve[0];
                Vector2 prevTangentPoint = wind.curve[1] - wind.curve[0];
                Vector2 prevV1 = prevPoint + wind.GetHeight(0) * prevTangentPoint.Perpendicular().SafeNormalize();
                Vector2 prevV2 = prevPoint - wind.GetHeight(0) * prevTangentPoint.Perpendicular().SafeNormalize();
                for (int i = 1; i < wind.curve.Length - 1; i++)
                {
                    Vector2 nextPoint = wind.curve[i];
                    Vector2 nextTangentPoint = wind.curve[i] - wind.curve[i - 1];
                    Vector2 nextV1 = nextPoint + wind.GetHeight(i) * nextTangentPoint.Perpendicular().SafeNormalize();
                    Vector2 nextV2 = nextPoint - wind.GetHeight(i) * nextTangentPoint.Perpendicular().SafeNormalize();
                    float transparency = this.transparency;
                    if (wind.GetPercent() / MAXPERCENT > 0.5F)
                    {
                        transparency = transparency - (wind.GetPercent() / MAXPERCENT - 0.5F) * transparency / (1 - 0.5F);
                    }

                    Color windColor = Color.White;
                    if (hsvBlending)
                    {
                        windColor = ColorHelper.HSVLerp(color, fadeColor, wind.percent / MAXPERCENT) * Ease.CubeInOut(Calc.Clamp(((wind.percent / MAXPERCENT < 0.5f) ? wind.percent / MAXPERCENT : (1f - wind.percent / MAXPERCENT)) * 2f, 0f, 1f));
                    }
                    else
                    {
                        windColor = Color.Lerp(color, fadeColor, wind.percent / MAXPERCENT) * Ease.CubeInOut(Calc.Clamp(((wind.percent / MAXPERCENT < 0.5f) ? wind.percent / MAXPERCENT : (1f - wind.percent / MAXPERCENT)) * 2f, 0f, 1f));
                    }

                    VertexPositionColor vertexPositionColor = new VertexPositionColor(new Vector3(prevV1 + cameraPosition + parallax , 0f), windColor * transparency);
                    VertexPositionColor vertexPositionColor2 = new VertexPositionColor(new Vector3(prevV2 + cameraPosition + parallax, 0f), windColor * transparency);
                    VertexPositionColor vertexPositionColor3 = new VertexPositionColor(new Vector3(nextV1 + cameraPosition + parallax, 0f), windColor * transparency);
                    VertexPositionColor vertexPositionColor4 = new VertexPositionColor(new Vector3(nextV2 + cameraPosition + parallax, 0f), windColor * transparency);

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
            if (windCounter > 1.0F / frequency)
            {
                windCounter = 0;
                float angle = initAngle + rand.NextFloat() * angleVariance - angleVariance / 2F;
                var num1 = EllipseHelper.PointOnEllipseFromAngle(screenDimensions, angle);
                var pointRangeOnTangentLine = EllipseHelper.TangentToEllipseAtPoint(screenDimensions, angle) * (rand.NextFloat() * 180 - 90);
                var normal = EllipseHelper.NormalToEllipseAtPoint(screenDimensions, angle);
                var randomStartingOffsetInDirectionOfWind = -normal * (rand.NextFloat() * screenDimensions.X / 2);
                var cameraPos = level.Camera.Position;
                Vector2 startPoint = screenDimensions / 2 + num1 + pointRangeOnTangentLine + cameraPos + randomStartingOffsetInDirectionOfWind;
                Vector2 playerSpeed = player == null ? Vector2.Zero : player.Speed;

                float cosineInBase = (playerSpeed.X * normal.X + playerSpeed.Y * normal.Y);//normal.Length() = 1, guarenteed.

                float playerSpeedAdjustment = -cosineInBase;
                if (playerSpeedAdjustment < 0) { playerSpeedAdjustment = 0; }
                winds.Add(Wind.MakeWind(startPoint, level.Camera.Position, rand, angle + 180F, speed + rand.NextFloat() * speedVarience - speedVarience / 2 + playerSpeedAdjustment / 60, twist, bend, pointsPerWind, maxBend));
            }
            vertexCount = vertexCounter;

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
