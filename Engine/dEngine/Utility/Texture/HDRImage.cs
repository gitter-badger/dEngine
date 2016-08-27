// HDRImage.cs - dEngine
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
using dEngine.Utility.Native;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

#pragma warning disable 1591

namespace dEngine.Utility.Texture
{
	internal class HDRImage : ITextureFormat
	{
		internal const int Magic = ('#' | ('?' << 8) | ('R' << 16) | ('A' << 24));
		private const int MinScanLine = 8;
		private const int MaxScanLine = 0x7fff;
		private static readonly char[] _magicBytes = "#?RADIANCE".ToCharArray();

		public HDRImage(Stream stream, bool staging = false)
		{
			int i;
			var str = new char[10];
			using (var reader = new BinaryReader(stream))
			{
				reader.Read(str, 0, 10);

				if (VisualC.CompareMemory(str, _magicBytes, 10) != 0)
					throw new InvalidDataException("Magic bytes invalid.");

				stream.Position++;

				var cmd = new char[200];
				i = 0;
				char c = (char)0, oldc;
				while (true)
				{
					oldc = c;
					c = reader.ReadChar();
					if (c == 0xa && oldc == 0xa)
						break;
					cmd[i++] = c;
				}

				var reso = new char[200];
				i = 0;
				while (true)
				{
					c = reader.ReadChar();
					reso[i++] = c;
					if (c == 0xa)
						break;
				}

				int w, h;

				if (VisualC.ScanString(new string(reso), "-Y %ld +X %ld", out h, out w) != 2)
					throw new InvalidDataException();

				Width = w;
				Height = h;

				var colours = new Color3[w * h];

				var scanline = new byte[w, 4];

				// convert image 
				i = 0;
				for (int y = h - 1; y >= 0; y--)
				{
					if (Decrunch(scanline, w, reader) == false)
						break;
					WorkOnRGBE(scanline, w, colours, i);
				}

				var stride = w * 4;
				var buffer = new DataStream(h * stride, true, true);

				for (var y = 0; y < h; ++y)
				{
					for (var x = 0; x < w; x++)
					{
						var colour = colours[y + x];
						var rgba = (int)colour.Red | ((int)colour.Green << 8) | ((int)colour.Blue << 16) | (1 << 24);
						buffer.Write(rgba);
					}
				}

				Texture2D = new Texture2D(Renderer.Device, new Texture2DDescription
				{
					Width = w,
					Height = h,
					ArraySize = 1,
					BindFlags = staging ? BindFlags.None : BindFlags.ShaderResource,
					Usage = staging ? ResourceUsage.Staging : ResourceUsage.Default,
					Format = Format.R8G8B8A8_UNorm,
					MipLevels = 1,
					OptionFlags = 0,
					CpuAccessFlags = staging ? CpuAccessFlags.Read : CpuAccessFlags.None,
					SampleDescription = new SampleDescription(1, 0)
				}, new DataRectangle(buffer.DataPointer, stride));
			}
		}

		public int Width { get; private set; }
		public int Height { get; private set; }

		public Texture2D Texture2D { get; }

		private float ConvertComponent(int expo, int val)
		{
			float v = val / 256.0f;
			float d = (float)Math.Pow(2, expo);
			return v * d;
		}

		private void WorkOnRGBE(byte[,] scan, int length, Color3[] colours, int index)
		{
			while (length-- > 0)
			{
				int expo = scan[0, 3] - 128;
				colours[index] = new Color3(ConvertComponent(expo, scan[0, 0]), ConvertComponent(expo, scan[0, 1]),
					ConvertComponent(expo, scan[0, 2]));
				index++;
			}
		}

		private static bool Decrunch(byte[,] scanline, int length, BinaryReader reader)
		{
			int index = 0;

			if (length < MinScanLine || length > MaxScanLine)
				return OldDecrunch(reader, scanline, ref index, ref length);

			int i = reader.ReadByte();
			if (i != 2)
			{
				reader.BaseStream.Seek(-1, SeekOrigin.Current);
				return OldDecrunch(reader, scanline, ref index, ref length);
			}

			scanline[0, 1] = reader.ReadByte();
			scanline[0, 2] = reader.ReadByte();
			i = reader.ReadByte();

			if (scanline[0, 1] != 2 || (scanline[0, 2] & 128) != 0)
			{
				scanline[0, 0] = 2;
				scanline[0, 3] = (byte)i;
				--length;
				return OldDecrunch(reader, scanline, ref index, ref length);
			}

			// read each component
			for (i = 0; i < 4; i++)
			{
				for (int j = 0; j < length;)
				{
					byte code = reader.ReadByte();
					if (code > 128)
					{
						// run
						code &= 127;
						var val = reader.ReadByte();
						while (code-- > 0)
							scanline[j++, i] = val;
					}
					else
					{
						// non-run
						while (code-- > 0)
							scanline[j++, i] = reader.ReadByte();
					}
				}
			}

			return reader.BaseStream.Position != reader.BaseStream.Length;
		}

		private static bool OldDecrunch(BinaryReader reader, byte[,] scanline, ref int index, ref int length)
		{
			int rshift = 0;

			while (length > 0)
			{
				scanline[index, 0] = reader.ReadByte();
				scanline[index, 1] = reader.ReadByte();
				scanline[index, 2] = reader.ReadByte();
				scanline[index, 3] = reader.ReadByte();

				if (reader.BaseStream.Position == reader.BaseStream.Length)
					return false;

				if (scanline[index, 0] == 1 &&
					scanline[index, 1] == 1 &&
					scanline[index, 2] == 1)
				{
					for (int i = scanline[index, 3] << rshift; i > 0; i--)
					{
						var prev = index - 1;
						scanline[index, 0] = scanline[prev, 0];
						scanline[index, 1] = scanline[prev, 1];
						scanline[index, 2] = scanline[prev, 2];
						scanline[index, 3] = scanline[prev, 3];
						index++;
						length--;
					}
					rshift += 8;
				}
				else
				{
					index++;
					length--;
					rshift = 0;
				}
			}
			return true;
		}
	}
}