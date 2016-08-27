// Renderer.cs - dEngine
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using C5;
using dEngine.Data;
using dEngine.Graphics.States;
using dEngine.Instances;
using dEngine.Services;
using dEngine.Settings.Global;
using dEngine.Utility;
using dEngine.Utility.Extensions;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;
using DeviceD3D = SharpDX.Direct3D11.Device1;
using DeviceD2D = SharpDX.Direct2D1.Device1;
using ContextD3D = SharpDX.Direct3D11.DeviceContext;
using ContextD2D = SharpDX.Direct2D1.DeviceContext1;
using Device = SharpDX.DXGI.Device;
using FactoryD2D = SharpDX.Direct2D1.Factory2;
using FactoryDXGI = SharpDX.DXGI.Factory2;
using FactoryWIC = SharpDX.WIC.ImagingFactory;
using FactoryDW = SharpDX.DirectWrite.Factory1;
using FactoryType = SharpDX.Direct2D1.FactoryType;
using FeatureLevel = SharpDX.Direct3D.FeatureLevel;
using Mesh = dEngine.Instances.Mesh;
using PixelFormatWIC = SharpDX.WIC.PixelFormat;
using TextAntialiasMode = SharpDX.Direct2D1.TextAntialiasMode;

namespace dEngine.Graphics
{
    /// <summary>
    /// Class for managing graphics.
    /// </summary>
    public static class Renderer
    {
        internal const float ThrottleFramerate = 1 / 15f;
        private static Vector2 _controlSize = Vector2.One;

        internal static DeviceD3D Device;
        internal static DeviceD2D Device2D;
        internal static DeviceDebug DeviceDebug;
        internal static Device DxgiDevice;

        internal static ContextD3D Context;
        internal static ContextD2D Context2D;

        internal static FactoryDXGI Factory;
        internal static FactoryDW DirectWriteFactory;
        internal static FactoryWIC ImagingFactory;
        internal static FactoryD2D Factory2D;

        internal static Buffer PointLightBuffer;
        internal static Buffer SpotLightBuffer;
        internal static MemoizingMRUCache<Colour, SolidColorBrush> Brushes;
        internal static UnorderedAccessView PointLightUAV;

        internal static GfxShader StandardShader;
        internal static GfxShader GouraudShader;
        internal static GfxShader LightingShader;
        internal static GfxShader AdornShader;
        internal static GfxShader.Pass MainPass;
        internal static GfxShader.Pass LightingPass;
        internal static GfxShader PostProcessShader;
        internal static GfxShader.Pass MergePass;
        internal static GfxShader SkyboxShader;
        internal static GfxShader.Pass SkyboxPass;
        internal static GfxShader.Pass AALinePass;
        internal static GfxShader.Pass AdornSelfLitPass;

        internal static Texture MaterialDiffuseBuffer;
        internal static Texture MaterialNormalBuffer;
        internal static Texture MaterialSpecularBuffer;

        internal static readonly object Locker = new object();
        internal static ILogger Logger = LogService.GetLogger();

        internal static float DeltaTime;

        static Renderer()
        {
            DeltaTime = 0;

            Meshes = new ConcurrentDictionary<Mesh, byte>();
        }

        internal static TreeSet<Part> TransparentParts { get; }
        internal static ConcurrentDictionary<Mesh, byte> Meshes { get; private set; }

        internal static PassType CurrentPassType { get; set; }

        /// <summary>
        /// If true, the manager has been initialized.
        /// </summary>
        public static bool IsInitialized { get; internal set; }

        /// <summary>
        /// When set to true, all resources will be resized the following frame.
        /// </summary>
        public static bool ResizeNextFrame { get; set; }

        /// <summary>
        /// The size of the <see cref="Engine.Control" />.
        /// </summary>
        public static Vector2 ControlSize
        {
            get { return _controlSize; }
            set
            {
                _controlSize = value;

                var camera = Game.FocusedCamera;
                if (camera != null)
                    camera.ViewportSize = _controlSize;
            }
        }

        internal static GfxShader.Pass CurrentPass { get; set; }

        /// <summary>
        /// Fired when the <see cref="Renderer" /> has been initialized.
        /// </summary>
        public static event Action Initialized;

