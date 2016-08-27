#define SHADINGMODEL_STANDARD 0
#define SHADINGMODEL_UNLIT 1
#define SHADINGMODEL_SUBSURFACE 2
#define SHADINGMODEL_CLEARCOAT 3
#define SHADINGMODEL_ROBLOX 4

struct MaterialConfig
{
	float TextureTiling;
	float DiffuseScale;
	float SpecularScale;
	float GlossScale;
	float ReflectionScale;
	float NormalShadowScale;
	float SpecularLod;
	float GlossLod;
	float NormalDetailingTiling;
	float NormalDetailingScale;
	float FarTiling;
	float DiffuseCutoff;
	float NormalCutoff;
	float SpecularCutoff;
	float BumpIntensity;
	float WangTilesScale;
	int UseBlendColour;
	int UseConstantNormal;
	int UseConstantDiffuse;
	int Unused;
};

struct PointLight // length: 32
{
	float3 position;
	float range;
	float3 colour;
	float falloff;
};

struct SpotLight // length: 64
{
	float3 Position;
	float pad0;
	float4 Colour;
	float3 Direction;
	float pad1;
	float3 Up;
	float pad2;
	float Range;
	float Aperture;
	uint id;
};

struct Material
{
	float3 baseColour;
	float3 worldNormal;
	float smoothness;
	float metallic;
	float specular;
	float opacity;
	float occlusion;
	float3 emissiveColour;
	int shadingModel;
	float perObjectData;
};

float2 EncodeNormal(float3 n)
{
	float2 enc = normalize(n.xy) * (sqrt(-n.z*0.5 + 0.5));
	enc = enc*0.5 + 0.5;
	return enc;
}

float3 DecodeNormal(float2 enc)
{
	float4 nn = float4(enc, 0, 0) *float4(2, 2, 0, 0) + float4(-1, -1, 1, -1);
	half l = dot(nn.xyz, -nn.xyw);
	nn.z = l;
	nn.xy *= sqrt(l);
	return nn.xyz * 2 + float3(0, 0, -1);
}

void DecodeGBuffer(float4 gbuffer0, float4 gbuffer1, float4 gbuffer2, float4 gbuffer3, out Material material)
{
	material.baseColour = gbuffer0.rgb;
	material.opacity = gbuffer0.a;

	material.metallic = gbuffer1.r;
	material.specular = gbuffer1.g;
	material.smoothness = gbuffer1.b;
	material.occlusion = gbuffer1.a;

	material.worldNormal = gbuffer2.rgb;
	material.shadingModel = gbuffer2.a;

	material.emissiveColour = gbuffer3.rgb;
	material.perObjectData = gbuffer3.a;
}

void EncodeGBuffer(Material material, out float4 gbuffer0, out float4 gbuffer1, out float4 gbuffer2, out float4 gbuffer3)
{
	gbuffer0.rgb = material.baseColour;
	gbuffer0.a = material.opacity;

	gbuffer1.r = material.metallic;
	gbuffer1.g = material.specular;
	gbuffer1.b = material.smoothness;
	gbuffer1.a = material.occlusion;

	//float2 normalEnc = EncodeNormal(material.worldNormal);

	gbuffer2 = 0;
	gbuffer2.r = material.worldNormal.r;
	gbuffer2.g = material.worldNormal.g;
	gbuffer2.b = material.worldNormal.b;
	gbuffer2.a = material.shadingModel;

	gbuffer3.rgb = material.emissiveColour;
	gbuffer3.a = material.perObjectData;
}