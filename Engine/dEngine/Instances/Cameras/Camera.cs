// Camera.cs - dEngine
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
using System.Diagnostics;
using System.Windows.Forms;
using C5;
using CSCore.XAudio2.X3DAudio;
using dEngine.Data;
using dEngine.Graphics;
using dEngine.Graphics.Structs;
using dEngine.Instances.Attributes;
using dEngine.Instances.Interfaces;
using dEngine.Serializer.V1;
using dEngine.Services;
using dEngine.Utility;
using Neo.IronLua;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using Device = SharpDX.Direct3D11.Device;
using DeviceContext = SharpDX.Direct3D11.DeviceContext;
using Resource = SharpDX.Direct3D11.Resource;

namespace dEngine.Instances
{
    /// <summary>
    /// A camera is used to render the scene from a certain perspective.
    /// </summary>
    /// <remarks>
    /// In a game, each client has its own Camera object.
    /// Cameras exist only on the viewer's client, within in a user's local <see cref="Workspace"/>. This means it can't be edited from the server.
    /// When code is [running on a client](index.html?title=LocalScript) it can access the Camera through the <see cref="Workspace.CurrentCamera"/> property.
    /// ## State
    /// A camera's state is defined in the following way:
    /// * The <see cref="CFrame" /> property represents the position and orientation of the camera.
    /// * The <see cref="Focus" /> property represents the point the camera is looking at. It's important to set this property,
    /// as it also represents where the game thinks you are in the world. Certain visuals will be more detailed and will update
    /// more frequently, depending on how close they are to the Focus.
    /// * The <see cref="CameraType" /> property represents the behaviour of the camera every frame.
    /// * The <see cref="FieldOfView" /> property represents the angle the user can see out of the sides of the Camera.
    /// </remarks>
    [TypeId(31), ToolboxGroup("Gameplay"), ExplorerOrder(1)]
    public partial class Camera : Instance, IWorld, IListenable
    {
        private readonly AmbientOcclusionEffect _ambientOcclusion;
        private readonly AttachBehaviour _attachBehaviour;
        private readonly BloomEffect _bloomEffect;
        private readonly ClassicBehaviour _classicBehaviour;
        private readonly ColourCorrectionEffect _colourCorrection;

        private readonly FixedBehaviour _fixedBehaviour;
        private readonly FollowBehaviour _followBehaviour;
        private readonly WorldRenderer _roProvider;
        private readonly ScriptableBehaviour _scriptableBehaviour;
        private readonly TrackBehaviour _trackBehaviour;
        private readonly WatchBehaviour _watchBehaviour;

        internal readonly object CFrameLocker = new object();

        internal readonly object PostEffectsLocker = new object();

        private ICameraSubject _cameraSubject;
        private CameraType _cameraType;
        private CFrame _cframe;
        private float _clipFar;
        private float _clipNear;
        private Control _control;
        private Behaviour _currentBehaviour;
        private CFrame _focus;
        private float _fovY;

        private bool _frustumCulling = true;
        private Projection _projection;
        private Matrix _projectionMatrix;
        private IntPtr _renderHandle;
        private Surface _surface;
        private Vector2 _viewportSize;
        private Matrix _viewProjectionMatrix;
        internal Character CharacterSubject;

        internal ConstantBuffer<CameraData> Constants;
        internal Model ModelSubject;
        internal Ray MouseRay;

        internal bool NeedsResize;

        internal Part PartSubject;
        internal Bitmap1 RenderTarget2D;
        internal SwapChain1 SwapChain;
        internal VehicleSeat VehicleSubject;
        internal ViewportF Viewport;

