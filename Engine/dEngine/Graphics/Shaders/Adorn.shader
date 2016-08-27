<Shader Name="Adorn">
	<Passes>
		<Pass Name="AALine" VertexShader="AALineVS vs_5_0" PixelShader="AALinePS ps_5_0"/>
		<Pass Name="AdornSelfLit" VertexShader="AdornSelfLitVS vs_5_0" PixelShader="AdornSelfLitPS ps_5_0"/>
	</Passes>
</Shader>

#include <Common>
#include <DeferredCommon>

struct VertexOutput
{
	float4 position : SV_POSITION;
	float3 colour : COLOR0;
	float transparency : TRANSPARENCY;
};

struct AALineVertexOutput
{
	float4 positionH : SV_POSITION;
	float4 position : POSITION1;
	float4 colour : COLOUR;
	float4 start : START;
	float4 end : END;
	float lineThickness : LINETHICKNESS;
};

cbuffer AALineConstants
{
	float LineThickness;
};

AALineVertexOutput AALineVS(VertexInstanced input)
{
	AALineVertexOutput result;

	input.position *= input.size;

	float4x4 worldMatrix = input.modelMatrix;
	float4x4 worldViewProj = mul(worldMatrix, Camera.viewProjMatrix);
	float4 position4 = float4(input.position, 1);
	float4 normal4 = float4(input.normal, 0);
	float4 positionWS = mul(position4, worldMatrix);
	float4 normalWS = mul(normal4, worldMatrix);

	float4 startPosW = mul(worldMatrix, float4(1, 0, 0, 1));
	float4 endPosW = mul(worldMatrix, float4(-1, 0, 0, 1));

	float w = dot(Camera.viewProjMatrix[3], position4);

	float radius = LineThickness + 2;

	if (length(positionWS - startPosW) < length(positionWS - endPosW))
	{
		float w = dot(Camera.viewProjMatrix[3], float4(startPosW.xyz, 1.0f));
		float pixel_radius = radius * w * Camera.ScreenParams.w;
		positionWS.xyz = startPosW.xyz + normalWS * pixel_radius;
	}
	else
	{
		float w = dot(Camera.viewProjMatrix[3], float4(endPosW.xyz, 1.0f));
		float pixel_radius = radius * w * Camera.ScreenParams.w;
		positionWS.xyz = endPosW.xyz + normalWS * pixel_radius;
	}

	result.positionH = mul(position4, worldViewProj);
	result.position = result.positionH;
	result.start = mul(startPosW, Camera.viewProjMatrix);
	result.end = mul(endPosW, Camera.viewProjMatrix);
	result.lineThickness = LineThickness;

	result.position.y *= Camera.ScreenParams.z;
	result.start.y *= Camera.ScreenParams.z;
	result.end.y *= Camera.ScreenParams.z;

	result.colour = input.instanceColour;

	return result;
}

float4 AALinePS(AALineVertexOutput input) : SV_TARGET0
{
	float4 colour = 1;
	/*
	input.position /= input.position.w;
	input.start /= input.start.w;
	input.end /= input.end.w;

	float2 lineDir = normalize(input.end.xy - input.start.xy);
	float2 fragToPoint = input.position.xy - input.start.xy;

	float startDist = dot(lineDir, fragToPoint);
	float endDist = dot(lineDir, -input.position.xy + input.end.xy);

	if (startDist < 0)
		discard;

	if (endDist < 0)
		discard;

	float2 perpLineDir = float2(lineDir.y, -lineDir.x);
	float dist = abs(dot(perpLineDir, fragToPoint));
	float highPoint = 1 + (input.lineThickness - 1) * 0.5;

	colour.a = saturate(highPoint - (dist * 0.5 * Camera.ScreenParams.x));
	colour *= input.colour;
	colour.a = pow(saturate(1 - colour.a), 1 / 2.2);
	colour.a = 1 - colour.a;
	*/
	// todo: fix
	colour = input.colour;

	return colour;
}

VertexOutput AdornSelfLitVS(VertexInstanced input)
{
	VertexOutput result;

	input.position *= input.size;

	float4 positionWS = mul(float4(input.position, 1), input.modelMatrix);
	float4 normalWS = mul(float4(input.normal, 0), input.modelMatrix);

	float3 light = normalize(Camera.position - positionWS.xyz);
	float3 ndotl = saturate(dot(normalWS, light));

	float3 ambient = 1.0f;

	float3 lighting = ambient + (1 - (ambient*input.instanceColour)) * ndotl;
	float3 specular = pow(ndotl, 64.0);

	result.position = mul(positionWS, Camera.viewProjMatrix);
	result.colour = float3(input.instanceColour * lighting + specular);
	result.transparency = input.transparency;

	return result;
}

float4 AdornSelfLitPS(VertexOutput input) : SV_TARGET0
{
	return float4(input.colour, 1 - input.transparency);
}






