// VisualC.cs - dEngine
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
using System.Runtime.InteropServices;

namespace dEngine.Utility.Native
{
	internal static class VisualC
	{
		[DllImport("msvcrt.dll", EntryPoint = "memcpy", CharSet = CharSet.Ansi,
			CallingConvention = CallingConvention.Cdecl)]
		public static extern int CopyMemory(IntPtr destination, IntPtr source, int length);

		[DllImport("msvcrt.dll", EntryPoint = "memcmp", CallingConvention = CallingConvention.Cdecl)]
		public static extern int CompareMemory(char[] b1, char[] b2, long count);

        [DllImport("msvcrt.dll", EntryPoint = "memcmp", CallingConvention = CallingConvention.Cdecl)]
        public static extern int CompareMemory(byte[] b1, char[] b2, long count);

        [DllImport("msvcrt.dll", EntryPoint = "memcmp", CallingConvention = CallingConvention.Cdecl)]
		public static extern int CompareMemory(byte[] b1, byte[] b2, long count);

		[DllImport("msvcrt.dll", EntryPoint = "sscanf", CharSet = CharSet.Ansi,
			CallingConvention = CallingConvention.Cdecl)]
		public static extern int ScanString(string buffer, string format, out int arg0, out int arg1);
	}
}