// DDSImage.cs - dEngine
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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using dEngine.Graphics;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using MapFlags = SharpDX.Direct3D11.MapFlags;

// ReSharper disable UnusedMember.Local

namespace dEngine.Utility.Texture
{
	// TODO: decompression
	internal class DDSImage : ITextureFormat, IDisposable
	{
		internal const int Magic = ('D' | ('D' << 8) | ('S' << 16) | (' ' << 24));
		private const int _ddsDxt1 = ('D' | ('X' << 8) | ('T' << 16) | ('1' << 24));
		private const int _ddsDxt2 = ('D' | ('X' << 8) | ('T' << 16) | ('2' << 24));
		private const int _ddsDxt3 = ('D' | ('X' << 8) | ('T' << 16) | ('3' << 24));
		private const int _ddsDxt4 = ('D' | ('X' << 8) | ('T' << 16) | ('4' << 24));
		private const int _ddsDxt5 = ('D' | ('X' << 8) | ('T' << 16) | ('5' << 24));
		private const int _ddsDx10 = ('D' | ('X' << 8) | ('1' << 16) | ('0' << 24));
		private const int _ddsAti1 = ('A' | ('T' << 8) | ('I' << 16) | ('1' << 24));
		private const int _ddsBc4U = ('B' | ('C' << 8) | ('4' << 16) | ('U' << 24));
		private const int _ddsBc4S = ('B' | ('C' << 8) | ('4' << 16) | ('S' << 24));
		private const int _ddsAti2 = ('A' | ('T' << 8) | ('I' << 16) | ('2' << 24));
		private const int _ddsBc5U = ('B' | ('C' << 8) | ('5' << 16) | ('U' << 24));
		private const int _ddsBc5S = ('B' | ('C' << 8) | ('5' << 16) | ('S' << 24));
		private const int _ddsRgbg = ('R' | ('G' << 8) | ('B' << 16) | ('G' << 24));
		private const int _ddsGrgb = ('G' | ('R' << 8) | ('G' << 16) | ('B' << 24));
		private const int _ddsYuy2 = ('Y' | ('U' << 8) | ('Y' << 16) | ('2' << 24));

		private const int _ddsAlphaPixels = 0x00000001;
		private const int _ddsAlpha = 0x00000002;
		private const int _ddsFourCc = 0x00000004;
		private const int _ddsRgb = 0x00000040;
		private const int _ddsYuv = 0x00000200;
		private const int _ddsLuminance = 0x00020000;
		private const int _ddsMipmapCount = 0x00020000;
		private const int _ddsHeight = 0x00000002;
		private const int _ddsWidth = 0x00000004;
		private const int _ddsVolume = 0x00800000;

		private const int _ddsCubemapPositiveX = 0x00000600; // DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_POSITIVEX
		private const int _ddsCubemapNegativeX = 0x00000a00; // DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_NEGATIVEX
		private const int _ddsCubemapPositiveY = 0x00001200; // DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_POSITIVEY
		private const int _ddsCubemapNegativeY = 0x00002200; // DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_NEGATIVEY
		private const int _ddsCubemapPositiveZ = 0x00004200; // DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_POSITIVEZ
		private const int _ddsCubemapNegativeZ = 0x00008200; // DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_NEGATIVEZ

		private const int _ddsCubemapAllFaces =
			(_ddsCubemapPositiveX | _ddsCubemapNegativeX | _ddsCubemapPositiveY | _ddsCubemapNegativeY |
			 _ddsCubemapPositiveZ | _ddsCubemapNegativeZ);

		private const int _ddsCubemap = 0x00000200; // DDSCAPS2_CUBEMAP
		public static byte[] MagicBytes = {(byte)'D', (byte)'D', (byte)'S', (byte)' '};

