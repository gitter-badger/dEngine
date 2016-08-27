// TerrainVertex.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

namespace dEngine.Graphics.Structs
{
	internal struct TerrainVertex
	{
		public Vector3 Position;
		public Vector3 Normal;
		public Vector4 Material0;
		public Vector4 Material1;

		public TerrainVertex(ref Vector3 position, ref Vector3 normal)
		{
			Position = position;
			Normal = normal;
			Material0 = new Vector4(0, 0, 0, 0);
			Material1 = new Vector4(0, 0, 0, 0);
		}

		public const int Stride = (4 * 3) + (4 * 3) + (4 * 4) + (4 * 4);

		public override string ToString()
		{
			return Position.ToString();
		}
	}
}