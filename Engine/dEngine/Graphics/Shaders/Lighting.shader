<Shader Name="Lighting">
	<Passes>
		<Pass Name="Main" VertexShader="VertexScreenSpace vs_5_0" PixelShader = "PixelMain ps_5_0"/>
	</Passes>
</Shader>

#include <Common>
#include <DeferredCommon>
#include <ScreenSpace>

Texture2D GBuffer0 : register(t0);
Texture2D GBuffer1 : register(t1);
Texture2D GBuffer2 : register(t2);
Texture2D GBuffer3 : register(t3);
Texture2D DepthMap : register(t4);
TextureCube IrradianceMap : register(t5);
TextureCube RadianceMap : register(t6);

cbuffer LightingConstants : register(b1)
{
	PointLight SunLight;
	float4 AmbientColour;
	float4 OutdoorAmbient;
};

//#include <ShadowSampling>

float3 BRDF_Diffuse_Lambert(float3 diffuseColour)
{
	return diffuseColour * RECIPROCAL_PI;
}

float F_Schlick(float3 specularColour, float dotHV)
{
	return specularColour + (1.0 - specularColour) * pow(1.0 - dotHV, 5.0);
	//float fresnel = exp2((-5.55473 * NdL - 6.98316) * NdL);
	//return (1.0 - specularColour) * fresnel + specularColour;
}

float D_GGX(float alpha, float dotNH)
{
	float a2 = pow2(alpha);
	float denom = pow2(dotNH) * (a2 - 1.0) + 1.0;
	return a2 / (RECIPROCAL_PI * denom * denom);
}

float G_GGX_SmithCorrelated(float alpha, float dotNL, float dotNV) 
{
	float k = alpha * 0.5;

	float gv = dotNV + sqrt((dotNV - dotNV * k) * dotNV + k);
	float gl = dotNL + sqrt((dotNL - dotNL * k) * dotNL + k);

	return 1.0 / max(gv * gl, 0.01);
}

float3 UE4MobileEnvBRDF(float3 SpecularColor, float Roughness, float NoV)
{
	const float4 c0 = { -1, -0.0275, -0.572, 0.022 };
	const float4 c1 = { 1, 0.0425, 1.04, -0.04 };
	half4 r = Roughness * c0 + c1;
	half a004 = min(r.x * r.x, exp2(-9.28 * NoV)) * r.x + r.y;
	half2 AB = float2(-1.04, 1.04) * a004 + r.zw;
	return SpecularColor * AB.x + AB.y;
}

float3 EnvDFGPolynomial(float3 specularColor, float gloss, float ndotv)
{
	float x = gloss;
	float y = ndotv;

	float b1 = -0.1688;
	float b2 = 1.895;
	float b3 = 0.9903;
	float b4 = -4.853;
	float b5 = 8.404;
	float b6 = -5.069;
	float bias = saturate(min(b1 * x + b2 * x * x, b3 + b4 * y + b5 * y * y + b6 * y * y * y));

	float d0 = 0.6045;
	float d1 = 1.699;
	float d2 = -0.5228;
	float d3 = -3.603;
	float d4 = 1.404;
	float d5 = 0.1939;
	float d6 = 2.661;
	float delta = saturate(d0 + d1 * x + d2 * y + d3 * x * x + d4 * x * y + d5 * y * y + d6 * x * x * x);
	float scale = delta - bias;

	bias *= saturate(50.0 * specularColor.y);
	return specularColor * scale + bias;
}

/*
void ComputeDirectLightRBX(float2 screenCoord, Material material, float3 normal, float3 lightDir, float3 viewDir, 
	float3 lightColour, float3 positionWS, float depth, out float3 diffuseResult, out float3 specularResult)
{
	// Dots
	float3 halfVec = normalize(lightDir + viewDir);
	float dotNL = saturate(dot(normal, lightDir));
	float dotLH = saturate(dot(lightDir, halfVec));

	float shadowVis = 1.0f;

	float diffuseIntensity = saturate(dotNL) * lightColour + max(-dotNL, 0) * lightColour;
	diffuseResult = (AmbientColour + diffuseIntensity * shadowVis + lightColour) * material.baseColour;

	float rbxSpecular = material.metallic;
	float rbxGloss = material.smoothness;
	float specularIntensity = step(0, dotNL) * rbxSpecular;
	float specularPower = rbxGloss;

	specularResult = lightColour * (specularIntensity * shadowVis * (float)(half)pow(saturate(dot(normal, normalize(-lightColour + normalize(viewDir)))), specularPower));
}

float3 RobloxShading(float2 screenCoord, MaterialConfig cfg, Material material, float3 viewDir, float3 normal, float3 positionWS, float depth)
{

	float specularPower = material.metallic;
	float gloss = material.smoothness;

	float3 result = 0;

	float dotNV = saturate(dot(normal, viewDir));

	// TODO: per light -----------------------------
	float3 lightDir = -SunLight.position.xyz;
	float3 lightColour = SunLight.colour;

	float fallOff = 1;

	float3 directDiffuse;
	float3 directSpecular;

	ComputeDirectLightRBX(screenCoord, material, normal, lightDir, viewDir, lightColour,
		positionWS, depth, directDiffuse, directSpecular);
	float3 directLighting = directDiffuse + directSpecular;

	result += directLighting * fallOff;
	// ------------------------

	return result;
}
*/

