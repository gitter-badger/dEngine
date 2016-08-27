// WEBPImage.cs - dEngine
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
using System.Runtime.InteropServices;
using dEngine.Graphics;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

// ReSharper disable UnusedMember.Local

#pragma warning disable 169

#pragma warning disable 1591

namespace dEngine.Utility.Texture
{
	internal class WEBPImage : ITextureFormat
	{
		internal const int Magic = ('W' | ('E' << 8) | ('B' << 16) | ('P' << 24));

		public WEBPImage(Stream stream, bool staging = false)
		{
			int width;
			int height;

			var dataSize = (UIntPtr)(ulong)stream.Length;
			var decodeStream = new DataStream((int)stream.Length, false, true);
			stream.CopyTo(decodeStream);
			decodeStream.Position = 0;

			if (GetInfo(decodeStream.DataPointer, dataSize, out width, out height) == 0)
				throw new InvalidDataException("Invalid WEBP header.");

			var stride = width * 4;
			var uploadStream = new DataStream(stride * height, false, true);

			var ptr = DecodeRGBAInto(decodeStream.DataPointer, dataSize, uploadStream.DataPointer,
				(UIntPtr)(ulong)uploadStream.Length, stride);

			if (ptr != IntPtr.Zero)
			{
				//Marshal.free(ptr);
			}

			Texture2D = new Texture2D(Renderer.Device, new Texture2DDescription
			{
				Width = width,
				Height = height,
				ArraySize = 1,
				BindFlags = staging ? BindFlags.None : BindFlags.ShaderResource,
				Usage = staging ? ResourceUsage.Staging : ResourceUsage.Default,
				Format = Format.R8G8B8A8_UNorm,
				MipLevels = 1,
				OptionFlags = 0,
				CpuAccessFlags = staging ? CpuAccessFlags.Read : CpuAccessFlags.None,
				SampleDescription = new SampleDescription(1, 0)
			}, new DataRectangle(uploadStream.DataPointer, stride));
		}

		public Texture2D Texture2D { get; }

		private static void CheckError(VP8StatusCode code)
		{
			if (code != VP8StatusCode.OK)
				throw new InvalidOperationException($"VP8StatusCode not OK: {code}");
		}

		[DllImport("WebpWrapper", CallingConvention = CallingConvention.Cdecl)]
		private static extern int GetInfo(IntPtr data, UIntPtr dataSize, out int width, out int height);

		[DllImport("WebpWrapper", CallingConvention = CallingConvention.Cdecl)]
		private static extern VP8StatusCode GetFeatures([In] IntPtr data, UIntPtr dataSize,
			out BitstreamFeatures features);

		[DllImport("WebpWrapper", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr DecodeRGBA([In] IntPtr data, UIntPtr dataSize, ref int width, ref int height);

		[DllImport("WebpWrapper", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr DecodeARGB([In] IntPtr data, UIntPtr dataSize, ref int width, ref int height);

		[DllImport("WebpWrapper", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr DecodeBGRA([In] IntPtr data, UIntPtr dataSize, ref int width, ref int height);

		[DllImport("WebpWrapper", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr DecodeRGB([In] IntPtr data, UIntPtr dataSize, ref int width, ref int height);

		[DllImport("WebpWrapper", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr DecodeBGR([In] IntPtr data, UIntPtr dataSize, ref int width, ref int height);

		[DllImport("WebpWrapper", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr DecodeRGBAInto([In] IntPtr data, UIntPtr dataSize, IntPtr outputBuffer,
			UIntPtr outputSize, int outputStride);

		[DllImport("WebpWrapper", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr DecodeBGRAInto([In] IntPtr data, UIntPtr dataSize, IntPtr outputBuffer,
			UIntPtr outputSize, int outputStride);

		private struct BitstreamFeatures
		{
			public int Width;
			public int Height;
			public int HasAlpha;
			public int BitstreamVersion;
			public int NoIncrementalDecoding;
			public int Rotate;
			public int UVSampling;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 3, ArraySubType = UnmanagedType.U4)] public uint[] Pad;
		}

		private enum VP8StatusCode
		{
			OK,
			OutOfMemory,
			InvalidParameters,
			BitstreamError,
			UnsupportedFeature,
			Suspended,
			UserAbort,
			NotEnoughData
		}
	}
}