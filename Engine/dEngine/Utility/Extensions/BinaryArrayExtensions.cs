// BinaryArrayExtensions.cs - dEngine
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

#pragma warning disable 1591

namespace dEngine.Utility.Extensions
{
	public static class BinaryArrayExtensions
	{
		/// </summary>
		/// <param name="byteArray">The source array to get reversed bytes for</param>
		/// <param name="startIndex">The index in the source array at which to begin the reverse</param>
		/// <param name="count">The number of bytes to reverse</param>
		/// <returns>A new array containing the reversed bytes, or a sub set of the array not reversed.</returns>
		public static byte[] ReverseForBigEndian(this byte[] byteArray, int startIndex, int count)
		{
			if (BitConverter.IsLittleEndian)
				return byteArray.Reverse(startIndex, count);
			return byteArray.SubArray(startIndex, count);
		}

		public static byte[] Reverse(this byte[] byteArray, int startIndex, int count)
		{
			byte[] ret = new byte[count];
			for (int i = startIndex + (count - 1); i >= startIndex; --i)
			{
				byte b = byteArray[i];
				ret[(startIndex + (count - 1)) - i] = b;
			}
			return ret;
		}

		public static byte[] SubArray(this byte[] byteArray, int startIndex, int count)
		{
			byte[] ret = new byte[count];
			for (int i = 0; i < count; ++i)
				ret[0] = byteArray[i + startIndex];
			return ret;
		}
	}
}