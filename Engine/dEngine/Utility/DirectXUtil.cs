// DirectXUtil.cs - dEngine
// Copyright (C) 2015-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SharpDX;

namespace dEngine.Utility
{
	internal static class DirectXUtil
	{
		public static SharpDX.Vector4 Round(this SharpDX.Vector4 vec)
		{
			return new SharpDX.Vector4((float)Math.Round(vec.X), (float)Math.Round(vec.Y), (float)Math.Round(vec.Z),
				(float)Math.Round(vec.W));
		}

		/// <summary>
		/// Converts BulletSharp Matrix4x4 to a CFrame.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Matrix BulletMatrixToSharpDX(BulletSharp.Math.Matrix m)
		{
			// TODO: check if correct order
			return new Matrix(m.M11, m.M12, m.M13, m.M14,
				m.M21, m.M22, m.M23, m.M24,
				m.M31, m.M32, m.M33, m.M34,
				m.M41, m.M42, m.M43, m.M44);
		}

		/*
		public static Texture2D Texture2DFromStream(Stream stream)
		{
			var decoder = new BitmapDecoder(Renderer.ImagingFactory, stream, DecodeOptions.CacheOnDemand);
			var frame = decoder.GetFrame(0);
			var converter = new FormatConverter(Renderer.ImagingFactory);

			converter.Initialize(
				frame,
				PixelFormat.Format32bppPRGBA,
				BitmapDitherType.None, null,
				0.0, BitmapPaletteType.Custom);


			Texture2DDescription desc;
			desc.Width = converter.Size.Width;
			desc.Height = converter.Size.Height;
			desc.ArraySize = 1;
			desc.BindFlags = BindFlags.ShaderResource;
			desc.Usage = ResourceUsage.Default;
			desc.CpuAccessFlags = CpuAccessFlags.None;
			desc.Format = Format.R8G8B8A8_UNorm;
			desc.MipLevels = 1;
			desc.OptionFlags = ResourceOptionFlags.None;
			desc.SampleDescription.Count = 1;
			desc.SampleDescription.Quality = 0;

			var s = new DataStream(converter.Size.Height * converter.Size.Width * 4, true, true);
			converter.CopyPixels(converter.Size.Width * 4, s);

			var rect = new DataRectangle(s.DataPointer, converter.Size.Width * 4);

			var t2D = new Texture2D(GraphicsManager.Device, desc, rect);

			return t2D;
		}
		*/

		public static DataRectangle LoadBitmapData(Bitmap bitmap)
		{
			var data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
				ImageLockMode.ReadOnly,
				PixelFormat.Format32bppPArgb);

			var stride = data.Stride;
			var buffer = new DataStream(data.Height * stride, true, true);

			for (var y = 0; y < data.Height; ++y)
			{
				var offset = stride * y;

				var bytes = new byte[stride];

				Marshal.Copy(data.Scan0 + offset, bytes, 0, stride);

				var subOffset = 0;
				for (var x = 0; x < data.Width; x++)
				{
					var b = bytes[subOffset++];
					var g = bytes[subOffset++];
					var r = bytes[subOffset++];
					var a = bytes[subOffset++];
					var rgba = r | (g << 8) | (b << 16) | (a << 24);
					buffer.Write(rgba);
				}
			}

			var rect = new DataRectangle(buffer.DataPointer, stride);

			bitmap.UnlockBits(data);

			return rect;
		}
	}
}