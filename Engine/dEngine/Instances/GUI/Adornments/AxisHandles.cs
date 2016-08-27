// AxisHandles.cs - dEngine
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
using dEngine.Graphics;
using dEngine.Graphics.Structs;
using dEngine.Instances.Attributes;
using dEngine.Services;
using dEngine.Utility;
using SharpDX;
using SharpDX.Direct3D11;

// ReSharper disable UnusedMember.Local
// ReSharper disable MemberCanBePrivate.Local

namespace dEngine.Instances
{
	/// <summary>
	/// An object which draws 3D handles around the axes of <see cref="PVAdornment.Adornee" />.
	/// </summary>
	[TypeId(168), ToolboxGroup("3D GUI"), ExplorerOrder(19)]
	public class AxisHandles : BaseHandles
	{
		private const float _transparency = 0.8f;
		private readonly ShapeAdornment[] _cones;
		private readonly ShapeAdornment[] _cylinders;
		private readonly OrientedBoundingBox[] _hitTesters;
		private readonly object _locker = new object();
		private readonly ShapeAdornment[] _spheres;

		/// <summary>
		/// Fired when the left mouse button is pressed over a handle.
		/// </summary>
		public readonly Signal<NormalId> MouseButton1Down;

		/// <summary>
		/// Fired when the left mouse button is released over a handle.
		/// </summary>
		public readonly Signal<NormalId> MouseButton1Up;

		/// <summary>
		/// Fired when a handle is dragged.
		/// </summary>
		public readonly Signal<NormalId, float> MouseDrag;

		/// <summary>
		/// Fired when the mouse button enters a handle.
		/// </summary>
		public readonly Signal<NormalId> MouseEnter;

		/// <summary>
		/// Fired when the mouse leaves a handle.
		/// </summary>
		public readonly Signal<NormalId> MouseLeave;

		private CFrame _cframe;
		private SharpDX.Vector3 _intersectPoint;
		private bool _mouseDown;
		private SharpDX.Plane _plane;
		private SharpDX.Ray _ray;
		private float[] _scales = new float[6];

		private NormalId? _selectedNormalId;
		private SharpDX.Vector3 _startIntersectPoint;
		private HandlesStyle _style;

		/// <inheritdoc />
		public AxisHandles()
		{
			_cframe = CFrame.Identity;

			_cones = new ShapeAdornment[6];
			_spheres = new ShapeAdornment[6];
			_cylinders = new ShapeAdornment[6];
			_hitTesters = new OrientedBoundingBox[6];

			var colours = new[]
			{
				Colour.Red,
				Colour.Green,
				Colour.Blue,
				Colour.Red,
				Colour.Green,
				Colour.Blue
			};

			for (int i = 0; i < 6; i++)
			{
				var nid = (NormalId)i;
				_cones[i] = new ShapeAdornment(this, (Shape)5, nid) {Colour = colours[i]};
				_spheres[i] = new ShapeAdornment(this, Shape.Sphere, nid) {Colour = colours[i]};
				_cylinders[i] = new ShapeAdornment(this, Shape.Cylinder, nid) {Colour = colours[i]};
				_hitTesters[i] = new OrientedBoundingBox(new SharpDX.Vector3(-0.5f, -0.5f, -0.5f),
					new SharpDX.Vector3(.5f, .5f, .5f));
			}

			MouseButton1Down = new Signal<NormalId>(this);
			MouseButton1Up = new Signal<NormalId>(this);
			MouseDrag = new Signal<NormalId, float>(this);
			MouseEnter = new Signal<NormalId>(this);
			MouseLeave = new Signal<NormalId>(this);

			InputService.Service.InputBegan.Event += OnInputBegan;
			InputService.Service.InputChanged.Event += OnInputChanged;
			InputService.Service.InputEnded.Event += OnInputEnded;
		}

