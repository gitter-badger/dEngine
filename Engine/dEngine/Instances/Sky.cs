// Sky.cs - dEngine
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
using dEngine.Graphics.Structs;
using dEngine.Instances.Attributes;
using dEngine.Services;
using dEngine.Utility;

using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace dEngine.Instances
{
	/// <summary>
	/// An object which draws the sky.
	/// </summary>
	[TypeId(81), ToolboxGroup("Gameplay"), ExplorerOrder(0)]
	public class Sky : Instance, IRenderable
	{
		private Reference<Cubemap> _irradRef;
		private Reference<Cubemap> _radRef;
		private Content<Cubemap> _skyboxContent;

		/// <inheritdoc />
		public Sky()
		{
			_skyboxContent = new Content<Cubemap>();

			CubemapId = "internal://textures/sky512.png";
            
            // this could be pre-computed
			Renderer.InvokeResourceDependent(GenerateStarfield);

			Changed.Event += propertyName =>
			{
				if (propertyName == nameof(Parent))
				{
					var lighting = Parent as Lighting;
					if (lighting != null)
					{
						lighting.Skybox = this;
					}
					else if (Game.Lighting.Skybox == this)
					{
						Game.Lighting.Skybox = null;
					}
				}
			};
		}

		internal Cubemap IrradianceMap => _irradRef;
		internal Cubemap RadianceMap => _radRef;

		/// <summary>
		/// The cubemap texture.
		/// </summary>
		[InstMember(1), EditorVisible("Data")]
		[ContentId(ContentType.Texture)]
		public Content<Cubemap> CubemapId
		{
			get { return _skyboxContent; }
			set
			{
				_skyboxContent = value;
				value.Subscribe(this);
				NotifyChanged();
			}
		}

		/// <summary>
		/// The texture for the stars at night.
		/// </summary>
		internal Texture Starfield { get; private set; }

		internal Cubemap Cubemap => _skyboxContent.Asset;

		private void SetTestMaps()
		{
			_irradRef = CacheableContentProvider<Cubemap>.Get(@"C:\Users\Dan\cmft_win64\irradiance.dds");
			_radRef = CacheableContentProvider<Cubemap>.Get(@"C:\Users\Dan\cmft_win64\radiance.dds");
		}

		private void GenerateStarfield()
		{
			SetTestMaps();
			Starfield?.Dispose();

			var data = new DataRectangle[6];

			for (int i = 0; i < 6; i++)
				data[i] = Noise.Render(1024, 1024);

			Starfield = new Texture(new Texture2D(Renderer.Device, new Texture2DDescription
			{
				Width = 1024,
				Height = 1024,
				ArraySize = 6,
				BindFlags = BindFlags.ShaderResource,
				Usage = ResourceUsage.Immutable,
				CpuAccessFlags = CpuAccessFlags.None,
				Format = Format.B8G8R8A8_UNorm,
				MipLevels = 1,
				OptionFlags = ResourceOptionFlags.TextureCube,
				SampleDescription = new SampleDescription(1, 0)
			}, data));
		}

		/// <inheritdoc />
		public override void Destroy()
		{
			base.Destroy();

			Cubemap?.Dispose();
			Starfield?.Dispose();
		}

		#region IRenderable

		/// <inheritdoc />
		public RenderObject RenderObject { get; set; }

		/// <inheritdoc />
		public int RenderIndex { get; set; }

		/// <inheritdoc />
		public InstanceRenderData RenderData { get; set; }

		/// <inheritdoc />
		public void UpdateRenderData()
		{
			var camera = Game.Workspace?.CurrentCamera;

			var cf = camera == null
				? CFrame.Identity
				: new CFrame(camera.CFrame.p);

			RenderData = new InstanceRenderData
			{
				Size = new Vector3(-100),
				Colour = Colour.Black,
				ModelMatrix = cf.GetModelMatrix(),
				ShadingModel = ShadingModel.Unlit,
				Emissive = 0,
			};

			RenderObject?.UpdateInstance(this);
		}

		#endregion
	}
}