		internal DDSImage(Stream data, bool staging = false)
		{
			var reader = new BinaryReader(data);

			// DDS files always start with the same magic number ("DDS ")
			uint dwMagicNumber = reader.ReadUInt32();
			if (dwMagicNumber != Magic)
			{
				throw new InvalidDataException("Missing magic number.");
			}

			Header = ReadHeader(reader);

			bool hasDxt10Header = ((Header.PixelFormat.Flags & _ddsFourCc) != 0) &&
								  (Header.PixelFormat.FourCc == _ddsDx10);

			if (hasDxt10Header)
			{
				HeaderDxt10 = ReadHeaderDxt10(reader);
			}

			const int sizeofDdsHeader = 128;
			const int sizeofDdsHeaderDxt10 = 10;

			var offset = sizeof(uint) + sizeofDdsHeader
						 + (hasDxt10Header ? sizeofDdsHeaderDxt10 : 0);
			var bitSize = (int)data.Length - offset;

			Texture2D = CreateTextureFromDds(reader, Header, false, staging);

			reader.Dispose();
			data.Dispose();
		}

		internal DdsHeader Header { get; }
		internal DdsHeaderDxt10 HeaderDxt10 { get; }

		public void Dispose()
		{
			Texture2D?.Dispose();
		}

		public Texture2D Texture2D { get; }

		private DdsHeaderDxt10 ReadHeaderDxt10(BinaryReader reader)
		{
			return new DdsHeaderDxt10
			{
				DxgiFormat = (Format)reader.ReadInt32(),
				ResourceDimension = (ResourceDimension)reader.ReadInt32(),
				MiscFlag = (ResourceOptionFlags)reader.ReadInt32(),
				ArraySize = reader.ReadInt32(),
				MiscFlags2 = reader.ReadInt32()
			};
		}

		private Texture2D CreateTextureFromDds(BinaryReader reader, DdsHeader header, bool vflip, bool staging)
		{
			var width = header.Width;
			var height = header.Height;
			var depth = header.Depth;

			var arraySize = 1;
			var format = Format.Unknown;
			var isCubeMap = false;

			var mipCount = header.MipMapCount;

			if (mipCount == 0)
				mipCount = 1;

			if (HeaderDxt10 != null)
			{
				arraySize = HeaderDxt10.ArraySize;
				if (arraySize == 0)
				{
					throw new InvalidDataException();
				}

				format = HeaderDxt10.DxgiFormat;

				switch (format)
				{
					case Format.AI44:
					case Format.IA44:
					case Format.P8:
					case Format.A8P8:
						throw new NotSupportedException();
					default:
						if (BitsPerPixel(HeaderDxt10.DxgiFormat) == 0)
						{
							throw new NotSupportedException();
						}
						break;
				}

				switch (HeaderDxt10.ResourceDimension)
				{
					case ResourceDimension.Texture1D:
						// D3DX writes 1D textures with a fixed Height of 1
						if (((int)header.Flags & _ddsHeight) != 0 && height != 1)
						{
							throw new InvalidDataException();
						}
						height = depth = 1;
						break;

					case ResourceDimension.Texture2D:
						if ((HeaderDxt10.MiscFlag & ResourceOptionFlags.TextureCube) != 0)
						{
							arraySize *= 6;
							isCubeMap = true;
						}
						depth = 1;
						break;
					case ResourceDimension.Texture3D:
						if (((int)header.Flags & _ddsVolume) == 0)
						{
							throw new InvalidDataException();
						}

						if (arraySize > 1)
						{
							throw new NotSupportedException();
						}
						break;

					default:
						throw new NotSupportedException();
				}
			}
			else
			{
				format = GetDxgiFormat(header.PixelFormat);

				if (format == Format.Unknown)
				{
					throw new NotSupportedException();
				}

				if (((int)header.Flags & _ddsVolume) != 0)
				{
				}
				else
				{
					if ((header.Caps2 & _ddsCubemap) != 0)
					{
						// We require all six faces to be defined
						if ((header.Caps2 & _ddsCubemapAllFaces) != _ddsCubemapAllFaces)
						{
							throw new NotSupportedException();
						}

						arraySize = 6;
						isCubeMap = true;
					}

					depth = 1;

					// Note there's no way for a legacy Direct3D 9 DDS to express a '1D' texture
				}

				Debug.Assert(BitsPerPixel(format) != 0);
			}

			var rectangles = new DataRectangle[mipCount * arraySize];

			var skipMip = 0;
			var twidth = 0;
			var theight = 0;
			var tdepth = 0;
			FillInitData(reader, width, height, depth, mipCount, arraySize, format, vflip, ref twidth, ref theight,
				ref tdepth, ref skipMip, ref rectangles);

			return new Texture2D(Renderer.Device, new Texture2DDescription
			{
				Width = width,
				Height = height,
				BindFlags = staging ? BindFlags.None : BindFlags.ShaderResource,
				Usage = staging ? ResourceUsage.Staging : ResourceUsage.Default,
				Format = format,
				ArraySize = arraySize,
				MipLevels = mipCount,
				OptionFlags = isCubeMap ? ResourceOptionFlags.TextureCube : ResourceOptionFlags.None,
				CpuAccessFlags = staging ? CpuAccessFlags.Read : CpuAccessFlags.None,
				SampleDescription = new SampleDescription(1, 0)
			}, rectangles);
		}