        internal static void Init()
        {
            Logger.Info("GraphicsManager initializing.");

            // Direct3D11 Init ---
            ResizeNextFrame = true;

            Factory = new FactoryDXGI();
            ImagingFactory = new FactoryWIC();
            Factory2D = new FactoryD2D(FactoryType.MultiThreaded,
                Engine.IsDebug ? DebugLevel.Error : DebugLevel.None);
            DirectWriteFactory = new FactoryDW(SharpDX.DirectWrite.FactoryType.Shared);

            CreateDevices(RenderSettings.GraphicsAdapter);

            Brushes =
                new MemoizingMRUCache<Colour, SolidColorBrush>(
                    (colour, _) => new SolidColorBrush(Context2D, (Color4)colour) { Opacity = colour.A }, int.MaxValue,
                    brush => brush.Dispose());
            // ------------------------------------

            Engine.HandleSet += hwnd =>
            {
                var camera = Game.Workspace.CurrentCamera;
                camera.RenderHandle = hwnd;
            };

            if (Engine.Handle != IntPtr.Zero)
            {
                var camera = Game.Workspace.CurrentCamera;
                camera.RenderHandle = Engine.Handle;
            }

            SamplerStates.Load();
            Shaders.Init();
            BlendStates.Load();
            DepthStencilStates.Load();
            RasterizerStates.Load();

            //StandardConstants = new ConstantBuffer<StandardConstantData>();

            StandardShader = Shaders.Get("Standard");
            MainPass = StandardShader.GetPass();
            LightingShader = Shaders.Get("Lighting");
            LightingPass = LightingShader.GetPass();

            PostProcessShader = Shaders.Get("PostProcess");

            SkyboxShader = Shaders.Get("Skybox");
            SkyboxPass = SkyboxShader.GetPass();

            AdornShader = Shaders.Get("Adorn");
            AALinePass = AdornShader.GetPass("AALine");
            AdornSelfLitPass = AdornShader.GetPass("AdornSelfLit");

            Shadows.Init();

            // ------------------

            IsInitialized = true;
            Initialized?.Invoke();
            Initialized = null;

            //PixHelper.AllowProfiling(Engine.IsDebug);

            Logger.Info("Renderer initialized.");
        }

        private static void CreateDevices(int graphicsAdapter)
        {
            var deviceCreationFlags = DeviceCreationFlags.BgraSupport;
#if DEBUG
            deviceCreationFlags |= DeviceCreationFlags.Debug;
#endif

            Logger.Info("Attempting to create device.");

            var adapter = Factory.GetAdapter(graphicsAdapter);

            var levels = new[]
            {
                FeatureLevel.Level_11_1,
                FeatureLevel.Level_11_0,
                FeatureLevel.Level_10_1,
                FeatureLevel.Level_10_0,
                FeatureLevel.Level_9_3
            };

            var device = new SharpDX.Direct3D11.Device(adapter, deviceCreationFlags, levels)
            { DebugName = adapter.Description.Description };

            Logger.Info(
                $"GPU{graphicsAdapter}: {device.DebugName} ({((long)adapter.Description.DedicatedVideoMemory).ToPrettySize()} VRAM)");

            Device = device.QueryInterface<DeviceD3D>();
            Device.DebugName = device.DebugName;
            Logger.Info("D3D Device created.");
            if (deviceCreationFlags.HasFlag(DeviceCreationFlags.Debug))
            {
                DeviceDebug = new DeviceDebug(Device);
                Logger.Info("Debug device created.");
            }

            DxgiDevice = Device.QueryInterface<Device>();
            Context = Device.ImmediateContext;

            Device2D = new DeviceD2D(Factory2D, DxgiDevice);

            Context2D = new ContextD2D(Device2D,
                DeviceContextOptions.EnableMultithreadedOptimizations)
            {
                TextAntialiasMode =
                    RenderSettings.UseClearTypeRendering
                        ? TextAntialiasMode.Cleartype
                        : (RenderSettings.GuiAntiAliasing ? TextAntialiasMode.Grayscale : TextAntialiasMode.Aliased),
                AntialiasMode = RenderSettings.GuiAntiAliasing ? AntialiasMode.PerPrimitive : AntialiasMode.Aliased,
                UnitMode = UnitMode.Pixels
            };
            Logger.Info("D2D Device created.");

            Logger.Info("Filling out DebugSettings GPU info.");
            DebugSettings.FillGpuInfo(adapter);

#if DEBUG
            try
            {
                DeviceDebug = new DeviceDebug(Device);
                Logger.Info("Debug device created.");
            }
            catch (SharpDXException)
            {
                Logger.Warn("DeviceDebug not supported.");
            }
#endif

            Logger.Info("Renderer initialized.");
        }

