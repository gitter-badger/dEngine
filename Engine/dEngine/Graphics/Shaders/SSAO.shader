<Shader Name="SSAO">
	<Passes>
		<Pass Name="SSAO_1" VertexShader="VertexScreenSpace vs_5_0" PixelShader="SSAOPS ps_5_0"/>
		<Pass Name="SSAO_2" VertexShader="VertexScreenSpace vs_5_0" PixelShader="SSAOPS ps_5_0">
			<Defines>
				<Define>SAMPLE_NOISE</Define>
			</Defines>
		</Pass>
		<Pass Name="SSAO_3" VertexShader="VertexScreenSpace vs_5_0" PixelShader="SSAOPS ps_5_0">
			<Defines>
				<Define>LIGHTING_CONTRIBUTION</Define>
			</Defines>
		</Pass>
		<Pass Name="SSAO_4" VertexShader="VertexScreenSpace vs_5_0" PixelShader="SSAOPS ps_5_0">
			<Defines>
				<Define>SAMPLE_NOISE</Define>
				<Define>LIGHTING_CONTRIBUTION</Define>
			</Defines>
		</Pass>
		<Pass Name="GaussianBlur" VertexShader="VertexScreenSpace vs_5_0" PixelShader="GaussianBlurPS ps_5_0"/>
		<Pass Name="HighQualityBilateralBlur" VertexShader="VertexScreenSpace vs_5_0" PixelShader="HQBilateralBlurPS ps_5_0"/>
		<Pass Name="Composite" VertexShader="VertexScreenSpace vs_5_0" PixelShader="Composite ps_5_0"/>
	</Passes>
</Shader>

#include <Common>
#include <ScreenSpace>

Texture2D MainTexture : register(t0);
Texture2D SSAOTexture : register(t1);
Texture2D DepthTexture : register(t2);
Texture2D NormalTexture : register(t3);
Texture2D NoiseTexture : register(t4);

cbuffer SSAOConstants
{
	float4 MainTextureTexelSize;
	float4 Params1;
	float4 Params2;
	float4 OcclusionColour;
	float2 Direction;
	float BilateralThreshold;
};

float4 SSAOPS(VertexOutput input) : SV_TARGET
{
	float4 scene = MainTexture.Sample(DefaultSampler, input.texCoord);
	return scene * float4(1, 0.6, 0.6, 1);
}

float4 GaussianBlurPS(VertexOutput input) : SV_TARGET
{
	return 0;
}

float4 HQBilateralBlurPS(VertexOutput input) : SV_TARGET
{
	return 0;
}

float4 Composite(VertexOutput input) : SV_TARGET
{
	return 0;
}