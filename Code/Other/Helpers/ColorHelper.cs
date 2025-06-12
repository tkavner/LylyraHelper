using Microsoft.Xna.Framework;
using System;

namespace Celeste.Mod.LylyraHelper.Other.Helpers;

public class ColorHelper
{

    public static Color HSVLerp(Color c1, Color c2, float amount)
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

    public static Vector3 HSVToRGB(Vector3 vector)
    {
        return HSVToRGB(vector.X, vector.Y, vector.Z);
    }

    //adapted from https://mattlockyer.github.io/iat455/documents/rgb-hsv.pdf
    public static Vector3 HSVToRGB(float h, float s, float v)
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
    public static Vector3 ColortoHSV(Color color)
    {
        return RGBToHSV(color.R, color.G, color.B, 256);
    }

    //returns h: value between 0 and 6, s (value between 0 and 1, and v 
    public static Vector3 RGBToHSV(float r, float g, float b, float valueRange = 1)
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

    private static float Mod(float x, float m)
    {
        return (x % m + m) % m;
    }
}