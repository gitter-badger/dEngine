// SamplerStates.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using SharpDX.Direct3D11;

namespace dEngine.Graphics.States
{
	internal static class SamplerStates
	{
		internal static SamplerState Default;
		internal static SamplerState Brick;
		internal static SamplerState Wrap;
		internal static SamplerState Point;
		internal static SamplerState Shadow;
		internal static SamplerState ShadowPcf;
		internal static SamplerState[] States;

		internal static void Load()
		{
			Default?.Dispose();
			Brick?.Dispose();
			Wrap?.Dispose();
			Point?.Dispose();
			Shadow?.Dispose();
			ShadowPcf?.Dispose();

			Default = new SamplerState(Renderer.Device, SamplerStateDescription.Default());

			Wrap = new SamplerState(Renderer.Device, new SamplerStateDescription
			{
				AddressU = TextureAddressMode.Wrap,
				AddressV = TextureAddressMode.Wrap,
				AddressW = TextureAddressMode.Wrap
			});

			Point = new SamplerState(Renderer.Device, new SamplerStateDescription
			{
				AddressU = TextureAddressMode.Clamp,
				AddressV = TextureAddressMode.Clamp,
				AddressW = TextureAddressMode.Wrap,
				Filter = Filter.MinMagMipPoint,
				ComparisonFunction = Comparison.Always,
				MaximumLod = 1,
				MinimumLod = 0,
				MipLodBias = 0,
				MaximumAnisotropy = 1
			});

			Shadow = new SamplerState(Renderer.Device, new SamplerStateDescription
			{
				AddressU = TextureAddressMode.Clamp,
				AddressV = TextureAddressMode.Clamp,
				AddressW = TextureAddressMode.Clamp,
				Filter = Filter.ComparisonMinMagMipPoint,
				ComparisonFunction = Comparison.LessEqual
			});

			ShadowPcf = new SamplerState(Renderer.Device, new SamplerStateDescription
			{
				AddressU = TextureAddressMode.Clamp,
				AddressV = TextureAddressMode.Clamp,
				AddressW = TextureAddressMode.Clamp,
				Filter = Filter.ComparisonMinMagMipLinear,
				ComparisonFunction = Comparison.LessEqual
			});

			Brick = new SamplerState(Renderer.Device, new SamplerStateDescription
			{
				AddressU = TextureAddressMode.Wrap,
				AddressV = TextureAddressMode.Wrap,
				AddressW = TextureAddressMode.Wrap,
				Filter = Filter.Anisotropic,
				MaximumAnisotropy = 11,
				MipLodBias = 0,
				MinimumLod = 0,
				MaximumLod = 11
			});

			States = new[] {Default, Brick, Wrap, Point, Shadow, ShadowPcf};
		}
	}
}