		private void FillInitData(BinaryReader reader, int width, int height, int depth, int mipCount, int arraySize,
			Format format,
			bool vflip, ref int twidth, ref int theight, ref int tdepth, ref int skipMip, ref DataRectangle[] rectangles)
		{
			skipMip = 0;
			twidth = 0;
			theight = 0;
			tdepth = 0;

			var NumBytes = 0;
			var RowBytes = 0;
			//const uint8_t* pSrcBits = bitData;
			//const uint8_t* pEndBits = bitData + bitSize;

			var index = 0;
			for (var j = 0; j < arraySize; j++)
			{
				var w = width;
				var h = height;
				var d = depth;
				var blocksWide = 0;

				for (var i = 0; i < mipCount; i++)
				{
					GetSurfaceInfo(w, h, format, out NumBytes, out RowBytes, out blocksWide);

					if (twidth != 0)
					{
						twidth = w;
						theight = h;
						tdepth = d;
					}

					if (vflip)
					{
						bool flipResult = true;
						if (IsCompressed(format, reader))
						{
							flipResult = FlipCompressedData(format, RowBytes, NumBytes, blocksWide, reader);
						}
						else
						{
							flipResult = FlipUncompressedData(RowBytes, NumBytes, reader);
						}

						if (!flipResult)
						{
							throw new InvalidOperationException();
						}
					}

					var data = new DataStream(NumBytes, true, true);
					data.WriteRange(reader.ReadBytes(NumBytes));
					rectangles[index] = new DataRectangle(data.DataPointer, RowBytes);


					w = w >> 1;
					h = h >> 1;
					d = d >> 1;
					if (w == 0)
						w = 1;
					if (h == 0)
						h = 1;
					if (d == 0)
						d = 1;

					twidth = w;
					theight = h;
					tdepth = d;

					index++;
				}
			}
		}

		private bool FlipUncompressedData(int rowBytes, int numBytes, BinaryReader reader)
		{
			throw new NotImplementedException();
		}

		private bool FlipCompressedData(Format format, int rowBytes, int numBytes, int blocksWide, BinaryReader reader)
		{
			throw new NotImplementedException();
		}

		private bool IsCompressed(Format format, BinaryReader reader)
		{
			throw new NotImplementedException();
		}

