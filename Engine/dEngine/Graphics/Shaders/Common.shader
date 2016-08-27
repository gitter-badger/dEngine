#define PI 3.14159265359
#define RECIPROCAL_PI 0.31830988618
#define EPSILON 1e-6

static const int MATERIAL_COUNT = 18;

SamplerState DefaultSampler : register(s0);
SamplerState BrickSampler : register(s1);
SamplerState WrapSampler : register(s2);
SamplerState PointSampler : register(s3);
SamplerComparisonState ShadowSampler : register(s4);
SamplerComparisonState ShadowPCFSampler : register(s5);

struct CFrame
{
	float3 p;
	float r00;
	float r01;
	float r02;
	float r10;
	float r11;
	float r12;
	float r20;
	float r21;
	float r22;
};

struct Vertex
{
	float3 position : POSITION;
	float3 normal: NORMAL;
	float2 texCoord: TEXCOORD;
	float4 colour: COLOUR;
	float3 tangent: TANGENT;
	float3 bitangent: BITANGENT;
};

struct VertexInstanced
{
	// Vertex
	float3 position : POSITION;
	float3 normal: NORMAL;
	float2 texCoord: TEXCOORD;
	float4 colour: COLOUR;
	float3 tangent: TANGENT;
	float3 bitangent: BITANGENT;
	// Instance
	float transparency : I_TRANSPARENCY;
	float3 size : I_SIZE;
	float4 instanceColour : I_COLOUR;
	int shadingModel : I_SHADINGMODEL;
	float emissive : I_EMISSIVE;
	float2 material : I_MATERIAL;
	row_major float4x4 modelMatrix : I_MODEL;
};

struct CameraData // length: 240
{
	row_major float4x4 viewMatrix;
	row_major float4x4 viewProjMatrix;
	row_major float4x4 inverseViewProjection;
	float3 position;
	float pad0;
	float ClipNear;
	float ClipFar;
	float DepthOfFieldNear;
	float DepthOfFieldFar;
	float4 ScreenParams;
};

cbuffer CameraBuffer
{
	CameraData Camera;
};

static matrix IdentityMatrix =
{
	{ 1, 0, 0, 0 },
	{ 0, 1, 0, 0 },
	{ 0, 0, 1, 0 },
	{ 0, 0, 0, 1 }
};

float3 ReconstructPositionWS(float2 texCoord, float depth)
{
	float4 positionSS;
	positionSS.x = texCoord.x * 2.0f - 1.0f;
	positionSS.y = -(texCoord.y * 2.0f - 1.0f);
	positionSS.z = depth;
	positionSS.w = 1.0f;
	float4 positionWS = mul(positionSS, Camera.inverseViewProjection);
	positionWS /= positionWS.w;
	return positionWS;
}

float pow2(float x)
{
	return x * x;
}

float pow5(float x)
{
	float xx = x*x;
	return xx * xx * x;
}

float rough(float smoothness)
{
	return clamp(pow(1 - smoothness, 4), 0.01, 1);
}

float smooth(float roughness)
{
	return clamp(1 - pow(roughness, 1.0 / 4), 0.01, 1);
}

float4x4 cfmatrix(CFrame cframe)
{
	return float4x4(
		float4(cframe.r00, cframe.r10, cframe.r20, 0),
		float4(cframe.r01, cframe.r11, cframe.r21, 0),
		float4(cframe.r02, cframe.r12, cframe.r22, 0),
		float4(cframe.p.x, cframe.p.y, cframe.p.z, 1));
}

float3 approximationSRgbToLinear(in float3 sRGBCol)
{
	return pow(sRGBCol, 2.2);
}

float3 approximationLinearToSRGB(in float3 linearCol)
{
	return pow(linearCol, 1 / 2.2);
}

float3 accurateSRGBToLinear(in float3 sRGBCol)
{
	float3 linearRGBLo = sRGBCol / 12.92;
	float3 linearRGBHi = pow((sRGBCol + 0.055) / 1.055, 2.4);
	float3 linearRGB = (sRGBCol <= 0.04045) ? linearRGBLo : linearRGBHi;
	return linearRGB;
}

float3 accurateLinearToSRGB(in float3 linearCol)
{
	float3 sRGBLo = linearCol * 12.92;
	float3 sRGBHi = (pow(abs(linearCol), 1.0 / 2.4) * 1.055) - 0.055;
	float3 sRGB = (linearCol <= 0.0031308) ? sRGBLo : sRGBHi;
	return sRGB;
}

float3 ACESFilm(float3 x)
{
	float a = 2.51f;
	float b = 0.03f;
	float c = 2.43f;
	float d = 0.59f;
	float e = 0.14f;
	return saturate((x*(a*x + b)) / (x*(c*x + d) + e));
}