        internal static void Update(double time)
        {
            if (!Game.IsInitialized)
                return;

            RunService.Service.RenderStepped.Fire(time);

            foreach (var world in Game.Worlds.Keys)
                Render(world, PassType.All);
        }

        /// <summary>
        /// Draws the scene from the perspective of the given camera to its <see cref="Camera.BackBuffer" />
        /// </summary>
        internal static void Render(IWorld world, PassType passes)
        {
            if (!world.IsRenderable)
                return;

            var camera = world.CurrentCamera;

            if (camera == null)
                return;

            lock (Locker)
            {
                if (camera.NeedsResize && (camera.SwapChain != null))
                {
                    camera.Resize(Device);
                    return;
                }

                if (!camera.CanRender)
                    return;

                if (passes.HasFlag(PassType.Shadow))
                    Shadows.RenderShadowMap(ref Context, ref world, ref camera);

                if (passes.HasFlag(PassType.Scene))
                {
                    PixHelper.BeginEvent(Color.Zero, "Geometry");
                    {
                        Context.Rasterizer.SetViewport(camera.Viewport);
                        Context.ClearDepthStencilView(camera.DepthStencilBuffer,
                            DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1, 0);
                        Context.ClearRenderTargetView(camera.Buffer0, Color.Zero);
                        Context.ClearRenderTargetView(camera.Buffer1, Color.Zero);
                        Context.ClearRenderTargetView(camera.Buffer2, Color.Zero);
                        Context.ClearRenderTargetView(camera.Buffer3, Color.Zero);

                        camera.UpdateConstants(ref Context);

                        Context.OutputMerger.SetRenderTargets(camera.DepthStencilBuffer,
                            camera.Buffer0,
                            camera.Buffer1,
                            camera.Buffer2,
                            camera.Buffer3);

                        CurrentPassType = PassType.Scene;

#pragma warning disable CS0728 // Possibly incorrect assignment to local which is the argument to a using or lock statement
                        world.RenderObjectProvider.Draw(ref Context, ref camera, false);

                        if (ReferenceEquals(world, Game.Workspace))
                            Game.Workspace.Terrain.Draw(ref Context);
#pragma warning restore CS0728 // Possibly incorrect assignment to local which is the argument to a using or lock statement
                    }
                    PixHelper.EndEvent();

                    if (passes.HasFlag(PassType.PhysicsDebug))
                    {
                        PixHelper.BeginEvent(Color.Zero, "Physics Debug");
                        world.Physics?.DrawWorld(ref Context, ref camera);
                        PixHelper.EndEvent();
                    }
                }
                else if (passes.HasFlag(PassType.Reflection))
                {
                }

                if (passes.HasFlag(PassType.Lighting))
                {
                    Atmosphere.Atmosphere.Update();
                    Lighting(ref camera); // rt is set to camera's backbuffer here.
                }

                if (passes.HasFlag(PassType.PostProcess))
                {
                    CurrentPassType = PassType.PostProcess;
                    PostProcess(ref camera); // uses camera's back buffer
                }

                if (passes.HasFlag(PassType.GUI))
                {
                    CurrentPassType = PassType.GUI;
                    // uses camera's back buffer
                    Draw3DGuis(camera);
                    DrawSurfaceGuis();
                    DrawBillboardGuis(camera);
                    DrawScreenGuis(camera);
                }

                if (camera.RenderHandle == IntPtr.Zero)
                    Context.Flush();
                else
                    camera.SwapChain.Present(1, 0);
            }
        }

        private static void Draw3DGuis(Camera camera)
        {
            //if (camera != Game.FocusedCamera) return;
            PixHelper.BeginEvent(Color.Zero, "3D Guis");
            lock (camera.Locker)
            {
                var guis = camera.Gui3Ds;
                var count = guis.Count;
                for (var i = 0; i < count; i++)
                    guis[i].Draw(ref Context, ref camera);
            }
            PixHelper.EndEvent();
        }

