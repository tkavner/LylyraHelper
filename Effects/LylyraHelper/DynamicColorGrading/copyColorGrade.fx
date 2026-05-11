#include "Common.fxh"

//-----------------------------------------------------------------------------
// Vertex Shaders.
//-----------------------------------------------------------------------------
struct VertexShaderOutput
{
  float4 position : SV_Position;
};

VertexShaderOutput VertexShaderFunction(float4 position:POSITION0, float4 color:COLOR0)
{
  VertexShaderOutput output;

  output.position = position;

  return output;
}
