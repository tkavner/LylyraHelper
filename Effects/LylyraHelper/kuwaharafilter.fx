#include "Common.fxh"

DECLARE_TEXTURE(source, 0);
DECLARE_TEXTURE(light, 1);
float4x4 World;

uniform float2 ScreenSize;

//ported to hlsl from https://www.shadertoy.com/view/MsXSz4
float4 kuwaharaFilter(float2 fragCoord )
{
    const float radius = 7.0;
    float4 fragColor = float4(0.0, 0.0, 0.0, 0.0);
    float2 src_size = float2 (1.0/ScreenSize.x, 1.0/ScreenSize.y);
    float2 uv = fragCoord.xy;
    float n = float((radius + 1) * (radius + 1));
    int i;
    int j;
    float3 m0 = float3(0.0, 0.0, 0.0); float3 m1 = float3(0.0, 0.0, 0.0); float3 m2 = float3(0.0, 0.0, 0.0); float3 m3 = float3(0.0, 0.0, 0.0);
    float3 s0 = float3(0.0, 0.0, 0.0); float3 s1 = float3(0.0, 0.0, 0.0); float3 s2 = float3(0.0, 0.0, 0.0); float3 s3 = float3(0.0, 0.0, 0.0);
    float3 c;

    for (int j = -radius; j <= 0; ++j)  {
        for (int i = -radius; i <= 0; ++i)  {
            c = SAMPLE_TEXTURE(source, uv + float2(i,j) * src_size).rgb;
            m0 += c;
            s0 += c * c;
        }
    }

    for (int j = -radius; j <= 0; ++j)  {
        for (int i = 0; i <= radius; ++i)  {
            c = SAMPLE_TEXTURE(source, uv + float2(i,j) * src_size).rgb;
            m1 += c;
            s1 += c * c;
        }
    }

    for (int j = 0; j <= radius; ++j)  {
        for (int i = 0; i <= radius; ++i)  {
            c = SAMPLE_TEXTURE(source, uv + float2(i,j) * src_size).rgb;
            m2 += c;
            s2 += c * c;
        }
    }

    for (int j = 0; j <= radius; ++j)  {
        for (int i = -radius; i <= 0; ++i)  {
            c = SAMPLE_TEXTURE(source, uv + float2(i,j) * src_size).rgb;
            m3 += c;
            s3 += c * c;
        }
    }


    float min_sigma2 = 1e+2;
    m0 /= n;
    s0 = abs(s0 / n - m0 * m0);

    float sigma2 = s0.r + s0.g + s0.b;
    if (sigma2 < min_sigma2) {
        min_sigma2 = sigma2;
        fragColor = float4(m0, 1.0);
    }

    m1 /= n;
    s1 = abs(s1 / n - m1 * m1);

    sigma2 = s1.r + s1.g + s1.b;
    if (sigma2 < min_sigma2) {
        min_sigma2 = sigma2;
        fragColor = float4(m1, 1.0);
    }

    m2 /= n;
    s2 = abs(s2 / n - m2 * m2);

    sigma2 = s2.r + s2.g + s2.b;
    if (sigma2 < min_sigma2) {
        min_sigma2 = sigma2;
        fragColor = float4(m2, 1.0);
    }

    m3 /= n;
    s3 = abs(s3 / n - m3 * m3);

    sigma2 = s3.r + s3.g + s3.b;
    if (sigma2 < min_sigma2) {
        min_sigma2 = sigma2;
        fragColor = float4(m3, 1.0);
    }
    return fragColor;
}

float4 PixelShaderFunction(float4 inPosition : SV_Position, float4 inColor : COLOR0, float2 uv : TEXCOORD0) : SV_TARGET0
{
    float4 s = SAMPLE_TEXTURE(source, uv);
    float4 l = SAMPLE_TEXTURE(light, uv);


    return lerp(kuwaharaFilter(uv), s, l.r);
}

technique NormalTechnique
{
    pass Base
    {
        PixelShader = compile PS_3_SHADER_COMPILER PixelShaderFunction();
    }
}
