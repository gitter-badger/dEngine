<Shader Name="ShadowMapping">
	<Passes>
		<Pass Name="Depth" VertexShader="VertexMain vs_5_0" PixelShader="PixelDepth ps_5_0"/>
	</Passes>
</Shader>
#include <Common>

struct VertexOutput
{
	float4 position : SV_POSITION;
};

cbuffer ShadowConstants : register(b0)
{
	row_major float4x4 ViewProjMatrix;
};

VertexOutput VertexMain(VertexInstanced input)
{
	VertexOutput result;

	float4x4 modelMatrix = input.modelMatrix;
	float4x4 modelViewProj = mul(modelMatrix, ViewProjMatrix);

	input.position.xyz = input.position.xyz * input.size;

	result.position = mul(float4(input.position, 1), modelViewProj);

	return result;
}

void PixelDepth(VertexOutput input)
{
}

