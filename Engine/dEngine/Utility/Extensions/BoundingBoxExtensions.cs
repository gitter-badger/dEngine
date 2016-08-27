// BoundingBoxExtensions.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using SharpDX;

namespace dEngine.Utility.Extensions
{
    internal static class BoundingBoxExtensions
    {
        public static CFrame GetCFrame(this OrientedBoundingBox bb)
        {
            return (CFrame)bb.Transformation;
        }

        public static Vector3 GetdEngineSize(this OrientedBoundingBox bb)
        {
            return (Vector3)bb.Size;
        }

        public static CFrame GetCFrame(this BoundingBox bb)
        {
            return new CFrame((Vector3)(bb.Maximum + bb.Minimum)/2.0f);
        }

        public static Vector3 GetSize(this BoundingBox bb)
        {
            return (Vector3)(bb.Maximum - bb.Minimum);
        }
    }
}