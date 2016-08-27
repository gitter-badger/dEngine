// SphericalHarmonics.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Runtime.InteropServices;
using dEngine.Data;
using dEngine.Utility;
using SharpDX;
using SharpDX.Direct3D11;

#pragma warning disable 1591

namespace dEngine.Graphics
{
	/// <summary>
	/// Spherical Harmonics library for IBL.
	/// </summary>
	public static class SphericalHarmonics
	{
		private const int _minOrder = 2;
		private const int _maxOrder = 6;
		private const int NSH0 = 1;
		private const int NSH1 = 4;
		private const int NSH2 = 9;
		private const int NSH3 = 16;
		private const int NSH4 = 25;
		private const int NSH5 = 36;
		private const int NSH6 = 49;
		private const int NSH7 = 64;
		private const int NSH8 = 81;
		private const int NSH9 = 100;
		private const int NL0 = 1;
		private const int NL1 = 3;
		private const int NL2 = 5;
		private const int NL3 = 7;
		private const int NL4 = 9;
		private const int NL5 = 11;
		private const int NL6 = 13;
		private const int NL7 = 15;
		private const int NL8 = 17;
		private const int NL9 = 19;

		private static readonly float M_PIjs = 4.0f * Mathf.Atan(1.0f);
		private static readonly float maxang = (M_PIjs / 2f);

		public static Color3[] ProjectCubeMap(ref DeviceContext context, ref Texture cubeMap, int order)
		{
			var length = order * order;

			float[] resultR;
			float[] resultG;
			float[] resultB;

			ProjectCubeMap(ref context, ref cubeMap, order, out resultR, out resultG, out resultB);

			var colours = new Color3[length];
			for (int i = 0; i < length; i++)
			{
				colours[i].Red = resultR[i];
				colours[i].Green = resultG[i];
				colours[i].Blue = resultB[i];
			}

			return colours;
			;
		}

		private static void ProjectCubeMap(ref DeviceContext context, ref Texture cubeMap, int order, out float[] resultR,
			out float[] resultG, out float[] resultB)
		{
			var desc = cubeMap.NativeTexture.Description;

			Texture2D texture;

			if (!desc.CpuAccessFlags.HasFlag(CpuAccessFlags.Read))
			{
				var stagingDesc = desc;
				stagingDesc.BindFlags = BindFlags.None;
				stagingDesc.CpuAccessFlags = CpuAccessFlags.Read;
				stagingDesc.Usage = ResourceUsage.Staging;

				texture = new Texture2D(context.Device, stagingDesc);
				context.CopyResource(cubeMap.NativeTexture, texture);
			}
			else
			{
				texture = cubeMap.NativeTexture;
			}

			float fSize = desc.Width;
			float fPicSize = 1.0f / fSize;
			float fB = -1.0f + 1.0f / fSize;
			float fS = (desc.Width > 1) ? (2.0f * (1.0f - 1.0f / fSize) / (fSize - 1.0f)) : 0f;

			float fWt = 0.0f;

			resultR = new float[order * order];
			resultG = new float[order * order];
			resultB = new float[order * order];

			var shBuff = new float[_maxOrder * _maxOrder];
			var shBuffB = new float[_maxOrder * _maxOrder];

			for (int face = 0; face < 6; face++)
			{
				var dindex = Resource.CalculateSubResourceIndex(0, face, desc.MipLevels);

				DataStream mapped;
				var box = context.MapSubresource(texture, dindex, MapMode.Read, 0, out mapped);
				var pSrc = mapped.DataPointer;
				for (var y = 0; y < desc.Height; ++y)
				{
					float fV = y * fS + fB;

					for (var x = 0; x < desc.Width; ++x)
					{
						float fU = x * fS + fB;

						float ix, iy, iz;
						switch (face)
						{
							case 0: // Positive X
								iz = 1.0f - (2.0f * x + 1.0f) * fPicSize;
								iy = 1.0f - (2.0f * y + 1.0f) * fPicSize;
								ix = 1.0f;
								break;

							case 1: // Negative X
								iz = -1.0f + (2.0f * x + 1.0f) * fPicSize;
								iy = 1.0f - (2.0f * y + 1.0f) * fPicSize;
								ix = -1;
								break;

							case 2: // Positive Y
								iz = -1.0f + (2.0f * y + 1.0f) * fPicSize;
								iy = 1.0f;
								ix = -1.0f + (2.0f * x + 1.0f) * fPicSize;
								break;

							case 3: // Negative Y
								iz = 1.0f - (2.0f * y + 1.0f) * fPicSize;
								iy = -1.0f;
								ix = -1.0f + (2.0f * x + 1.0f) * fPicSize;
								break;

							case 4: // Positive Z
								iz = 1.0f;
								iy = 1.0f - (2.0f * y + 1.0f) * fPicSize;
								ix = -1.0f + (2.0f * x + 1.0f) * fPicSize;
								break;

							case 5: // Negative Z
								iz = -1.0f;
								iy = 1.0f - (2.0f * y + 1.0f) * fPicSize;
								ix = 1.0f - (2.0f * x + 1.0f) * fPicSize;
								break;
							default:
								throw new AccessViolationException(nameof(order));
						}

						var dir = new Vector3(ix, iy, iz).unit;

						float fDiffSolid = 4.0f / ((1.0f + fU * fU + fV * fV) * Mathf.Sqrt(1.0f + fU * fU + fV * fV));
						fWt += fDiffSolid;

						EvalDirection(ref shBuff, order, ref dir);

						var colour = Marshal.PtrToStructure<Color4>(pSrc);
						pSrc += 4;

						SHAdd(ref resultR, order, ref resultR,
							SHScale(ref shBuffB, order, ref shBuff, colour.Red * fDiffSolid));
						SHAdd(ref resultG, order, ref resultG,
							SHScale(ref shBuffB, order, ref shBuff, colour.Green * fDiffSolid));
						SHAdd(ref resultB, order, ref resultB,
							SHScale(ref shBuffB, order, ref shBuff, colour.Blue * fDiffSolid));
					}
				}
				context.UnmapSubresource(texture, dindex);
			}
		}

