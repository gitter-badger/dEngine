// RenderSettings.cs - dEngine
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
using System.Runtime.CompilerServices;
using dEngine.Graphics;
using dEngine.Instances.Attributes;
using dEngine.Utility;
using Neo.IronLua;
using SharpDX.Direct2D1;


namespace dEngine.Settings.Global
{
	/// <summary>
	/// Settings for rendering.
	/// </summary>
	[TypeId(186)]
	public class RenderSettings : Settings
	{
		private static int _graphicsAdapter;
		private static NumberSequence _manualCascadeSplits;
		private static CascadePartitionMode _cascadePartitionMode;
		private static int _shadowMapSize;
		private static float _pssmLambda;
		private static bool _areCascadesShown;
		private static GraphicsMode _graphicsMode;
		private static float _shadowOffsetScale;
		private static float _shadowDepthBias;
	    private static string _shaderCache;
	    private static SampleCount _sampleCount;
	    private static DownsampleSize _ssaoDownsampleSize;
	    private static int _ssaoBlurPasses;
	    private static BlurMode _ssaoBlurMode;
	    private static bool _ssaoDownsampling;
	    private static bool _showSsaoVisualization;
	    private static bool _useFilterableShadows;
	    private static ShadowMode _shadowMode;
	    private static ShadowFormat _shadowMapFormat;
	    private static bool _useShadowMips;
        private static float _cascadeSplit0;
        private static float _cascadeSplit1;
        private static float _cascadeSplit2;
        private static float _cascadeSplit3;
	    private static bool _useClearTypeRendering;
	    private static bool _guiAntiAliasing;

	    /// <summary>
        /// Gets the tempature of the graphics card.
        /// </summary>
        /// <returns></returns>
	    public LuaResult GetGpuTemps()
	    {
	        switch (DebugSettings.GpuVendor)
	        {
	            case GpuVendor.Nvidia:
	                NvApi.ThermalSettings thermalSettings = new NvApi.ThermalSettings();
	                if (NvApi.GetThermalSettings(Renderer.Device.NativePointer, 0, ref thermalSettings) != 0)
                        throw new Exception("NvidiaControlPanel.GetThermalSettings() returned false.");
	                var sensor = thermalSettings.Sensors[0];
                    return new LuaResult(sensor.CurrentTemp, sensor.DefaultMinTemp, sensor.DefaultMaxTemp);
                case GpuVendor.AMD:
                    throw new NotImplementedException();
	            case GpuVendor.Intel:
                    throw new NotImplementedException();
	            default:
                    throw new NotSupportedException($"Tempature measurements are not supported for vendor {DebugSettings.GpuVendor}");
            }
	    }

	    /// <summary>
	    /// The resolution of the shadow maps.
	    /// </summary>
	    [EditorVisible("Shadows", "Shadow Map Resolution")]
	    public static int ShadowMapSize
	    {
	        get { return _shadowMapSize; }
	        set
	        {
	            _shadowMapSize = Math.Min(16384, Math.Max(256, value));
	            Shadows.ShadowMapsNeedRemade = true;
	            NotifyChangedStatic();
	        }
	    }

        /// <summary>
        /// The current shader cache file. If set to blank, the cache will be rebuilt.
        /// </summary>
        [EditorVisible("Shaders", "Shader Cache")]
        public static string ShaderCache
	    {
	        get { return _shaderCache; }
	        set
	        {
	            _shaderCache = value;
	            NotifyChangedStatic();
	        }
	    }

	    /// <summary>
	    /// The first cascade manual split.
	    /// </summary>
	    [EditorVisible("Shadows", "Cascade Split 0")]
	    public static float CascadeSplit0
	    {
	        get { return _cascadeSplit0; }
	        set
	        {
                _cascadeSplit0 = value;
	            Shadows.AreSplitsDirty = true;
	            NotifyChangedStatic();
	        }
        }

        /// <summary>
        /// The second cascade manual split.
        /// </summary>
        [EditorVisible("Shadows", "Cascade Split 1")]
        public static float CascadeSplit1
        {
            get { return _cascadeSplit1; }
            set
            {
                _cascadeSplit1 = value;
                Shadows.AreSplitsDirty = true;
                NotifyChangedStatic();
            }
        }

        /// <summary>
        /// The third cascade manual split.
        /// </summary>
        [EditorVisible("Shadows", "Cascade Split 2")]
        public static float CascadeSplit2
        {
            get { return _cascadeSplit2; }
            set
            {
                _cascadeSplit2 = value;
                Shadows.AreSplitsDirty = true;
                NotifyChangedStatic();
            }
        }

