<Shader Name = "Standard">
	<Passes>
		<Pass Name = "Main" VertexShader = "VertexMain vs_5_0" PixelShader = "PixelMain ps_5_0" />
	</Passes>
</Shader>
#include <Common>
#include <DeferredCommon>

Texture2DArray ShadowMaps : register(t0);

struct VertexOutput
{
	float4 position : SV_POSITION;
	float4 positionWS : POSITION_WORLD;
	float3 normal : NORMAL;
	float3 normalWS : NORMAL_WORLD;
	float3 tangent : TANGENT;
	float3 bitangent : BITANGENT;
	float3 lightDir : LIGHTDIR;
	float3 viewDir : VIEWDIR;
	float4 colour : COLOUR;
	float3 size : BRICK_SIZE;
	float transparency : TRANSPARENCY;
	float2 texCoord : TEXCOORD;
	float2 depthZW : DEPTH_ZW;
	int shadingModel : SHADING_MODEL;
	float emissive : EMISSIVE;
	float2 material : MATERIAL;
	//float metalMasks[MATERIAL_COUNT];
};

struct PixelOutput
{
	float4 GBuffer0 : SV_TARGET0;
	float4 GBuffer1 : SV_TARGET1;
	float4 GBuffer2 : SV_TARGET2;
	float4 GBuffer3 : SV_TARGET3;
};

cbuffer LightingConstants : register(b1)
{
	PointLight SunLight;
	float4 AmbientColour;
	float4 OutdoorAmbient;
}

#define UP float3(0, 1, 0)
#define DOWN float3(0, -1, 0)
#define LEFT float3(-1, 0, 0)
#define RIGHT float3(1, 0, 0)
#define FRONT float3(0, 0, -1)
#define BACK float3(0, 0, 1)

#include <ShadowSampling>

float2 CalculateUVs(float2 texCoord, float2 size)
{
	float2 output;
	output.x = (size.x * texCoord.x) - (size.x - floor(size.x) / 0.25f);
	output.y = (size.y * texCoord.y) - (size.y - floor(size.y) / 1.0f);

	return output;
}

VertexOutput VertexMain(VertexInstanced input)
{
	VertexOutput result;

	input.position *= input.size;

	float4x4 worldMatrix = input.modelMatrix;
	float4 position4 = float4(input.position, 1);
	float4 normal4 = float4(input.normal, 0);

	result.positionWS = mul(position4, worldMatrix);
	result.position = mul(result.positionWS, Camera.viewProjMatrix);

	result.normal = input.normal;
	result.normalWS = mul(input.normal, (float3x3)worldMatrix).xyz;
	result.depthZW = result.position.zw;

	result.tangent = mul(worldMatrix, float4(input.tangent, 1));
	result.bitangent = mul(float4(input.bitangent, 0), worldMatrix);

	result.lightDir = SunLight.position.xyz;
	result.viewDir = -(result.positionWS.xyz - Camera.position).xyz;

#ifdef USE_LOG_DEPTH
	result.position.z = log2(max(1e-6, result.position.w)) * Camera.Fcoef + 1.0;
#endif

	result.colour = input.colour + input.instanceColour;
	result.size = input.size;
	result.transparency = input.transparency;
	result.shadingModel = input.shadingModel;
	result.emissive = input.emissive;

	result.material = input.material;

	float2 uvSize;
	float3 normal = abs(input.normal);
	if (all(normal == float3(0, 0, 1)))
	{
		uvSize = float2(input.size.x, input.size.y);
	}
	else if (all(normal == float3(1, 0, 0)))
	{
		uvSize = float2(input.size.z, input.size.y);
	}
	else
	{
		uvSize = float2(input.size.x, input.size.z);
	}
	uvSize *= (0.75 / 5);
	result.texCoord = CalculateUVs(input.texCoord, uvSize);
	result.texCoord.x = result.texCoord.x;

	return result;
}

float3 UnpackNormal(float4 value)
{
	float2 xy = value.ag * 2 - 1;
	return float3(xy, sqrt(saturate(1 + dot(-xy, xy))));
}

float4 SampleFar(Texture2DArray tex, SamplerState smp, float3 uv, float fade, float cutoff, float tiling)
{
	if (cutoff == 0)
		return tex.Sample(smp, uv);
	else
	{
		float cscale = 1 / (1 - cutoff);

		return lerp(tex.Sample(smp, uv * tiling), tex.Sample(smp, uv), saturate(fade * cscale - cutoff * cscale));
	}
}

PixelOutput PixelMain(VertexOutput input) : SV_TARGET0
{
	PixelOutput result;

	float2 uv = input.texCoord;

	float3 albedo = 1;
	float metallic;
	float smoothness;
	float3 worldNormal;

	int shadingModel;

	float NdotL = saturate(dot(input.normalWS, input.lightDir));

	float3 shadowVis = ShadowVisibility(input.positionWS.xyz, input.position.w, NdotL, input.normalWS.xyz, uint2(input.position.xy));

	albedo = input.colour * shadowVis;
	shadingModel = input.shadingModel;
	worldNormal = input.normalWS.xyz;
	smoothness = input.material.x;
	metallic = input.material.y;

	Material material;
	material.baseColour = albedo;
	material.worldNormal = worldNormal;
	material.specular = 0.5;
	material.smoothness = smoothness;
	material.metallic = metallic;
	material.opacity = 1 - input.transparency;
	material.occlusion = 0;
	material.emissiveColour = input.emissive;
	material.perObjectData = 0;
	material.shadingModel = shadingModel;

	EncodeGBuffer(material, result.GBuffer0, result.GBuffer1, result.GBuffer2, result.GBuffer3);

	return result;
}



