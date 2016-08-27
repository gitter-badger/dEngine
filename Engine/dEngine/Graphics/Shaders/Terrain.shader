<Shader Name="Terrain">
	<Passes>
		<Pass Name="Main" VertexShader="VertexMain vs_5_0" PixelShader="PixelMain ps_5_0"/>
	</Passes>
</Shader>
#include <Common>
#include <DeferredCommon>

Texture2DArray ShadowMaps : register(t0);
Texture2D DiffuseAtlas : register(t1);
Texture2D NormalAtlas : register(t2);
Texture2D SpecularAtlas : register(t3);

struct TerrainVertex
{
	float3 position : POSITION;
	float3 normal : NORMAL;
	float4 material0 : TEXCOORD0;
	float4 material1 : TEXCOORD1;
};

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
	float2 texCoord : TEXCOORD;
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

cbuffer TerrainConstants : register(b2)
{
	float4x4 ChunkMatrix;
}

#include <ShadowSampling>

float2 CalculateUVs(float2 texCoord, float2 size)
{
	float2 output;
	output.x = (size.x * texCoord.x) - (size.x - floor(size.x) / 0.25f);
	output.y = (size.y * texCoord.y) - (size.y - floor(size.y) / 1.0f);

	return output;
}

VertexOutput VertexMain(TerrainVertex input)
{
	VertexOutput result;

	input.position *= 2;

	float4x4 worldViewProj = mul(ChunkMatrix, Camera.viewProjMatrix);
	float4 position4 = float4(input.position, 1);
	float4 positionWS = mul(position4, ChunkMatrix);
	float4 normalWS = mul(input.normal, ChunkMatrix);

	result.position = mul(position4, worldViewProj);
	result.positionWS = positionWS;

	result.normal = input.normal;
	result.normalWS = normalWS.xyz;
	
	result.tangent = input.material0.yzw;
	result.bitangent = input.material0.yzw;
	
	result.lightDir = SunLight.position.xyz;
	result.viewDir = -(positionWS.xyz - Camera.position).xyz;

	result.texCoord = float2(0, 0);

	return result;
}

PixelOutput PixelMain(VertexOutput input) : SV_TARGET0
{
	PixelOutput result;
	
	float3 worldNormal = input.normal;
	float shadowVis = 1;
	
	Material material;
	material.baseColour = 1;
	material.worldNormal = worldNormal;
	material.specular = 0.5;
	material.smoothness = 0.2;
	material.metallic = 0;
	material.opacity = 1;
	material.occlusion = 1 - shadowVis;
	material.emissiveColour = 0;
	material.perObjectData = 0;
	material.shadingModel = 0;

	EncodeGBuffer(material, result.GBuffer0, result.GBuffer1, result.GBuffer2, result.GBuffer3);

	return result;
}