        /// <summary>
        /// The fourth cascade manual split when.
        /// </summary>
        [EditorVisible("Shadows", "Cascade Split 0")]
        public static float CascadeSplit3
        {
            get { return _cascadeSplit3; }
            set
            {
                _cascadeSplit3 = value;
                Shadows.AreSplitsDirty = true;
                NotifyChangedStatic();
            }
        }

        /// <summary>
        /// The mode for partitioning the shadow map cascades.
        /// </summary>
        [EditorVisible("Shadows", "Cascade Partition Mode")]
	    public static CascadePartitionMode CascadePartitionMode
	    {
	        get { return _cascadePartitionMode; }
	        set
	        {
	            _cascadePartitionMode = value;
	            Shadows.AreSplitsDirty = true;
	            NotifyChangedStatic();
	        }
	    }

	    /// <summary>
	    /// The mix between linear and logarithmic partitioning when using the PSSM partition mode.
	    /// </summary>
	    [EditorVisible("Shadows", "Parallel-Split Lambda")]
	    public static float PssmLambda
	    {
	        get { return _pssmLambda; }
	        set
	        {
	            _pssmLambda = value;
	            Shadows.AreSplitsDirty = true;
	            NotifyChangedStatic();
	        }
	    }

	    /// <summary>
	    /// The shadow depth bias. Used to mitigate self-shadowing issues.
	    /// </summary>
	    /// <remarks>
	    /// If the bias is too large "peter panning" will occur. If the number is too small self-shadowing will occur.
	    /// </remarks>
	    [EditorVisible("Shadows", "Depth Bias")]
	    public static float ShadowDepthBias
	    {
	        get { return _shadowDepthBias; }
	        set
	        {
	            _shadowDepthBias = value;
                Shadows.ReceiverConstants.Data.ShadowDepthBias = value;
                NotifyChangedStatic();
	        }
	    }

	    /// <summary>
	    /// The normal-offset bias. Used to mitigate self-shadowing issues.
	    /// </summary>
	    /// <remarks>
	    /// If the offset is too long "peter panning" will occur. If the number is too small self-shadowing will occur.
	    /// </remarks>
	    [EditorVisible("Shadows", "Offset Scale")]
	    public static float ShadowOffsetScale
	    {
	        get { return _shadowOffsetScale; }
	        set
	        {
	            _shadowOffsetScale = value;
	            Shadows.ReceiverConstants.Data.ShadowOffsetScale = value;
                NotifyChangedStatic();
	        }
	    }

	    /// <summary>
	    /// Determines whether a debug visualization of the cascade splits is drawn.
	    /// </summary>
	    [EditorVisible("Shadows", "Are Cascades Shown")]
	    public static bool AreCascadesShown
	    {
	        get { return _areCascadesShown; }
	        set
	        {
	            _areCascadesShown = value;
	            Shadows.ReceiverConstants.Data.VisualizeCascades = value ? 1 : 0;
                NotifyChangedStatic();
	        }
        }

        /// <summary>
        /// Determines whether filterable shadow maps are created.
        /// </summary>
        [EditorVisible("Shadows", "Use Filterable Shadows")]
        public static bool UseFilterableShadows
        {
            get { return _useFilterableShadows; }
            set
            {
                _useFilterableShadows = value;
                Shadows.ShadowMapsNeedRemade = true;
                NotifyChangedStatic();
            }
        }

        /// <summary>
        /// Determines the type of shadow maps to use.
        /// </summary>
        [EditorVisible("Shadows", "Shadow Mode")]
        public static ShadowMode ShadowMode
        {
            get { return _shadowMode; }
            set
            {
                _shadowMode = value;
                Shadows.ShadowMapsNeedRemade = true;
                NotifyChangedStatic();
            }
        }

        /// <summary>
        /// Determines the texture format to use for shadow maps.
        /// </summary>
        [EditorVisible("Shadows", "Shadow Map Format")]
        public static ShadowFormat ShadowMapFormat
        {
            get { return _shadowMapFormat; }
            set
            {
                _shadowMapFormat = value;
                Shadows.ShadowMapsNeedRemade = true;
                NotifyChangedStatic();
            }
        }

        /// <summary>
        /// Determines if shadow maps use mips.
        /// </summary>
        [EditorVisible("Shadows", "Use Shadow Mips")]
        public static bool UseShadowMips
        {
            get { return _useShadowMips; }
            set
            {
                _useShadowMips = value;
                Shadows.ShadowMapsNeedRemade = true;
                NotifyChangedStatic();
            }
        }

        /// <summary>
        /// The primary graphics adapter.
        /// </summary>
        [EditorVisible("General", "Graphics Adapter")]
	    public static int GraphicsAdapter
	    {
	        get { return _graphicsAdapter; }
	        set
	        {
	            _graphicsAdapter = value;
	            //if (Renderer.IsInitialized)
	            //	Renderer.SetAdapter(value);
	            NotifyChangedStatic();
	        }
	    }

