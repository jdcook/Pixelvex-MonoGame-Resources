#include "CommonLight.fxh"
#include "Macros.fxh"
#include "ModelLight.fxh"


float LightWidth       		   _vs(c9)  _ps(c10) _cb(c9);
float Roundedness       	   _vs(c10)  _ps(c11) _cb(c10);
float3 LightOrigin             _vs(c12) _ps(c13) _cb(c12);
float3 LightEndPoint           _vs(c13) _ps(c15) _cb(c13);


float4 PSCapsuleLight(ModelLightVSOutput input) : COLOR0
{
    input.ScreenPos.xy /= input.ScreenPos.w;
    float2 texCoord = 0.5f * (float2(input.ScreenPos.x,-input.ScreenPos.y) + 1);
	
	float4 normalData = SAMPLE_TEXTURE(NormalMap, texCoord);
	float3 normal = normalData.xyz;//decodeNormals(normalData.xy);
    //float specularPower = normalData.a * 255;
	//float specularIntensity = normalData.z;
	
	float4 colorData = SAMPLE_TEXTURE(ColorMap, texCoord);
	
	
    float depthVal = SAMPLE_TEXTURE(DepthMap, texCoord).r;
    float4 position;
    position.xy = input.ScreenPos.xy;
    position.z = depthVal;
    position.w = 1.0f;
    position = mul(position, InverseViewProjection);
    position /= position.w;
	
	
	//Calculate Diffuse light
	float3 lineDiff = LightEndPoint - LightOrigin;
	//find the point on the light's axis that is closest to this pixel's 3d position
	//reference: http://mathworld.wolfram.com/Point-LineDistance3-Dimensional.html
	//t = -[(x1 - x0) dot (x2 - x1)] / length(x2 - x1)^2
	float alongLine = -(dot ((LightOrigin - position), lineDiff)) / (lineDiff.x * lineDiff.x + lineDiff.y * lineDiff.y + lineDiff.z * lineDiff.z);
	//round off the edges, making it a capsule shape
	if(alongLine < .1f)
	{
		alongLine = .1f;
	}
	else if(alongLine > .9f)
	{
		alongLine = .9f;
	}
	//calculate light falloff (attenuation)
	float3 lightPos = LightOrigin + lineDiff * alongLine;
	float3 lightVector = lightPos - position;
	float attenuation = saturate(1.0f - length(lightVector)/LightWidth);
	lightVector = normalize(lightVector);
	float light = saturate(dot(normal,lightVector));
	
    light *= attenuation;
	
	//DirectX 9 couldn't handle another possible control flow, so both discard cases were combined here
	//instead of one after calculating the line position and one after calculating light attenuation.
	if(alongLine < 0 || alongLine > 1 || light <= 0)
	{
		discard;
	}
	
	//cel shading
	if (light> threshHigh)
	{
        light = intensityHigh;
	}
    else if (light > threshMed)
	{
        light = intensityMed;
	}
    else
	{
        light = intensityLow;
    }
	
	float3 diffuseColor = light * colorData * Color;


	
    return float4(diffuseColor.rgb, 1);
}

TECHNIQUE(CapsuleLight, ModelVSLight, PSCapsuleLight);