		internal NormalId? SelectedNormalId
		{
			get { return _selectedNormalId; }
			private set
			{
				if (value == _selectedNormalId) return;
				_selectedNormalId = value;
				if (value.HasValue)
				{
					SharpDX.Vector3 norm;

					switch (value)
					{
						case NormalId.Top:
							norm = new SharpDX.Vector3(1, 0, 1);
							break;
						case NormalId.Bottom:
							norm = new SharpDX.Vector3(-1, 0, -1);
							break;
						case NormalId.Left:
							norm = new SharpDX.Vector3(0, 0, -1);
							break;
						case NormalId.Right:
							norm = new SharpDX.Vector3(0, 0, 1);
							break;
						case NormalId.Front:
							norm = new SharpDX.Vector3(-1, 0, 0);
							break;
						case NormalId.Back:
							norm = new SharpDX.Vector3(1, 0, 0);
							break;
						default:
							throw new ArgumentOutOfRangeException(nameof(value), value, null);
					}

					_plane = new SharpDX.Plane((SharpDX.Vector3)_cframe.p, norm);
				}
			}
		}

		internal Axis? SelectedAxis { get; private set; }

		/// <summary>
		/// The style for the handles.
		/// </summary>
		public HandlesStyle Style
		{
			get { return _style; }
			set
			{
				if (value == _style) return;
				_style = value;
				ChangeRenderObject();
				NotifyChanged(nameof(Style));
			}
		}

		private void OnInputBegan(InputObject input)
		{
		    if (input.Handled)
		        return;

			lock (_locker)
			{
				if (input.InputType == InputType.MouseButton1 && SelectedNormalId.HasValue)
				{
					MouseButton1Down.Fire(SelectedNormalId.Value);
					_startIntersectPoint = _intersectPoint;
					_mouseDown = true;
				}
			}
		}

		private void OnInputChanged(InputObject input)
        {
            if (input.Handled)
                return;

            lock (_locker)
			{
				if (input.InputType == InputType.MouseMovement && _mouseDown)
				{
					SharpDX.Vector3 v;
					_plane.Intersects(ref _ray, out v);

					var delta = GetDelta(v, _startIntersectPoint);
					Debug.Assert(SelectedNormalId != null, "SelectedNormalId != null");
					MouseDrag.Fire(SelectedNormalId.Value, delta);
				}
			}
		}

		private void OnInputEnded(InputObject input)
        {
            if (input.Handled)
                return;

            lock (_locker)
			{
				if (input.InputType == InputType.MouseButton1)
				{
					if (SelectedNormalId.HasValue)
					{
						MouseButton1Up.Fire(SelectedNormalId.Value);
					}
					_mouseDown = false;
				}
			}
		}

