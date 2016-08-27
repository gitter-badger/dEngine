// Point.cs - dEngine
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

#pragma warning disable 1591
namespace dEngine.Graphics.Structs
{
    public struct Point
    {
        public Vector3 Position;
        public Colour Colour;

        public static int Stride = Marshal.SizeOf<Point>(); // 12 + 16

        public Point(ref BulletSharp.Math.Vector3 position, ref BulletSharp.Math.Vector3 colour)
        {
            Position = new Vector3(position.X, position.Y, position.Z);
            Colour = new Colour(colour.X, colour.Y, colour.Z);
        }
    }
}