        /// <inheritdoc />
        public Camera()
        {
            Constants = new ConstantBuffer<CameraData>();

            _fovY = 85;
            ClipNear = 0.3f;
            ClipFar = 2000f;
            _projectionMatrix = Matrix.Identity;
            _cframe = CFrame.Identity;
            _focus = CFrame.Identity;

            _attachBehaviour = new AttachBehaviour(this);
            _classicBehaviour = new ClassicBehaviour(this);
            _fixedBehaviour = new FixedBehaviour(this);
            _followBehaviour = new FollowBehaviour(this);
            _scriptableBehaviour = new ScriptableBehaviour(this);
            _trackBehaviour = new TrackBehaviour(this);
            _watchBehaviour = new WatchBehaviour(this);

            CameraType = CameraType.Fixed;

            LayerCollectors = new SortedArray<LayerCollector>(new LayerCollector.PriorityComparer());
            Gui3Ds = new ArrayList<GuiBase3D>();
            ViewportSizeChanged = new Signal<Vector2>(this);
            Moved = new Signal(this);

            _roProvider = new WorldRenderer();

            FrustumCulling = true;

            PostEffects = new SortedArray<PostEffect>(new PostEffect.PostEffectSorter());

            _bloomEffect = new BloomEffect();
            _bloomEffect.SetCamera(this);
            _ambientOcclusion = new AmbientOcclusionEffect();
            _ambientOcclusion.SetCamera(this);
            _colourCorrection = new ColourCorrectionEffect();
            _colourCorrection.SetCamera(this);
            PostEffects.Add(_bloomEffect);
            PostEffects.Add(_ambientOcclusion);
            PostEffects.Add(_colourCorrection);

            NeedsResize = true;
        }

        internal Vector3 Velocity { get; private set; }

        /// <summary>
        /// The handle to the render control.
        /// </summary>
        internal IntPtr RenderHandle
        {
            get { return _renderHandle; }
            set
            {
                lock (Renderer.Locker)
                {
                    if (value == _renderHandle) return;

                    _renderHandle = value;
                    if (_control != null)
                        _control.Resize -= OnControlResize;
                    _control = null;
                    SwapChain?.Dispose();

                    if (value == IntPtr.Zero) return;
                    
                    if ((_control = Control.FromHandle(value)) != null)
                    {
                        _control.Resize += OnControlResize;
                        _control.GotFocus += OnGotFocus;
                        _control.LostFocus += OnLostFocus;
                    }

                    CreateSwapChain(value, Renderer.Device);
                    NeedsResize = true;
                }
            }
        }

        internal Texture BackBuffer { get; set; }

        /// <summary>
        /// Diffuse Colour, Occlusion
        /// </summary>
        internal Texture Buffer0 { get; set; }

        /// <summary>
        /// Metallic, Specular, Roughness
        /// </summary>
        internal Texture Buffer1 { get; set; }

        /// <summary>
        /// World space normal, ShadingModelID
        /// </summary>
        internal Texture Buffer2 { get; set; }

        /// <summary>
        /// Emission, PerObjectData
        /// </summary>
        internal Texture Buffer3 { get; set; }

        internal Texture BufferDownscaleQuarter0 { get; set; }
        internal Texture BufferDownscaleQuarter1 { get; set; }
        internal Texture BufferDownscaleHalf0 { get; set; }
        internal Texture BufferDownscaleHalf1 { get; set; }

        internal Texture DepthStencilBuffer { get; set; }

        /// <summary>
        /// The vertical field of view in degrees.
        /// </summary>
        [InstMember(1), EditorVisible("Camera")]
        public float FieldOfView
        {
            get { return _fovY; }
            set
            {
                _fovY = value;
                UpdateProjectionMatrix();
                RebuildFrustum(_frustumCulling);
                NotifyChanged(nameof(FieldOfView));
            }
        }

        /// <summary>
        /// The minimum draw distance.
        /// </summary>
        [InstMember(2), EditorVisible("Camera")]
        public float ClipNear
        {
            get { return _clipNear; }
            set
            {
                _clipNear = value;
                Constants.Data.ClipNear = value;
                UpdateProjectionMatrix();
                RebuildFrustum(_frustumCulling);
                NotifyChanged(nameof(ClipNear));
            }
        }

        /// <summary>
        /// The maximum draw distance.
        /// </summary>
        [InstMember(3), EditorVisible("Camera")]
        public float ClipFar
        {
            get { return _clipFar; }
            set
            {
                _clipFar = value;
                Constants.Data.ClipFar = value;
                UpdateProjectionMatrix();
                RebuildFrustum(_frustumCulling);
                NotifyChanged(nameof(ClipFar));
            }
        }

        /// <summary>
        /// The position/rotation of the camera.
        /// </summary>
        [InstMember(4), EditorVisible("Data")]
        public CFrame CFrame
        {
            get { return _cframe; }
            set
            {
                _cframe = value;
                if (_frustumCulling)
                    _frustumGhost.WorldTransform = (BulletSharp.Math.Matrix)value;
                Moved.Fire();
            }
        }

