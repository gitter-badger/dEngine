// BillboardGui.cs - dEngine
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
using dEngine.Graphics;
using dEngine.Graphics.Structs;
using dEngine.Instances.Attributes;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace dEngine.Instances
{
	/// <summary>
	/// A gui which renders to a billboard - a plane that always faces the camera.
	/// </summary>
	// TODO: reimplement billboard gui
	[TypeId(97), ToolboxGroup("GUI")]
	public class BillboardGui : LayerCollector, IRenderable
	{
		private Part _adornee;
		private bool _alwaysOnTop;
		private Vector3 _offset;
		private UDim2 _size;
		private Texture2D _texture;

		/// <inheritdoc />
		public BillboardGui()
		{
			_size = new UDim2(0, 128, 0, 128);
			_offset = Vector3.Zero;
			_alwaysOnTop = false;

			BuildTexture();
			UpdateRenderData();

			/*
			InstanceBuffer = new Buffer(Renderer.Device, GraphicsManager.RenderDataObjectSize, ResourceUsage.Dynamic,
				BindFlags.VertexBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None,
				GraphicsManager.RenderDataObjectSize);

			GraphicsManager.BillboardVao.Add(this);
			*/
		}

		internal Buffer InstanceBuffer { get; }

		internal Bitmap1 Bitmap1 { get; }

		internal ShaderResourceView ShaderResource { get; }

		/// <summary>
		/// The size of the billboard.
		/// </summary>
		/// <remarks>
		/// <see cref="UDim2.Scale" /> is relative to the size of the adornee.
		/// <see cref="UDim2.Absolute" /> is in screen pixels.
		/// </remarks>
		[InstMember(1), EditorVisible("Data")]
		public UDim2 Size
		{
			get { return _size; }
			set
			{
				_size = value;
				BuildTexture();
				NotifyChanged(nameof(Size));
			}
		}

		/// <summary>
		/// The offset from the adornee Position.
		/// </summary>
		[InstMember(2), EditorVisible("Data")]
		public Vector3 Offset
		{
			get { return _offset; }
			set
			{
				_offset = value;
				NotifyChanged(nameof(Offset));
			}
		}

		/// <summary>
		/// The object this billboard is adorned to.
		/// </summary>
		[InstMember(3), EditorVisible("Data")]
		public Part Adornee
		{
			get { return _adornee; }
			set
			{
				_adornee = value;
				NotifyChanged(nameof(Adornee));
			}
		}

		/// <summary>
		/// Determines if the billboard is rendered ontop of other objects.
		/// </summary>
		[InstMember(4), EditorVisible("Behaviour")]
		public bool AlwaysOnTop
		{
			get { return _alwaysOnTop; }
			set
			{
				_alwaysOnTop = value;
				NotifyChanged(nameof(AlwaysOnTop));
			}
		}

		/// <inheritdoc />
		[EditorVisible("Data")]
		public override Vector2 AbsolutePosition
		{
			get { return Vector2.Zero; }
			internal set { throw new NotSupportedException(); }
		}

		/// <inheritdoc />
		[EditorVisible("Data")]
		public override Vector2 AbsoluteSize
		{
			get { return new Vector2(256, 256); }
			internal set { throw new NotImplementedException(); }
		}

		public RenderObject RenderObject { get; set; }
		public int RenderIndex { get; set; }
		public InstanceRenderData RenderData { get; set; }

		public void UpdateRenderData()
		{
			throw new NotImplementedException();
		}

		private InstanceRenderData GetInstanceData()
		{
			var bbPos = _adornee?.CFrame.p ?? Vector3.Zero;
			bbPos += _offset;

			return new InstanceRenderData
			{
				Size = new Vector3(AbsoluteSize, 1) / 2
			};
		}

		internal void UpdateVao()
		{
		}

		private void BuildTexture()
		{
			/*
			Utilities.Dispose(ref _shaderResource);
			Utilities.Dispose(ref _renderTarget);
			Utilities.Dispose(ref _texture);

			var device = Renderer.Device;

			_texture = new Texture2D(device, new Texture2DDescription
			{
				Width = Math.Max(1, (int)AbsoluteSize.X),
				Height = Math.Max(1, (int)AbsoluteSize.Y),
				Format = RenderConstants.BackBufferFormat,
				ArraySize = 1,
				MipLevels = 1,
				SampleDescription = new SampleDescription(1, 0),
				Usage = ResourceUsage.Default,
				BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
				CpuAccessFlags = CpuAccessFlags.None,
				OptionFlags = ResourceOptionFlags.None
			});

			_shaderResource = new ShaderResourceView(Renderer.Device, _texture);

			using (var surface = _texture.QueryInterface<Surface>())
			{
				_renderTarget = new Bitmap1(Renderer.ContextD2D, surface,
					new BitmapProperties1(new PixelFormat(RenderConstants.BackBufferFormat, AlphaMode.Premultiplied), 0,
						0, BitmapOptions.Target));
			}
			*/
		}

		/// <inheritdoc />
		public override void Destroy()
		{
			base.Destroy();

			//Primitives.BillboardRbo.Remove(this);
		}

		internal void UpdateLayout()
		{
			BuildTexture();
		}

		internal static Matrix CreateBillboardMatrix(Camera camera, Vector3 objectPosition)
		{
			var cameraPosition = camera.CFrame.p;
			var cameraUpVector = camera.CFrame.up;
			var cameraForwardVector = Vector3.Forward;

			var result1 = cameraPosition - objectPosition;
			float num = result1.mag2;
			if (num < 9.99999974737875E-05)
				result1 = -cameraForwardVector;
			else
				Vector3.Multiply(ref result1, 1f / (float)Math.Sqrt(num), out result1);
			Vector3 result2;
			Vector3.Cross(ref cameraUpVector, ref result1, out result2);
			result2 = result2.unit;
			Vector3 result3;
			Vector3.Cross(ref result1, ref result2, out result3);

			var matrix = new Matrix(result2.X, result2.Y, result2.Z, 0,
				result3.X, result3.Y, result3.Z, 0,
				result1.X, result1.Y, result1.Z, 0,
				objectPosition.X, objectPosition.Y, objectPosition.Z, 1);
			return matrix;
		}
	}
}