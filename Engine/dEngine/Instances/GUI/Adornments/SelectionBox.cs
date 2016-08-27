// SelectionBox.cs - dEngine
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
using dEngine.Utility.Extensions;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;
using MapFlags = SharpDX.Direct3D11.MapFlags;

namespace dEngine.Instances
{
	/// <summary>
	/// An object which draws a box outline around its adornee.
	/// </summary>
	[TypeId(129), ToolboxGroup("3D GUI"), ExplorerOrder(21)]
	public class SelectionBox : PVAdornment
	{
		internal static Colour DefaultColour = Colour.fromRGB(25, 153, 255);
		private readonly SelectionLine[] _handles;

		private bool _boundingBoxDirty;
		private bool _bufferDirty;
		private Buffer _instanceBuffer;
		private VertexBufferBinding _instanceBufferBinding;
		private Buffer _lineConstantBuffer;
		private float _lineThickness;

		/// <inheritdoc />
		public SelectionBox()
		{
			_handles = new SelectionLine[12];
			_lineThickness = 0.05f;
			Colour = DefaultColour;

			Renderer.InvokeResourceDependent(OnRendererInitialized);

			Changed.Event += s =>
			{
				switch (s)
				{
					case nameof(Transparency):
						_bufferDirty = true;
						break;
					case nameof(Visible):
						_bufferDirty = true;
						break;
					case nameof(Colour):
						_bufferDirty = true;
						break;
				}
			};

			for (var i = 0; i < 12; i++)
			{
				_handles[i] = new SelectionLine();
			}
		}

		/// <summary>
		/// The thickness of the lines.
		/// </summary>
		[InstMember(1), EditorVisible("Appearance")]
		public float LineThickness
		{
			get { return _lineThickness; }
			set
			{
				if (value == _lineThickness)
					return;

				_lineThickness = Math.Max(0, value);
				UpdateConstantBuffer();

				//TransformLines();
				NotifyChanged(nameof(LineThickness));
			}
		}

		private void UpdateConstantBuffer()
		{
			if (!Renderer.IsInitialized)
				return;

			_lineConstantBuffer?.Dispose();
			_lineConstantBuffer = Buffer.Create(Renderer.Device, BindFlags.ConstantBuffer, ref _lineThickness, 16,
				ResourceUsage.Immutable);
		}

		private void OnRendererInitialized()
		{
			_instanceBuffer = new Buffer(Renderer.Device, new BufferDescription
			{
				BindFlags = BindFlags.VertexBuffer,
				Usage = ResourceUsage.Dynamic,
				StructureByteStride = InstanceRenderData.Stride,
				SizeInBytes = InstanceRenderData.Stride * 12,
				CpuAccessFlags = CpuAccessFlags.Write,
				OptionFlags = ResourceOptionFlags.None
			});

			_instanceBufferBinding = new VertexBufferBinding(_instanceBuffer, InstanceRenderData.Stride, 0);

			UpdateConstantBuffer();
		}

		/// <inheritdoc />
		protected override void OnAdorneeSet(PVInstance adornee, PVInstance oldAdornee)
		{
			if (oldAdornee != null)
				oldAdornee.Changed.Event -= OnAdorneeChangedEvent;

			_bufferDirty = true;
			_boundingBoxDirty = true;
			//TransformLines();

			if (adornee != null)
				adornee.Changed.Event += OnAdorneeChangedEvent;
		}

		/// <inheritdoc />
		public override void Destroy()
		{
			Adornee = null;
			base.Destroy();
			_lineConstantBuffer?.Dispose();
			lock (Renderer.Locker)
			{
				_instanceBuffer?.Dispose();
			}
		}

		/// <summary>
		/// Called when the Adornee.Changed event fires.
		/// </summary>
		private void OnAdorneeChangedEvent(string s)
		{
			if (IsDestroyed) return;

			if (s == nameof(Part.CFrame) || s == nameof(Part.Size))
			{
				_boundingBoxDirty = true;
			}
		}

