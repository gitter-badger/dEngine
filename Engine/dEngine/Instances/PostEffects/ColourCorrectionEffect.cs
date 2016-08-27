// FilmEffect.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using dEngine.Data;
using dEngine.Graphics;
using dEngine.Instances.Attributes;
using SharpDX.Direct3D11;

namespace dEngine.Instances
{
	[TypeId(176), ExplorerOrder(0)]
	public sealed class ColourCorrectionEffect : PostEffect
    {
        private readonly GfxShader.Pass _correctionPass = Shaders.Get("PostProcess").GetPass("GammaCorrection");

        /// <summary/>
	    public ColourCorrectionEffect()
	    {
            _effectOrder = (int)EffectPrority.ColourCorrection;
        }

	    private Content<Texture> _lookupId;

	    private Colour _tintColour;

	    /// <summary>
	    /// The colour filter that is applied to the HDR scene colour.
	    /// </summary>
	    [InstMember(1), EditorVisible("Tone Mapping")]
	    public Colour TintColour
	    {
	        get { return _tintColour; }
	        set
	        {
	            if (value == _tintColour) return;
	            _tintColour = value;
	            NotifyChanged();
	        }
	    }

	    private float _toeFactor;

	    /// <summary>
	    /// Summary
	    /// </summary>
	    [InstMember(2), EditorVisible("Tone Mapping"), Range(0, 1)]
	    public float ToeFactor
	    {
	        get { return _toeFactor; }
	        set
	        {
	            if (value == _toeFactor) return;
	            _toeFactor = value;
	            NotifyChanged();
	        }
	    }

	    private float _intensity;

	    /// <summary>
	    /// The intensity of the colour grading.
	    /// </summary>
	    [InstMember(3), EditorVisible("Data"), Range(0, 1)]
	    public float Intensity
	    {
	        get { return _intensity; }
	        set
	        {
	            if (value == _intensity) return;
	            _intensity = value;
	            NotifyChanged();
	        }
	    }



        /// <summary>
        /// The lookup texture for the colour correction.
        /// </summary>
        [InstMember(4), EditorVisible("Colour Grading")]
	    public Content<Texture> LookupId
	    {
	        get { return _lookupId; }
	        set
	        {
	            if (value == _lookupId) return;
	            _lookupId = value;
	            NotifyChanged();
	        }
	    }

		internal override void Render(ref DeviceContext context)
        {
            context.OutputMerger.SetTargets((DepthStencilView)null, Camera.BackBuffer);
            context.PixelShader.SetShaderResources(0, Camera.Buffer0);
            _correctionPass.Use(ref context);
            context.Draw(4, 0);
        }
	}
}