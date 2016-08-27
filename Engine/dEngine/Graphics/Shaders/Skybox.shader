<Shader Name="Skybox">
	<Passes>
		<Pass Name="Main" VertexShader="VertexMain vs_5_0" PixelShader="PixelMain ps_5_0" />
	</Passes>
</Shader>
#include <Common>
#include <DeferredCommon>

TextureCube Cubemap : register(t0);
TextureCube Starfield : register(t1);

struct VertexOutput
{
	float4 position : SV_POSITION;
	float4 positionObj : POSITION_OBJ;
	float4 normalWS : NORMAL_WORLDSPACE;
	float4 colour : COLOR;
	float starAlpha : STAR_ALPHA;
};

struct PixelOutput
{
	float4 GBuffer0 : SV_TARGET0;
	float4 GBuffer1 : SV_TARGET1;
	float4 GBuffer2 : SV_TARGET2;
	float4 GBuffer3 : SV_TARGET3;
};

cbuffer SkyConstants : register(b1)
{
	float4 Colour0;
	float4 Colour1;
	float StarAlpha;
};

VertexOutput VertexMain(VertexInstanced input)
{
	VertexOutput result;

	input.position *= input.size;

	float4x4 modelMatrix = float4x4(
		float4(1, 0, 0, 0),
		float4(0, 1, 0, 0),
		float4(0, 0, 1, 0),
		float4(Camera.position, 1));
	float4x4 worldViewProj = mul(modelMatrix, Camera.viewProjMatrix);
	float4 position4 = float4(input.position, 1);

	float target = (input.size.y * -1.1);

	result.position = mul(position4, worldViewProj);
	result.positionObj = position4;
	result.normalWS = mul(float4(input.normal, 1), input.modelMatrix);
	result.colour = lerp(Colour1, Colour0, position4.y / target);
	result.starAlpha = StarAlpha;

	return result;
}

PixelOutput PixelMain(VertexOutput input) : SV_TARGET
{
	PixelOutput result;

	float4 sky = Cubemap.Sample(WrapSampler, input.positionObj.xyz);
	float4 stars = Starfield.Sample(DefaultSampler, input.positionObj.xyz);

	Material material;
	material.baseColour = (sky * input.colour) + (stars * input.starAlpha);
	material.worldNormal = input.normalWS;
	material.specular = 0;
	material.smoothness = 0;
	material.metallic = 0;
	material.opacity = 1;
	material.occlusion = 0;
	material.emissiveColour = 0;
	material.perObjectData = 69;
	material.shadingModel = 1;

	EncodeGBuffer(material, result.GBuffer0, result.GBuffer1, result.GBuffer2, result.GBuffer3);

	return result;
}