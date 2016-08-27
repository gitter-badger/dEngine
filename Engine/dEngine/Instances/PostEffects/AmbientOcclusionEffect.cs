// AmbientOcclusionEffect.cs - dEngine
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
using dEngine.Data;
using dEngine.Graphics;
using dEngine.Instances.Attributes;
using dEngine.Services;
using dEngine.Settings.Global;
using SharpDX;
using SharpDX.Direct3D11;
// ReSharper disable UnusedMember.Local

namespace dEngine.Instances
{
    [TypeId(180), Uncreatable]
    internal sealed class AmbientOcclusionEffect : PostEffect
    {
        private static Texture _noiseTexture;

        private readonly ConstantBuffer<SSAOConstantData> _constantBuffer;
        private readonly GfxShader.Pass _ssaoPass1 = Shaders.Get("SSAO").GetPass("SSAO_1");
        private readonly GfxShader.Pass _ssaoPass2 = Shaders.Get("SSAO").GetPass("SSAO_2");
        private readonly GfxShader.Pass _ssaoPass3 = Shaders.Get("SSAO").GetPass("SSAO_3");
        private readonly GfxShader.Pass _ssaoPass4 = Shaders.Get("SSAO").GetPass("SSAO_3");
        private readonly GfxShader.Pass _gaussianPass = Shaders.Get("SSAO").GetPass("GaussianBlur");
        private readonly GfxShader.Pass _hqBilateralPass = Shaders.Get("SSAO").GetPass("HighQualityBilateralBlur");
        private readonly GfxShader.Pass _compositePass = Shaders.Get("SSAO").GetPass("Composite");
        private readonly GfxShader.Pass _upsamplePass = Shaders.Get("PostProcess").GetPass("Upsample");

        private float _blurBilateralThreshold;
        private float _bias;
        private float _distance;
        private float _radius;
        private float _falloff;
        private float _intensity;
        private float _lumContribution;
        private float _maxDistance;
        private Colour _occlusionColour;
        private bool _constantsDirty;

        static AmbientOcclusionEffect()
        {
            Renderer.InvokeResourceDependent(
                () => { _noiseTexture = CacheableContentProvider<Texture>.Get("internal://textures/noise.png"); });
        }

        public AmbientOcclusionEffect()
        {
            _effectOrder = (int)EffectPrority.AmbientOcclusion;
            _constantBuffer = new ConstantBuffer<SSAOConstantData> { Data = { NoiseSize = _noiseTexture.Width } };
            Radius = 0.125f;
            Intensity = 2f;
            Distance = 1f;
            Bias = 0.1f;
            LumContribution = 0.5f;
            OcclusionColour = Colour.Black;
        }

        /// <summary>
        /// The maximum radius of a gap that will introduce ambient occlusion.
        /// </summary>
        [InstMember(1), EditorVisible("Data"), Range(0.01f, 1.25f)]
        public float Radius
        {
            get { return _radius; }
            set
            {
                if (value == _radius) return;
                _radius = Math.Max(0.01f, Math.Min(value, 1.25f));
                _constantBuffer.Data.SampleRadius = _radius;
                _constantsDirty = true;
                NotifyChanged();
            }
        }

        /// <summary>
        /// The degree of darkness added by ambient occlusion.
        /// </summary>
        [InstMember(2), EditorVisible("Data"), Range(0f, 16f)]
        public float Intensity
        {
            get { return _intensity; }
            set
            {
                if (value == _intensity) return;
                _intensity = Math.Max(0f, Math.Min(value, 16f));
                _constantBuffer.Data.Intensity = _intensity;
                _constantsDirty = true;
                NotifyChanged();
            }
        }