		private static float[] SHAdd(ref float[] result, int order, ref float[] inputA, float[] inputB)
		{
			var numcoeff = order * order;

			for (var i = 0; i < numcoeff; ++i)
			{
				result[i] = inputA[i] + inputB[i];
			}

			return result;
		}

		private static float[] SHScale(ref float[] result, int order, ref float[] input, float scale)
		{
			var numcoeff = order * order;

			for (var i = 0; i < numcoeff; ++i)
			{
				result[i] = scale * input[i];
			}

			return result;
		}

		private static void EvalDirection(ref float[] result, int order, ref Vector3 dir)
		{
			float fX = dir.x;
			float fY = dir.y;
			float fZ = dir.z;

			switch (order)
			{
				case 2:
					sh_eval_basis_1(fX, fY, fZ, ref result);
					break;

				case 3:
					sh_eval_basis_2(fX, fY, fZ, ref result);
					break;

				case 4:
					sh_eval_basis_3(fX, fY, fZ, ref result);
					break;

				case 5:
					sh_eval_basis_4(fX, fY, fZ, ref result);
					break;

				case 6:
					sh_eval_basis_5(fX, fY, fZ, ref result);
					break;

				default:
					throw new ArgumentOutOfRangeException(nameof(order));
			}
		}

		static void sh_eval_basis_1(float x, float y, float z, ref float[] b)
		{
			/* m=0 */

			// l=0
			float p_0_0 = 0.282094791773878140f;
			b[0] = p_0_0; // l=0,m=0
			// l=1
			float p_1_0 = 0.488602511902919920f * z;
			b[2] = p_1_0; // l=1,m=0


			/* m=1 */

			float s1 = y;
			float c1 = x;

			// l=1
			float p_1_1 = -0.488602511902919920f;
			b[1] = p_1_1 * s1; // l=1,m=-1
			b[3] = p_1_1 * c1; // l=1,m=+1
		}

