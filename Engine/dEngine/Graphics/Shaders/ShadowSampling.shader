//=================================================================================================
//
//	Shadows Sample
//  by MJP
//  http://mynameismjp.wordpress.com/
//
//  All code licensed under the MIT license
//
//=================================================================================================
#define FilterAcrossCascades_
#define FilterSize_ 5
#define CASCADE_COUNT 4

#include <PCFKernels>

cbuffer ShadowConstants : register(b2)
{
	row_major float4x4 ShadowMatrix; // 64
	float4 CascadeSplits; // 16
	float4 CascadeOffsets[CASCADE_COUNT]; // 64
	float4 CascadeScales[CASCADE_COUNT]; // 64
	float DepthBias;
	float OffsetScale;
	int VisualizeCascades;
}

float2 ComputeReceiverPlaneDepthBias(float3 texCoordDX, float3 texCoordDY)
{
	float2 biasUV;
	biasUV.x = texCoordDY.y * texCoordDX.z - texCoordDX.y * texCoordDY.z;
	biasUV.y = texCoordDX.x * texCoordDY.z - texCoordDY.x * texCoordDX.z;
	biasUV *= 1.0f / ((texCoordDX.x * texCoordDY.y) - (texCoordDX.y * texCoordDY.x));
	return biasUV;
}

float SampleShadowMap(
	float2 base_uv, float u, float v, float2 shadowMapSizeInv,
	uint cascadeIdx, float depth, float2 receiverPlaneDepthBias)
{
	float2 uv = base_uv + float2(u, v) * shadowMapSizeInv;

#ifdef UsePlaneDepthBias_
	float z = depth + dot(float2(u, v) * shadowMapSizeInv, receiverPlaneDepthBias);
#else
	float z = depth;
#endif

	return ShadowMaps.SampleCmpLevelZero(ShadowPCFSampler, float3(uv, cascadeIdx), z);
}