void ComputeDirectLightPBR(float2 screenCoord, Material material, float3 normal, float3 lightDir, float3 viewDir,
	float3 lightColour, float dotNV, float3 positionWS, float depth, float roughness,
	out float3 diffuseResult, out float3 specularResult)
{
	// Dots
	float3 halfVec = normalize(lightDir + viewDir);
	float dotNL = saturate(dot(normal, lightDir));
	float dotNH = saturate(dot(normal, halfVec));
	float dotLH = saturate(dot(lightDir, halfVec));
	float dotHV = saturate(dot(viewDir, halfVec));

	//float3 shadowVis = ShadowVisibility(positionWS.xyz, depth, dotNL, normal, uint2(screenCoord));

	diffuseResult = dotNL * lightColour * 1;

	// Compute reduction function to have
	// energy-conserving blinn-phong specular.
	// NOTE: This includes the division by 4 as well
	// as the multiplication by PI from the lighting equation.
	// The reduction factor for Phong is        (g + 2) / 2pi
	// The reduction factor for Blinn-Phong     (g + 8) / 8pi
	// The reduction factor for Blinn-Phong IBL (g + 2) / 8pi
	// float reduction = (material.smoothness + 2.0) / 8.0;

	float alpha = roughness*roughness;
	float fresnel = F_Schlick(material.specular, dotHV);
	float visibility = G_GGX_SmithCorrelated(alpha, dotNL, dotNV);
	float distro = D_GGX(alpha, dotNH);

	float specular = distro * fresnel * visibility * dotNL;

	specularResult = specular * lightColour;
}

float3 StandardShading(in float2 screenCoord, in Material material, in float3 viewDir, in float3 normal, in float3 positionWS, in float depth)
{
	float3 result = 0;

	/// Smoothness re-mapping
	float roughness = rough(material.smoothness);
	float linearRoughness = 1 - material.smoothness;

	float3 baseColour = material.baseColour;
	float metallic = material.metallic;

	float nonMetalSpec = 0.08 * material.specular;
	float3 specularColour = (nonMetalSpec - nonMetalSpec * metallic) + baseColour * metallic;
	float3 diffuseColour = baseColour - baseColour * metallic;

	float dotNV = saturate(dot(normal, viewDir));

	// TODO: per light -----------------------------
	float3 lightDir = SunLight.position.xyz;
	float3 lightColour = SunLight.colour;

	float fallOff = 1;

	float3 directDiffuse;
	float3 directSpecular;

	ComputeDirectLightPBR(screenCoord, material, normal, lightDir, viewDir, lightColour, dotNV, positionWS, depth, roughness,
		directDiffuse, directSpecular);
	float3 directLighting = diffuseColour.rgb * directDiffuse + directSpecular * specularColour.rgb;

	result += directLighting * fallOff;

	// ------------------------

	float4 irradiance = IrradianceMap.Sample(BrickSampler, normal);
	float3 rv = reflect(-viewDir.xyz, normal.xyz);

	// Horizon Fade (http://marmosetco.tumblr.com/post/81245981087)
	float horiz = dot(rv, normal.xyz);
	float horizFadePower = 1.0 - roughness;
	horiz = saturate(1.0 + horizFadePower * horiz);
	horiz *= horiz;

	float3 indirectDiffuse = 0;
	float3 indirectSpecular = 0;
	indirectDiffuse = irradiance * material.baseColour.rgb;

	float mipLevel = log2(roughness) * 1.2 + 6.0 - 1.0;
	float3 prefilteredColour = RadianceMap.SampleLevel(BrickSampler, rv, mipLevel);
	indirectSpecular = prefilteredColour * EnvDFGPolynomial(specularColour, material.smoothness, dotNV);
	indirectSpecular *= horiz;

	float3 indirectLighting = indirectDiffuse + indirectSpecular;
	result += indirectLighting;

	//float dotNL = saturate(dot(normal, lightDir));
	//float3 shadowVis = ShadowVisibility(positionWS.xyz, depth, dotNL, normal, uint2(screenCoord));
	//result *= shadowVis;

	return result;
}

float4 PixelMain(VertexOutput input) : SV_TARGET0
{
	float4 gbuffer0 = GBuffer0.Sample(DefaultSampler, input.texCoord);
	float4 gbuffer1 = GBuffer1.Sample(DefaultSampler, input.texCoord);
	float4 gbuffer2 = GBuffer2.Sample(DefaultSampler, input.texCoord);
	float4 gbuffer3 = GBuffer3.Sample(DefaultSampler, input.texCoord);

	Material material;
	DecodeGBuffer(gbuffer0, gbuffer1, gbuffer2, gbuffer3, material);

	float depth = DepthMap.Sample(DefaultSampler, input.texCoord);
	float3 positionWS = ReconstructPositionWS(input.texCoord, depth);

	float3 N = normalize(material.worldNormal);
	float3 V = normalize(Camera.position - positionWS);

	float3 outputColour;

	switch (material.shadingModel)
	{
		case SHADINGMODEL_UNLIT:
			outputColour = material.baseColour;
			break;
		case SHADINGMODEL_STANDARD:
			outputColour = StandardShading(input.position.xy, material, V, N, positionWS, depth);
			break;
		/*
		case SHADINGMODEL_ROBLOX:
			outputColour = material.baseColour; // not supported
			break;
			*/
		case SHADINGMODEL_CLEARCOAT:
			outputColour = material.baseColour; // TODO
			break;
	}

	//outputColour = N;

	return float4(outputColour, 1);
}
