#include "Common.fxh"

float4x4 World;
float4 cameraPos;
float4 parallax;
float windPercent;
float4 color;
float4 fadeColor;
float transparency;
float thickness;
float pointsPerWind;
float hsvBlending;

struct VertexShaderOutput
{
    float4 position : SV_Position;
};


float GetHeightOrig(float v)
{
    float percent = windPercent * 250;
    int indexWeight = 100;
    //percents are out of a 200 point system right?! this actually occurs at 150% progress on our 200 point system
    if (percent - indexWeight + indexWeight * (pointsPerWind - v * pointsPerWind) / pointsPerWind > 100)
    {
        float num1 = clamp(200 - percent + (indexWeight * v), 0, 100) / 100.0;
        if (num1 < 0.3) num1 = 0;
        if (num1 > 0.5) num1 = 0.5;
        return num1 * thickness;
    }
    else
    {
        float num1 = clamp(percent - indexWeight + indexWeight * (pointsPerWind - v * pointsPerWind) / pointsPerWind, 0, 100) / 100.0;

        if (num1 < 0.3) num1 = 0;
        if (num1 > 0.5) num1 = 0.5;
        return num1 * thickness;
    }
}

float GetHeight(float position) {
    float openingPercent = 0.2;
    float totalPercent = openingPercent * 2 + 1;
    float offset = -openingPercent;
    float curvespacePercent = windPercent * totalPercent + offset;
    float peakHeight = 1;
    float windPercentDiff = peakHeight - abs(curvespacePercent - position);
    return clamp(windPercentDiff, 0, 1) * thickness;
}

VertexShaderOutput VertexShaderFunction(float4 position:POSITION0, float3 normal: NORMAL0, float2 uv:TEXTURE0)
{
    VertexShaderOutput output;

    output.position = mul(position + float4(normal.xy * GetHeightOrig(normal.z) * 2.0, 0, 0), World);
    return output;
}

//////////////////////////////////////////////////////////////////////////
// Pixel Shader
//////////////////////////////////////////////////////////////////////////


//taken from here https://chilliant.com/rgb2hsv.html



float3 HUEtoRGB(in float H)
{
    float R = abs(H * 6 - 3) - 1;
    float G = 2 - abs(H * 6 - 2);
    float B = 2 - abs(H * 6 - 4);
    return saturate(float3(R,G,B));
}

float Epsilon = 1e-10;

float3 RGBtoHCV(in float3 RGB)
{
    // Based on work by Sam Hocevar and Emil Persson
    float4 P = (RGB.g < RGB.b) ? float4(RGB.bg, -1.0, 2.0/3.0) : float4(RGB.gb, 0.0, -1.0/3.0);
    float4 Q = (RGB.r < P.x) ? float4(P.xyw, RGB.r) : float4(RGB.r, P.yzx);
    float C = Q.x - min(Q.w, Q.y);
    float H = abs((Q.w - Q.y) / (6 * C + Epsilon) + Q.z);
    return float3(H, C, Q.x);
}

float3 RGBtoHSV(in float3 RGB)
{
    float3 HCV = RGBtoHCV(RGB);
    float S = HCV.y / (HCV.z + Epsilon);
    return float3(HCV.x, S, HCV.z);
}

float3 HSVtoRGB(in float3 HSV)
{
    float3 RGB = HUEtoRGB(HSV.x);
    return ((RGB - 1) * HSV.y + 1) * HSV.z;
}

float3 HSVLerp(float3 c1, float c2, float amount)
{
    float3 hsv1 = RGBtoHSV(c1);
    float3 hsv2 = RGBtoHSV(c2);
    if (abs(hsv2.x - hsv1.x) * 6 > 3)
    {
        if (hsv2.x > hsv1.x)
        {
            hsv1.x += 1;
        }
        else
        {
            hsv2.x += 1;
        }
    }
    float3 lerped = hsv1 * amount + (1.0 - amount) * hsv2;
    lerped = frac(lerped);
    float3 vector3 = HSVtoRGB(lerped);
    return float3(vector3.x, vector3.y, vector3.z);
}

float CubeIn(float f) {
    return f * f * f;
}

float CubeOut(float f) {
    return 1.0 - CubeIn(1.0 - f);
}

float CubeInOut(float f) {
    if (f > 0.5) return CubeOut(f * 2.0 - 1.0) * 0.5 + 0.5;
    else return CubeIn(f * 2.0) * 0.5;
}

float4 PixelShaderFunction(VertexShaderOutput input) : SV_TARGET0
{
    float transparency1 = transparency;
    if (windPercent / 250 > 0.5)
    {
        transparency1 = transparency1 - (windPercent / 250 - 0.5) * transparency1 * 2.0;
    }

    float3 windColor = color.rgb;
    if (hsvBlending > 0.5)
    {
        windColor = HSVLerp(color.rgb, fadeColor.rgb, CubeInOut(windPercent));
    }
    else
    {
        windColor = lerp(color.rgb, fadeColor.rgb, CubeInOut(windPercent));
    }
    return float4(windColor , 1.0);
}

technique NormalTechnique
{
    pass Base
    {
        AlphaBlendEnable = TRUE;
        VertexShader = compile VS_SHADER_COMPILER VertexShaderFunction();
        PixelShader = compile PS_3_SHADER_COMPILER PixelShaderFunction();
    }
}
