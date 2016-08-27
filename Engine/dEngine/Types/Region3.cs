// Region3.cs - dEngine
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

namespace dEngine
{
	/// <summary>
	/// A Region3 is used to represent a size and a location in 3D space, given two corners.
	/// </summary>
	public struct Region3 : IDataType, IEquatable<Region3>
	{
		/// <summary>
		/// Creates a Region3 out of two Vector3s.
		/// </summary>
		public Region3(Vector3 min, Vector3 max)
		{
			if (min.X > max.X || min.Y > max.Y || min.Z > max.Z)
				throw new InvalidOperationException("Min must be < Max.");

			Min = min;
			Max = max;
		}

		/// <summary>
		/// The minimum vector.
		/// </summary>
		public Vector3 Min { get; }

		/// <summary>
		/// The maximum vector.
		/// </summary>
		public Vector3 Max { get; }

		/// <summary>
		/// Expands the <see cref="Region3" /> so that it is aligned with the voxel grid.
		/// </summary>
		/// <param name="resolution">The resolution of the grid alignment.</param>
		/// <returns>The expanded <see cref="Region3" /></returns>
		public Region3 ExpandToGrid(float resolution)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Tests equality of two <see cref="Region3" /> values.
		/// </summary>
		public bool Equals(Region3 other)
		{
			return Min == other.Min && Max == other.Max;
		}

        /// <summary/>
        public void Load(BinaryReader reader)
	    {
            var min = new Vector3();
            var max = new Vector3();
            min.Load(reader);
            max.Load(reader);
	    }

        /// <summary/>
        public void Save(BinaryWriter writer)
	    {
            Min.Save(writer);
	        Max.Save(writer);
	    }
	}
}