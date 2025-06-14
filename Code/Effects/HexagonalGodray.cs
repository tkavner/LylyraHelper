﻿using Celeste.Mod.Backdrops;
using Celeste.Mod.LylyraHelper.Other.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;

// Adapted from Celeste.Godrays, most base code taken from said file
namespace Celeste.Mod.LylyraHelper.Effects;

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

        public float xExtend;

        public float yExtend;

        public void Reset(float minRotation, float maxRotation)
        {
            Percent = 0f;
            X = Calc.Random.NextFloat(384f + xExtend);
            Y = Calc.Random.NextFloat(244f + yExtend);
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
    private bool hsvBlending;
    private float minRotation;
    private float maxRotation;
    private float yExtend;
    private float xExtend;


    public HexagonalGodray(BinaryPacker.Element child) : this(child.Attr("color"), child.Attr("fadeColor"), child.AttrInt("numberOfRays"), child.AttrFloat("speedX"), 
        child.AttrFloat("speedY"), child.AttrFloat("rotation"), child.AttrFloat("rotationRandomness"), child.Attr("blendingMode", "HSV"), child.AttrFloat("renderBorderExtendX", 0F), child.AttrFloat("renderBorderExtendY", 0F))
    {

    }
           
    public HexagonalGodray(string color, string fadeToColor, int numRays, float speedx, float speedy, float minRotation, float maxRotation, string blendingMode, float xExtend, float yExtend)
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
        this.hsvBlending = blendingMode == "HSV";

        this.minRotation = minRotation;
        this.maxRotation = Math.Max(0, maxRotation);
        this.xExtend = xExtend;
        this.yExtend = yExtend;

        for (int i = 0; i < rays.Length; i++)
        {
            rays[i] = new HexRay();
            rays[i].xExtend = xExtend; //At this point, this is barely a struct. maybe a full class is justified at this point
            rays[i].yExtend = yExtend;
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
            float num2 = -32f + Mod(rays[i].X - level.Camera.X * 0.9f, 384f + xExtend);
            float num3 = -32f + Mod(rays[i].Y - level.Camera.Y * 0.9f, 244f + yExtend);
            float angle = rays[i].angle;
            int length = rays[i].length;
            Vector2 vector3 = new Vector2((int)num2, (int)num3);

            Color color = Color.White;
            if (hsvBlending)
            {
                color = ColorHelper.HSVLerp(rayColor, fadeColor, percent) * Ease.CubeInOut(Calc.Clamp(((percent < 0.5f) ? percent : (1f - percent)) * 2f, 0f, 1f)) * fade;
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


    public override void Render(Scene scene)
    {
        base.Render(scene);
        if (vertexCount > 0 && fade > 0f)
        {
            GFX.DrawVertices(Matrix.Identity, vertices, vertexCount);
        }
    }
}