		// routine generated programmatically for evaluating SH basis for degree 2
		// inputs (x,y,z) are a point on the sphere (i.e., must be unit length)
		// output is vector b with SH basis evaluated at (x,y,z).
		//
		static void sh_eval_basis_2(float x, float y, float z, ref float[] b)
		{
			float z2 = z * z;


			/* m=0 */

			// l=0
			float p_0_0 = 0.282094791773878140f;
			b[0] = p_0_0; // l=0,m=0
			// l=1
			float p_1_0 = 0.488602511902919920f * z;
			b[2] = p_1_0; // l=1,m=0
			// l=2
			float p_2_0 = 0.946174695757560080f * z2 + -0.315391565252520050f;
			b[6] = p_2_0; // l=2,m=0


			/* m=1 */

			float s1 = y;
			float c1 = x;

			// l=1
			float p_1_1 = -0.488602511902919920f;
			b[1] = p_1_1 * s1; // l=1,m=-1
			b[3] = p_1_1 * c1; // l=1,m=+1
			// l=2
			float p_2_1 = -1.092548430592079200f * z;
			b[5] = p_2_1 * s1; // l=2,m=-1
			b[7] = p_2_1 * c1; // l=2,m=+1


			/* m=2 */

			float s2 = x * s1 + y * c1;
			float c2 = x * c1 - y * s1;

			// l=2
			float p_2_2 = 0.546274215296039590f;
			b[4] = p_2_2 * s2; // l=2,m=-2
			b[8] = p_2_2 * c2; // l=2,m=+2
		}

		private static void sh_eval_basis_3(float x, float y, float z, ref float[] b)
		{
			var z2 = z * z;

			/* m=0 */

			// l=0
			float p_0_0 = 0.282094791773878140f;
			b[0] = p_0_0; // l=0,m=0
			// l=1
			var p_1_0 = 0.488602511902919920f * z;
			b[2] = p_1_0; // l=1,m=0
			// l=2
			var p_2_0 = 0.946174695757560080f * z2 + -0.315391565252520050f;
			b[6] = p_2_0; // l=2,m=0
			// l=3
			var p_3_0 = z * 1.865881662950577000f * z2 + -1.119528997770346200f;
			b[12] = p_3_0; // l=3,m=0


			/* m=1 */
			var s1 = y;
			var c1 = x;

			// l=1
			float p_1_1 = -0.488602511902919920f;
			b[1] = p_1_1 * s1; // l=1,m=-1
			b[3] = p_1_1 * c1; // l=1,m=+1
			// l=2
			var p_2_1 = -1.092548430592079200f * z;
			b[5] = p_2_1 * s1; // l=2,m=-1
			b[7] = p_2_1 * c1; // l=2,m=+1
			// l=3
			var p_3_1 = -2.285228997322328800f * z2 + 0.457045799464465770f;
			b[11] = p_3_1 * s1; // l=3,m=-1
			b[13] = p_3_1 * c1; // l=3,m=+1


			/* m=2 */
			var s2 = x * s1 + y * c1;
			var c2 = x * c1 - y * s1;

			// l=2
			float p_2_2 = 0.546274215296039590f;
			b[4] = p_2_2 * s2; // l=2,m=-2
			b[8] = p_2_2 * c2; // l=2,m=+2
			// l=3
			var p_3_2 = 1.445305721320277100f * z;
			b[10] = p_3_2 * s2; // l=3,m=-2
			b[14] = p_3_2 * c2; // l=3,m=+2


			/* m=3 */

			var s3 = x * s2 + y * c2;
			var c3 = x * c2 - y * s2;

			// l=3
			float p_3_3 = -0.590043589926643520f;
			b[9] = p_3_3 * s3; // l=3,m=-3
			b[15] = p_3_3 * c3; // l=3,m=+3
		}