		private void GetSurfaceInfo(int width, int height, Format format, out int numBytes, out int rowBytes,
			out int numBlocksWide)
		{
			var numRows = 0;
			numBlocksWide = 0;

			bool bc = false;
			bool packed = false;
			bool planar = false;
			var bpe = 0;

			switch (format)
			{
				case Format.BC1_Typeless:
				case Format.BC1_UNorm:
				case Format.BC1_UNorm_SRgb:
				case Format.BC4_Typeless:
				case Format.BC4_UNorm:
				case Format.BC4_SNorm:
					bc = true;
					bpe = 8;
					break;

				case Format.BC2_Typeless:
				case Format.BC2_UNorm:
				case Format.BC2_UNorm_SRgb:
				case Format.BC3_Typeless:
				case Format.BC3_UNorm:
				case Format.BC3_UNorm_SRgb:
				case Format.BC5_Typeless:
				case Format.BC5_UNorm:
				case Format.BC5_SNorm:
				case Format.BC6H_Typeless:
				case Format.BC6H_Uf16:
				case Format.BC6H_Sf16:
				case Format.BC7_Typeless:
				case Format.BC7_UNorm:
				case Format.BC7_UNorm_SRgb:
					bc = true;
					bpe = 16;
					break;

				case Format.R8G8_B8G8_UNorm:
				case Format.G8R8_G8B8_UNorm:
				case Format.YUY2:
					packed = true;
					bpe = 4;
					break;

				case Format.Y210:
				case Format.Y216:
					packed = true;
					bpe = 8;
					break;

				case Format.NV12:
				case Format.Opaque420:
					planar = true;
					bpe = 2;
					break;

				case Format.P010:
				case Format.P016:
					planar = true;
					bpe = 4;
					break;
			}

			if (bc)
			{
				if (width > 0)
				{
					numBlocksWide = Math.Max(1, (width + 3) / 4);
				}
				var numBlocksHigh = 0;
				if (height > 0)
				{
					numBlocksHigh = Math.Max(1, (height + 3) / 4);
				}
				rowBytes = numBlocksWide * bpe;
				numRows = numBlocksHigh;
				numBytes = rowBytes * numBlocksHigh;
			}
			else if (packed)
			{
				rowBytes = ((width + 1) >> 1) * bpe;
				numRows = height;
				numBytes = rowBytes * height;
			}
			else if (format == Format.NV11)
			{
				rowBytes = ((width + 3) >> 2) * 4;
				numRows = height * 2;
				// Direct3D makes this simplifying assumption, although it is larger than the 4:1:1 data
				numBytes = rowBytes * numRows;
			}
			else if (planar)
			{
				rowBytes = ((width + 1) >> 1) * bpe;
				numBytes = (rowBytes * height) + ((rowBytes * height + 1) >> 1);
				numRows = height + ((height + 1) >> 1);
			}
			else
			{
				var bpp = BitsPerPixel(format);
				rowBytes = (width * bpp + 7) / 8; // round up to nearest byte
				numRows = height;
				numBytes = rowBytes * height;
			}
		}

		private bool IsBitMask(PixelFormat pf, uint r, uint g, uint b, uint a)
		{
			return (pf.RBitMask == r && pf.GBitMask == g && pf.BBitMask == b && pf.ABitMask == a);
		}

