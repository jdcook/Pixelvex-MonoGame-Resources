struct VSInputNmTx
{
    float4 Position : SV_Position;
    float3 Normal   : NORMAL;
    float2 TexCoord : TEXCOORD0;
};

struct VSInputNmTxMsk
{
    float4 Position : SV_Position;
    float3 Normal   : NORMAL;
    float2 TexCoord1 : TEXCOORD0;
    float2 TexCoord2 : TEXCOORD1;
};

struct VSInputNmTxWeights
{
    float4 Position : SV_Position;
    float3 Normal   : NORMAL;
    float2 TexCoord : TEXCOORD0;
    int4   Indices  : BLENDINDICES0;
    float4 Weights  : BLENDWEIGHT0;
};

struct ToonVSOutput
{
    float2 TexCoord   : TEXCOORD0;
    float4 PositionPS : SV_Position;
	float3 Normal     : TEXCOORD1;
	float2 Depth : TEXCOORD2;
};

struct ToonVSMskOutput
{
    float2 TexCoord   : TEXCOORD0;
    float4 PositionPS : SV_Position;
	float3 Normal     : TEXCOORD1;
	float2 Depth : TEXCOORD2;
	float2 AlphaMaskTexCoord : TEXCOORD3;
};

struct ShadowVSOutput
{
    float4 PositionPS : SV_Position;
	float2 Depth : TEXCOORD2;
};

struct ShadowVSMskOutput
{
    float4 PositionPS : SV_Position;
	float2 AlphaMaskTexCoord : TEXCOORD1;
	float2 Depth : TEXCOORD2;
};

struct GBufferPSOutput
{
	float4 Depth : COLOR0;
	float4 Color : COLOR1;
	float4 Normal : COLOR2;
};

