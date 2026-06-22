using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Numerics;
using Celeste.Mod.LylyraHelper.Other.Helpers;
using Celeste.Mod.Backdrops;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Vector3 = Microsoft.Xna.Framework.Vector3;
using Vector4 = Microsoft.Xna.Framework.Vector4;

namespace Celeste.Mod.LylyraHelper.Effects.SecretSanta;

[CustomBackdrop("LylyraHelper/SS2024/AtmosphericWind")]
public class AtmosphericWind : Backdrop
{
    public class Wind
    {
        private static Vector2 endPoint;
        public float thicc;
        public Vector2[] curve;
        public float percent = 75F;
        public Vector2 startingPoint;
        public int pointsPerWind;
        public Vector2 startingCamera;
        public VertexPositionNormalTexture[] Vertices;
        public WindBuilder builder;

        public static Wind MakeWind(Vector2 startingPoint, Vector2 startingCamera, Random rand, float initAngle,
            float speed, float twist, float bend, int pointsPerWind, float maxBend)
        {
            initAngle *= (float)Math.PI / 180F;
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
            wind.builder = new WindBuilder(wind);

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
                var num1 =
                    Calc.Clamp(percent - indexWeight + indexWeight * (pointsPerWind - v) / pointsPerWind, 0, 100) / 100;
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
    public AtmosphericWind(float initAngle, float angleVariance, float speed, float speedVariance, float twist,
        float bend, float frequency, float lifespan, float maxBend, int pointsPointWind)
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
        hsvBlending = child.AttrBool("hsvBlending", true) && !color.Equals(fadeColor); //turn off hsv blending if the colors are equal because its more computationally costly
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

    private static int MAXBUILD = 100;

    public override void Update(Scene scene)
    {
        base.Update(scene);
        if (!Visible) return;
        Level level = scene as Level;

        Player player = level.Tracker.GetEntity<Player>();
        if (player == null) return;
        int vertexCounter = 0;
        List<Wind> oldWinds = new List<Wind>();
        Vector2 cameraPosition = -level.Camera.Position;
        foreach (Wind wind in winds)
        {
            if (wind.Vertices != null)
            {
                wind.percent += Engine.DeltaTime * 75 * 6.5F / windLifespan;
            }
            else
            {
                if (wind.builder.Build(MAXBUILD, level))
                {
                    wind.Vertices = wind.builder.Vertices;
                }
            }
            if (wind.percent > MAXPERCENT)
            {
                oldWinds.Add(wind);
            }
        }

        winds.RemoveAll(w => { return oldWinds.Contains(w); }
        );
        windCounter += Engine.DeltaTime;
        if (windCounter > 1.0F / frequency)
        {
            windCounter = 0;
            float angle = initAngle + rand.NextFloat() * angleVariance - angleVariance / 2F;
            var pointAtScreenEdge = EllipseHelper.PointOnEllipseFromAngle(screenDimensions, angle);
            float pointRangeTLLength =
                (float)(320 -
                        140 * Math.Cos(
                            angle)); //what we should do is map the ellipse to a rectangle using parametric coordinates
            var pointRangeOnTangentLine = EllipseHelper.TangentToEllipseAtPoint(screenDimensions, angle) *
                                          (rand.NextFloat() * pointRangeTLLength - pointRangeTLLength / 2);
            var normal = EllipseHelper.NormalToEllipseAtPoint(screenDimensions, angle);
            var randomStartingOffsetInDirectionOfWind = -normal * (rand.NextFloat() * screenDimensions.X / 2);
            var cameraPos = level.Camera.Position;
            Vector2 startPoint = screenDimensions / 2 + pointAtScreenEdge + pointRangeOnTangentLine +
                                 randomStartingOffsetInDirectionOfWind;
            Vector2 playerSpeed = player == null ? Vector2.Zero : player.Speed;

            float cosineInBase =
                (playerSpeed.X * normal.X + playerSpeed.Y * normal.Y); //normal.Length() = 1, guarenteed.

            float playerSpeedAdjustment = -cosineInBase;
            if (playerSpeedAdjustment < 0)
            {
                playerSpeedAdjustment = 0;
            }

            winds.Add(Wind.MakeWind(startPoint, level.Camera.Position, rand, angle + 180F,
                speed + rand.NextFloat() * speedVarience - speedVarience / 2 + playerSpeedAdjustment / 60, twist, bend,
                pointsPerWind, maxBend));
        }

        vertexCount = vertexCounter;
    }


    public override void Render(Scene scene)
    {
        base.Render(scene);
        if (!Visible) return;
        var prev = Engine.Graphics.GraphicsDevice.RasterizerState.CullMode;
        Engine.Graphics.GraphicsDevice.RasterizerState.CullMode = CullMode.None;
        var effect = LylyraHelperGFX.atmosphericWind;
        var technique = effect.Techniques[0];
        
            
            
        foreach (Wind wind in winds)
        {
            if (wind.Vertices != null)
            {
                ApplyEffect((Level)scene, effect, wind);
                foreach (var pass in technique.Passes)
                {
                    pass.Apply();
                    Engine.Graphics.GraphicsDevice.DrawUserPrimitives
                    (
                        PrimitiveType.TriangleList,
                        wind.Vertices, 0, wind.Vertices.Length / 3
                    );
                }

            }
        }

        Engine.Graphics.GraphicsDevice.RasterizerState.CullMode = prev;
    }
    
    public void ApplyEffect(Level level, Effect eff, Wind wind)
    {
        
        Vector2 vector = new Vector2(GameplayBuffers.Gameplay?.Width ?? 320, GameplayBuffers.Gameplay?.Height ?? 180);
        Matrix matrix = level.Camera.Matrix * Matrix.CreateTranslation(new Vector3(level.Camera.Position, 0f));
        matrix *= Matrix.CreateScale(1f / vector.X * 2f, (0f - 1f / vector.Y) * 2f, 1f);
        matrix *= Matrix.CreateTranslation(-1f, 1f, 0f); //why do we need this offset? I couldn't tell you. but we do.
        eff.Parameters["World"].SetValue(matrix);
        Vector2 CameraPosition = level.Camera.Position;
        
        eff.Parameters["cameraPos"].SetValue(new Vector4(CameraPosition.X, - CameraPosition.Y,  0f, 0f));
        
        
        Vector4 parallax = new Vector4((-scrollX) * (CameraPosition.X - wind.startingCamera.X),
            (-scrollY) * (CameraPosition.Y - wind.startingCamera.Y), 0f, 0f);
        
        Vector4 parallax2 = new Vector4(1f * (CameraPosition.X - wind.startingCamera.X),
            1f * (CameraPosition.Y - wind.startingCamera.Y), 0f, 0f);//natural offset due to gpu rendering now occurs
        eff.Parameters["parallax"].SetValue(parallax);

        parallax += parallax2;
        
        eff.Parameters["windPercent"]?.SetValue(wind.percent / MAXPERCENT);
        eff.Parameters["color"]?.SetValue(color.ToVector4());
        eff.Parameters["fadeColor"]?.SetValue(fadeColor.ToVector4());
        eff.Parameters["transparency"]?.SetValue(transparency);
        eff.Parameters["thickness"]?.SetValue(wind.thicc);
        eff.Parameters["pointsPerWind"]?.SetValue(wind.pointsPerWind);
        eff.Parameters["hsvBlending"]?.SetValue(hsvBlending ? 1f : 0f);
    }
}

public class WindBuilder
{
    public VertexPositionNormalTexture[] Vertices;
    public bool Done;
    private AtmosphericWind.Wind Wind;
    private int buildPoint;
    private int vertexCounter;