//-------------------------------------------------------------------------------------------------
// The method used in The Witness
//-------------------------------------------------------------------------------------------------
float SampleShadowMapOptimizedPCF(in float3 shadowPos, in float3 shadowPosDX, in float3 shadowPosDY, in uint cascadeIdx)
{
	float2 shadowMapSize;
	float numSlices;
	ShadowMaps.GetDimensions(shadowMapSize.x, shadowMapSize.y, numSlices);

	float lightDepth = shadowPos.z;

#ifdef UsePlaneDepthBias_
	float2 texelSize = 1.0f / shadowMapSize;

	float2 receiverPlaneDepthBias = ComputeReceiverPlaneDepthBias(shadowPosDX, shadowPosDY);

	// Static depth biasing to make up for incorrect fractional sampling on the shadow map grid
	float fractionalSamplingError = 2 * dot(float2(1.0f, 1.0f) * texelSize, abs(receiverPlaneDepthBias));
	lightDepth -= min(fractionalSamplingError, 0.01f);
#else
	float2 receiverPlaneDepthBias;
	lightDepth -= DepthBias;
#endif

	float2 uv = shadowPos.xy * shadowMapSize; // 1 unit - 1 texel

	float2 shadowMapSizeInv = 1.0 / shadowMapSize;

	float2 base_uv;
	base_uv.x = floor(uv.x + 0.5);
	base_uv.y = floor(uv.y + 0.5);

	float s = (uv.x + 0.5 - base_uv.x);
	float t = (uv.y + 0.5 - base_uv.y);

	base_uv -= float2(0.5, 0.5);
	base_uv *= shadowMapSizeInv;

	float sum = 0;

#if FilterSize_ == 2
	return ShadowMaps.SampleCmpLevelZero(ShadowPCFSampler, float3(shadowPos.xy, cascadeIdx), lightDepth);
#elif FilterSize_ == 3

	float uw0 = (3 - 2 * s);
	float uw1 = (1 + 2 * s);

	float u0 = (2 - s) / uw0 - 1;
	float u1 = s / uw1 + 1;

	float vw0 = (3 - 2 * t);
	float vw1 = (1 + 2 * t);

	float v0 = (2 - t) / vw0 - 1;
	float v1 = t / vw1 + 1;

	sum += uw0 * vw0 * SampleShadowMap(base_uv, u0, v0, shadowMapSizeInv, cascadeIdx, lightDepth, receiverPlaneDepthBias);
	sum += uw1 * vw0 * SampleShadowMap(base_uv, u1, v0, shadowMapSizeInv, cascadeIdx, lightDepth, receiverPlaneDepthBias);
	sum += uw0 * vw1 * SampleShadowMap(base_uv, u0, v1, shadowMapSizeInv, cascadeIdx, lightDepth, receiverPlaneDepthBias);
	sum += uw1 * vw1 * SampleShadowMap(base_uv, u1, v1, shadowMapSizeInv, cascadeIdx, lightDepth, receiverPlaneDepthBias);

	return sum * 1.0f / 16;

#elif FilterSize_ == 5

	float uw0 = (4 - 3 * s);
	float uw1 = 7;
	float uw2 = (1 + 3 * s);

	float u0 = (3 - 2 * s) / uw0 - 2;
	float u1 = (3 + s) / uw1;
	float u2 = s / uw2 + 2;

	float vw0 = (4 - 3 * t);
	float vw1 = 7;
	float vw2 = (1 + 3 * t);

	float v0 = (3 - 2 * t) / vw0 - 2;
	float v1 = (3 + t) / vw1;
	float v2 = t / vw2 + 2;

	sum += uw0 * vw0 * SampleShadowMap(base_uv, u0, v0, shadowMapSizeInv, cascadeIdx, lightDepth, receiverPlaneDepthBias);
	sum += uw1 * vw0 * SampleShadowMap(base_uv, u1, v0, shadowMapSizeInv, cascadeIdx, lightDepth, receiverPlaneDepthBias);
	sum += uw2 * vw0 * SampleShadowMap(base_uv, u2, v0, shadowMapSizeInv, cascadeIdx, lightDepth, receiverPlaneDepthBias);

	sum += uw0 * vw1 * SampleShadowMap(base_uv, u0, v1, shadowMapSizeInv, cascadeIdx, lightDepth, receiverPlaneDepthBias);
	sum += uw1 * vw1 * SampleShadowMap(base_uv, u1, v1, shadowMapSizeInv, cascadeIdx, lightDepth, receiverPlaneDepthBias);
	sum += uw2 * vw1 * SampleShadowMap(base_uv, u2, v1, shadowMapSizeInv, cascadeIdx, lightDepth, receiverPlaneDepthBias);

	sum += uw0 * vw2 * SampleShadowMap(base_uv, u0, v2, shadowMapSizeInv, cascadeIdx, lightDepth, receiverPlaneDepthBias);
	sum += uw1 * vw2 * SampleShadowMap(base_uv, u1, v2, shadowMapSizeInv, cascadeIdx, lightDepth, receiverPlaneDepthBias);
	sum += uw2 * vw2 * SampleShadowMap(base_uv, u2, v2, shadowMapSizeInv, cascadeIdx, lightDepth, receiverPlaneDepthBias);

	return sum * 1.0f / 144;

#else // FilterSize_ == 7

	float uw0 = (5 * s - 6);
	float uw1 = (11 * s - 28);
	float uw2 = -(11 * s + 17);
	float uw3 = -(5 * s + 1);

	float u0 = (4 * s - 5) / uw0 - 3;
	float u1 = (4 * s - 16) / uw1 - 1;
	float u2 = -(7 * s + 5) / uw2 + 1;
	float u3 = -s / uw3 + 3;

	float vw0 = (5 * t - 6);
	float vw1 = (11 * t - 28);
	float vw2 = -(11 * t + 17);
	float vw3 = -(5 * t + 1);

	float v0 = (4 * t - 5) / vw0 - 3;
	float v1 = (4 * t - 16) / vw1 - 1;
	float v2 = -(7 * t + 5) / vw2 + 1;
	float v3 = -t / vw3 + 3;

	sum += uw0 * vw0 * SampleShadowMap(base_uv, u0, v0, shadowMapSizeInv, cascadeIdx, lightDepth, receiverPlaneDepthBias);
	sum += uw1 * vw0 * SampleShadowMap(base_uv, u1, v0, shadowMapSizeInv, cascadeIdx, lightDepth, receiverPlaneDepthBias);
	sum += uw2 * vw0 * SampleShadowMap(base_uv, u2, v0, shadowMapSizeInv, cascadeIdx, lightDepth, receiverPlaneDepthBias);
	sum += uw3 * vw0 * SampleShadowMap(base_uv, u3, v0, shadowMapSizeInv, cascadeIdx, lightDepth, receiverPlaneDepthBias);

	sum += uw0 * vw1 * SampleShadowMap(base_uv, u0, v1, shadowMapSizeInv, cascadeIdx, lightDepth, receiverPlaneDepthBias);
	sum += uw1 * vw1 * SampleShadowMap(base_uv, u1, v1, shadowMapSizeInv, cascadeIdx, lightDepth, receiverPlaneDepthBias);
	sum += uw2 * vw1 * SampleShadowMap(base_uv, u2, v1, shadowMapSizeInv, cascadeIdx, lightDepth, receiverPlaneDepthBias);
	sum += uw3 * vw1 * SampleShadowMap(base_uv, u3, v1, shadowMapSizeInv, cascadeIdx, lightDepth, receiverPlaneDepthBias);

	sum += uw0 * vw2 * SampleShadowMap(base_uv, u0, v2, shadowMapSizeInv, cascadeIdx, lightDepth, receiverPlaneDepthBias);
	sum += uw1 * vw2 * SampleShadowMap(base_uv, u1, v2, shadowMapSizeInv, cascadeIdx, lightDepth, receiverPlaneDepthBias);
	sum += uw2 * vw2 * SampleShadowMap(base_uv, u2, v2, shadowMapSizeInv, cascadeIdx, lightDepth, receiverPlaneDepthBias);
	sum += uw3 * vw2 * SampleShadowMap(base_uv, u3, v2, shadowMapSizeInv, cascadeIdx, lightDepth, receiverPlaneDepthBias);

	sum += uw0 * vw3 * SampleShadowMap(base_uv, u0, v3, shadowMapSizeInv, cascadeIdx, lightDepth, receiverPlaneDepthBias);
	sum += uw1 * vw3 * SampleShadowMap(base_uv, u1, v3, shadowMapSizeInv, cascadeIdx, lightDepth, receiverPlaneDepthBias);
	sum += uw2 * vw3 * SampleShadowMap(base_uv, u2, v3, shadowMapSizeInv, cascadeIdx, lightDepth, receiverPlaneDepthBias);
	sum += uw3 * vw3 * SampleShadowMap(base_uv, u3, v3, shadowMapSizeInv, cascadeIdx, lightDepth, receiverPlaneDepthBias);

	return sum * 1.0f / 2704;

#endif
}

