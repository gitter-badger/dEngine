<Shader Name="SMAA">
	<Passes>
		<Pass Name="LumaEdgeDetection" VertexShader="VertexScreenSpace vs_5_0" PixelShader="LumaEdgeDetection ps_5_0" />
		<Pass Name="ColourEdgeDetection" VertexShader="VertexScreenSpace vs_5_0" PixelShader="ColourEdgeDetection ps_5_0" />
		<Pass Name="DepthEdgeDetection" VertexShader="VertexScreenSpace vs_5_0" PixelShader="DepthEdgeDetection ps_5_0" />
	</Passes>
</Shader>
#include <Common>
#include <ScreenSpace>

Texture2D ColourTexture : register(t0);
Texture2D CameraDepthTexture : register(t1);
Texture2D CameraNormalsTexture : register(t2);
Texture2D DitherTexture : register(t3);
Texture2D SsaoTexture : register(t4); // Merge

float4 LumaEdgeDetection(VertexOutput input) : SV_TARGET
{
	return 0;
}

float4 ColourEdgeDetection(VertexOutput input) : SV_TARGET
{
	return 0;
}

float4 DepthEdgeDetection(VertexOutput input) : SV_TARGET
{
	return 0;
}