		private float GetDelta(SharpDX.Vector3 lhs, SharpDX.Vector3 rhs)
		{
			switch (SelectedNormalId)
			{
				case NormalId.Right:
					return lhs.X - rhs.X;
				case NormalId.Top:
					return lhs.Y - rhs.Y;
				case NormalId.Back:
					return lhs.Z - rhs.Z;
				case NormalId.Left:
					return rhs.X - lhs.X;
				case NormalId.Bottom:
					return rhs.Y - lhs.Y;
				case NormalId.Front:
					return rhs.Z - lhs.Z;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		/// <inheritdoc />
		protected override void ChangeRenderObject()
		{
			if (Adornee == null)
			{
				for (int i = 0; i < 6; i++)
				{
					_spheres[i].IsEnabled = false;
					_cones[i].IsEnabled = false;
					_cylinders[i].IsEnabled = false;
				}
				return;
			}

			if (_style == HandlesStyle.Movement)
			{
				for (int i = 0; i < 6; i++)
				{
					_cones[i].IsEnabled = true; //
					_cylinders[i].IsEnabled = true; //
					_spheres[i].IsEnabled = false;
				}
			}
			else if (_style == HandlesStyle.Resize)
			{
				for (int i = 0; i < 6; i++)
				{
					_cones[i].IsEnabled = false;
					_cylinders[i].IsEnabled = false;
					_spheres[i].IsEnabled = true; //
				}
			}
		}

		/// <inheritdoc />
		protected override void OnAdorneeSet(PVInstance newAdornee, PVInstance oldAdornee)
		{
			base.OnAdorneeSet(newAdornee, oldAdornee);
			ChangeRenderObject();
		}

		/// <inheritdoc />
		public override void Destroy()
		{
			base.Destroy();
			for (int i = 0; i < 6; i++)
			{
				_cones[i].Dispose();
				_spheres[i].Dispose();
				_cylinders[i].Dispose();
			}
		}

		internal override void Draw(ref DeviceContext context, ref Camera camera)
		{
			lock (_locker)
			{
				var adornee = Adornee;
				if (adornee == null || Visible == false)
					return;
				var adorneeCF = adornee.CFrame;
				var adorneeSize = adornee.Size;

				_cframe = adorneeCF;

				GetDistanceToNormals(10, 0.6f, ref adorneeCF, ref adorneeSize, ref _scales);

				bool? anySelected = false;

				var ray = camera.GetMouseRay();
				_ray.Position.X = ray.Origin.X;
				_ray.Position.Y = ray.Origin.Y;
				_ray.Position.Z = ray.Origin.Z;
				_ray.Direction.X = ray.Direction.X;
				_ray.Direction.Y = ray.Direction.Y;
				_ray.Direction.Z = ray.Direction.Z;
				_plane.Intersects(ref _ray, out _intersectPoint);

				Renderer.AdornSelfLitPass.Use(ref context);
				for (int i = 0; i < 6; i++)
				{
					var normalId = (NormalId)i;

					var cylinder = _cylinders[i];
					var cone = _cones[i];
					var sphere = _spheres[i];
					var tester = _hitTesters[i];

					var sizeY = cylinder.Size.Y / 2 + cone.Size.Y / 2;
					tester.Transformation = (cylinder.CFrame + cylinder.CFrame.up * (sizeY / 4)).GetMatrix();
					tester.Extents.X = cone.Size.X / 2;
					tester.Extents.Y = sizeY;
					tester.Extents.Z = cone.Size.Z / 2;

					if (!_mouseDown)
					{
						if (tester.Intersects(ref _ray))
						{
							cylinder.Transparency = 0;
							cone.Transparency = 0;
							sphere.Transparency = 0;

							if (SelectedNormalId != normalId)
								MouseEnter.Fire(normalId);
							HeldHandles.TryAdd(this);

							SelectedNormalId = normalId;
							SelectedAxis = normalId.ToAxis();
							anySelected = true;
						}
						else
						{
							if (SelectedNormalId == normalId)
								MouseLeave.Fire(normalId);

							cylinder.Transparency = 0.5f;
							cone.Transparency = 0.5f;
							sphere.Transparency = 0.5f;
						}
					}

					cylinder.Update(ref adorneeCF, ref adorneeSize, ref _scales);
					cone.Update(ref adorneeCF, ref adorneeSize, ref _scales);
					sphere.Update(ref adorneeCF, ref adorneeSize, ref _scales);

					_cones[i].Draw(ref context, ref camera);
					_spheres[i].Draw(ref context, ref camera);
					_cylinders[i].Draw(ref context, ref camera);
				}

				if (anySelected == false && !_mouseDown)
				{
					SelectedNormalId = null;
					SelectedAxis = null;
					HeldHandles.TryRemove(this);
				}
			}
		}

		private class ShapeAdornment : IRenderable, IDisposable
		{
			private const float _offset = 0.2f;
			private const float _cylinderThickness = 0.1f;
			private const float _cylinderLength = 2.4f;
			private const float _coneThickness = 0.35f;
			private const float _coneLength = 0.7f;
			private readonly AxisHandles _handle;
			private readonly RenderObject _renderObject;
			private readonly Shape _shape;
			private bool _isEnabled;
			private NormalId _normalId;

			public ShapeAdornment(AxisHandles handle, Shape shape, NormalId normalId)
			{
			    var debugName = $"AxisHandles_{normalId}_{shape}";

                CFrame = CFrame.Identity;
				Size = Vector3.One;
				_shape = shape;
				_handle = handle;
				_normalId = normalId;
				_renderObject = (int)shape == 5 ? new RenderObject(debugName, Primitives.ConeGeometry) : new RenderObject(debugName,
					Primitives.GeometryFromShape(shape));
				Transparency = 0.5f;
				UpdateRenderData();
			}

			public Colour Colour { get; set; }

			public Vector3 Size { get; set; }

			public CFrame CFrame { get; set; }

			public bool IsEnabled
			{
				get { return _isEnabled; }
				set
				{
					if (value == _isEnabled) return;
					_isEnabled = value;

					if (value)
					{
						_renderObject.Add(this);
					}
					else
					{
						_renderObject.Remove(this);
					}
				}
			}

			public void Dispose()
			{
				if (_isEnabled)
					_renderObject.Remove(this);
			}

			public void Draw(ref DeviceContext context, ref Camera camera)
			{
				if (_isEnabled)
					_renderObject.Draw(ref context, ref camera);
			}

			public void Update(ref CFrame adorneeCF, ref Vector3 adorneeSize, ref float[] scales)
			{
				var id = (int)_normalId;
				var scale = scales[id];

				var cylinderThickness = _cylinderThickness * scale;
				var cylinderLength = _cylinderLength * scale;
				var coneThickness = _coneThickness * scale;
				var coneLength = _coneLength * scale;

				CFrame angles;

				switch (_normalId)
				{
					case NormalId.Top:
						angles = CFrame.Angles(0, 0, 0);
						break;
					case NormalId.Bottom:
						angles = CFrame.Angles(Mathf.Pi, 0, 0);
						break;
					case NormalId.Right:
						angles = CFrame.Angles(0, 0, -Mathf.Pi / 2);
						break;
					case NormalId.Left:
						angles = CFrame.Angles(0, 0, Mathf.Pi / 2);
						break;
					case NormalId.Back:
						angles = CFrame.Angles(Mathf.Pi / 2, 0, 0);
						break;
					case NormalId.Front:
						angles = CFrame.Angles(-Mathf.Pi / 2, 0, 0);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				switch (_shape)
				{
					case Shape.Sphere:
						break;
					case (Shape)5:
						var cylinderCF = _handle._cylinders[id].CFrame;
						Size = new Vector3(coneThickness, coneLength, coneThickness);
						CFrame = cylinderCF + cylinderCF.up * (cylinderLength / 2 + coneLength / 2);
						break;
					case Shape.Cylinder:
						Size = new Vector3(cylinderThickness, cylinderLength, cylinderThickness);
						CFrame = (adorneeCF +
								  adorneeCF[_normalId] * (ComponentValueFromNormalId(ref adorneeSize, ref _normalId) / 2 + cylinderLength / 2)) *
								 angles;
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
				UpdateRenderData();
			}

			private static float ComponentValueFromNormalId(ref Vector3 vector, ref NormalId normalId)
			{
				switch (normalId)
				{
					case NormalId.Right:
						return vector.X;
					case NormalId.Top:
						return vector.Y;
					case NormalId.Back:
						return vector.Z;
					case NormalId.Left:
						return vector.X;
					case NormalId.Bottom:
						return vector.Y;
					case NormalId.Front:
						return vector.Z;
					default:
						throw new ArgumentOutOfRangeException(nameof(normalId), normalId, null);
				}
			}

			#region IRenderable

			public RenderObject RenderObject { get; set; }
			public int RenderIndex { get; set; }
			public InstanceRenderData RenderData { get; set; }
			public float Transparency { get; set; }

			public void UpdateRenderData()
			{
				RenderData = new InstanceRenderData
				{
					Colour = Colour,
					Emissive = 0,
					ShadingModel = ShadingModel.Unlit,
					Size = Size,
					ModelMatrix = CFrame.GetModelMatrix(),
					Transparency = Transparency
				};

				if (_isEnabled)
					_renderObject.UpdateInstance(this);
			}

			#endregion
		}
	}
}