		private static void sh_eval_basis_4(float x, float y, float z, ref float[] b)
		{
			var z2 = z * z;


			/* m=0 */

			// l=0
			var p_0_0 = 0.282094791773878140f;
			b[0] = p_0_0; // l=0,m=0
			// l=1
			var p_1_0 = 0.488602511902919920f * z;
			b[2] = p_1_0; // l=1,m=0
			// l=2
			var p_2_0 = 0.946174695757560080f * z2 + -0.315391565252520050f;
			b[6] = p_2_0; // l=2,m=0
			// l=3
			var p_3_0 = z * 1.865881662950577000f * z2 + -1.119528997770346200f;
			b[12] = p_3_0; // l=3,m=0
			// l=4
			var p_4_0 = 1.984313483298443000f * z * p_3_0 + -1.006230589874905300f * p_2_0;
			b[20] = p_4_0; // l=4,m=0


			/* m=1 */

			var s1 = y;
			var c1 = x;

			// l=1
			var p_1_1 = -0.488602511902919920f;
			b[1] = p_1_1 * s1; // l=1,m=-1
			b[3] = p_1_1 * c1; // l=1,m=+1
			// l=2
			var p_2_1 = -1.092548430592079200f * z;
			b[5] = p_2_1 * s1; // l=2,m=-1
			b[7] = p_2_1 * c1; // l=2,m=+1
			// l=3
			var p_3_1 = -2.285228997322328800f * z2 + 0.457045799464465770f;
			b[11] = p_3_1 * s1; // l=3,m=-1
			b[13] = p_3_1 * c1; // l=3,m=+1
			// l=4
			var p_4_1 = z * (-4.683325804901024000f * z2 + 2.007139630671867200f);
			b[19] = p_4_1 * s1; // l=4,m=-1
			b[21] = p_4_1 * c1; // l=4,m=+1


			/* m=2 */

			var s2 = x * s1 + y * c1;
			var c2 = x * c1 - y * s1;

			// l=2
			var p_2_2 = 0.546274215296039590f;
			b[4] = p_2_2 * s2; // l=2,m=-2
			b[8] = p_2_2 * c2; // l=2,m=+2
			// l=3
			var p_3_2 = 1.445305721320277100f * z;
			b[10] = p_3_2 * s2; // l=3,m=-2
			b[14] = p_3_2 * c2; // l=3,m=+2
			// l=4
			var p_4_2 = 3.311611435151459800f * z2 + -0.473087347878779980f;
			b[18] = p_4_2 * s2; // l=4,m=-2
			b[22] = p_4_2 * c2; // l=4,m=+2


			/* m=3 */

			var s3 = x * s2 + y * c2;
			var c3 = x * c2 - y * s2;

			// l=3
			var p_3_3 = -0.590043589926643520f;
			b[9] = p_3_3 * s3; // l=3,m=-3
			b[15] = p_3_3 * c3; // l=3,m=+3
			// l=4
			var p_4_3 = -1.770130769779930200f * z;
			b[17] = p_4_3 * s3; // l=4,m=-3
			b[23] = p_4_3 * c3; // l=4,m=+3


			/* m=4 */

			var s4 = x * s3 + y * c3;
			var c4 = x * c3 - y * s3;

			// l=4
			var p_4_4 = 0.625835735449176030f;
			b[16] = p_4_4 * s4; // l=4,m=-4
			b[24] = p_4_4 * c4; // l=4,m=+4
		}

