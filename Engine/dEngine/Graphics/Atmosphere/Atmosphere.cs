// Atmosphere.cs - dEngine
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
using System.IO;
using dEngine.Data;
using dEngine.Utility;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.WIC;
using Bitmap = System.Drawing.Bitmap;

namespace dEngine.Graphics.Atmosphere
{
    internal static class Atmosphere
    {
        internal static void Update()
        {
            if (!Precompute.IsInitialized)
                Precompute.Init();

            //if (Precompute.NeedsRecompute)
            //    Precompute.StartPrecompute();


        }
        
        internal static class Precompute
        {
            public enum DepthSample
            {
                X1 = 1,
                X4 = 4,
            }

            public const int TransmittanceWidth = 256;
            public const int TransmittanceHeight = 64;
            public const int InscatterWidth = 256;
            public const int InscatterHeight = 128;

            private static Vector4 _finalBetaR;
            private static float _atmosphereThickness = 1.0f;
            private static Vector3 _waveLengths = new Vector3(680, 550, 440);
            private static Colour _skyTint = Colour.fromRGB(128, 128, 128);

            public static float AtmosphereThickness
            {
                get { return _atmosphereThickness; }
                set
                {
                    _atmosphereThickness = value;
                    NeedsRecompute = true;
                }
            }

            public static Vector3 WaveLengths
            {
                get { return _waveLengths; }
                set
                {
                    _waveLengths = value;
                    NeedsRecompute = true;
                }
            }

            public static Colour SkyTint
            {
                get { return _skyTint; }
                set
                {
                    _skyTint = value;
                    NeedsRecompute = true;
                }
            }

            public static DepthSample InscatterAltitudeSample = DepthSample.X1;

            internal static bool NeedsRecompute;
            internal static Texture Transmittance2D;
            internal static Texture Inscatter2D;
            internal static GfxShader.Pass TransmittancePass;
            internal static GfxShader.Pass InscatterPass;
            private static PrecomputeConstants _constants;

            internal static bool IsInitialized;

            internal static void Init()
            {
                _constants = new PrecomputeConstants();
                var atmosphere = Shaders.Get("Atmosphere");
                TransmittancePass = atmosphere.GetPass("PrecomputeTransmittance");
                InscatterPass = atmosphere.GetPass("PrecomputeInscatter");
                NeedsRecompute = true;
                IsInitialized = true;
            }

            internal static bool PrecomputeTextureResource(int totalInscatterTextureHeight)
            {
                if (Transmittance2D == null)
                {
                    Transmittance2D = new Texture(TransmittanceWidth, TransmittanceHeight, Format.R32G32_Float, true, BindFlags.RenderTarget | BindFlags.ShaderResource)
                    {
                        Name = "Transmittance2D"
                    };
                    // set_asioLevel 0
                    // setWrapMode 1
                }

                if (Inscatter2D != null && Inscatter2D.Height != totalInscatterTextureHeight)
                {
                    Inscatter2D.Resize(InscatterWidth, totalInscatterTextureHeight);
                    return true;
                }

                if (Inscatter2D == null)
                {
                    Inscatter2D = new Texture(InscatterWidth, totalInscatterTextureHeight, Format.R32G32B32A32_Float, false, BindFlags.RenderTarget | BindFlags.ShaderResource)
                    {
                        Name = "Inscatter2D"
                    };
                }

                return Transmittance2D != null && Inscatter2D != null;
            }

            internal static void StartPrecompute()
            {
                int num = 128 * (int)InscatterAltitudeSample;

                if (!PrecomputeTextureResource(num))
                    return;

                UpdateRayleigh();
                var context = Renderer.Context;

                context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

                _constants.Data = new PrecomputeConstantData { BetaR = _finalBetaR, ResR = (int)InscatterAltitudeSample };
                _constants.Update(ref context);
                context.PixelShader.SetConstantBuffers(0, null, _constants);

                TransmittancePass.Use(ref context);
                context.OutputMerger.SetRenderTargets(null, Transmittance2D);
                context.Draw(4, 0);

                InscatterPass.Use(ref context);
                context.OutputMerger.SetRenderTargets(null, Inscatter2D);
                context.PixelShader.SetShaderResources(0, Transmittance2D);
                context.Draw(4, 0);

                var bytes = Inscatter2D.GetBytesPBGRA();

                /*
                var file = File.Create("C:/Users/Dan/Desktop/test.png");

                var test = new PngBitmapEncoder(Renderer.ImagingFactory);
                test.Initialize(file);

                var frameEncoder = new BitmapFrameEncode(test);
                frameEncoder.Initialize();
                frameEncoder.SetSize(Inscatter2D.Width, Inscatter2D.Height);
                var pf = PixelFormat.Format32bppPBGRA;
                frameEncoder.SetPixelFormat(ref pf);
                frameEncoder.WritePixels(Inscatter2D.Height, 4*Inscatter2D.Width, bytes);
                frameEncoder.Commit();

                test.Commit();
                file.Dispose();
                */

                //NeedsRecompute = false;
            }

            private class PrecomputeConstants : ConstantBuffer<PrecomputeConstantData>
            {
            }

            private struct PrecomputeConstantData : IConstantBufferData
            {
                public Vector4 BetaR;
                public int ResR;
            }

            public static void UpdateRayleigh()
            {
                var vector = new Vector3(Mathf.Lerp(WaveLengths.x + 150f, WaveLengths.x - 150f, SkyTint.r), Mathf.Lerp(WaveLengths.y + 150f, WaveLengths.y - 150f, SkyTint.g), Mathf.Lerp(WaveLengths.z + 150f, WaveLengths.z - 150f, SkyTint.b));
                vector.x = Mathf.Clamp(vector.x, 380f, 780f);
                vector.y = Mathf.Clamp(vector.y, 380f, 780f);
                vector.z = Mathf.Clamp(vector.z, 380f, 780f);
                Vector3 vector2 = vector * 1E-09f;
                Vector3 vector3 = new Vector3(Mathf.Pow(vector2.x, 4f), Mathf.Pow(vector2.y, 4f), Mathf.Pow(vector2.z, 4f));
                Vector3 vector4 = 7.635E+25f * vector3 * 5.755f;
                float num = 8f * Mathf.Pow(3.14159274f, 3f) * Mathf.Pow(0.0006002188f, 2f) * 6.105f;
                Vector3 vector5 = new Vector3(num / vector4.x, num / vector4.y, num / vector4.z);
                vector5 *= 1000f * Math.Max(0.01f, AtmosphereThickness);
                _finalBetaR = new Vector4(vector5.x,
                    vector5.y,
                vector5.z,
                Math.Max(Mathf.Pow(AtmosphereThickness, 3.14159274f), 1f));
            }
        }
    }
}
 