// Shadows.cs - dEngine
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
using dEngine.Graphics.States;
using dEngine.Instances;
using dEngine.Services;
using dEngine.Settings.Global;
using dEngine.Utility;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using DxVector3 = SharpDX.Vector3;
using DxVector4 = SharpDX.Vector4;
using MapFlags = SharpDX.Direct3D11.MapFlags;

namespace dEngine.Graphics
{
    internal static class Shadows
    {
        public const float ShadowNearClip = 1.0f;
        public const bool UseComputeReduction = false;
        private static readonly ConstantBuffer<Matrix> _depthConstants;
        private static DxVector3[] _frustumCorners;
        public static ConstantBuffer<ShadowReceiverData> ReceiverConstants;
        public static GfxShader.Pass DepthOnlyPass;

        static Shadows()
        {
            _depthConstants = new ConstantBuffer<Matrix>();

            _frustumCorners = new DxVector3[8];

            ReceiverConstants = new ConstantBuffer<ShadowReceiverData>
            {
                Data =
                {
                    CascadeOffsets = new DxVector4[4],
                    CascadeScales = new DxVector4[4],
                    CascadeSplits = new float[4]
                }
            };
        }

        public static bool ShadowMapsNeedRemade;
        public static bool AreSplitsDirty;
        public static Texture ShadowMap;
        private static Texture _varianceShadowMap;
        private static Texture _tempVsm;
        private static Matrix _texScaleBias;
        public static DxVector3 Direction;

        public static void Init()
        {
            DepthOnlyPass = Shaders.Get("ShadowMapping").GetPass("Depth");
            ShadowMapsNeedRemade = true;
            AreSplitsDirty = true;
            _texScaleBias = Matrix.Scaling(0.5f, -0.5f, 1.0f);
            _texScaleBias.TranslationVector = new DxVector3(0.5f, 0.5f, 0.0f);
        }

        public static void MakeGlobalShadowMatrix(ref Camera camera, out Matrix globalShadowMatrix)
        {
            var cameraCb = camera.Constants;

            ResetFrustumCorners(ref _frustumCorners);
            var frustumCenter = DxVector3.Zero;
            for (int i = 0; i < 8; ++i)
            {
                DxVector3.TransformCoordinate(ref _frustumCorners[i], ref cameraCb.Data.InverseViewProjection,
                    out _frustumCorners[i]);
                DxVector3.Add(ref frustumCenter, ref _frustumCorners[i], out frustumCenter);
            }
            DxVector3.Divide(ref frustumCenter, 8.0f, out frustumCenter);

            var upDir = DxVector3.Up;

            var lightCameraPos = frustumCenter;
            DxVector3 lookAt;
            DxVector3.Subtract(ref frustumCenter, ref Direction, out lookAt);
            Matrix lightView;
            Matrix.LookAtLH(ref lightCameraPos, ref lookAt, ref upDir, out lightView);

            DxVector3 shadowCameraPos;
            //DxVector3.Multiply(ref Lighting.LightDirection, -0.5f, out shadowCameraPos);
            //DxVector3.Add(ref frustumCenter, ref shadowCameraPos, out shadowCameraPos);
            shadowCameraPos = frustumCenter + Direction * -0.5f;

            Matrix viewMatrix;
            Matrix projMatrix;
            Matrix viewProjMatrix;
            Matrix.OrthoOffCenterLH(-0.5f, 0.5f, -0.5f, 0.5f, 0.0f, 1.0f, out projMatrix);
            Matrix.LookAtLH(ref shadowCameraPos, ref frustumCenter, ref upDir, out viewMatrix);
            Matrix.Multiply(ref viewMatrix, ref projMatrix, out viewProjMatrix);

            Matrix.Multiply(ref viewProjMatrix, ref _texScaleBias, out globalShadowMatrix);
        }

