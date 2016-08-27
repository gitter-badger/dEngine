<Shader Name="PostProcess">
	<Passes>
		<Pass Name="Downsample4x4Glow" VertexShader="Downsample4x4VS vs_5_0" PixelShader="Downsample4x4GlowPS ps_5_0"/>
		<Pass Name="Downsample4x4Avg" VertexShader="Downsample4x4VS vs_5_0" PixelShader="Downsample4x4AvgPS ps_5_0"/>
		<Pass Name="Downsample4x4Max" VertexShader="Downsample4x4VS vs_5_0" PixelShader="Downsample4x4MaxPS ps_5_0"/>
		<Pass Name="GodRays" VertexShader="VertexScreenSpace vs_5_0" PixelShader="GodRaysPS ps_5_0"/>
		<Pass Name="RadialBlur" VertexShader="VertexScreenSpace vs_5_0" PixelShader="RadialBlurPS ps_5_0"/>
		<Pass Name="GammaCorrection" VertexShader="VertexScreenSpace vs_5_0" PixelShader="GammaCorrectionPS ps_5_0"/>
		<Pass Name="Upsample" VertexShader="VertexScreenSpace vs_5_0" PixelShader="UpsamplePS ps_5_0"/>
	</Passes>
</Shader>
#include <Common>
#include <ScreenSpace>

Texture2D<float4> Texture0 : register(t0);
Texture2D<float4> Texture1 : register(t1);
Texture2D<float4> Texture2 : register(t2);
Texture2D<float4> Texture3 : register(t3);

struct VertexOutput_4uv
{
	float4 position : SV_POSITION;
	float2 uv       : TEXCOORD0;
	float4 uv12     : TEXCOORD1;
	float4 uv34     : TEXCOORD2;
};

cbuffer PostProcessConstants : register(b1)
{
	float4 TextureSize;
	float4 Params1;
	float4 Params2;
	float4 Params3;
	float4 Params4;
	float4 BloomParams; // .x - strength (0 disables), .y - threshold
}

float2 ConvertUv(float4 p)
{
	return p.xy * 0.5 + 0.5;
}

VertexOutput_4uv Downsample4x4VS(uint id: SV_VERTEXID)
{
	VertexOutput_4uv result;

	result.uv = float2((id << 1) & 2, id & 2);
	result.position = float4(result.uv * float2(2.0f, -2.0f) + float2(-1.0f, 1.0f), 0.0f, 1.0f);

	float2 uvOffset = TextureSize.zw * 0.25f;

	result.uv12.xy = result.uv + uvOffset * float2(-1, -1);
	result.uv12.zw = result.uv + uvOffset * float2(+1, -1);
	result.uv34.xy = result.uv + uvOffset * float2(-1, +1);
	result.uv34.zw = result.uv + uvOffset * float2(+1, +1);

	return result;
}

float3 NDCToViewSpace(float2 coord)
{
	return normalize(float3(coord * Params4.xy + Params4.zw, -1));
}

float4 GlowBloom(float4 colour)
{
	float glowFactor = 1 - colour.a;
	float bloomFactor = BloomParams.x * pow(max(max(colour.r, colour.g), colour.b), BloomParams.y);
	return float4(colour.rgb, 1) * max(glowFactor, bloomFactor);
}

float4 Downsample4x4GlowPS(VertexOutput_4uv input) : SV_TARGET
{
	float4 gb0 = GlowBloom(Texture0.Sample(DefaultSampler, input.uv12.xy));
	float4 gb1 = GlowBloom(Texture0.Sample(DefaultSampler, input.uv12.zw));
	float4 gb2 = GlowBloom(Texture0.Sample(DefaultSampler, input.uv34.xy));
	float4 gb3 = GlowBloom(Texture0.Sample(DefaultSampler, input.uv34.zw));

	return (gb0 + gb1 + gb2 + gb3) * 0.25;
}

float4 Downsample4x4MaxPS(VertexOutput_4uv input) : SV_TARGET
{
	float3 avgColor = Texture0.Sample(DefaultSampler, input.uv12.xy).rgb;
	avgColor = max(avgColor, Texture0.Sample(DefaultSampler, input.uv12.zw).rgb);
	avgColor = max(avgColor, Texture0.Sample(DefaultSampler, input.uv34.xy).rgb);
	avgColor = max(avgColor, Texture0.Sample(DefaultSampler, input.uv34.zw).rgb);

	return float4(avgColor, 1);
}