        /// <summary>
        /// The distance between an occluded sample and its occluder.
        /// </summary>
        [InstMember(3), EditorVisible("Data"), Range(0, 10f)]
        public float Distance
        {
            get { return _distance; }
            set
            {
                if (value == _distance) return;
                _distance = Math.Max(0f, Math.Min(value, 10f));
                _constantBuffer.Data.Distance = _distance;
                _constantsDirty = true;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Offsets the occlusion cone.
        /// </summary>
        [InstMember(4), EditorVisible("Data"), Range(0, 1f)]
        public float Bias
        {
            get { return _bias; }
            set
            {
                if (value == _bias) return;
                _bias = Math.Max(0f, Math.Min(value, 1f));
                _constantBuffer.Data.Bias = _bias;
                _constantsDirty = true;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Determines how much ambient occlusion should be added in bright areas.
        /// </summary>
        [InstMember(5), EditorVisible("Data"), Range(0f, 1f)]
        public float LumContribution
        {
            get { return _lumContribution; }
            set
            {
                if (value == _lumContribution) return;
                _lumContribution = Math.Max(0f, Math.Min(value, 1f));
                _constantBuffer.Data.LumContribution = _lumContribution;
                _constantsDirty = true;
                NotifyChanged();
            }
        }

        /// <summary>
        /// The colour of the occlusion. Usually black.
        /// </summary>
        [InstMember(6), EditorVisible("Data")]
        public Colour OcclusionColour
        {
            get { return _occlusionColour; }
            set
            {
                if (value == _occlusionColour) return;
                _occlusionColour = value;
                _constantBuffer.Data.OcclusionColour = value;
                _constantsDirty = true;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Determines how sharp the bilateral blur is.
        /// </summary>
        [InstMember(7), EditorVisible("Bilateral"), Range(0.05f, 1f)]
        public float BlurBilateralThreshold
        {
            get { return _blurBilateralThreshold; }
            set
            {
                if (value == _blurBilateralThreshold) return;
                _blurBilateralThreshold = Math.Max(0.05f, Math.Min(value, 1f));
                _constantBuffer.Data.BilateralThreshold = value;
                _constantsDirty = true;
                NotifyChanged();
            }
        }

        /// <summary>
        /// The maximum distance at which ambient occlusion is applied to objects.
        /// </summary>
        [InstMember(8), EditorVisible("Distance Cutoff")]
        public float MaxDistance
        {
            get { return _maxDistance; }
            set
            {
                if (value == _maxDistance) return;
                _maxDistance = value;
                _constantBuffer.Data.DistanceCutoff = Math.Max(0, value);
                _constantsDirty = true;
                NotifyChanged();
            }
        }

        /// <summary>
        /// The falloff of the effect beyond <see cref="MaxDistance" />.
        /// </summary>
        [InstMember(9), EditorVisible("Distance Cutoff")]
        public float Falloff
        {
            get { return _falloff; }
            set
            {
                if (value == _falloff) return;
                _falloff = value;
                _constantBuffer.Data.CutoffFalloff = value;
                _constantsDirty = true;
                NotifyChanged();
            }
        }

        internal override void UpdateSize(Camera camera)
        {
            _constantBuffer.Data.TexelSize = Camera.Buffer0?.TexelSize ?? Vector4.Zero;
            _constantsDirty = true;
        }

        internal override void Render(ref DeviceContext context)
        {
            var ssaoPass = _ssaoPass4;

            if (_constantsDirty)
            {
                _constantBuffer.Update(ref context);
                _constantsDirty = false;
            }
            context.PixelShader.SetConstantBuffer(1, _constantBuffer);

            if (RenderSettings.SSAOBlurMode == BlurMode.None)
            {
                var rt = GetDownsampledTexture();
                context.ClearRenderTargetView(rt, Color.White);
                
                context.OutputMerger.SetRenderTargets(null, rt);
                context.PixelShader.SetShaderResources(1, null, Camera.DepthStencilBuffer, Camera.Buffer2, _noiseTexture);
                context.PixelShader.SetShaderResource(0, Camera.Buffer0);

                if (RenderSettings.ShowAmbientOcclusion)
                {
                    ssaoPass.Use(ref context);
                    context.Draw(4, 0);

                    _upsamplePass.Use(ref context);
                    context.PixelShader.SetShaderResources(0, rt);
                    context.OutputMerger.SetRenderTargets(null, Camera.Buffer0);
                    //context.Draw(4, 0);
                }

                //context.Draw(4, 0);
            }
            else
            {
                
            }
        }

        private Texture GetDownsampledTexture()
        {
            switch (RenderSettings.SSAODownsampleSize)
            {
                case DownsampleSize.FullSize:
                    return Camera.Buffer1;
                case DownsampleSize.HalfSize:
                    return Camera.BufferDownscaleHalf0;
                case DownsampleSize.ThirdSize:
                    throw new NotImplementedException();
                case DownsampleSize.QuarterSize:
                    return Camera.BufferDownscaleQuarter0;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void Destroy()
        {
            base.Destroy();
            _constantBuffer.Dispose();
        }

        // ReSharper disable NotAccessedField.Local
#pragma warning disable 414
        private struct SSAOConstantData : IConstantBufferData
        {
            public Vector4 TexelSize;

            public float NoiseSize;
            public float SampleRadius;
            public float Intensity;
            public float Distance;

            public float Bias;
            public float LumContribution;
            public float DistanceCutoff;
            public float CutoffFalloff;

            public Colour OcclusionColour;

            public Vector2 Direction;
            public float BilateralThreshold;
        }
#pragma warning restore 414
        // ReSharper restore NotAccessedField.Local
    }
}