        [StructLayout(LayoutKind.Explicit, Size = 220, Pack = 16)]
        internal struct ShadowReceiverData : IConstantBufferData
        {
            [FieldOffset(0)] public Matrix ShadowMatrix;
            [FieldOffset(64)] public float[] CascadeSplits;
            [FieldOffset(80)] public DxVector4[] CascadeOffsets;
            [FieldOffset(144)] public DxVector4[] CascadeScales;
            [FieldOffset(208)] public float ShadowDepthBias;
            [FieldOffset(212)] public float ShadowOffsetScale;
            [FieldOffset(216)] public int VisualizeCascades;
        }

        private static void CreateShadowMaps()
        {
            int shadowMapSize = RenderSettings.ShadowMapSize;

            ShadowMap?.Dispose();

            if (RenderSettings.UseFilterableShadows)
            {
                ShadowMap = new Texture(shadowMapSize, shadowMapSize, Format.R24_UNorm_X8_Typeless, true, BindFlags.DepthStencil) {Name="ShadowMap"};

                Format shadowMapFormat;

                switch (RenderSettings.ShadowMode)
                {
                    case ShadowMode.Evsm4:
                        shadowMapFormat = RenderSettings.ShadowMapFormat == ShadowFormat.Sm16Bit ? Format.R16G16B16A16_Float : Format.R32G32B32A32_Float;
                        break;
                    case ShadowMode.Evsm2:
                        shadowMapFormat = RenderSettings.ShadowMapFormat == ShadowFormat.Sm16Bit ? Format.R16G16_Float : Format.R32G32_Float;
                        break;
                    case ShadowMode.MsmHamburger:
                    case ShadowMode.MsmHausdorff:
                        shadowMapFormat = RenderSettings.ShadowMapFormat == ShadowFormat.Sm16Bit ? Format.R16G16B16A16_UNorm : Format.R32G32B32A32_Float;
                        break;
                    default:
                        shadowMapFormat = RenderSettings.ShadowMapFormat == ShadowFormat.Sm16Bit ? Format.R16G16_UNorm : Format.R32G32_Float;
                        break;
                }

                _varianceShadowMap = new Texture(shadowMapSize, shadowMapSize, shadowMapFormat,
                    RenderSettings.UseShadowMips, BindFlags.RenderTarget | BindFlags.ShaderResource, CascadeCount)
                {
                    Name = "VsmSMap"
                };
                _tempVsm = new Texture(shadowMapSize, shadowMapSize, shadowMapFormat, false, BindFlags.RenderTarget | BindFlags.ShaderResource, 1) {Name="TempSMap"};
            }
            else
            {
                ShadowMap = new Texture(shadowMapSize, shadowMapSize, Format.R24G8_Typeless, false,
                    BindFlags.DepthStencil | BindFlags.ShaderResource, CascadeCount) {Name = "ShadowMap"};
            }

            _viewport = new ViewportF(0, 0, shadowMapSize, shadowMapSize, 0, 1);

            ShadowMapsNeedRemade = false;
        }

        public const int CascadeCount = 4;
        public const int MinCascadeDistance = 0;
        public const int MaxCascadeDistance = 1;
        private static Camera _lastCamera;

        private static float[] _cascadeSplits;
        private static RawViewportF _viewport;