//-------------------------------------------------------------------------------------------------
// Samples the appropriate shadow map cascade
//-------------------------------------------------------------------------------------------------
float3 SampleShadowCascade(in float3 shadowPosition, in float3 shadowPosDX,
	in float3 shadowPosDY, in uint cascadeIdx,
	in uint2 screenPos)
{
	shadowPosition += CascadeOffsets[cascadeIdx].xyz;
	shadowPosition *= CascadeScales[cascadeIdx].xyz;

	shadowPosDX *= CascadeScales[cascadeIdx].xyz;
	shadowPosDY *= CascadeScales[cascadeIdx].xyz;

	float3 cascadeColor = 1.0f;

	if (VisualizeCascades == 1)
	{
		const float3 CascadeColors[CASCADE_COUNT] =
		{
			float3(1.0f, 0.0, 0.0f),
			float3(0.0f, 1.0f, 0.0f),
			float3(0.0f, 0.0f, 1.0f),
			float3(1.0f, 1.0f, 0.0f)
		};

		cascadeColor = CascadeColors[cascadeIdx];
	}

	/*
	#if UseEVSM_
	float shadow = SampleShadowMapEVSM(shadowPosition, shadowPosDX, shadowPosDY, cascadeIdx);
	#elif UseMSM_
	//float shadow = SampleShadowMapMSM(shadowPosition, shadowPosDX, shadowPosDY, cascadeIdx);
	#elif ShadowMode_ == ShadowModeVSM_
	//float shadow = SampleShadowMapVSM(shadowPosition, shadowPosDX, shadowPosDY, cascadeIdx);
	#elif ShadowMode_ == ShadowModeFixedSizePCF_
	//float shadow = SampleShadowMapFixedSizePCF(shadowPosition, shadowPosDX, shadowPosDY, cascadeIdx);
	#elif ShadowMode_ == ShadowModeGridPCF_
	//float shadow = SampleShadowMapGridPCF(shadowPosition, shadowPosDX, shadowPosDY, cascadeIdx);
	#elif ShadowMode_ == ShadowModeRandomDiscPCF_
	//float shadow = SampleShadowMapRandomDiscPCF(shadowPosition, shadowPosDX, shadowPosDY, cascadeIdx, screenPos);
	#else //if ShadowMode_ == SampleShadowMapOptimizedPCF_*/
	float shadow = SampleShadowMapOptimizedPCF(shadowPosition, shadowPosDX, shadowPosDY, cascadeIdx);
	//#endif

	return shadow * cascadeColor;
}