		private Format GetDxgiFormat(PixelFormat pf)
		{
			if ((pf.Flags & _ddsRgb) != 0)
			{
				// Note that sRGB formats are written using the "DX10" extended header
				switch (pf.RgbBitCount)
				{
					case 32:
						if (IsBitMask(pf, 0x000000ff, 0x0000ff00, 0x00ff0000, 0xff000000))
						{
							return Format.R8G8B8A8_UNorm;
						}

						if (IsBitMask(pf, 0x00ff0000, 0x0000ff00, 0x000000ff, 0xff000000))
						{
							return Format.B8G8R8A8_UNorm;
						}

						if (IsBitMask(pf, 0x00ff0000, 0x0000ff00, 0x000000ff, 0x00000000))
						{
							return Format.B8G8R8X8_UNorm;
						}

						// No DXGI format maps to IsBitMask(pf, 0x000000ff,0x0000ff00,0x00ff0000,0x00000000) aka D3DFMT_X8B8G8R8

						// Note that many common DDS reader/writers (including D3DX) swap the
						// the RED/BLUE masks for 10:10:10:2 formats. We assume
						// below that the 'backwards' header mask is being used since it is most
						// likely written by D3DX. The more robust solution is to use the 'DX10'
						// header extension and specify the Format.R10G10B10A2_UNorm format directly

						// For 'correct' writers, this should be 0x000003ff,0x000ffc00,0x3ff00000 for RGB data
						if (IsBitMask(pf, 0x3ff00000, 0x000ffc00, 0x000003ff, 0xc0000000))
						{
							return Format.R10G10B10A2_UNorm;
						}

						// No DXGI format maps to IsBitMask(pf, 0x000003ff,0x000ffc00,0x3ff00000,0xc0000000) aka D3DFMT_A2R10G10B10

						if (IsBitMask(pf, 0x0000ffff, 0xffff0000, 0x00000000, 0x00000000))
						{
							return Format.R16G16_UNorm;
						}

						if (IsBitMask(pf, 0xffffffff, 0x00000000, 0x00000000, 0x00000000))
						{
							// Only 32-bit color channel format in D3D9 was R32F
							return Format.R32_Float; // D3DX writes this out as a FourCC of 114
						}
						break;

					case 24:
						// No 24bpp DXGI formats aka D3DFMT_R8G8B8
						break;

					case 16:
						if (IsBitMask(pf, 0x7c00, 0x03e0, 0x001f, 0x8000))
						{
							return Format.B5G5R5A1_UNorm;
						}
						if (IsBitMask(pf, 0xf800, 0x07e0, 0x001f, 0x0000))
						{
							return Format.B5G6R5_UNorm;
						}

						// No DXGI format maps to IsBitMask(pf, 0x7c00,0x03e0,0x001f,0x0000) aka D3DFMT_X1R5G5B5

						if (IsBitMask(pf, 0x0f00, 0x00f0, 0x000f, 0xf000))
						{
							return Format.B4G4R4A4_UNorm;
						}

						// No DXGI format maps to IsBitMask(pf, 0x0f00,0x00f0,0x000f,0x0000) aka D3DFMT_X4R4G4B4

						// No 3:3:2, 3:3:2:8, or paletted DXGI formats aka D3DFMT_A8R3G3B2, D3DFMT_R3G3B2, D3DFMT_P8, D3DFMT_A8P8, etc.
						break;
				}
			}
			else if ((pf.Flags & _ddsLuminance) != 0)
			{
				if (8 == pf.RgbBitCount)
				{
					if (IsBitMask(pf, 0x000000ff, 0x00000000, 0x00000000, 0x00000000))
					{
						return Format.R8_UNorm; // D3DX10/11 writes this out as DX10 extension
					}

					// No DXGI format maps to IsBitMask(pf, 0x0f,0x00,0x00,0xf0) aka D3DFMT_A4L4
				}

				if (16 == pf.RgbBitCount)
				{
					if (IsBitMask(pf, 0x0000ffff, 0x00000000, 0x00000000, 0x00000000))
					{
						return Format.R16_UNorm; // D3DX10/11 writes this out as DX10 extension
					}
					if (IsBitMask(pf, 0x000000ff, 0x00000000, 0x00000000, 0x0000ff00))
					{
						return Format.R8G8_UNorm; // D3DX10/11 writes this out as DX10 extension
					}
				}
			}
			else if ((pf.Flags & _ddsAlpha) != 0)
			{
				if (8 == pf.RgbBitCount)
				{
					return Format.A8_UNorm;
				}
			}
			else if ((pf.Flags & _ddsFourCc) != 0)
			{
				switch (pf.FourCc)
				{
					case _ddsDxt1:
						return Format.BC1_UNorm;
					case _ddsDxt3:
						return Format.BC2_UNorm;
					case _ddsDxt5:
						return Format.BC3_UNorm;
					case _ddsDxt2:
						return Format.BC2_UNorm;
					case _ddsDxt4:
						return Format.BC3_UNorm;
					case _ddsAti1:
						return Format.BC4_UNorm;
					case _ddsBc4U:
						return Format.BC4_UNorm;
					case _ddsBc4S:
						return Format.BC4_SNorm;
					case _ddsAti2:
						return Format.BC5_UNorm;
					case _ddsBc5U:
						return Format.BC5_UNorm;
					case _ddsBc5S:
						return Format.BC5_SNorm;
					case _ddsRgbg:
						return Format.R8G8_B8G8_UNorm;
					case _ddsGrgb:
						return Format.G8R8_G8B8_UNorm;
					case _ddsYuy2:
						return Format.YUY2;

					case 36: // D3DFMT_A16B16G16R16
						return Format.R16G16B16A16_UNorm;

					case 110: // D3DFMT_Q16W16V16U16
						return Format.R16G16B16A16_SNorm;

					case 111: // D3DFMT_R16F
						return Format.R16_Float;

					case 112: // D3DFMT_G16R16F
						return Format.R16G16_Float;

					case 113: // D3DFMT_A16B16G16R16F
						return Format.R16G16B16A16_Float;

					case 114: // D3DFMT_R32F
						return Format.R32_Float;

					case 115: // D3DFMT_G32R32F
						return Format.R32G32_Float;

					case 116: // D3DFMT_A32B32G32R32F
						return Format.R32G32B32A32_Float;
				}
			}

			return Format.Unknown;
		}

