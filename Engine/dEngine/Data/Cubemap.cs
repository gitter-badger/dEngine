// Cubemap.cs - dEngine
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
using dEngine.Graphics;
using dEngine.Instances.Attributes;
using dEngine.Utility.Native;
using dEngine.Utility.Texture;

using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using SharpDX.WIC;

namespace dEngine.Data
{
	[TypeId(27)]
	public class Cubemap : AssetBase
	{
		internal Texture Texture;

		public override ContentType ContentType => ContentType.Cubemap;
        
	    protected override void OnLoad(BinaryReader reader)
        {
            LoadTexture(reader.BaseStream, out Texture);
            IsLoaded = true;
        }

	    protected override void OnSave(BinaryWriter writer)
	    {
	        base.OnSave(writer);
            throw new NotImplementedException();
	    }

	    protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			Texture?.Dispose();

			_disposed = true;
		}

		private static void LoadTexture(Stream input, out Texture output)
		{
			if (input == null)
			{
				output = null;
				return;
			}

			var magic = new byte[4];
			input.Read(magic, 0, 4);
			input.Position = 0;

			var a = (char)magic[0];
			var b = (char)magic[1];
			var c = (char)magic[2];
			var d = (char)magic[3];

			if (VisualC.CompareMemory(magic, DDSImage.MagicBytes, 4) == 0)
			{
				var test = new DDSImage(input);

				output = new Texture(test.Texture2D) {Name = "Cubemap (DDS)"};
				return;
			}

			var faces = new DataRectangle[6];

			using (var decoder = new BitmapDecoder(Renderer.ImagingFactory, input, DecodeOptions.CacheOnDemand))
			{
				var frame = decoder.GetFrame(0);
				var converter = new FormatConverter(Renderer.ImagingFactory);

				converter.Initialize(
					frame,
					PixelFormat.Format32bppPRGBA,
					BitmapDitherType.None, null,
					0.0, BitmapPaletteType.Custom);

				var width = frame.Size.Width;
				var height = frame.Size.Height;
				var ratio = width / (float)height;

				if (Math.Abs(ratio - 2) < 0.001) // panorama
				{
					throw new NotImplementedException();
				}
				if (Math.Abs(ratio - 1.333f) < 0.001) // cubic
				{
					var tileDim = (int)(width / 4f);
					var stride = tileDim * 4;

					Action<int, int, int> loadFace = delegate(int index, int coordX, int coordY)
					{
						var faceStream = new DataStream(tileDim * stride, false, true);

						using (var crop = new BitmapClipper(Renderer.ImagingFactory))
						{
							crop.Initialize(converter, new RawBox(coordX, coordY, tileDim, tileDim));
							crop.CopyPixels(stride, faceStream);
						}

						faces[index] = new DataRectangle(faceStream.DataPointer, stride);
					};

					loadFace(0, tileDim * 2, tileDim);
					loadFace(1, 0, tileDim);
					loadFace(2, tileDim, 0);
					loadFace(3, tileDim, tileDim * 2);
					loadFace(4, tileDim, tileDim);
					loadFace(5, tileDim * 3, tileDim);

					var texture2D = new Texture2D(Renderer.Device, new Texture2DDescription
					{
						Width = tileDim,
						Height = tileDim,
						ArraySize = 6,
						BindFlags = BindFlags.ShaderResource,
						Usage = ResourceUsage.Immutable,
						CpuAccessFlags = CpuAccessFlags.None,
						Format = Format.R8G8B8A8_UNorm,
						MipLevels = 1,
						OptionFlags = ResourceOptionFlags.TextureCube,
						SampleDescription = new SampleDescription(1, 0)
					}, faces);

					converter.Dispose();

					output = new Texture(texture2D);
				    output.Name = output.NativeTexture.Tag.ToString();
				}
				else
				{
					throw new NotImplementedException();
				}
			}
		}

		private static void LoadPanorama(ref Texture2D input, out Texture output)
		{
			throw new NotImplementedException();
		}

		private static Cubemap PanoramaToCubemap(Texture2D panorama)
		{
			throw new NotSupportedException("Panoramic cubemaps not currently supported.");
		}
	}
}