	    /// <summary>
	    /// The graphics library to use.
	    /// </summary>
	    [EditorVisible("General", "Graphics Mode")]
	    public static GraphicsMode GraphicsMode
	    {
	        get { return _graphicsMode; }
	        set
	        {
	            _graphicsMode = value;
	            NotifyChangedStatic();
	        }
	    }

        /// <summary>
        /// The sample count for the SSAO effect.
        /// </summary>
        [EditorVisible("SSAO", "Sample Count")]
	    public static SampleCount SSAOSampleCount
	    {
	        get { return _sampleCount; }
	        set
	        {
	            _sampleCount = value;
	            NotifyChangedStatic();
	        }
	    }

        /// <summary>
        /// Show a visualization of the ambient occlusion.
        /// </summary>
        [EditorVisible("SSAO", "Show Ambient Occlusion")]
        public static bool ShowAmbientOcclusion
	    {
	        get { return _showSsaoVisualization; }
	        set
	        {
	            _showSsaoVisualization = value;
	            NotifyChangedStatic();
	        }
	    }

	    /// <summary>
	    /// The downsample size for the SSAO effect.
	    /// </summary>
	    [EditorVisible("SSAO", "Downsample Size")]
        public static DownsampleSize SSAODownsampleSize
	    {
	        get { return _ssaoDownsampleSize; }
	        set { _ssaoDownsampleSize = value;
                NotifyChangedStatic();
            }
        }

        /// <summary>
        /// The number of blur passes to perform.
        /// </summary>
        [EditorVisible("SSAO", "Blur Passes")]
        public static int SSAOBlurPasses
        {
            get { return _ssaoBlurPasses; }
            set
            {
                _ssaoBlurPasses = Math.Min(1, Math.Min(value, 4));
                NotifyChangedStatic();
            }
        }

        /// <summary>
        /// The type of blurring to use.
        /// </summary>
        [EditorVisible("SSAO", "Blur Mode")]
        public static BlurMode SSAOBlurMode
        {
            get { return _ssaoBlurMode; }
            set
            {
                _ssaoBlurMode = value;
                NotifyChangedStatic();
            }
        }

        /// <summary>
        /// Determines whether the blur is performed on the downsampled image.
        /// </summary>
        [EditorVisible("SSAO", "Blur Downsampling")]
        public static bool SSAOBlurDownsampling
        {
            get { return _ssaoDownsampling; }
            set
            {
                _ssaoDownsampling = value;
                NotifyChangedStatic();
            }
        }

        /// <summary>
        /// Determines whether text is anti-aliased.
        /// </summary>
        [EditorVisible("GUI", "Use ClearType Rendering")]
	    public static bool UseClearTypeRendering
	    {
	        get { return _useClearTypeRendering; }
	        set
	        {
	            _useClearTypeRendering = value;
	            NotifyChangedStatic();
	        }
        }

        /// <summary>
        /// Determines whether anti-aliasing is performed on GUI elements.
        /// </summary>
        [EditorVisible("GUI", "Gui Anti Aliasing")]
        public static bool GuiAntiAliasing
        {
            get { return _guiAntiAliasing; }
            set
            {
                _guiAntiAliasing = value;
                NotifyChangedStatic();
            }
        }

        private static void NotifyChangedStatic([CallerMemberName] string propertyName = null)
	    {
	        Engine.Settings?.RenderSettings?.NotifyChanged(propertyName);
	    }

	    /// <inheritdoc />
	    public override void RestoreDefaults()
	    {
	        GraphicsAdapter = 0;
	        GraphicsMode = GraphicsMode.Direct3D11;

	        ShadowMapSize = 2048;
	        CascadeSplit0 = 0.05f;
	        CascadeSplit1 = 0.3f;
	        CascadeSplit2 = 0.65f;
	        CascadeSplit3 = 1.0f;
	        CascadePartitionMode = CascadePartitionMode.Logarithmic;
	        AreCascadesShown = false;
	        PssmLambda = 0.01f;
	        ShadowDepthBias = 0.0009f;
	        ShadowOffsetScale = 2.0f;
	        ShaderCache = "";
            ShadowMode = ShadowMode.FixedSizePcf;
            ShadowMapFormat = ShadowFormat.Sm16Bit;
	        UseFilterableShadows = false;
	        UseShadowMips = false;

	        SSAOBlurPasses = 1;
            SSAODownsampleSize = DownsampleSize.QuarterSize;
	        SSAOSampleCount = SampleCount.Medium;
	        SSAOBlurMode = BlurMode.HighQualityBilateral;

	        GuiAntiAliasing = true;
	        UseClearTypeRendering = true;
	    }
	}
}