float4 Downsample4x4AvgPS(VertexOutput_4uv input) : SV_TARGET
{
	float4 avgColor = Texture0.Sample(DefaultSampler, input.uv12.xy);
	avgColor += Texture0.Sample(DefaultSampler, input.uv12.zw);
	avgColor += Texture0.Sample(DefaultSampler, input.uv34.xy);
	avgColor += Texture0.Sample(DefaultSampler, input.uv34.zw);

	return avgColor * 0.25;
}

#define RADIAL_BLUR_SAMPLES 12
#define SUN_SHAFTS_SAMPLES 12
#define SUN_SHAFTS_NOISE_RES 8

float AABQIntersection(float2 texel, float2 sunPos)
{
	float2 texToSun = sunPos - texel;

	float2 t = (sign(texToSun) - texel) / texToSun;
	t.x = t.x < 0 ? 1 : t.x;
	t.y = t.y < 0 ? 1 : t.y;
	return min(min(t.x, t.y), 1);
}

float HenyeyGreenstein(float dotLV, float g)
{
	float result = 1.0f - g;
	result *= result;
	result /= (4.0f * PI * pow(1.0f + g * g - (2.0f * g) * dotLV, 1.5f));
	return result;
}

float4 RadialBlurPS(VertexOutput input) : SV_TARGET0
{
	float2 texCoord = input.texCoord;

	float2 posNDC = input.position.xy;
	float2 blurCenterNDC = Params1.xy;
	float2 dirNDC = posNDC - blurCenterNDC;

	float2 deltaTexCoord = dirNDC * 0.5;
	deltaTexCoord *= AABQIntersection(posNDC, blurCenterNDC);
	deltaTexCoord *= 1.0 / RADIAL_BLUR_SAMPLES * 0.5;

	float3 sum = float3(0, 0, 0);
	for (int i = 0; i < RADIAL_BLUR_SAMPLES; i++)
	{
		texCoord -= deltaTexCoord;
		sum += Texture0.Sample(DefaultSampler, texCoord).rgb;
	}
	return float4(sum / RADIAL_BLUR_SAMPLES, 1);
}

float4 GodRaysPS(VertexOutput input) : SV_TARGET
{
	float3 shaftsColor = Params3.rgb;
	float intensity = Params2.x;

	float2 ScreenLightPos = Params1.xy;

	float2 posNDC = input.position.xy;
	float2 sunPosNDC = ScreenLightPos.xy;
	float2 dirNCD = posNDC - sunPosNDC;

	float depth = Texture0.Sample(DefaultSampler, input.texCoord);

	float2 deltaTexCoord = dirNCD * 0.5; // *0.5 to go to Tex space
	deltaTexCoord *= AABQIntersection(posNDC, sunPosNDC);
	deltaTexCoord *= 1.0f / SUN_SHAFTS_SAMPLES;

	float2 texCoord = input.texCoord;
	float noiseSample = Texture1.Sample(DefaultSampler, texCoord * TextureSize.xy / 8).r;
	texCoord += deltaTexCoord * noiseSample;

	float value = 0;
	for (int i = 0; i < SUN_SHAFTS_SAMPLES; i++)
	{
		texCoord -= deltaTexCoord;
		value += (Texture0.Sample(DefaultSampler, texCoord).r >= 0.999);
	}

	float3 positionWS = ReconstructPositionWS(posNDC, depth);
	float3 viewDir = normalize(Camera.position - positionWS);
	float3 sunDir = float3(Params1.zw, Params2.w);
	float dotLV = saturate(dot(sunDir, viewDir));

	return float4(shaftsColor * (value * intensity * HenyeyGreenstein(dotLV, 0)), 1);
}

float4 GammaCorrectionPS(VertexOutput input) : SV_TARGET
{
	float4 colour = Texture0.Sample(DefaultSampler, input.texCoord);
	float gamma = 2.2;
	return pow(colour, gamma / 1);
}

float4 UpsamplePS(VertexOutput input) : SV_TARGET
{
	float4 colour = Texture0.Sample(DefaultSampler, input.texCoord);
	return colour;
}