    public WindBuilder(AtmosphericWind.Wind wind)
    {
        Wind = wind;
        Vertices = new VertexPositionNormalTexture[Wind.curve.Length * 6];
    }

    public bool Build(int maxBuild, Level level)
    {
        /*
        Vector2 parallax = -new Vector2((1 - scrollX) * (cameraPosition.X + wind.startingCamera.X),
            (1 - scrollY) * (cameraPosition.Y + wind.startingCamera.Y));
        wind.percent += Engine.DeltaTime * 75 * 6.5F / windLifespan;*/

        Vector2 prevPoint = Wind.curve[buildPoint];
        Vector2 prevTangentPoint = Wind.curve[buildPoint + 1] - Wind.curve[buildPoint];
        Vector2 prevNormal = prevTangentPoint.Perpendicular().SafeNormalize();
        Vector2 prevV1 = prevPoint;
        Vector2 prevV2 = prevPoint;
        for (int i = buildPoint + 1; i < Math.Min(Wind.curve.Length - 1, buildPoint + maxBuild); i++)
        {
            Vector2 nextPoint = Wind.curve[i];
            Vector2 nextTangentPoint = Wind.curve[i] - Wind.curve[i - 1];
            Vector2 nextNormal = nextTangentPoint.Perpendicular().SafeNormalize();
            Vector2 nextV1 = nextPoint + nextNormal * 0.0f;
            Vector2 nextV2 = nextPoint - nextNormal * 0.0f;

            VertexPositionNormalTexture vertexPositionColor =
                new VertexPositionNormalTexture(new Vector3(prevV1, 0f), new Vector3(-prevNormal, i / (float)Wind.curve.Length), Vector2.Zero);
            VertexPositionNormalTexture vertexPositionColor2 =
                new VertexPositionNormalTexture(new Vector3(prevV2, 0f), new Vector3(prevNormal, i / (float)Wind.curve.Length), Vector2.Zero);
            VertexPositionNormalTexture vertexPositionColor3 =
                new VertexPositionNormalTexture(new Vector3(nextV1, 0f), new Vector3(-nextNormal, i / (float)Wind.curve.Length), Vector2.Zero);
            VertexPositionNormalTexture vertexPositionColor4 =
                new VertexPositionNormalTexture(new Vector3(nextV2, 0f), new Vector3(nextNormal, i / (float)Wind.curve.Length), Vector2.Zero);

            Vertices[vertexCounter++] = vertexPositionColor;
            Vertices[vertexCounter++] = vertexPositionColor2;
            Vertices[vertexCounter++] = vertexPositionColor3;

            Vertices[vertexCounter++] = vertexPositionColor2;
            Vertices[vertexCounter++] = vertexPositionColor3;
            Vertices[vertexCounter++] = vertexPositionColor4;

            prevV1 = nextV1;
            prevV2 = nextV2;
            prevNormal = nextNormal;

        }

        buildPoint += maxBuild - 1;

        return buildPoint >= Wind.curve.Length;
    }
}