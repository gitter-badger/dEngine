<Shader Name="Line">
<Passes>
	<Pass Name="Main" VertexShader="VertexMain vs_5_0" PixelShader="PixelMain ps_5_0"/>
	</Passes>
</Shader>
#include <Common>

struct LineVertex
{
	float3 position : POSITION;
	float4 colour : TEXCOORD0;
};

struct VertexOutput
{
	float4 position : SV_POSITION;
	float4 colour : COLOUR;
};

struct PixelOutput
{
	float4 colour : SV_TARGET0;
};

VertexOutput VertexMain(LineVertex input)
{
	VertexOutput result;

	float4x4 worldMatrix = float4x4(float4(1, 0, 0, 0), float4(0, 1, 0, 0), float4(0, 0, 1, 0), float4(0, 0, 0, 1));
	float4x4 worldViewProj = mul(worldMatrix, Camera.viewProjMatrix);
	result.position = mul(float4(input.position, 1), worldViewProj);
	result.colour = input.colour;

	return result;
}

PixelOutput PixelMain(VertexOutput input)
{
	PixelOutput result;

	result.colour = input.colour;

	return result;
}