		private static DdsHeader ReadHeader(BinaryReader reader)
		{
			var header = new DdsHeader
			{
				Size = reader.ReadInt32(),
				Flags = (DdsHeaderFlags)reader.ReadInt32(),
				Height = reader.ReadInt32(),
				Width = reader.ReadInt32(),
				PitchOrLinearSize = reader.ReadInt32(),
				Depth = reader.ReadInt32(),
				MipMapCount = reader.ReadInt32()
			};

			for (int i = 0; i < 11; ++i)
			{
				header.Reserved1[i] = reader.ReadInt32();
			}

			header.PixelFormat = ReadPixelFormat(reader);
			header.Caps = reader.ReadInt32();
			header.Caps2 = reader.ReadInt32();
			header.Caps3 = reader.ReadInt32();
			header.Caps4 = reader.ReadInt32();
			header.Reserved2 = reader.ReadInt32();

			return header;
		}

		private static PixelFormat ReadPixelFormat(BinaryReader reader)
		{
			return new PixelFormat
			{
				Size = reader.ReadInt32(),
				Flags = reader.ReadInt32(),
				FourCc = reader.ReadInt32(),
				RgbBitCount = reader.ReadInt32(),
				RBitMask = reader.ReadInt32(),
				GBitMask = reader.ReadInt32(),
				BBitMask = reader.ReadInt32(),
				ABitMask = reader.ReadInt32()
			};
		}

