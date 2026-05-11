
#include "Common.fxh"

//-----------------------------------------------------------------------------
// Globals.
//-----------------------------------------------------------------------------

DECLARE_TEXTURE(map, 0);
float4x4 World;
float4x4 RotationMatrix; //passed in at a 45 degree offset since the initial angle of all perlin noise is a 45 degree angle

uniform float Time;
uniform float2 Dimensions;
uniform float2 CamPos;
uniform float Tapering; //[0.0- 1.0] permitted values


//adapted from https://www.shadertoy.com/view/XdXGW8
float3 grad( float3 zz )  // replace this anything that returns a random vector
{

    //
    float n = frac(sin(dot(zz,
                         float3(12.9898,78.233,24.478)))
                 * 43758.5453123);

    float3 x = float3(1.0,0.0,0.0);
    float3 y = float3(0.0,1.0,0.0);
    float3 z = float3(0.0,0.0,1.0);
    if (n < 0.0625) return  x + y;
    if (n < 0.125) return  -x + y;
    if (n < 0.0625 * 3.0) return  x - y;
    if (n < 0.0625 * 4.0) return  -x - y;
    if (n < 0.0625* 5.0) return  x + z;
    if (n < 0.0625 * 6.0) return  -x + z;
    if (n < 0.0625 * 7.0) return  -x - z;
    if (n < 0.0625 * 8.0) return  y + z;
    if (n < 0.0625 * 9.0) return  -y + z;
    if (n < 0.0625* 10.0) return  y - z;
    if (n < 0.0625 * 11.0) return  -y - z;
    if (n < 0.0625 * 12.0) return  y + x;
    if (n < 0.0625 * 13.0) return  -y + z;
    if (n < 0.0625 * 14.0) return  y - x;
    if (n < 0.0625 * 15.0) return  -y - z;
    return x;
}

float noise( in float3 p )
{
    float3 i = floor( p );
    float3 f = frac( p );

	float3 u = f*f*(3.0-2.0*f); // feel free to replace by a quintic smoothstep instead

    return lerp(
    lerp( lerp( dot( grad( i+float3(0,0,0) ), f-float3(0.0,0.0,0.0) ),
                     dot( grad( i+float3(1,0,0) ), f-float3(1.0,0.0,0.0) ), u.x),
                lerp( dot( grad( i+float3(0,1,0) ), f-float3(0.0,1.0,0.0) ),
                     dot( grad( i+float3(1,1,0) ), f-float3(1.0,1.0,0.0) ), u.x), u.y),
    lerp( lerp( dot( grad( i+float3(0,0,1) ), f-float3(0.0,0.0,1.0) ),
                     dot( grad( i+float3(1,0,1) ), f-float3(1.0,0.0,1.0) ), u.x),
                lerp( dot( grad( i+float3(0,1,1) ), f-float3(0.0,1.0,1.0) ),
                     dot( grad( i+float3(1,1,1) ), f-float3(1.0,1.0,1.0) ), u.x), u.y),
                     u.z);
}

float4 PixelShaderFunction(float4 inPosition : SV_Position, float4 inColor : COLOR0, float2 uv1 : TEXCOORD0) : SV_TARGET0
{
    float3 p = float3((inPosition.xy) /Dimensions.xy + float2(0.0, -Time) + 0.5 * mul(float4(CamPos, 0,0), World).xy, Time);


	float3 uv = p*float3(Dimensions.x/Dimensions.y,1.0, 1.0);


	float f = 0.0;

    uv *= 32.0;
    float3x3 m = float3x3(1.6, 1.2,0.0, -1.2, 1.6,0.0,0.0,0.0,1.0 );
	f  = 0.5000*noise( uv ); uv = mul(m, uv);
	f += 0.2500*noise( uv ); uv = mul(m, uv);
	f += 0.1250*noise( uv ); uv = mul(m, uv);
	f += 0.0625*noise( uv ); uv = mul(m, uv);
	f += 0.0312*noise( uv ); uv = mul(m, uv);

	float g = f;


    // Output to screen

    f *= smoothstep( 0.0, 0.05, inPosition.x/Dimensions.x);
    f = 0.5 + 0.5 *f;


    g *= smoothstep( 0.0, 0.05, inPosition.y/Dimensions.y);
    g = 0.5 + 0.5 *g;


	float3 distort = mul(float4(f - 0.5, 1-g - 0.5, f - 0.5, 0), RotationMatrix).xyz + float3(0.5, 0.5, 0.5);

	float3 distortPos = float3((distort.r * 2.0 - 1.0) * 0.044, (distort.g * 2.0 - 1.0) * 0.078, (distort.b * 2.0 - 1.0) * 0.044);

	distortPos *=2;


	float3 threeDUV = float3(frac(16*uv1.x), uv1.y, floor(uv1.x * 16) / 16.0);
	float3 displacedUV = threeDUV + distortPos;

	displacedUV.z = floor(displacedUV.z * 16) / 16;
	clamp(displacedUV, float4(0,0,0,0), float4(1,1,1,1));


	float2 recompressedUV = float2(displacedUV.x / 16 + displacedUV.z, displacedUV.y);
	return SAMPLE_TEXTURE(map, recompressedUV);
}

technique Distort
{
    pass
    {
        AlphaBlendEnable = TRUE;
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
