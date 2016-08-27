// Region3int16.cs - dEngine
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
using System.Runtime.CompilerServices;

namespace dEngine
{
	/// <summary>
	/// A variant of <see cref="Region3" /> which contains 16-bit integers intead of floats.
	/// </summary>
	public struct Region3int16 : IDataType, IEquatable<Region3int16>
	{
		/// <summary>
		/// The min vector.
		/// </summary>
		public readonly Vector3int16 Min;

		/// <summary>
		/// The max vector.
		/// </summary>
		public readonly Vector3int16 Max;

		/// <summary />
		public Region3int16(Vector3int16 min, Vector3int16 max)
		{
			if (min >= max)
				throw new InvalidOperationException("Min must be less than Max.");
			Min = min;
			Max = max;
		}

		/// <summary />
		public Region3int16(int minX, int minY, int minZ, int maxX, int maxY, int maxZ)
		{
			Min = new Vector3int16(minX, minY, minZ);
			Max = new Vector3int16(maxX, maxY, maxZ);
			if (Min >= Max)
				throw new InvalidOperationException("Min must be less than Max.");
		}

		/// <summary />
		public static Region3int16 @new(Vector3int16 min, Vector3int16 max)
		{
			return new Region3int16(min, max);
		}

		/// <summary />
		public static Region3int16 @new(int minX, int minY, int minZ, int maxX, int maxY, int maxZ)
		{
			return new Region3int16(minX, minY, minZ, maxX, maxY, maxZ);
		}

		/// <summary />
		public bool Equals(Region3int16 other)
		{
			return Min.Equals(other.Min) && Max.Equals(other.Max);
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is Region3int16 && Equals((Region3int16)obj);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			unchecked
			{
				return (Min.GetHashCode() * 397) ^ Max.GetHashCode();
			}
		}

		/// <summary />
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(Region3int16 left, Region3int16 right)
		{
			return left.Equals(right);
		}

		/// <summary />
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(Region3int16 left, Region3int16 right)
		{
			return !left.Equals(right);
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"{Min.x}, {Min.y}, {Min.z}; {Max.x}, {Max.y}, {Max.z}";
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