		private static int BitsPerPixel(Format fmt)
		{
			switch (fmt)
			{
				case Format.R32G32B32A32_Typeless:
				case Format.R32G32B32A32_Float:
				case Format.R32G32B32A32_UInt:
				case Format.R32G32B32A32_SInt:
					return 128;

				case Format.R32G32B32_Typeless:
				case Format.R32G32B32_Float:
				case Format.R32G32B32_UInt:
				case Format.R32G32B32_SInt:
					return 96;

				case Format.R16G16B16A16_Typeless:
				case Format.R16G16B16A16_Float:
				case Format.R16G16B16A16_UNorm:
				case Format.R16G16B16A16_UInt:
				case Format.R16G16B16A16_SNorm:
				case Format.R16G16B16A16_SInt:
				case Format.R32G32_Typeless:
				case Format.R32G32_Float:
				case Format.R32G32_UInt:
				case Format.R32G32_SInt:
				case Format.R32G8X24_Typeless:
				case Format.D32_Float_S8X24_UInt:
				case Format.R32_Float_X8X24_Typeless:
				case Format.X32_Typeless_G8X24_UInt:
				case Format.Y416:
				case Format.Y210:
				case Format.Y216:
					return 64;

				case Format.R10G10B10A2_Typeless:
				case Format.R10G10B10A2_UNorm:
				case Format.R10G10B10A2_UInt:
				case Format.R11G11B10_Float:
				case Format.R8G8B8A8_Typeless:
				case Format.R8G8B8A8_UNorm:
				case Format.R8G8B8A8_UNorm_SRgb:
				case Format.R8G8B8A8_UInt:
				case Format.R8G8B8A8_SNorm:
				case Format.R8G8B8A8_SInt:
				case Format.R16G16_Typeless:
				case Format.R16G16_Float:
				case Format.R16G16_UNorm:
				case Format.R16G16_UInt:
				case Format.R16G16_SNorm:
				case Format.R16G16_SInt:
				case Format.R32_Typeless:
				case Format.D32_Float:
				case Format.R32_Float:
				case Format.R32_UInt:
				case Format.R32_SInt:
				case Format.R24G8_Typeless:
				case Format.D24_UNorm_S8_UInt:
				case Format.R24_UNorm_X8_Typeless:
				case Format.X24_Typeless_G8_UInt:
				case Format.R9G9B9E5_Sharedexp:
				case Format.R8G8_B8G8_UNorm:
				case Format.G8R8_G8B8_UNorm:
				case Format.B8G8R8A8_UNorm:
				case Format.B8G8R8X8_UNorm:
				case Format.R10G10B10_Xr_Bias_A2_UNorm:
				case Format.B8G8R8A8_Typeless:
				case Format.B8G8R8A8_UNorm_SRgb:
				case Format.B8G8R8X8_Typeless:
				case Format.B8G8R8X8_UNorm_SRgb:
				case Format.AYUV:
				case Format.Y410:
				case Format.YUY2:
					return 32;

				case Format.P010:
				case Format.P016:
					return 24;

				case Format.R8G8_Typeless:
				case Format.R8G8_UNorm:
				case Format.R8G8_UInt:
				case Format.R8G8_SNorm:
				case Format.R8G8_SInt:
				case Format.R16_Typeless:
				case Format.R16_Float:
				case Format.D16_UNorm:
				case Format.R16_UNorm:
				case Format.R16_UInt:
				case Format.R16_SNorm:
				case Format.R16_SInt:
				case Format.B5G6R5_UNorm:
				case Format.B5G5R5A1_UNorm:
				case Format.A8P8:
				case Format.B4G4R4A4_UNorm:
					return 16;

				case Format.NV12:
				case Format.Opaque420:
				case Format.NV11:
					return 12;

				case Format.R8_Typeless:
				case Format.R8_UNorm:
				case Format.R8_UInt:
				case Format.R8_SNorm:
				case Format.R8_SInt:
				case Format.A8_UNorm:
				case Format.AI44:
				case Format.IA44:
				case Format.P8:
					return 8;

				case Format.R1_UNorm:
					return 1;

				case Format.BC1_Typeless:
				case Format.BC1_UNorm:
				case Format.BC1_UNorm_SRgb:
				case Format.BC4_Typeless:
				case Format.BC4_UNorm:
				case Format.BC4_SNorm:
					return 4;

				case Format.BC2_Typeless:
				case Format.BC2_UNorm:
				case Format.BC2_UNorm_SRgb:
				case Format.BC3_Typeless:
				case Format.BC3_UNorm:
				case Format.BC3_UNorm_SRgb:
				case Format.BC5_Typeless:
				case Format.BC5_UNorm:
				case Format.BC5_SNorm:
				case Format.BC6H_Typeless:
				case Format.BC6H_Uf16:
				case Format.BC6H_Sf16:
				case Format.BC7_Typeless:
				case Format.BC7_UNorm:
				case Format.BC7_UNorm_SRgb:
					return 8;

				default:
					return 0;
			}
		}

