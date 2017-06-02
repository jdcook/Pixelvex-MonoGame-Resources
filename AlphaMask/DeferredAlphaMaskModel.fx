#include "Macros.fxh"
#include "NormalEncoding.fxh"
#include "Structures.fxh"

Texture2D<float4> Texture : register(t0);
sampler TextureSampler = sampler_state
{
   Texture = <Texture>;
   MinFilter = Linear;
   MagFilter = Linear;
   MipFilter = Linear;
   AddressU  = Wrap;
   AddressV  = Wrap;
};
Texture2D<float4> AlphaMaskTexture : register(t1);
sampler AlphaMaskTextureSampler = sampler_state
{
   Texture = <AlphaMaskTexture>;
   MinFilter = Linear;
   MagFilter = Linear;
   MipFilter = Linear;
   AddressU  = Wrap;
   AddressV  = Wrap;
};

cbuffer Parameters : register(b0)
{
	float3 colorTint;
	float tintLerp;
	bool hasOutline;
	float alphaCutoff;
	bool invertCutoff;
	float4x4 World;
	float4x4 WorldViewProj;
}

ToonVSMskOutput VSDeferred(VSInputNmTxMsk vin)
{
    ToonVSMskOutput output;
    
    output.TexCoord = vin.TexCoord1;
    output.PositionPS = mul(vin.Position, WorldViewProj);
	output.Normal = mul(vin.Normal, World);
	output.Depth.xy = output.PositionPS.zw;
	output.AlphaMaskTexCoord = vin.TexCoord2;
	

    return output;
}

GBufferPSOutput PSDeferred(ToonVSMskOutput pin) : SV_Target0
{
	GBufferPSOutput output;

	//THE ALPHA CUTOFF PART
	float4 alphaMask = AlphaMaskTexture.Sample(AlphaMaskTextureSampler, pin.AlphaMaskTexCoord);
	if(invertCutoff && alphaMask.a <= alphaCutoff || !invertCutoff && alphaMask.a > alphaCutoff)
	{
		discard;
	}

	//just generic model rendering
	float4 tex = Texture.Sample(TextureSampler, pin.TexCoord);
	output.Color.rgb = lerp(tex, colorTint, tintLerp);
	output.Color.a = (hasOutline? 1 : 0);
	
	output.Normal.xy = encodeNormals(normalize(pin.Normal));
	output.Normal.z = 0;//specular intensity
	output.Normal.w = 0;//specular power
	
	output.Depth = pin.Depth.x / pin.Depth.y;
	return output;
}

TECHNIQUE(Deferred, VSDeferred, PSDeferred);


