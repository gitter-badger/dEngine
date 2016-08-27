<Shader Name = "Atmosphere">
	<Passes>
		<Pass Name = "PrecomputeTransmittance" VertexShader = "VertexScreenSpace vs_5_0" PixelShader = "PrecomputeTransmittance ps_5_0" />
		<Pass Name = "PrecomputeInscatter" VertexShader = "VertexScreenSpace vs_5_0" PixelShader = "PrecomputeInscatter ps_5_0" />
	</Passes>
</Shader>
#include <Common>
#include <ScreenSpace>

Texture2D InputTexture : register(t0);

cbuffer PostProcessConstants : register(b1)
{
	float4 TextureSize;
	float4 Params1;
	float4 Params2;
	float4 Params3;
	float4 Params4;
	float4 BloomParams; // .x - strength (0 disables), .y - threshold
}

cbuffer Constants : register(b1)
{
	float4 BetaR;
	int ResR;
};

float4 PrecomputeTransmittance(VertexOutput input) : SV_TARGET0
{
	float2 TexCoord = input.texCoord;

	float r_1;
	float muS_2;
	r_1 = (6360.0 + ((TexCoord.y * TexCoord.y) * 60.0));
	float theta_3;
	theta_3 = (1.5 * TexCoord.x);
	muS_2 = (-0.15 + ((
	(sin(theta_3) / cos(theta_3))
	/ 14.10142) * 1.15));
	float r_4;
	r_4 = r_1;
	float mu_5;
	mu_5 = muS_2;
	float yi_7;
	float dx_8;
	float result_9;
	result_9 = 0.0;
	float dout_10;
	float tmpvar_11;
	tmpvar_11 = ((-(r_1)* muS_2) + sqrt((
	((r_1 * r_1) * ((muS_2 * muS_2) - 1.0))
	+ 4.122924e+07)));
	dout_10 = tmpvar_11;
	float tmpvar_12;
	tmpvar_12 = (((r_1 * r_1) * (
	(muS_2 * muS_2)
	- 1.0)) + 4.04496e+07);
	if ((tmpvar_12 >= 0.0)) {
		float tmpvar_13;
		tmpvar_13 = ((-(r_1)* muS_2) - sqrt(tmpvar_12));
		if ((tmpvar_13 >= 0.0)) {
			dout_10 = min(tmpvar_11, tmpvar_13);
		};
	};
	dx_8 = (dout_10 / 50.0);
	yi_7 = exp(((6360.0 - r_1) / 8.0));
	for (int i_6 = 1; i_6 <= 50; i_6++) {
		float tmpvar_14;
		tmpvar_14 = (float(i_6) * dx_8);
		float tmpvar_15;
		tmpvar_15 = exp(((6360.0 -
			sqrt((((r_4 * r_4) + (tmpvar_14 * tmpvar_14)) + ((2.0 * tmpvar_14) * (r_4 * mu_5))))
			) / 8.0));
		result_9 = (result_9 + ((
			(yi_7 + tmpvar_15)
			/ 2.0) * dx_8));
		yi_7 = tmpvar_15;
	};
	float tmpvar_16;
	tmpvar_16 = sqrt((1.0 - (
	(6360.0 / r_1)
	*
	(6360.0 / r_1)
	)));
	float tmpvar_17;
	if ((muS_2 < -(tmpvar_16))) {
		tmpvar_17 = 1e+09;
	}
	else {
		tmpvar_17 = result_9;
	};
	float r_18;
	r_18 = r_1;
	float mu_19;
	mu_19 = muS_2;
	float yi_21;
	float dx_22;
	float result_23;
	result_23 = 0.0;
	float dout_24;
	float tmpvar_25;
	tmpvar_25 = ((-(r_1)* muS_2) + sqrt((
	((r_1 * r_1) * ((muS_2 * muS_2) - 1.0))
	+ 4.122924e+07)));
	dout_24 = tmpvar_25;
	float tmpvar_26;
	tmpvar_26 = (((r_1 * r_1) * (
	(muS_2 * muS_2)
	- 1.0)) + 4.04496e+07);
	if ((tmpvar_26 >= 0.0)) {
		float tmpvar_27;
		tmpvar_27 = ((-(r_1)* muS_2) - sqrt(tmpvar_26));
		if ((tmpvar_27 >= 0.0)) {
			dout_24 = min(tmpvar_25, tmpvar_27);
		};
	};
	dx_22 = (dout_24 / 50.0);
	yi_21 = exp(((6360.0 - r_1) / 1.2));
	for (int i_20 = 1; i_20 <= 50; i_20++) {
		float tmpvar_28;
		tmpvar_28 = (float(i_20) * dx_22);
		float tmpvar_29;
		tmpvar_29 = exp(((6360.0 -
			sqrt((((r_18 * r_18) + (tmpvar_28 * tmpvar_28)) + ((2.0 * tmpvar_28) * (r_18 * mu_19))))
			) / 1.2));
		result_23 = (result_23 + ((
			(yi_21 + tmpvar_29)
			/ 2.0) * dx_22));
		yi_21 = tmpvar_29;
	};
	float tmpvar_30;
	tmpvar_30 = sqrt((1.0 - (
	(6360.0 / r_1)
	*
	(6360.0 / r_1)
	)));
	float tmpvar_31;
	if ((muS_2 < -(tmpvar_30))) {
		tmpvar_31 = 1e+09;
	}
	else {
		tmpvar_31 = result_23;
	};
	float4 tmpvar_32;
	tmpvar_32.w = 1.0;
	tmpvar_32.xyz = exp(-((
	(BetaR.xyz * tmpvar_17)
	+
	(float3(0.004444445, 0.004444445, 0.004444445) * tmpvar_31)
	)));

	return tmpvar_32;
}