		internal static Texture2D LoadTextureArray(IList<DDSImage> ddsImages, bool isCubemap = false)
		{
			var first = ddsImages.First();
			var firstDesc = first.Texture2D.Description;

			var sliceCount = ddsImages.Count;
			var mipMapCount = first.Header.MipMapCount;
			var rectangles = new DataRectangle[sliceCount * mipMapCount];
			var stagingTextures = new Texture2D[sliceCount];

			var stagingDesc = firstDesc;
			stagingDesc.Usage = ResourceUsage.Staging;
			stagingDesc.CpuAccessFlags = CpuAccessFlags.Read;
			stagingDesc.BindFlags = BindFlags.None;

			var arrayDesc = new Texture2DDescription
			{
				Width = firstDesc.Width,
				Height = firstDesc.Height,
				ArraySize = sliceCount,
				BindFlags = BindFlags.ShaderResource,
				Usage = ResourceUsage.Immutable,
				CpuAccessFlags = CpuAccessFlags.None,
				Format = firstDesc.Format,
				MipLevels = mipMapCount,
				OptionFlags = isCubemap ? ResourceOptionFlags.TextureCube : 0,
				SampleDescription = new SampleDescription(1, 0)
			};

			var context = Renderer.Context;
			lock (Renderer.Locker)
			{
				var index = 0;
				for (var sliceIndex = 0; sliceIndex < sliceCount; sliceIndex++)
				{
					using (var slice = ddsImages[sliceIndex])
					{
						var stagingTexture = new Texture2D(Renderer.Device, stagingDesc);
						stagingTextures[sliceIndex] = stagingTexture;

						Renderer.Context.CopyResource(slice.Texture2D, stagingTexture);
						// copy data from gpu to cpu

						for (var mipIndex = 0; mipIndex < slice.Header.MipMapCount; mipIndex++)
						{
							DataStream dataStream;

							var dataBox = Renderer.Context.MapSubresource(stagingTexture, mipIndex, 0,
								MapMode.Read,
								MapFlags.None, out dataStream);

							rectangles[index] = new DataRectangle(dataBox.DataPointer, dataBox.RowPitch);

							index++;
						}
					}
				}
			}

			var textureArray = new Texture2D(Renderer.Device, arrayDesc, rectangles);

			foreach (var st in stagingTextures)
			{
				for (var mipIndex = 0; mipIndex < first.Header.MipMapCount; mipIndex++)
				{
					int mipSize;
					Renderer.Context.UnmapSubresource(st,
						st.CalculateSubResourceIndex(mipIndex, 0, out mipSize));
				}
			}

			return textureArray;
		}

		internal enum DdsHeaderFlags
		{
			AlphaPixels = 0x00000001,
			Alpha = 0x00000002,
			FourCc = 0x00000004,
			Rgb = 0x00000040,
			Yuv = 0x00000200,
			DdpfLuminance = 0x00020000
		}

		internal class DdsHeader
		{
			internal readonly int[] Reserved1 = new int[11];
			internal int Caps;
			internal int Caps2;
			internal int Caps3;
			internal int Caps4;
			internal int Depth;
			internal DdsHeaderFlags Flags;
			internal int Height;
			internal int MipMapCount;
			internal int PitchOrLinearSize;
			internal PixelFormat PixelFormat = new PixelFormat();
			internal int Reserved2;
			internal int Size;
			internal int Width;
		}

		internal class PixelFormat
		{
			internal int ABitMask;
			internal int BBitMask;
			internal int Flags;
			internal int FourCc;
			internal int GBitMask;
			internal int RBitMask;
			internal int RgbBitCount;
			internal int Size;
		}

		internal class DdsHeaderDxt10
		{
			internal int ArraySize;
			internal Format DxgiFormat;
			internal ResourceOptionFlags MiscFlag; // see D3D11_RESOURCE_MISC_FLAG
			internal int MiscFlags2;
			internal ResourceDimension ResourceDimension;
		}

		internal class Dxt1Block
		{
			internal short[] Colors = new short[4];
			internal byte[] Data = new byte[4];
		}

		internal class Dxt3Block
		{
			internal short[] Alpha = new short[4];
			internal Dxt1Block Dxt1Data;
		}

		internal class Dxt5AlphaBlock
		{
			internal byte[] Colors = new byte[2];
			internal byte[] Data = new byte[6];
		}

		internal class Dxt5Block
		{
			Dxt5AlphaBlock _alpha;
			Dxt1Block _dxt1Data;
		}
	}
}