        private static void Lighting(ref Camera camera)
        {
            PixHelper.BeginEvent(Color.Zero, "Lighting");
            {
                Context.ClearRenderTargetView(camera.BackBuffer, Color.Zero);
                Context.OutputMerger.SetTargets((DepthStencilView)null, camera.BackBuffer);
                Context.PixelShader.SetShaderResources(0, camera.Buffer0,
                    camera.Buffer1,
                    camera.Buffer2,
                    camera.Buffer3,
                    camera.DepthStencilBuffer,
                    Game.Lighting.Skybox?.IrradianceMap?.Texture,
                    Game.Lighting.Skybox?.RadianceMap?.Texture);
                Context.PixelShader.SetConstantBuffers(0, camera.Constants, Services.Lighting.LightingConstantBuffer);

                LightingPass.Use(ref Context);

                Context.Draw(4, 0);

                Context.CopyResource(camera.BackBuffer.NativeTexture, camera.Buffer0.NativeTexture);

                Context.PixelShader.SetShaderResources(0, null, null, null, null, null, null, null, null);
            }
            PixHelper.EndEvent();
        }

        private static void PostProcess(ref Camera camera)
        {
            PixHelper.BeginEvent(Color.Zero, "Post Processing");
            {
                var postEffects = camera.PostEffects;
                lock (camera.PostEffectsLocker)
                {
                    for (var i = 0; i < postEffects.Count; i++)
                        camera.PostEffects[i].Render(ref Context);
                }
            }
            PixHelper.EndEvent();
        }

        private static void DrawScreenGuis(Camera camera)
        {
            PixHelper.BeginEvent(Color.Zero, "ScreenGuis");
            {
                if (camera.RenderTarget2D == null)
                    return;

                var starterGui = Game.StarterGui;

                lock (camera.Locker)
                {
                    var collectors = camera.LayerCollectors;
                    var count = collectors.Count;

                    for (var i = 0; i < count; i++)
                    {
                        var gui = collectors[i];
                        if ((RunService.SimulationState != SimulationState.Stopped) && (gui.Parent == starterGui))
                            continue;

                        gui.Render();
                    }
                }
            }
            PixHelper.EndEvent();
        }

        private static void DrawSurfaceGuis()
        {
        }

        private static void DrawBillboardGuis(Camera camera)
        {
        }

        internal static void Shutdown()
        {
            lock (Locker)
            {
                Factory2D?.Dispose();
                Device2D?.Dispose();
                Brushes.InvalidateAll();
                Brushes = null;

                DirectWriteFactory?.Dispose();
                ImagingFactory?.Dispose();

                Factory.Dispose();
                Device.Dispose();
                DeviceDebug?.Dispose();
                DxgiDevice?.Dispose();

                Shaders.DisposeAll();
            }
        }

        /// <summary>
        /// Gets the nearest 16:9 resolution.
        /// </summary>
        public static Vector2 GetNearestResolution(Vector2 res)
        {
            const int count = 7680 / 16; // 8K max res (16x9 * 480)

            for (var i = count; i > 0; i--)
            {
                var width = i * 16;
                var height = i * 9;

                if ((res.x >= width) && (res.y >= height))
                    return new Vector2(width, height);
            }

            return new Vector2(16, 9);
        }

        /// <summary>
        /// If the renderer has not yet been initialized, subscribe to the event, otherwise invoke the callback immediately.
        /// </summary>
        /// <param name="callback">The function to call if/when the renderer is initialized.</param>
        internal static void InvokeResourceDependent(Action callback)
        {
            if (IsInitialized)
                callback();
            else
                Initialized += callback;
        }

        internal class PartDistanceComparer : IComparer<Part>
        {
            public Vector3 Position;

            public int Compare(Part x, Part y)
            {
                if (Equals(x, y)) return 0;

                var xDist = (x.Position - Position).magnitude;
                var yDist = (y.Position - Position).magnitude;

                return xDist.CompareTo(yDist);
            }
        }

        [Flags]
        internal enum PassType
        {
            Shadow = 0,

            /// <summary>
            /// Draws all objects.
            /// </summary>
            Scene = 0 >> 1,

            /// <summary>
            /// Draws a minimal render of the environment: terrain, sky and important buildings.
            /// </summary>
            Reflection = 0 >> 2,

            /// <summary>
            /// Performs post-process.
            /// </summary>
            PostProcess = 0 >> 3,

            /// <summary>
            /// Draws GUI objects.
            /// </summary>
            GUI = 0 >> 4,

            /// <summary>
            /// Performs lighting pass.
            /// </summary>
            Lighting = 0 >> 5,

            /// <summary>
            /// Draws physics debug.
            /// </summary>
            PhysicsDebug = 0 >> 6,

            /// <summary>
            /// </summary>
            Irradiance = 0 >> 7,


            All = Shadow | Scene | Reflection | PostProcess | GUI | Lighting | PhysicsDebug
        }
    }
}