float4 PrecomputeInscatter(VertexOutput input) : SV_TARGET0
{
	float2 TexCoord = input.texCoord;

	float3 uvLayer_1;
	if ((ResR > 3)) {
		float3 tmpvar_2;
		if ((TexCoord.y > 0.75)) {
			float3 tmpvar_3;
			tmpvar_3.z = 16.0;
			tmpvar_3.x = TexCoord.x;
			tmpvar_3.y = ((TexCoord.y * float(ResR)) - 3.0);
			tmpvar_2 = tmpvar_3;
		}
		else {
			float3 tmpvar_4;
			if ((TexCoord.y > 0.5)) {
				float3 tmpvar_5;
				tmpvar_5.z = 4.0;
				tmpvar_5.x = TexCoord.x;
				tmpvar_5.y = ((TexCoord.y * float(ResR)) - 2.0);
				tmpvar_4 = tmpvar_5;
			}
			else {
				float3 tmpvar_6;
				if ((TexCoord.y > 0.25)) {
					float3 tmpvar_7;
					tmpvar_7.z = 2.0;
					tmpvar_7.x = TexCoord.x;
					tmpvar_7.y = ((TexCoord.y * float(ResR)) - 1.0);
					tmpvar_6 = tmpvar_7;
				}
				else {
					float3 tmpvar_8;
					tmpvar_8.z = 0.0;
					tmpvar_8.x = TexCoord.x;
					tmpvar_8.y = (TexCoord.y * float(ResR));
					tmpvar_6 = tmpvar_8;
				};
				tmpvar_4 = tmpvar_6;
			};
			tmpvar_2 = tmpvar_4;
		};
		uvLayer_1 = tmpvar_2;
	}
	else {
		float3 tmpvar_9;
		tmpvar_9.z = 2.0;
		tmpvar_9.xy = TexCoord;
		uvLayer_1 = tmpvar_9;
	};
	int layer_10;
	layer_10 = int(uvLayer_1.z);
	float r_11;
	r_11 = (float(layer_10) / 31.0);
	r_11 = (r_11 * r_11);
	float tmpvar_12;
	tmpvar_12 = sqrt((4.04496e+07 + (r_11 * 766800.0)));
	float tmpvar_13;
	if ((layer_10 == 0)) {
		tmpvar_13 = 0.01;
	}
	else {
		float tmpvar_14;
		if ((float(layer_10) == 31.0)) {
			tmpvar_14 = -0.001;
		}
		else {
			tmpvar_14 = 0.0;
		};
		tmpvar_13 = tmpvar_14;
	};
	r_11 = (tmpvar_12 + tmpvar_13);
	float tmpvar_15;
	tmpvar_15 = (sqrt((
		(r_11 * r_11)
		- 4.04496e+07)) + 875.6711);
	float tmpvar_16;
	tmpvar_16 = sqrt(((r_11 * r_11) - 4.04496e+07));
	float4 tmpvar_17;
	tmpvar_17.x = (6420.0 - r_11);
	tmpvar_17.y = tmpvar_15;
	tmpvar_17.z = (r_11 - 6360.0);
	tmpvar_17.w = tmpvar_16;
	float mu_18;
	float muS_19;
	float tmpvar_20;
	tmpvar_20 = ((uvLayer_1.x * 256.0) - 0.5);
	float tmpvar_21;
	tmpvar_21 = ((uvLayer_1.y * 128.0) - 0.5);
	if ((tmpvar_21 < 64.0)) {
		float tmpvar_22;
		tmpvar_22 = min(max(tmpvar_17.z, (
			(1.0 - (tmpvar_21 / 63.0))
			* tmpvar_16)), (tmpvar_16 * 0.999));
		mu_18 = (((4.04496e+07 -
			(r_11 * r_11)
			) - (tmpvar_22 * tmpvar_22)) / ((2.0 * r_11) * tmpvar_22));
		mu_18 = min(mu_18, (-(
			sqrt((1.0 - ((6360.0 / r_11) * (6360.0 / r_11))))
			) - 0.001));
	}
	else {
		float tmpvar_23;
		tmpvar_23 = min(max(tmpvar_17.x, (
			((tmpvar_21 - 64.0) / 63.0)
			* tmpvar_15)), (tmpvar_15 * 0.999));
		mu_18 = (((4.12164e+07 -
			(r_11 * r_11)
			) - (tmpvar_23 * tmpvar_23)) / ((2.0 * r_11) * tmpvar_23));
	};
	muS_19 = ((tmpvar_20 - (32.0 *
		floor((tmpvar_20 / 32.0))
		)) / 31.0);
	float theta_24;
	theta_24 = (((
		(2.0 * muS_19)
		- 1.0) + 0.26) * 1.1);
	muS_19 = ((sin(theta_24) / cos(theta_24)) / 5.349625);
	float r_25;
	r_25 = r_11;
	float mu_26;
	mu_26 = mu_18;
	float muS_27;
	muS_27 = muS_19;
	float nu_28;
	nu_28 = (-1.0 + ((
		floor((tmpvar_20 / 32.0))
		/ 7.0) * 2.0));
	float3 ray_29;
	float mie_30;
	float miei_32;
	float3 rayi_33;
	float dx_34;
	ray_29 = float3(0.0, 0.0, 0.0);
	mie_30 = 0.0;
	float dout_35;
	float tmpvar_36;
	tmpvar_36 = ((-(r_11)* mu_18) + sqrt((
		((r_11 * r_11) * ((mu_18 * mu_18) - 1.0))
		+ 4.122924e+07)));
	dout_35 = tmpvar_36;
	float tmpvar_37;
	tmpvar_37 = (((r_11 * r_11) * (
		(mu_18 * mu_18)
		- 1.0)) + 4.04496e+07);
	if ((tmpvar_37 >= 0.0)) {
		float tmpvar_38;
		tmpvar_38 = ((-(r_11)* mu_18) - sqrt(tmpvar_37));
		if ((tmpvar_38 >= 0.0)) {
			dout_35 = min(tmpvar_36, tmpvar_38);
		};
	};
	dx_34 = (dout_35 / 25.0);
	float3 ray_39;
	float mie_40;
	ray_39 = float3(0.0, 0.0, 0.0);
	mie_40 = 0.0;
	float tmpvar_41;
	tmpvar_41 = sqrt((r_11 * r_11));
	float tmpvar_42;
	tmpvar_42 = ((muS_19 * r_11) / (tmpvar_41 * lerp(1.0, BetaR.w,
		max(0.0, muS_19)
	)));
	float tmpvar_43;
	tmpvar_43 = max(6360.0, tmpvar_41);
	float tmpvar_44;
	tmpvar_44 = sqrt((1.0 - (4.04496e+07 /
		(tmpvar_43 * tmpvar_43)
		)));
	if ((tmpvar_42 >= -(tmpvar_44))) {
		float3 result_45;
		float tmpvar_46;
		tmpvar_46 = sqrt((r_11 * r_11));
		float tmpvar_47;
		tmpvar_47 = ((r_11 * mu_18) / tmpvar_46);
		if ((mu_18 > 0.0)) {
			float y_over_x_48;
			y_over_x_48 = (((mu_18 + 0.15) / 1.15) * 14.10142);
			float tmpvar_49;
			tmpvar_49 = (min(abs(y_over_x_48), 1.0) / max(abs(y_over_x_48), 1.0));
			float tmpvar_50;
			tmpvar_50 = (tmpvar_49 * tmpvar_49);
			tmpvar_50 = (((
				((((
				((((-0.01213232 * tmpvar_50) + 0.05368138) * tmpvar_50) - 0.1173503)
					* tmpvar_50) + 0.1938925) * tmpvar_50) - 0.3326756)
				* tmpvar_50) + 0.9999793) * tmpvar_49);
			tmpvar_50 = (tmpvar_50 + (float(
				(abs(y_over_x_48) > 1.0)
				) * (
				(tmpvar_50 * -2.0)
					+ 1.570796)));
			float2 tmpvar_51;
			tmpvar_51.x = ((tmpvar_50 * sign(y_over_x_48)) / 1.5);
			tmpvar_51.y = sqrt(((r_11 - 6360.0) / 60.0));
			float y_over_x_52;
			y_over_x_52 = (((tmpvar_47 + 0.15) / 1.15) * 14.10142);
			float tmpvar_53;
			tmpvar_53 = (min(abs(y_over_x_52), 1.0) / max(abs(y_over_x_52), 1.0));
			float tmpvar_54;
			tmpvar_54 = (tmpvar_53 * tmpvar_53);
			tmpvar_54 = (((
				((((
				((((-0.01213232 * tmpvar_54) + 0.05368138) * tmpvar_54) - 0.1173503)
					* tmpvar_54) + 0.1938925) * tmpvar_54) - 0.3326756)
				* tmpvar_54) + 0.9999793) * tmpvar_53);
			tmpvar_54 = (tmpvar_54 + (float(
				(abs(y_over_x_52) > 1.0)
				) * (
				(tmpvar_54 * -2.0)
					+ 1.570796)));
			float2 tmpvar_55;
			tmpvar_55.x = ((tmpvar_54 * sign(y_over_x_52)) / 1.5);
			tmpvar_55.y = sqrt(((tmpvar_46 - 6360.0) / 60.0));
			result_45 = min((InputTexture.Sample(DefaultSampler, tmpvar_51).xyz / InputTexture.Sample(DefaultSampler, tmpvar_55).xyz), float3(1.0, 1.0, 1.0));
		}
		else {
			float y_over_x_56;
			y_over_x_56 = (((
				-(tmpvar_47)
				+0.15) / 1.15) * 14.10142);
			float tmpvar_57;
			tmpvar_57 = (min(abs(y_over_x_56), 1.0) / max(abs(y_over_x_56), 1.0));
			float tmpvar_58;
			tmpvar_58 = (tmpvar_57 * tmpvar_57);
			tmpvar_58 = (((
				((((
				((((-0.01213232 * tmpvar_58) + 0.05368138) * tmpvar_58) - 0.1173503)
					* tmpvar_58) + 0.1938925) * tmpvar_58) - 0.3326756)
				* tmpvar_58) + 0.9999793) * tmpvar_57);
			tmpvar_58 = (tmpvar_58 + (float(
				(abs(y_over_x_56) > 1.0)
				) * (
				(tmpvar_58 * -2.0)
					+ 1.570796)));
			float2 tmpvar_59;
			tmpvar_59.x = ((tmpvar_58 * sign(y_over_x_56)) / 1.5);
			tmpvar_59.y = sqrt(((tmpvar_46 - 6360.0) / 60.0));
			float y_over_x_60;
			y_over_x_60 = (((
				-(mu_18)
				+0.15) / 1.15) * 14.10142);
			float tmpvar_61;
			tmpvar_61 = (min(abs(y_over_x_60), 1.0) / max(abs(y_over_x_60), 1.0));
			float tmpvar_62;
			tmpvar_62 = (tmpvar_61 * tmpvar_61);
			tmpvar_62 = (((
				((((
				((((-0.01213232 * tmpvar_62) + 0.05368138) * tmpvar_62) - 0.1173503)
					* tmpvar_62) + 0.1938925) * tmpvar_62) - 0.3326756)
				* tmpvar_62) + 0.9999793) * tmpvar_61);
			tmpvar_62 = (tmpvar_62 + (float(
				(abs(y_over_x_60) > 1.0)
				) * (
				(tmpvar_62 * -2.0)
					+ 1.570796)));
			float2 tmpvar_63;
			tmpvar_63.x = ((tmpvar_62 * sign(y_over_x_60)) / 1.5);
			tmpvar_63.y = sqrt(((r_11 - 6360.0) / 60.0));
			result_45 = min((InputTexture.Sample(DefaultSampler, tmpvar_59).xyz / InputTexture.Sample(DefaultSampler, tmpvar_63).xyz), float3(1.0, 1.0, 1.0));
		};
		float y_over_x_64;
		y_over_x_64 = (((tmpvar_42 + 0.15) / 1.15) * 14.10142);
		float tmpvar_65;
		tmpvar_65 = (min(abs(y_over_x_64), 1.0) / max(abs(y_over_x_64), 1.0));
		float tmpvar_66;
		tmpvar_66 = (tmpvar_65 * tmpvar_65);
		tmpvar_66 = (((
			((((
			((((-0.01213232 * tmpvar_66) + 0.05368138) * tmpvar_66) - 0.1173503)
				* tmpvar_66) + 0.1938925) * tmpvar_66) - 0.3326756)
			* tmpvar_66) + 0.9999793) * tmpvar_65);
		tmpvar_66 = (tmpvar_66 + (float(
			(abs(y_over_x_64) > 1.0)
			) * (
			(tmpvar_66 * -2.0)
				+ 1.570796)));
		float2 tmpvar_67;
		tmpvar_67.x = ((tmpvar_66 * sign(y_over_x_64)) / 1.5);
		tmpvar_67.y = sqrt(((tmpvar_43 - 6360.0) / 60.0));
		float3 tmpvar_68;
		tmpvar_68 = (result_45 * InputTexture.Sample(DefaultSampler, tmpvar_67).xyz);
		ray_39 = (exp((
			(6360.0 - tmpvar_43)
			/ 8.0)) * tmpvar_68);
		mie_40 = (exp((
			(6360.0 - tmpvar_43)
			/ 1.2)) * tmpvar_68).x;
	};
	rayi_33 = ray_39;
	miei_32 = mie_40;
	for (int i_31 = 1; i_31 <= 25; i_31++) {
		float tmpvar_69;
		tmpvar_69 = (float(i_31) * dx_34);
		float3 ray_70;
		float mie_71;
		ray_70 = float3(0.0, 0.0, 0.0);
		mie_71 = 0.0;
		float tmpvar_72;
		tmpvar_72 = sqrt(((
			(r_25 * r_25)
			+
			(tmpvar_69 * tmpvar_69)
			) + (
			(2.0 * r_25)
				*
				(mu_26 * tmpvar_69)
				)));
		float tmpvar_73;
		tmpvar_73 = (((nu_28 * tmpvar_69) + (muS_27 * r_25)) / (tmpvar_72 * lerp(1.0, BetaR.w,
			max(0.0, muS_27)
		)));
		float tmpvar_74;
		tmpvar_74 = max(6360.0, tmpvar_72);
		float tmpvar_75;
		tmpvar_75 = sqrt((1.0 - (4.04496e+07 /
			(tmpvar_74 * tmpvar_74)
			)));
		if ((tmpvar_73 >= -(tmpvar_75))) {
			float3 result_76;
			float tmpvar_77;
			tmpvar_77 = sqrt(((
				(r_25 * r_25)
				+
				(tmpvar_69 * tmpvar_69)
				) + (
				(2.0 * r_25)
					*
					(mu_26 * tmpvar_69)
					)));
			float tmpvar_78;
			tmpvar_78 = (((r_25 * mu_26) + tmpvar_69) / tmpvar_77);
			if ((mu_26 > 0.0)) {
				float y_over_x_79;
				y_over_x_79 = (((mu_26 + 0.15) / 1.15) * 14.10142);
				float tmpvar_80;
				tmpvar_80 = (min(abs(y_over_x_79), 1.0) / max(abs(y_over_x_79), 1.0));
				float tmpvar_81;
				tmpvar_81 = (tmpvar_80 * tmpvar_80);
				tmpvar_81 = (((
					((((
					((((-0.01213232 * tmpvar_81) + 0.05368138) * tmpvar_81) - 0.1173503)
						* tmpvar_81) + 0.1938925) * tmpvar_81) - 0.3326756)
					* tmpvar_81) + 0.9999793) * tmpvar_80);
				tmpvar_81 = (tmpvar_81 + (float(
					(abs(y_over_x_79) > 1.0)
					) * (
					(tmpvar_81 * -2.0)
						+ 1.570796)));
				float2 tmpvar_82;
				tmpvar_82.x = ((tmpvar_81 * sign(y_over_x_79)) / 1.5);
				tmpvar_82.y = sqrt(((r_25 - 6360.0) / 60.0));
				float y_over_x_83;
				y_over_x_83 = (((tmpvar_78 + 0.15) / 1.15) * 14.10142);
				float tmpvar_84;
				tmpvar_84 = (min(abs(y_over_x_83), 1.0) / max(abs(y_over_x_83), 1.0));
				float tmpvar_85;
				tmpvar_85 = (tmpvar_84 * tmpvar_84);
				tmpvar_85 = (((
					((((
					((((-0.01213232 * tmpvar_85) + 0.05368138) * tmpvar_85) - 0.1173503)
						* tmpvar_85) + 0.1938925) * tmpvar_85) - 0.3326756)
					* tmpvar_85) + 0.9999793) * tmpvar_84);
				tmpvar_85 = (tmpvar_85 + (float(
					(abs(y_over_x_83) > 1.0)
					) * (
					(tmpvar_85 * -2.0)
						+ 1.570796)));
				float2 tmpvar_86;
				tmpvar_86.x = ((tmpvar_85 * sign(y_over_x_83)) / 1.5);
				tmpvar_86.y = sqrt(((tmpvar_77 - 6360.0) / 60.0));
				result_76 = min((InputTexture.Sample(DefaultSampler, tmpvar_82).xyz / InputTexture.Sample(DefaultSampler, tmpvar_86).xyz), float3(1.0, 1.0, 1.0));
			}
			else {
				float y_over_x_87;
				y_over_x_87 = (((
					-(tmpvar_78)
					+0.15) / 1.15) * 14.10142);
				float tmpvar_88;
				tmpvar_88 = (min(abs(y_over_x_87), 1.0) / max(abs(y_over_x_87), 1.0));
				float tmpvar_89;
				tmpvar_89 = (tmpvar_88 * tmpvar_88);
				tmpvar_89 = (((
					((((
					((((-0.01213232 * tmpvar_89) + 0.05368138) * tmpvar_89) - 0.1173503)
						* tmpvar_89) + 0.1938925) * tmpvar_89) - 0.3326756)
					* tmpvar_89) + 0.9999793) * tmpvar_88);
				tmpvar_89 = (tmpvar_89 + (float(
					(abs(y_over_x_87) > 1.0)
					) * (
					(tmpvar_89 * -2.0)
						+ 1.570796)));
				float2 tmpvar_90;
				tmpvar_90.x = ((tmpvar_89 * sign(y_over_x_87)) / 1.5);
				tmpvar_90.y = sqrt(((tmpvar_77 - 6360.0) / 60.0));
				float y_over_x_91;
				y_over_x_91 = (((
					-(mu_26)
					+0.15) / 1.15) * 14.10142);
				float tmpvar_92;
				tmpvar_92 = (min(abs(y_over_x_91), 1.0) / max(abs(y_over_x_91), 1.0));
				float tmpvar_93;
				tmpvar_93 = (tmpvar_92 * tmpvar_92);
				tmpvar_93 = (((
					((((
					((((-0.01213232 * tmpvar_93) + 0.05368138) * tmpvar_93) - 0.1173503)
						* tmpvar_93) + 0.1938925) * tmpvar_93) - 0.3326756)
					* tmpvar_93) + 0.9999793) * tmpvar_92);
				tmpvar_93 = (tmpvar_93 + (float(
					(abs(y_over_x_91) > 1.0)
					) * (
					(tmpvar_93 * -2.0)
						+ 1.570796)));
				float2 tmpvar_94;
				tmpvar_94.x = ((tmpvar_93 * sign(y_over_x_91)) / 1.5);
				tmpvar_94.y = sqrt(((r_25 - 6360.0) / 60.0));
				result_76 = min((InputTexture.Sample(DefaultSampler, tmpvar_90).xyz / InputTexture.Sample(DefaultSampler, tmpvar_94).xyz), float3(1.0, 1.0, 1.0));
			};
			float y_over_x_95;
			y_over_x_95 = (((tmpvar_73 + 0.15) / 1.15) * 14.10142);
			float tmpvar_96;
			tmpvar_96 = (min(abs(y_over_x_95), 1.0) / max(abs(y_over_x_95), 1.0));
			float tmpvar_97;
			tmpvar_97 = (tmpvar_96 * tmpvar_96);
			tmpvar_97 = (((
				((((
				((((-0.01213232 * tmpvar_97) + 0.05368138) * tmpvar_97) - 0.1173503)
					* tmpvar_97) + 0.1938925) * tmpvar_97) - 0.3326756)
				* tmpvar_97) + 0.9999793) * tmpvar_96);
			tmpvar_97 = (tmpvar_97 + (float(
				(abs(y_over_x_95) > 1.0)
				) * (
				(tmpvar_97 * -2.0)
					+ 1.570796)));
			float2 tmpvar_98;
			tmpvar_98.x = ((tmpvar_97 * sign(y_over_x_95)) / 1.5);
			tmpvar_98.y = sqrt(((tmpvar_74 - 6360.0) / 60.0));
			float3 tmpvar_99;
			tmpvar_99 = (result_76 * InputTexture.Sample(DefaultSampler, tmpvar_98).xyz);
			ray_70 = (exp((
				(6360.0 - tmpvar_74)
				/ 8.0)) * tmpvar_99);
			mie_71 = (exp((
				(6360.0 - tmpvar_74)
				/ 1.2)) * tmpvar_99).x;
		};
		ray_29 = (ray_29 + ((
			(rayi_33 + ray_70)
			/ 2.0) * dx_34));
		mie_30 = (mie_30 + ((
			(miei_32 + mie_71)
			/ 2.0) * dx_34));
		rayi_33 = ray_70;
		miei_32 = mie_71;
	};
	ray_29 = (ray_29 * BetaR.xyz);
	mie_30 = (mie_30 * 0.004);
	float4 tmpvar_100;
	tmpvar_100.xyz = ray_29;
	tmpvar_100.w = mie_30;
	return tmpvar_100;
}

