/*
* This is based on Microsoft's 3D particle shader for XNA (http://xbox.create.msdn.com/en-US/education/catalog/sample/particle_3d).
* I've adjusted it to be compatile with MonoGame's shader language, made it work with a deferred rendering system, 
* added soft particles (lines 152-159), and added more options for rotation and color change over time.
*
*/

#include "Macros.fxh"

Texture2D<float4> Texture : register(t0); 
sampler TextureSampler : register(s0);

Texture2D<float4> DepthMap : register(t2); 
sampler DepthMapSampler : register(s2);


cbuffer Parameters : register (b0) 
{
    float4x4 View;
    float4x4 Projection;
    float2 ViewportScale;
    float CurrentTime;
    float Duration;
    float DurationRandomness;
    float3 Gravity;
    float EndVelocity;
	bool MultiStageColor;
    float4 MinColor;
	float4 Color2;
	float4 Color3;
    float4 MaxColor;
    float2 Rotation;
	bool StaticRotation;
    float2 StartSize;
    float2 EndSize;
	float Zoom;
};

struct VSInputTx 
{ 
    float2 Corner : SV_Position;
    float3 Pos : NORMAL0;
    float3 Velocity : NORMAL1;
    float4 Random : COLOR0;
    float Time : TEXCOORD0;
    float SizePercent : TEXCOORD1;
};

struct VSOutputTx 
{ 
    float4 Pos : SV_Position;
    float4 Color : COLOR0;
    float2 TextureCoordinate : TEXCOORD0;
	float4 ScreenPos : TEXCOORD1;
};

float4 ComputeParticlePosition(float3 position, float3 velocity, float age, float normalizedAge)
{
	if(!(velocity.x == 0 && velocity.y == 0 && velocity.z == 0))
	{
		float startVelocity = length(velocity);
		float endVelocity = startVelocity * EndVelocity;
		float velocityIntegral = startVelocity * normalizedAge + (endVelocity - startVelocity) * normalizedAge * normalizedAge / 2;
		position += normalize(velocity) * velocityIntegral * Duration;
	}
    
	position += Gravity * age * normalizedAge;
    return mul(mul(float4(position, 1), View), Projection);
}

float ComputeParticleSize(float randomValue, float normalizedAge)
{
    float startSize = lerp(StartSize.x, StartSize.y, randomValue);
    float endSize = lerp(EndSize.x, EndSize.y, randomValue);
    float size = lerp(startSize, endSize, normalizedAge);
    return size * Projection._m11;
}

float4 ComputeParticleColor(float4 projectedPosition, float normalizedAge)
{
	float4 color;
	if(MultiStageColor)
	{
		if(normalizedAge < .33f)
		{
			color = lerp(MinColor, Color2, normalizedAge * 3);
		}
		else if(normalizedAge < .66f)
		{
			color = lerp(Color2, Color3, (normalizedAge - .33) * 3);
		}
		else
		{
			color = lerp(Color3, MaxColor, (normalizedAge - .66) * 3);
		}
	}
	else
	{
		color = lerp(MinColor, MaxColor, normalizedAge);
	}
    
	color.a *= normalizedAge * (1-normalizedAge) * (1-normalizedAge) * 6.7;
    return color;
}

float2x2 ComputeParticleRotation(float randomValue, float age)
{
	float rotation = lerp(Rotation.x, Rotation.y, randomValue);
	
	//if SetRotation is true, then the particle will not keep rotating, but stay at the given rotation
	if(!StaticRotation)
	{
		rotation = rotation * age;
	}
	
	float c = cos(rotation);
	float s = sin(rotation);
	return float2x2(c, -s, s, c);
}

VSOutputTx VS(VSInputTx input)
{
    VSOutputTx output;
    float age = CurrentTime - input.Time;
    age *= 1 + input.Random.x * DurationRandomness;
    float normalizedAge = saturate(age / Duration);
    output.Pos = ComputeParticlePosition(input.Pos, input.Velocity, age, normalizedAge);
    float size = ComputeParticleSize(input.Random.y, normalizedAge);
	
	//if this system isn't being rotated, don't bother calculating rotation
	if(Rotation.x != 0 || Rotation.y != 0)
	{
		float2x2 rotation = ComputeParticleRotation(input.Random.w, age);
		output.Pos.xy += mul(input.Corner, rotation) * size * input.SizePercent * ViewportScale;
	}
	else
	{
		output.Pos.xy += input.Corner * size * input.SizePercent * ViewportScale;
	}
	
    output.Color = ComputeParticleColor(output.Pos, normalizedAge);
    output.TextureCoordinate = (input.Corner + 1) / 2;
	output.ScreenPos = output.Pos;
    return output;
}

float4 PS(VSOutputTx input) : SV_Target
{
	float particleDepth = input.ScreenPos.z / input.ScreenPos.w;
	input.ScreenPos.xy /= input.ScreenPos.w;
	float2 texCoord = .5f * (float2(input.ScreenPos.x, -input.ScreenPos.y) + 1);
	float sceneDepth = DepthMap.Sample(DepthMapSampler, texCoord).r;
	if(particleDepth > sceneDepth)
		discard;
	

	//The 'magic number' in this line is based on the scale of Metagalactic Blitz's screen space and camera. 
	//Adjust this as necessary for your own game to get the particles to blend nicely.
	float fade = (sceneDepth - particleDepth) * 312.5f * Zoom; 
	
    float4 color = Texture.Sample(TextureSampler, input.TextureCoordinate) * input.Color;
    color.a = min(color.a, fade);
    return color;
}


TECHNIQUE(Particles, VS, PS);