        /// <summary>
        /// The type of projection this camera uses.
        /// </summary>
        [InstMember(5), EditorVisible("Camera")]
        public Projection Projection
        {
            get { return _projection; }
            set
            {
                if (value == _projection) return;
                _projection = Projection.Perspective;
                if (value == Projection.Orthographic)
                    Logger.Warn("Orthographic projection is not implemented.");
                NotifyChanged();
            }
        }

        /// <summary>
        /// The type of camera movement.
        /// </summary>
        [InstMember(6), EditorVisible("Camera")]
        public CameraType CameraType
        {
            get { return _cameraType; }
            set
            {
                _cameraType = value;

                switch (_cameraType)
                {
                    case CameraType.Fixed:
                        CurrentBehaviour = _fixedBehaviour;
                        break;
                    case CameraType.Attach:
                        CurrentBehaviour = _attachBehaviour;
                        break;
                    case CameraType.Watch:
                        CurrentBehaviour = _watchBehaviour;
                        break;
                    case CameraType.Track:
                        CurrentBehaviour = _trackBehaviour;
                        break;
                    case CameraType.Follow:
                        CurrentBehaviour = _followBehaviour;
                        break;
                    case CameraType.Custom:
                        CurrentBehaviour = _classicBehaviour;
                        break;
                    case CameraType.Scriptable:
                        CurrentBehaviour = _scriptableBehaviour;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                NotifyChanged();
            }
        }

        /// <summary>
        /// Determines whether the camera uses frustum culling.
        /// </summary>
        [InstMember(7)]
        public bool FrustumCulling
        {
            get { return _frustumCulling; }
            set
            {
                if (value == _frustumCulling) return;
                RebuildFrustum(value);
                _frustumCulling = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// The focus position of the camera.
        /// </summary>
        [InstMember(8), EditorVisible("Data")]
        public CFrame Focus
        {
            get { return _focus; }
            set
            {
                if (value == CFrame.Zero) value = CFrame.Identity;
                _focus = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// The camera subject.
        /// </summary>
        [EditorVisible("Camera")]
        public ICameraSubject CameraSubject
        {
            get { return _cameraSubject; }
            set
            {
                PartSubject = null;
                ModelSubject = null;
                CharacterSubject = null;
                VehicleSubject = null;

                Debug.Assert(value is Instance);
                _cameraSubject = value;

                PartSubject = value as Part;
                ModelSubject = value as Model;
                CharacterSubject = value as Character;
                VehicleSubject = value as VehicleSeat;

                NotifyChanged();
            }
        }

        /// <summary>
        /// The size of the client's viewport.
        /// </summary>
        [EditorVisible("Data")]
        public Vector2 ViewportSize
        {
            get { return _viewportSize; }
            internal set
            {
                lock (Renderer.Locker) // must be locked incase size changes while resizing
                {
                    _viewportSize = value;
                    AspectRatio = value.x / value.y;
                    NeedsResize = true;
                    ViewportSizeChanged.Fire(value);
                    NotifyChanged();
                }
            }
        }

        internal BoundingFrustum Frustum { get; set; }

        /// <summary>
        /// The aspect ratio of the <see cref="ViewportSize" />.
        /// </summary>
        public float AspectRatio { get; private set; }

        internal ArrayList<GuiBase3D> Gui3Ds { get; set; }
        internal SortedArray<LayerCollector> LayerCollectors { get; set; }
        internal SortedArray<PostEffect> PostEffects { get; }

        internal bool CanRender { get; set; }

        internal Behaviour CurrentBehaviour
        {
            get { return _currentBehaviour; }
            set
            {
                _currentBehaviour?.Deactivate();
                _currentBehaviour = value;
                value.Activate();
            }
        }

        void IListenable.UpdateListener(ref Listener listener)
        {
            ((IListenable)_cframe).UpdateListener(ref listener);
        }

        public bool IsLoaded => true;

        Camera IWorld.CurrentCamera
        {
            get { return this; }
            set { throw new InvalidOperationException(); }
        }

        PhysicsSimulation IWorld.Physics => World.Physics;
        WorldRenderer IWorld.RenderObjectProvider => _roProvider;
        bool IWorld.IsRenderable => true;

        [Obsolete]
        LuaTuple<Part, Vector3, Vector3> IWorld.FindPartOnRay(Ray ray, float maxLength)
        {
            throw new NotSupportedException();
        }

        [Obsolete]
        LuaTuple<Part, Vector3, Vector3> IWorld.FindPartOnRay(Ray ray, LuaTable filterTable, float maxLength)
        {
            throw new NotSupportedException();
        }

        [Obsolete]
        LuaTuple<Part, Vector3, Vector3> IWorld.FindPartOnRay(Ray ray, Func<dynamic, dynamic> filterFunc, float maxLength)
        {
            throw new NotSupportedException();
        }

        internal override void AfterDeserialization(Inst.Context context)
        {
            base.AfterDeserialization(context);

            lock (PostEffectsLocker)
            {
                foreach (var child in Children)
                {
                    var effect = child as PostEffect;
                    if (effect != null)
                    {
                        PostEffects.Add(effect);
                        effect.SetCamera(this);
                    }
                }
            }
        }

        private void OnGotFocus(object sender, EventArgs eventArgs)
        {
            lock (Locker)
            {
                InputService.CurrentControl = _control;
                Game.FocusedCamera = this;
            }
        }

        private void OnLostFocus(object sender, EventArgs eventArgs)
        {
            lock (Locker)
            {
                if (InputService.CurrentControl == _control)
                {
                    InputService.CurrentControl = null;
                }
            }
        }

        private void OnControlResize(object sender, EventArgs eventArgs)
        {
            ViewportSize = new Vector2(_control.Width, _control.Height);
        }

        /// <inheritdoc />
        protected override void OnChildAdded(Instance child)
        {
            base.OnChildAdded(child);
            PostEffect effect;
            ICameraUser cameraUser;

            if ((effect = child as PostEffect) != null)
            {
                lock (PostEffectsLocker)
                {
                    PostEffects.Add(effect);
                    effect.SetCamera(this);

                    // replace default effect with user effect
                    if (effect is BloomEffect)
                        PostEffects.Remove(_bloomEffect);
                    else if (effect is ColourCorrectionEffect)
                        PostEffects.Remove(_colourCorrection);
                }
            }
            else if ((cameraUser = child as ICameraUser) != null)
            {
                cameraUser.Camera = this;
            }
        }

        /// <inheritdoc />
        protected override void OnChildRemoved(Instance child)
        {
            base.OnChildRemoved(child);
            PostEffect effect;
            ICameraUser cameraUser;

            if ((effect = child as PostEffect) != null)
            {
                lock (PostEffectsLocker)
                {
                    PostEffects.Remove(effect);
                    effect.SetCamera(null);

                    // replace user effect with default effect
                    if (effect is BloomEffect)
                        PostEffects.Add(_bloomEffect);
                    else if (effect is ColourCorrectionEffect)
                        PostEffects.Add(_colourCorrection);
                }
            }
            else if ((cameraUser = child as ICameraUser) != null)
            {
                cameraUser.Camera = null;
            }
        }

        internal void UpdateCamera(double step)
        {
            _currentBehaviour.Update(step);
            Velocity = _cameraSubject?.GetVelocity() ?? Vector3.Zero;
        }

        private void CreateSwapChain(IntPtr handle, Device device)
        {
            var desc = new SwapChainDescription1
            {
                Format = RenderConstants.BackBufferFormat,
                BufferCount = RenderConstants.FrameCount,
                Flags = SwapChainFlags.None,
                AlphaMode = SharpDX.DXGI.AlphaMode.Ignore,
                Width = 1,
                Height = 1,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Scaling = Scaling.Stretch,
                Usage = Usage.RenderTargetOutput | Usage.ShaderInput,
                Stereo = false
            };
            SwapChain = new SwapChain1(Renderer.Factory, Renderer.Device, handle, ref desc);
        }

        /// <summary>
        /// Resizes the GBuffer.
        /// </summary>
        internal void Resize(Device device)
        {
            UpdateProjectionMatrix();
            RebuildFrustum(_frustumCulling);

            CanRender = false;

            var newWidth = Math.Max(1, (int)_viewportSize.x);
            var newHeight = Math.Max(1, (int)_viewportSize.y);

            RenderTarget2D?.Dispose();
            RenderTarget2D = null;

            Renderer.Context2D.Target = null;

            _surface?.Dispose();
            BackBuffer?.Dispose();
            Buffer0?.Dispose();
            Buffer1?.Dispose();
            Buffer2?.Dispose();
            Buffer3?.Dispose();
            DepthStencilBuffer?.Dispose();

            Viewport = new ViewportF(0, 0, ViewportSize.x, ViewportSize.y);

            try
            {
                SwapChain.ResizeBuffers(RenderConstants.FrameCount, newWidth, newHeight,
                    RenderConstants.BackBufferFormat, SwapChainFlags.None);
            }
            catch (SharpDXException e)
            {
                Logger.Error(e);
                throw;
            }

            var backBufferTexture = Resource.FromSwapChain<Texture2D>(SwapChain, 0);
            BackBuffer = new Texture(backBufferTexture);

            Buffer0 = CreateCameraBuffer("Buffer0", 1f, RenderConstants.BackBufferFormat);
            Buffer1 = CreateCameraBuffer("Buffer1", 1f, Format.R8G8B8A8_UNorm);
            Buffer2 = CreateCameraBuffer("Buffer2", 1f, Format.R16G16B16A16_Float);
            Buffer3 = CreateCameraBuffer("Buffer3", 1f, Format.R8G8B8A8_UNorm);
            BufferDownscaleHalf0 = CreateCameraBuffer("BufferDownsampleHalf0", 1 / 2f, Format.R8G8B8A8_UNorm);
            BufferDownscaleHalf1 = CreateCameraBuffer("BufferDownsampleHalf1", 1 / 2f, Format.R8G8B8A8_UNorm);
            BufferDownscaleQuarter0 = CreateCameraBuffer("BufferDownsampleQuarter0", 1 / 4f, Format.R8G8B8A8_UNorm);
            BufferDownscaleQuarter1 = CreateCameraBuffer("BufferDownsampleQuarter1", 1 / 4f, Format.R8G8B8A8_UNorm);

            var depthStencilBuffer = new Texture2D(Renderer.Device, new Texture2DDescription
            {
                Format = Format.R24G8_Typeless,
                ArraySize = 1,
                MipLevels = 1,
                Width = newWidth,
                Height = newHeight,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil | BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            });

            DepthStencilBuffer = new Texture(depthStencilBuffer, new DepthStencilViewDescription
            {
                Format = Format.D24_UNorm_S8_UInt,
                Dimension = DepthStencilViewDimension.Texture2D
            }, new ShaderResourceViewDescription
            {
                Format = Format.R24_UNorm_X8_Typeless,
                Dimension = ShaderResourceViewDimension.Texture2D,
                Texture2D = new ShaderResourceViewDescription.Texture2DResource
                {
                    MostDetailedMip = 0,
                    MipLevels = -1
                }
            });

            _surface = BackBuffer.QueryInterface<Surface>();
            _surface.DebugName = "BackBufferSurface";
            
            try
            {
                RenderTarget2D = new Bitmap1(Renderer.Context2D, _surface,
                    new BitmapProperties1(new PixelFormat(_surface.Description.Format, AlphaMode.Premultiplied), 96,
                        96, BitmapOptions.Target | BitmapOptions.CannotDraw));
            }
            catch (Exception)
            {
                _surface.Dispose();
                RenderTarget2D?.Dispose();
                Logger.Info("An exception occured while creating the GUI surface render target.");
                throw;
            }

            lock (Locker)
            {
                foreach (var collector in LayerCollectors)
                {
                    collector.Target = RenderTarget2D;
                }
            }

            lock (PostEffectsLocker)
            {
                foreach (var effect in PostEffects)
                {
                    effect.UpdateSize(this);
                }
            }

            var cb = Constants;
            cb.Data.ScreenParams.X = newWidth;
            cb.Data.ScreenParams.Y = newHeight;
            cb.Data.ScreenParams.Z = AspectRatio;
            cb.Data.ScreenParams.W = FieldOfView * 0.5f / newHeight;

            NeedsResize = false;
            CanRender = true;
            //Logger.Trace("Camera resized.");
        }

        internal Texture CreateCameraBuffer(string bufferName, float scale = 1,
            Format format = RenderConstants.BackBufferFormat)
        {
            var desc = new Texture2DDescription
            {
                ArraySize = 1,
                SampleDescription = new SampleDescription(1, 0),
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                Format = format,
                Width = Math.Max(1, (int)(_viewportSize.x * scale)),
                Height = Math.Max(1, (int)(_viewportSize.y * scale)),
                MipLevels = 1,
                Usage = ResourceUsage.Default,
                OptionFlags = ResourceOptionFlags.None,
                CpuAccessFlags = CpuAccessFlags.None
            };

            return new Texture(new Texture2D(Renderer.Device, desc) {DebugName = bufferName});
        }

        internal void UpdateProjectionMatrix()
        {
            _projectionMatrix = Matrix.PerspectiveFovRH(FieldOfView * Mathf.Deg2Rad, AspectRatio,
                _clipNear, _clipFar);
        }

        internal Ray ScreenPointToRay(float x, float y)
        {
            Vector3 nearPoint;
            Vector3 farPoint;

            var cb = Constants;

            var vpx = _viewportSize.X;
            var vpy = _viewportSize.Y;

            Vector3.Unproject(x, y, 0, 0, 0, vpx, vpy, 0, 1,
                ref _viewProjectionMatrix, out nearPoint);
            Vector3.Unproject(x, y, 1, 0, 0, vpx, vpy, 0, 1,
                ref _viewProjectionMatrix, out farPoint);

            Vector3 direction;
            Vector3.Subtract(ref farPoint, ref nearPoint, out direction);
            direction.Normalize();

            return new Ray(ref nearPoint, ref direction);
        }

        /// <summary>
        /// Gets a 3D ray from a 2D point on the screen.
        /// </summary>
        public Ray ScreenPointToRay(ref Vector2 point)
        {
            return ScreenPointToRay(point.x, point.y);
        }

        /// <summary>
        /// Gets a 3D ray from a 2D point on the screen.
        /// </summary>
        public Ray ScreenPointToRay(Vector2 point)
        {
            return ScreenPointToRay(ref point);
        }

        /*
        internal Ray GetMouseRay(bool useCache)
        {
            return useCache && MouseRay != null ? MouseRay : ScreenPointToRay(InputService.CursorPosition);
        }
        */

        /// <summary>
        /// Gets a picking ray for the cursor.
        /// </summary>
        public Ray GetMouseRay()
        {
            return MouseRay;
        }

        /// <inheritdoc />
        public override void Destroy()
        {
            base.Destroy();

            lock (Renderer.Locker)
            {
                CanRender = false;

                _bloomEffect.Destroy();
                _ambientOcclusion.Destroy();

                RenderHandle = IntPtr.Zero;

                lock (Locker)
                {
                    LayerCollectors.Clear();
                }

                Constants.Dispose();

                RenderTarget2D?.Dispose();

                BackBuffer?.Dispose();
                Buffer0?.Dispose();
                Buffer1?.Dispose();
                Buffer2?.Dispose();
                Buffer3?.Dispose();

                BufferDownscaleHalf0?.Dispose();
                BufferDownscaleHalf1?.Dispose();
                BufferDownscaleQuarter0?.Dispose();
                BufferDownscaleQuarter1?.Dispose();

                DepthStencilBuffer?.Dispose();

                SwapChain?.Dispose();

                BackBuffer = null;
                Buffer0 = null;
                Buffer1 = null;
                Buffer2 = null;
                Buffer3 = null;
            }
        }

        internal void UpdateConstants(ref DeviceContext context)
        {
            var cb = Constants;
            CFrame.GetViewMatrix(out cb.Data.ViewMatrix);
            Matrix.Multiply(ref cb.Data.ViewMatrix, ref _projectionMatrix, out _viewProjectionMatrix);
            cb.Data.ViewProjectionMatrix = _viewProjectionMatrix;
            Matrix.Invert(ref cb.Data.ViewProjectionMatrix, out cb.Data.InverseViewProjection);
            cb.Data.Position = CFrame.p;

            Constants.Update(ref context);
        }

        /// <summary>
        /// Fired when the camera's CFrame is changed.
        /// </summary>
        public readonly Signal Moved;

        /// <summary>
        /// Fires when <see cref="ViewportSize" /> changes.
        /// </summary>
        public readonly Signal<Vector2> ViewportSizeChanged;
    }
}