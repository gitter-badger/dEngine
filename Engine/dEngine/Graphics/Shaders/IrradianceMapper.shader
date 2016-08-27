<Shader Name="IrradianceMapper">
	<Passes>
		<Pass Name="Main" VertexShader="VertexMain vs_5_0" PixelShader="PixelMain ps_5_0" />
	</Passes>
</Shader>
#include <Common>

TextureCube DiffuseMap : register(t0);

struct VertexOutput
{
	float4 position : SV_POSITION;
	float3 normalWS : NORMAL_WORLD;
	float3 viewDirWS : VIEWDIR_WORLD;
	float3 irradiance : IRRADIANCE;
};

cbuffer IrradianceConstants : register(b1)
{
	float4x4 SHMatrix[3];
};

float3 ComputeIrradiance(float3 normal, float4x4 M[3])
{
	float4 n = float4(normal, 1.0f);

	float3 output;
	output.r = dot(n, mul(n, M[0]));
	output.g = dot(n, mul(n, M[1]));
	output.b = dot(n, mul(n, M[2]));

	return output;
}

VertexOutput VertexMain(VertexInstanced input)
{
	VertexOutput result;

	input.position *= input.size;

	float4x4 worldMatrix = float4x4(
		float4(1, 0, 0, 0),
		float4(0, 1, 0, 0),
		float4(0, 0, 1, 0),
		float4(Camera.position, 1));
	float4x4 worldViewProj = mul(worldMatrix, Camera.viewProjMatrix);
	float4 position4 = float4(input.position, 1);

	float target = (input.size.y * -1.1);

	result.position = mul(position4, worldViewProj);
	result.normalWS = mul(float4(input.normal, 1), worldMatrix);

	// World Space view direction from world space position
	float3 positionWS = mul(position4, worldMatrix);
	result.viewDirWS = normalize(positionWS - Camera.position);

	result.irradiance = ComputeIrradiance(result.normalWS, SHMatrix);

	return result;
}

float4 PixelMain(VertexOutput input) : SV_TARGET
{
	float3 r = reflect(input.viewDirWS, input.normalWS);
	float3 reflectColor = DiffuseMap.Sample(DefaultSampler, r).rgb;
	return float4(input.irradiance, 1);
}