// PointLightData.cs - dEngine
// Copyright (C) 2015-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using SharpDX;

namespace dEngine.Graphics.Structs
{
	internal struct PointLightData
	{
		public Vector3 Position;
		public float Range;
		public Color3 Colour;
		public float Falloff;

		public const int Stride = 32;
	}
}