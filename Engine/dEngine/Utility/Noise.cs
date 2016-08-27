// Noise.cs - dEngine
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
using SharpDX;

namespace dEngine.Utility
{
	internal static class Noise
	{
		private const int _intensity = 100;
		private const int _saturation = 0;
		private const double _coverage = 0.5 * 0.02;
		private const int tableSize = 16384;
		private static Random _threadRand = new Random();
		private static int[] lookup;

		static Noise()
		{
			InitLookup();
		}

		private static double NormalCurve(double x, double scale)
		{
			return scale * Math.Exp(-x * x / 2);
		}

		private static void InitLookup()
		{
			int[] curve = new int[tableSize];
			int[] integral = new int[tableSize];

			double l = 5;
			double r = 10;
			double scale = 50;
			double sum = 0;

			while (r - l > 0.0000001)
			{
				sum = 0;
				scale = (l + r) * 0.5;

				for (int i = 0; i < tableSize; ++i)
				{
					sum += NormalCurve(16.0 * ((double)i - tableSize / 2) / tableSize, scale);

					if (sum > 1000000)
					{
						break;
					}
				}

				if (sum > tableSize)
				{
					r = scale;
				}
				else if (sum < tableSize)
				{
					l = scale;
				}
				else
				{
					break;
				}
			}

			lookup = new int[tableSize];
			sum = 0;
			int roundedSum = 0, lastRoundedSum;

			for (int i = 0; i < tableSize; ++i)
			{
				sum += NormalCurve(16.0 * ((double)i - tableSize / 2) / tableSize, scale);
				lastRoundedSum = roundedSum;
				roundedSum = (int)sum;

				for (int j = lastRoundedSum; j < roundedSum; ++j)
				{
					lookup[j] = (i - tableSize / 2) * 65536 / tableSize;
				}
			}
		}

		internal static unsafe ColorBGRA* GetPointAddressUnchecked(DataRectangle surfaceRect, int x, int y)
		{
			return unchecked(x + (ColorBGRA*)(surfaceRect.DataPointer + (y * surfaceRect.Pitch)));
		}

		internal static unsafe DataRectangle Render(int width, int height)
		{
			int dev = _intensity * _intensity / 4;
			int sat = _saturation * 4096 / 100;

			if (_threadRand == null)
			{
				_threadRand = new Random(System.Threading.Thread.CurrentThread.GetHashCode() ^
										 unchecked((int)System.DateTime.Now.Ticks));
			}

			Random localRand = _threadRand;
			int[] localLookup = lookup;

			var rect = new Rectangle(0, 0, width, height);

			var dataStream = new DataStream(width * height * 4, true, true);
			var dataRect = new DataRectangle(dataStream.DataPointer, width * 4);

			for (int y = rect.Top; y < rect.Bottom; ++y)
			{
				var dstPtr = GetPointAddressUnchecked(dataRect, rect.Left, y);

				for (int x = 0; x < rect.Width; ++x)
				{
					if (localRand.NextDouble() > _coverage)
					{
						//*dstPtr = *srcPtr;
					}
					else
					{
						var r = localLookup[localRand.Next(tableSize)];
						var g = localLookup[localRand.Next(tableSize)];
						var b = localLookup[localRand.Next(tableSize)];

						var i = (4899 * r + 9618 * g + 1867 * b) >> 14;

						r = i + (((r - i) * sat) >> 12);
						g = i + (((g - i) * sat) >> 12);
						b = i + (((b - i) * sat) >> 12);

						dstPtr->R = ClampToByte((r * dev + 32768) >> 16);
						dstPtr->G = ClampToByte((g * dev + 32768) >> 16);
						dstPtr->B = ClampToByte((b * dev + 32768) >> 16);
						dstPtr->A = 1;
					}

					++dstPtr;
				}
			}

			return dataRect;
		}

		private static byte ClampToByte(double x)
		{
			if (x > 255)
				return 255;
			if (x < 0)
				return 0;
			return (byte)x;
		}
	}
}