        private static void ComputeSplits(ref Camera camera)
        {
            _cascadeSplits = new float[4];

            var partitionMode = RenderSettings.CascadePartitionMode;
            switch (partitionMode)
            {
                case CascadePartitionMode.Manual:
                    _cascadeSplits[0] = MinCascadeDistance + RenderSettings.CascadeSplit0 * MaxCascadeDistance;
                    _cascadeSplits[1] = MinCascadeDistance + RenderSettings.CascadeSplit1 * MaxCascadeDistance;
                    _cascadeSplits[2] = MinCascadeDistance + RenderSettings.CascadeSplit2 * MaxCascadeDistance;
                    _cascadeSplits[3] = MinCascadeDistance + RenderSettings.CascadeSplit3 * MaxCascadeDistance;
                    break;
                case CascadePartitionMode.Logarithmic:
                case CascadePartitionMode.ParallelSplit:

                    var lambda = partitionMode == CascadePartitionMode.ParallelSplit
                        ? RenderSettings.PssmLambda
                        : 1.0f;

                    var nearClip = camera.ClipNear;
                    var farClip = camera.ClipFar;
                    var clipRange = farClip - nearClip;

                    var minZ = nearClip + MinCascadeDistance * clipRange;
                    var maxZ = nearClip + MaxCascadeDistance * clipRange;

                    var range = maxZ - minZ;
                    var ratio = maxZ / minZ;

                    for (int i = 0; i < CascadeCount; i++)
                    {
                        float p = (i + 1) / (float)CascadeCount;
                        float log = minZ * Mathf.Pow(ratio, p);
                        float uniform = minZ + range * p;
                        float d = lambda * (log - uniform) + uniform;
                        _cascadeSplits[i] = (d - nearClip);
                    }
                    
                    // TODO: debug
                    _cascadeSplits = new [] { 0.00120558857f, 0.0120992521f, 0.110534795f, 1.0f };

                    break;
            }

            AreSplitsDirty = false;
        }

