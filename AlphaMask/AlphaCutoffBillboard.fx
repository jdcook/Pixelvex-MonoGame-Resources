/*
 * A shader for cutting off pixels that don't have the required alpha values.
 */

#include "Macros.fxh"

DECLARE_TEXTURE(Texture, 1);
DECLARE_TEXTURE(DepthMap, 2);

cbuffer Parameters : register(b0)
{
	float4x4 View;
	float4x4 Projection;
	float3 colorTint;
	float alpha;
	float alphaCutoff;
	bool invertCutoff;
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
	
	//divide by homogenous coordinate
	input.ScreenPos.xy /= input.ScreenPos.w;
	//transform from [-1, 1] to [0, 1]
	float2 texCoord = .5f * (float2(input.ScreenPos.x, -input.ScreenPos.y) + 1);
	float sceneDepth = SAMPLE_TEXTURE(DepthMap, texCoord);
	if(billboardDepth > sceneDepth)
	{
		discard;
	}
	

	//THE ACTUAL ALPHA CUTOFF PART
	float4 color = SAMPLE_TEXTURE(Texture, input.TexCoord);
	
	//if transparent / black, discard (not part of texture)
	if(color.a == 0)
	{
		discard;
	}
	
	//otherwise, use cut off texture at given alpha value
	if(invertCutoff)
	{
		if(color.a <= alphaCutoff)
		{
			discard;
		}
	}
	else
	{
		if(color.a > alphaCutoff)
		{
			discard;
		}
	}
	
	color = color * float4(colorTint, 1);
	//finally, use custom alpha (the texture's gradient is not shown in final product)
	color.a = alpha;
	return color;
}

TECHNIQUE(Billboard, VS, PS);

