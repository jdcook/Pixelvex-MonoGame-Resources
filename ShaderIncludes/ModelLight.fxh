#include "Macros.fxh"

float4x4 WorldViewProjection  _vs(19)           _cb(c15);

struct ModelLightVSOutput
{
	float4 Position : SV_POSITION;
	float4 ScreenPos : TEXCOORD0;
};

struct ModelVSInput
{
    float3 Position : SV_Position;
};
ModelLightVSOutput ModelVSLight(ModelVSInput input)
{
	ModelLightVSOutput output;
    output.Position = mul(float4(input.Position,1), WorldViewProjection);
    output.ScreenPos = output.Position;
	return output;
}