		private void TransformLines()
		{
			var adornee = Adornee;
			Camera camera;

		    if (adornee == null || (camera = Game.FocusedCamera) == null)
		    {
		        _boundingBoxDirty = false;
                return;
		    }

		    if (_boundingBoxDirty)
			{
				_boundingBoxDirty = false;
				_bufferDirty = true;
			}

			var adorneeCFrame = _boundingBox.GetCFrame();
			var adorneeSize = _boundingBox.GetdEngineSize();

			var thickness = _lineThickness;

			const int halfthick = 0;

			_handles[0].Size = new Vector3(thickness, thickness, adorneeSize.z);
			_handles[0].CFrame = adorneeCFrame *
								 new CFrame(adorneeSize.x / 2 + halfthick, adorneeSize.y / 2 + halfthick, 0);

			_handles[1].Size = new Vector3(thickness, thickness, adorneeSize.z);
			_handles[1].CFrame = adorneeCFrame *
								 new CFrame(adorneeSize.x / 2 + halfthick, -(adorneeSize.y / 2) - halfthick, 0);

			_handles[2].Size = new Vector3(thickness, thickness, adorneeSize.z);
			_handles[2].CFrame = adorneeCFrame *
								 new CFrame(-(adorneeSize.x / 2) - halfthick, -(adorneeSize.y / 2) - halfthick, 0);

			_handles[3].Size = new Vector3(thickness, thickness, adorneeSize.z);
			_handles[3].CFrame = adorneeCFrame *
								 new CFrame(-(adorneeSize.x / 2) - halfthick, adorneeSize.y / 2 + halfthick, 0);

			_handles[4].Size = new Vector3(adorneeSize.x, thickness, thickness);
			_handles[4].CFrame = adorneeCFrame *
								 new CFrame(0, adorneeSize.y / 2 + halfthick, adorneeSize.z / 2 + halfthick);

			_handles[5].Size = new Vector3(adorneeSize.x, thickness, thickness);
			_handles[5].CFrame = adorneeCFrame *
								 new CFrame(0, -adorneeSize.y / 2 - halfthick, adorneeSize.z / 2 + halfthick);

			_handles[6].Size = new Vector3(adorneeSize.x, thickness, thickness);
			_handles[6].CFrame = adorneeCFrame *
								 new CFrame(0, -adorneeSize.y / 2 - halfthick, -adorneeSize.z / 2 - halfthick);

			_handles[7].Size = new Vector3(adorneeSize.x, thickness, thickness);
			_handles[7].CFrame = adorneeCFrame *
								 new CFrame(0, adorneeSize.y / 2 + halfthick, -adorneeSize.z / 2 - halfthick);

			_handles[8].Size = new Vector3(thickness, adorneeSize.y + thickness, thickness);
			_handles[8].CFrame = adorneeCFrame *
								 new CFrame(adorneeSize.x / 2 + halfthick, 0, adorneeSize.z / 2 + halfthick);

			_handles[9].Size = new Vector3(thickness, adorneeSize.y + thickness, thickness);
			_handles[9].CFrame = adorneeCFrame *
								 new CFrame(-adorneeSize.x / 2 - halfthick, 0, adorneeSize.z / 2 + halfthick);

			_handles[10].Size = new Vector3(thickness, adorneeSize.y + thickness, thickness);
			_handles[10].CFrame = adorneeCFrame *
								  new CFrame(adorneeSize.x / 2 + halfthick, 0, -adorneeSize.z / 2 - halfthick);

			_handles[11].Size = new Vector3(thickness, adorneeSize.y + thickness, thickness);
			_handles[11].CFrame = adorneeCFrame *
								  new CFrame(-adorneeSize.x / 2 - halfthick, 0, -adorneeSize.z / 2 - halfthick);
		}

	    private RenderObject _rbo;
	    private int _indexCount;
	    private Buffer _indexBuffer;
	    private VertexBufferBinding _bufferBinding;
	    private OrientedBoundingBox _boundingBox;

	    internal override void Draw(ref DeviceContext context, ref Camera camera)
		{
			if (!Visible || Adornee == null) return;

		    if (_rbo == null)
		    {
		        _rbo = Game.Workspace.RenderObjectProvider[Shape.Cube];
		        _indexCount = _rbo.Geometry.IndexCount;
		        _indexBuffer = _rbo.Geometry.IndexBuffer;
                _bufferBinding = _rbo.Bindings[0];
            }

		    TransformLines();

			lock (Renderer.Locker)
			{
				Renderer.AALinePass.Use(ref context);

				context.VertexShader.SetConstantBuffer(2, _lineConstantBuffer);

				if (_bufferDirty)
				{
					DataStream stream;
					context.MapSubresource(_instanceBuffer, MapMode.WriteDiscard, MapFlags.None, out stream);
					for (int i = 0; i < 12; i++)
					{
						var line = _handles[i];
						stream.Write(new InstanceRenderData
						{
							Size = line.Size,
							Colour = Colour,
							ModelMatrix = line.Matrix,
							Transparency = Transparency,
							ShadingModel = ShadingModel.Unlit
						});
					}
					context.UnmapSubresource(_instanceBuffer, 0);
					_bufferDirty = false;
				}

				context.InputAssembler.SetVertexBuffers(0, _bufferBinding, _instanceBufferBinding);
				context.InputAssembler.SetIndexBuffer(_indexBuffer, Format.R32_UInt, 0);
				context.DrawIndexedInstanced(_indexCount, 12, 0, 0, 0);
			}
		}

		/// <summary>
		/// Forces the selection box to update.
		/// </summary>
		internal void Dirty()
		{
			_boundingBoxDirty = true;
		}

		private class SelectionLine
		{
			public Matrix Matrix;
			public Vector3 Size;

			public CFrame CFrame
			{
				set { Matrix = (Matrix)value; }
			}
		}
	}
}