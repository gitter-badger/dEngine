// WICTranslate.cs - dEngine
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
using System.Collections.Generic;
using System.Linq;
using SharpDX.DXGI;
using SharpDX.WIC;

namespace dEngine.Utility
{
	internal static class WICTranslate
	{
		private static readonly Dictionary<Guid, Format> _dictionary = new Dictionary<Guid, Format>
		{
			{
				PixelFormat.Format128bppRGBAFloat, Format.R32G32B32A32_Float
			},

			{
				PixelFormat.Format64bppRGBAHalf, Format.R16G16B16A16_Float
			},
			{PixelFormat.Format64bppRGBA, Format.R16G16B16A16_UNorm},
			{PixelFormat.Format32bppRGBA, Format.R8G8B8A8_UNorm},
			{PixelFormat.Format32bppBGRA, Format.B8G8R8A8_UNorm}, // DXGI 1.1
			{PixelFormat.Format32bppBGR, Format.B8G8R8X8_UNorm}, // DXGI 1.1

			{PixelFormat.Format32bppRGBA1010102XR, Format.R10G10B10_Xr_Bias_A2_UNorm}, // DXGI 1.1
			{PixelFormat.Format32bppRGBA1010102, Format.R10G10B10A2_UNorm},
			{PixelFormat.Format32bppRGBE, Format.R9G9B9E5_Sharedexp},
			{PixelFormat.Format16bppBGRA5551, Format.B5G5R5A1_UNorm},
			{PixelFormat.Format16bppBGR565, Format.B5G6R5_UNorm},
			{PixelFormat.Format32bppGrayFloat, Format.R32_Float},
			{PixelFormat.Format16bppGrayHalf, Format.R16_Float},
			{PixelFormat.Format16bppGray, Format.R16_UNorm},
			{PixelFormat.Format8bppGray, Format.R8_UNorm},
			{PixelFormat.Format8bppAlpha, Format.A8_UNorm},
			{PixelFormat.Format96bppRGBFloat, Format.R32G32B32_Float}
		};

		public static Format FormatFromGUID(Guid guid)
		{
			return _dictionary[guid];
		}

		public static Guid GUIDFromFormat(Format format)
		{
			return _dictionary.First(kv => kv.Value == format).Key;
		}
	}
}