        public static void RenderShadowMap(ref DeviceContext context, ref IWorld world, ref Camera camera)
        {
            PixHelper.BeginEvent(default(RawColorBGRA), "Shadow Rendering");
            {
                if (ShadowMapsNeedRemade)
                    CreateShadowMaps();

                var shadowMapSize = ShadowMap.Width;

                if (AreSplitsDirty || camera != _lastCamera)
                    ComputeSplits(ref camera);

                _lastCamera = camera;
                
                MakeGlobalShadowMatrix(ref camera, out ReceiverConstants.Data.ShadowMatrix);

                context.Rasterizer.SetViewport(_viewport);

                for (int cascadeIndex = 0; cascadeIndex < CascadeCount; ++cascadeIndex)
                {
                    PixHelper.BeginEvent(default(RawColorBGRA), "Cascade");
                    {
                        ResetFrustumCorners(ref _frustumCorners);

                        var prevSplitDist = cascadeIndex == 0 ? MinCascadeDistance : _cascadeSplits[cascadeIndex - 1];
                        var splitDist = _cascadeSplits[cascadeIndex];

                        for (int i = 0; i < 8; i++)
                            DxVector3.TransformCoordinate(ref _frustumCorners[i],
                                ref camera.Constants.Data.InverseViewProjection, out _frustumCorners[i]);

                        for (int i = 0; i < 4; i++)
                        {
                            DxVector3 cornerRay;
                            DxVector3 nearCornerRay;
                            DxVector3 farCornerRay;
                            DxVector3.Subtract(ref _frustumCorners[i + 4], ref _frustumCorners[i], out cornerRay);
                            DxVector3.Multiply(ref cornerRay, prevSplitDist, out nearCornerRay);
                            DxVector3.Multiply(ref cornerRay, splitDist, out farCornerRay);
                            DxVector3.Add(ref _frustumCorners[i], ref farCornerRay, out _frustumCorners[i + 4]);
                            DxVector3.Add(ref _frustumCorners[i], ref nearCornerRay, out _frustumCorners[i]);
                        }

                        var frustumCenter = DxVector3.Zero;
                        for (int i = 0; i < 8; i++)
                            DxVector3.Add(ref frustumCenter, ref _frustumCorners[i], out frustumCenter);
                        DxVector3.Multiply(ref frustumCenter, 0.125F, out frustumCenter);

                        var upDir = DxVector3.Up;

                        float sphereRadius = 0.0f;
                        for (int i = 0; i < 8; ++i)
                        {
                            DxVector3 sum;
                            DxVector3.Subtract(ref _frustumCorners[i], ref frustumCenter, out sum);
                            sphereRadius = Math.Max(sphereRadius, sum.Length());
                        }

                        sphereRadius = (float)Math.Ceiling(sphereRadius * 16.0f) / 16.0f;

                        var maxExtents = new DxVector3(sphereRadius, sphereRadius, sphereRadius);
                        DxVector3 minExtents;
                        DxVector3.Negate(ref maxExtents, out minExtents);

                        DxVector3 cascadeExtents;
                        DxVector3.Subtract(ref maxExtents, ref minExtents, out cascadeExtents);

                        // TODO: optimize
                        var shadowCameraPos = frustumCenter + Direction * -minExtents.Z;

                        Matrix viewMatrix;
                        Matrix projectionMatrix;
                        Matrix viewProjMatrix;

                        Matrix.LookAtLH(ref shadowCameraPos, ref frustumCenter, ref upDir, out viewMatrix);
                        Matrix.OrthoOffCenterLH(minExtents.X, maxExtents.X, minExtents.Y,
                            maxExtents.Y, 0.0f, cascadeExtents.Z, out projectionMatrix);
                        Matrix.Multiply(ref viewMatrix, ref projectionMatrix, out viewProjMatrix);

                        var shadowOrigin = DxVector4.UnitW;
                        DxVector4.Transform(ref shadowOrigin, ref viewProjMatrix, out shadowOrigin);
                        DxVector4.Multiply(ref shadowOrigin, shadowMapSize / 2.0f, out shadowOrigin);

                        var roundedOrigin = new DxVector4(Mathf.Round(shadowOrigin.X), Mathf.Round(shadowOrigin.Y),
                            Mathf.Round(shadowOrigin.Z), Mathf.Round(shadowOrigin.W));
                        DxVector4 roundOffset;
                        DxVector4.Subtract(ref roundedOrigin, ref shadowOrigin, out roundOffset);
                        DxVector4.Multiply(ref roundOffset, 2.0f / shadowMapSize, out roundOffset);
                        roundOffset.Z = 0.0f;
                        roundOffset.W = 0.0f;

                        projectionMatrix.Row4 += roundOffset; // R[3]
                        Matrix.Multiply(ref viewMatrix, ref projectionMatrix, out viewProjMatrix);

                        // RenderDepthCPU
                        SetupRenderDepthState(ref context, ref viewProjMatrix, cascadeIndex);
                        world.RenderObjectProvider.Draw(ref context, ref camera, true);

                        var texScaleBias = new Matrix
                        {
                            Row1 = new SharpDX.Vector4(0.5f, 0.0f, 0.0f, 0.0f),
                            Row2 = new SharpDX.Vector4(0.0f, -0.5f, 0.0f, 0.0f),
                            Row3 = new SharpDX.Vector4(0.0f, 0.0f, 1.0f, 0.0f),
                            Row4 = new SharpDX.Vector4(0.5f, 0.5f, 0.0f, 1.0f)
                        };
                        Matrix.Multiply(ref viewProjMatrix, ref texScaleBias, out viewProjMatrix);

                        var clipNear = camera.ClipNear;
                        var clipFar = camera.ClipFar;
                        var clipDist = clipFar - clipNear;
                        ReceiverConstants.Data.CascadeSplits[cascadeIndex] = clipNear + splitDist * clipDist;

                        Matrix invCascadeMatrix;
                        Matrix.Invert(ref viewProjMatrix, out invCascadeMatrix);

                        var zero = DxVector3.Zero;
                        DxVector3 cascadeCorner;
                        DxVector3.TransformCoordinate(ref zero, ref invCascadeMatrix, out cascadeCorner);
                        DxVector3.TransformCoordinate(ref cascadeCorner, ref ReceiverConstants.Data.ShadowMatrix,
                            out cascadeCorner);

                        var one = DxVector3.One;
                        DxVector3 otherCorner;
                        DxVector3.TransformCoordinate(ref one, ref invCascadeMatrix, out otherCorner);
                        DxVector3.TransformCoordinate(ref otherCorner, ref ReceiverConstants.Data.ShadowMatrix, out otherCorner);

                        var cascadeScale = DxVector3.One / (otherCorner - cascadeCorner);
                        ReceiverConstants.Data.CascadeOffsets[cascadeIndex] = new DxVector4(-cascadeCorner, 0.0f);
                        ReceiverConstants.Data.CascadeScales[cascadeIndex] = new DxVector4(cascadeScale, 1.0f);

                        if (RenderSettings.UseFilterableShadows)
                            ConvertToVsm(ref context, cascadeIndex,
                                ref ReceiverConstants.Data.CascadeSplits[cascadeIndex],
                                ref ReceiverConstants.Data.CascadeScales[0]);
                    }
                    PixHelper.EndEvent();
                }
            }
            PixHelper.EndEvent();

            DataStream stream;
            context.MapSubresource(ReceiverConstants.Buffer, MapMode.WriteDiscard, MapFlags.None, out stream);
            stream.Write(ReceiverConstants.Data.ShadowMatrix);
            stream.WriteRange(ReceiverConstants.Data.CascadeSplits, 0, 4);
            stream.WriteRange(ReceiverConstants.Data.CascadeOffsets, 0, 4);
            stream.WriteRange(ReceiverConstants.Data.CascadeScales, 0, 4);
            stream.Write(ReceiverConstants.Data.ShadowDepthBias);
            stream.Write(ReceiverConstants.Data.ShadowOffsetScale);
            stream.Write(ReceiverConstants.Data.VisualizeCascades);

            context.UnmapSubresource(ReceiverConstants.Buffer, 0);

            //ReceiverConstants.Update(ref context);
        }

