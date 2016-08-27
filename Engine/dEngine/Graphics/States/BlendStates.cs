// BlendStates.cs - dEngine
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
	internal static class BlendStates
	{
		public static BlendState Default;
        public static BlendState Standard;
        public static BlendState Disabled;
	    public static BlendState ColourWriteDisabled;

	    internal static void Load()
		{
			Default = new BlendState(Renderer.Device, BlendStateDescription.Default());

	        SetStandardDisabled();
            SetBlendDisabled();
	        SetColourWriteDisabled();
		}

	    private static void SetColourWriteDisabled()
        {
            var blendDesc = new BlendStateDescription { IndependentBlendEnable = false, AlphaToCoverageEnable = false };
            for (int i = 0; i < 8; ++i)
            {
                blendDesc.RenderTarget[i].IsBlendEnabled = false;
                blendDesc.RenderTarget[i].BlendOperation = BlendOperation.Add;
                blendDesc.RenderTarget[i].AlphaBlendOperation = BlendOperation.Add;
                blendDesc.RenderTarget[i].DestinationBlend = BlendOption.InverseSourceAlpha;
                blendDesc.RenderTarget[i].DestinationAlphaBlend = BlendOption.One;
                blendDesc.RenderTarget[i].RenderTargetWriteMask = 0;
                blendDesc.RenderTarget[i].SourceBlend = BlendOption.SourceAlpha;
                blendDesc.RenderTarget[i].SourceAlphaBlend = BlendOption.One;
            }
            ColourWriteDisabled = new BlendState(Renderer.Device, blendDesc);
        }

	    private static void SetStandardDisabled()
        {
            var blendDesc = new BlendStateDescription { IndependentBlendEnable = true };

            blendDesc.RenderTarget[0].IsBlendEnabled = true;
            blendDesc.RenderTarget[0].SourceBlend = BlendOption.SourceAlpha; // Colour Src
            blendDesc.RenderTarget[0].DestinationBlend = BlendOption.InverseSourceAlpha; // Colour Dst
            blendDesc.RenderTarget[0].BlendOperation = BlendOperation.Add; // Colour Op
            blendDesc.RenderTarget[0].SourceAlphaBlend = BlendOption.InverseDestinationAlpha; // Alpha Src
            blendDesc.RenderTarget[0].DestinationAlphaBlend = BlendOption.One; // Alpha Dst
            blendDesc.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add; // Alpha Op
            blendDesc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;

            for (int i = 1; i < blendDesc.RenderTarget.Length; i++)
            {
                blendDesc.RenderTarget[i].IsBlendEnabled = false;
                blendDesc.RenderTarget[i].RenderTargetWriteMask = ColorWriteMaskFlags.All;
            }

            Standard = new BlendState(Renderer.Device, blendDesc);
        }


	    private static void SetBlendDisabled()
	    {
            var blendDesc = new BlendStateDescription { IndependentBlendEnable = false, AlphaToCoverageEnable = true};
	        blendDesc.RenderTarget[0].IsBlendEnabled = false;
            blendDesc.RenderTarget[0].BlendOperation = BlendOperation.Add;
            blendDesc.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
            blendDesc.RenderTarget[0].DestinationBlend = BlendOption.InverseSourceAlpha;
            blendDesc.RenderTarget[0].DestinationAlphaBlend = BlendOption.One;
            blendDesc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;
            blendDesc.RenderTarget[0].SourceBlend = BlendOption.SourceAlpha;
            blendDesc.RenderTarget[0].SourceAlphaBlend = BlendOption.One;
            Disabled = new BlendState(Renderer.Device, blendDesc);
        }
	}
}