float3 GetShadowPosOffset(in float nDotL, in float3 normal)
{
	float2 shadowMapSize;
	float numSlices;
	ShadowMaps.GetDimensions(shadowMapSize.x, shadowMapSize.y, numSlices);
	float texelSize = 2.0f / shadowMapSize.x;
	float nmlOffsetScale = saturate(1.0f - nDotL);
	return texelSize * OffsetScale * nmlOffsetScale * normal;
}

float3 ShadowVisibility(in float3 positionWS, in float depthVS, in float nDotL, in float3 normal,
	in uint2 screenPos)
{
	float3 shadowVisibility = 1.0f;
	uint cascadeIdx = 0;

	// Figure out which cascade to sample from
	[unroll]
	for (int i = 0; i < CASCADE_COUNT - 1; ++i)
	{
		[flatten]
		if (depthVS > CascadeSplits[i])
			cascadeIdx = i + 1;
	}

	// Apply offset
	float3 offset = GetShadowPosOffset(nDotL, normal) / abs(CascadeScales[cascadeIdx].z);

	// Project into shadow space
	float3 samplePos = positionWS + offset;
	float3 shadowPosition = mul(float4(samplePos, 1.0f), ShadowMatrix).xyz;
	float3 shadowPosDX = ddx_fine(shadowPosition);
	float3 shadowPosDY = ddy_fine(shadowPosition);

	shadowVisibility = SampleShadowCascade(shadowPosition, shadowPosDX, shadowPosDY,
		cascadeIdx, screenPos);

#ifdef FilterAcrossCascades_
	// Sample the next cascade, and blend between the two results to
	// smooth the transition
	const float BlendThreshold = 0.1f;
	float nextSplit = CascadeSplits[cascadeIdx];
	float splitSize = cascadeIdx == 0 ? nextSplit : nextSplit - CascadeSplits[cascadeIdx - 1];
	float splitDist = (nextSplit - depthVS) / splitSize;

	[branch]
	if (splitDist <= BlendThreshold && cascadeIdx != CASCADE_COUNT - 1)
	{
		float3 nextSplitVisibility = SampleShadowCascade(shadowPosition, shadowPosDX,
			shadowPosDY, cascadeIdx + 1,
			screenPos);
		float lerpAmt = smoothstep(0.0f, BlendThreshold, splitDist);
		shadowVisibility = lerp(nextSplitVisibility, shadowVisibility, lerpAmt);
	}
#endif

	return shadowVisibility;
}