        private static void ConvertToVsm(ref DeviceContext context, int cascadeIndex, ref float cascadeSplit, ref DxVector4 cascadeScale)
        {
            throw new NotImplementedException();
        }

        private static void ResetFrustumCorners(ref DxVector3[] corners)
        {
            corners[0] = new DxVector3(-1.0f, 1.0f, 0.0f);
            corners[1] = new DxVector3( 1.0f,  1.0f, 0.0f);
            corners[2] = new DxVector3( 1.0f, -1.0f, 0.0f);
            corners[3] = new DxVector3(-1.0f, -1.0f, 0.0f);
            corners[4] = new DxVector3(-1.0f, 1.0f, 1.0f);
            corners[5] = new DxVector3(1.0f, 1.0f, 1.0f);
            corners[6] = new DxVector3(1.0f, -1.0f, 1.0f);
            corners[7] = new DxVector3(-1.0f, -1.0f, 1.0f);
        }

        private static void SetupRenderDepthState(ref DeviceContext context, ref Matrix shadowMatrix, int cascadeIndex)
        {
            _depthConstants.Data = shadowMatrix;
            _depthConstants.Update(ref context);
            context.VertexShader.SetConstantBuffer(0, _depthConstants);

            var dsv = ShadowMap.DsvArraySlices[RenderSettings.UseFilterableShadows ? 0 : cascadeIndex];
            context.OutputMerger.SetRenderTargets(dsv);
            context.ClearDepthStencilView(dsv, DepthStencilClearFlags.Depth, 1, 0);

            if (cascadeIndex == 0)
            {
                //context.OutputMerger.SetBlendState(BlendStates.Disabled);
                //context.OutputMerger.SetDepthStencilState(DepthStencilStates.DepthEnabled);
                //context.Rasterizer.State = RasterizerStates.NoCulling;

                DepthOnlyPass.Use(ref context);
                context.VertexShader.SetConstantBuffer(0, _depthConstants);
                context.PixelShader.SetShaderResources(0, null, null, null);
            }
        }
    }
}