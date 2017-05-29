#include "Macros.fxh"


DECLARE_TEXTURE(Texture, 1);
DECLARE_TEXTURE(DepthMap, 2);

cbuffer Parameters : register(b0)
{
	float4x4 View;
	float4x4 Projection;
	float3 colorTint;
	float alpha;
}


struct VSIn
{
    float4 Position : SV_Position;
    float2 TexCoord : TEXCOORD0;
};

struct VSOut
{
    float4 Position  : SV_Position;
    float2 TexCoord  : TEXCOORD0;
	float4 ScreenPos : TEXCOORD1;
};

VSOut VS(VSIn input)
{
    VSOut output;

	output.Position = mul(mul(input.Position, View), Projection);
	
	//also output the position as a texcoord, so that it 
	//is interpolated correctly for figuring out position on the depth map
	output.ScreenPos = output.Position;
	output.TexCoord = input.TexCoord;

    return output;
}

float4 PS(VSOut input) : SV_Target
{
	float billboardDepth = input.ScreenPos.z / input.ScreenPos.w;
	
	//divide by homogenous coordinate because math
	input.ScreenPos.xy /= input.ScreenPos.w;
	//transform from [-1, 1] to [0, 1]
	float2 texCoord = .5f * (float2(input.ScreenPos.x, -input.ScreenPos.y) + 1);
	float sceneDepth = SAMPLE_TEXTURE(DepthMap, texCoord);
	if(billboardDepth > sceneDepth)
	{
		discard;
	}
	
	return SAMPLE_TEXTURE(Texture, input.TexCoord) * float4(colorTint, alpha);
}

TECHNIQUE(Billboard, VS, PS);

