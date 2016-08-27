// Vertex2D.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System.Runtime.InteropServices;

namespace dEngine.Graphics.Structs
{
#pragma warning disable 1591
	/// <summary>
	/// Vertex data structure for 2D guis.
	/// </summary>
	public struct Vertex2D
	{
		public Vector2 Position;
		public Vector2 TexCoord;

		public Vertex2D(Vector2 position, Vector2 texCoord)
		{
			Position = position;
			TexCoord = texCoord;
		}

		public static int Stride = Marshal.SizeOf<Vertex2D>();
	}
#pragma warning restore 1591
}