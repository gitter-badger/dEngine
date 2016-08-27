// RasterizerStates.cs - dEngine
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
	internal static class RasterizerStates
	{
        public static RasterizerState NoCulling;
        public static RasterizerState Shadow;
        public static RasterizerState Wireframe;
	    public static RasterizerState BackFaceCull;

	    internal static void Load()
		{
            BackFaceCull = new RasterizerState(Renderer.Device, new RasterizerStateDescription
            {
                IsAntialiasedLineEnabled = false,
                CullMode = CullMode.Back,
                DepthBias = 0,
                DepthBiasClamp = 0.0f,
                IsDepthClipEnabled = true,
                FillMode = FillMode.Solid,
                IsFrontCounterClockwise = false,
                SlopeScaledDepthBias = 0,
            });

			NoCulling = new RasterizerState(Renderer.Device, new RasterizerStateDescription
			{
				FillMode = FillMode.Solid,
				CullMode = CullMode.None,
				DepthBias = 0,
				DepthBiasClamp = 0,
				IsAntialiasedLineEnabled = false,
				IsDepthClipEnabled = false,
				IsFrontCounterClockwise = false,
				IsMultisampleEnabled = false,
				IsScissorEnabled = false,
				SlopeScaledDepthBias = 0
			});

			Shadow = new RasterizerState(Renderer.Device, new RasterizerStateDescription
			{
				CullMode = CullMode.Front,
				DepthBias = 0,
				DepthBiasClamp = 0,
				SlopeScaledDepthBias = 0,
				FillMode = FillMode.Solid,
				IsAntialiasedLineEnabled = false,
				IsDepthClipEnabled = true,
				IsFrontCounterClockwise = false,
				IsMultisampleEnabled = false,
				IsScissorEnabled = false
			});

            Wireframe = new RasterizerState(Renderer.Device, new RasterizerStateDescription
            {
                IsAntialiasedLineEnabled  = false,
                CullMode = CullMode.None,
                DepthBias = 0,
                DepthBiasClamp = 0.0f,
                IsDepthClipEnabled = true,
                FillMode = FillMode.Wireframe,
                IsFrontCounterClockwise = false,
                IsMultisampleEnabled = true,
                IsScissorEnabled = false,
                SlopeScaledDepthBias = 0,
            });
		}
	}
}