		private static void sh_eval_basis_5(float x, float y, float z, ref float[] b)
		{
			var z2 = z * z;

			/* m=0 */

			// l=0
			var p_0_0 = 0.282094791773878140f;
			b[0] = p_0_0; // l=0,m=0
			// l=1
			var p_1_0 = 0.488602511902919920f * z;
			b[2] = p_1_0; // l=1,m=0
			// l=2
			var p_2_0 = 0.946174695757560080f * z2 + -0.315391565252520050f;
			b[6] = p_2_0; // l=2,m=0
			// l=3
			var p_3_0 = z * (1.865881662950577000f * z2 + -1.119528997770346200f);
			b[12] = p_3_0; // l=3,m=0
			// l=4
			var p_4_0 = 1.984313483298443000f * z * p_3_0 + -1.006230589874905300f * p_2_0;
			b[20] = p_4_0; // l=4,m=0
			// l=5
			var p_5_0 = 1.989974874213239700f * z * p_4_0 + -1.002853072844814000f * p_3_0;
			b[30] = p_5_0; // l=5,m=0


			/* m=1 */

			var s1 = y;
			var c1 = x;

			// l=1
			var p_1_1 = -0.488602511902919920f;
			b[1] = p_1_1 * s1; // l=1,m=-1
			b[3] = p_1_1 * c1; // l=1,m=+1
			// l=2
			var p_2_1 = -1.092548430592079200f * z;
			b[5] = p_2_1 * s1; // l=2,m=-1
			b[7] = p_2_1 * c1; // l=2,m=+1
			// l=3
			var p_3_1 = -2.285228997322328800f * z2 + 0.457045799464465770f;
			b[11] = p_3_1 * s1; // l=3,m=-1
			b[13] = p_3_1 * c1; // l=3,m=+1
			// l=4
			var p_4_1 = z * (-4.683325804901024000f * z2 + 2.007139630671867200f);
			b[19] = p_4_1 * s1; // l=4,m=-1
			b[21] = p_4_1 * c1; // l=4,m=+1
			// l=5
			var p_5_1 = 2.031009601158990200f * z * p_4_1 + -0.991031208965114650f * p_3_1;
			b[29] = p_5_1 * s1; // l=5,m=-1
			b[31] = p_5_1 * c1; // l=5,m=+1


			/* m=2 */

			var s2 = x * s1 + y * c1;
			var c2 = x * c1 - y * s1;

			// l=2
			var p_2_2 = 0.546274215296039590f;
			b[4] = p_2_2 * s2; // l=2,m=-2
			b[8] = p_2_2 * c2; // l=2,m=+2
			// l=3
			var p_3_2 = 1.445305721320277100f * z;
			b[10] = p_3_2 * s2; // l=3,m=-2
			b[14] = p_3_2 * c2; // l=3,m=+2
			// l=4
			var p_4_2 = 3.311611435151459800f * z2 + -0.473087347878779980f;
			b[18] = p_4_2 * s2; // l=4,m=-2
			b[22] = p_4_2 * c2; // l=4,m=+2
			// l=5
			var p_5_2 = z * (7.190305177459987500f * z2 + -2.396768392486662100f);
			b[28] = p_5_2 * s2; // l=5,m=-2
			b[32] = p_5_2 * c2; // l=5,m=+2


			/* m=3 */

			var s3 = x * s2 + y * c2;
			var c3 = x * c2 - y * s2;

			// l=3
			var p_3_3 = -0.590043589926643520f;
			b[9] = p_3_3 * s3; // l=3,m=-3
			b[15] = p_3_3 * c3; // l=3,m=+3
			// l=4
			var p_4_3 = -1.770130769779930200f * z;
			b[17] = p_4_3 * s3; // l=4,m=-3
			b[23] = p_4_3 * c3; // l=4,m=+3
			// l=5
			var p_5_3 = -4.403144694917253700f * z2 + 0.489238299435250430f;
			b[27] = p_5_3 * s3; // l=5,m=-3
			b[33] = p_5_3 * c3; // l=5,m=+3


			/* m=4 */

			var s4 = x * s3 + y * c3;
			var c4 = x * c3 - y * s3;

			// l=4
			var p_4_4 = 0.625835735449176030f;
			b[16] = p_4_4 * s4; // l=4,m=-4
			b[24] = p_4_4 * c4; // l=4,m=+4
			// l=5
			var p_5_4 = 2.075662314881041100f * z;
			b[26] = p_5_4 * s4; // l=5,m=-4
			b[34] = p_5_4 * c4; // l=5,m=+4


			/* m=5 */

			var s5 = x * s4 + y * c4;
			var c5 = x * c4 - y * s4;

			// l=5
			var p_5_5 = -0.656382056840170150f;
			b[25] = p_5_5 * s5; // l=5,m=-5
			b[35] = p_5_5 * c5; // l=5,m=+5
		}
	}
}