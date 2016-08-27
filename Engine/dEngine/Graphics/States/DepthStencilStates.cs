// DepthStencilStates.cs - dEngine
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
	internal static class DepthStencilStates
	{
		internal static DepthStencilState Standard;
		internal static DepthStencilState NoDepth;
	    internal static DepthStencilState DepthEnabled;

        internal static void Load()
		{
			Standard?.Dispose();
			NoDepth?.Dispose();

            MakeNoDepthDesc();
            MakeDepthEnabledState();
            Standard = new DepthStencilState(Renderer.Device, DepthStencilStateDescription.Default());
		}

	    private static void MakeNoDepthDesc()
	    {
	        var depthDesc = new DepthStencilStateDescription
	        {
	            IsDepthEnabled = false,
	            DepthWriteMask = DepthWriteMask.Zero,
	            DepthComparison = Comparison.LessEqual,
	            IsStencilEnabled = false,
	            StencilReadMask = byte.MaxValue,
	            StencilWriteMask = byte.MaxValue,
	            FrontFace =
	            {
	                DepthFailOperation = StencilOperation.Keep,
	                FailOperation = StencilOperation.Keep,
	                PassOperation = StencilOperation.Replace,
	                Comparison = Comparison.Always
	            }
	        };
	        depthDesc.BackFace = depthDesc.FrontFace;
            NoDepth = new DepthStencilState(Renderer.Device, depthDesc);
        }

        private static void MakeDepthEnabledState()
        {
            var depthDesc = new DepthStencilStateDescription
            {
                IsDepthEnabled = true,
                DepthWriteMask = DepthWriteMask.Zero,
                DepthComparison = Comparison.LessEqual,
                IsStencilEnabled = false,
                StencilReadMask = byte.MaxValue,
                StencilWriteMask = byte.MaxValue,
                FrontFace =
                {
                    DepthFailOperation = StencilOperation.Keep,
                    FailOperation = StencilOperation.Keep,
                    PassOperation = StencilOperation.Replace,
                    Comparison = Comparison.Always
                }
            };
            depthDesc.BackFace = depthDesc.FrontFace;
            DepthEnabled = new DepthStencilState(Renderer.Device, depthDesc);
        }
	}
}