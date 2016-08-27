// Vector3int16.cs - dEngine
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
	/// A variant of <see cref="Vector3" /> which contains 16-bit integers intead of floats.
	/// </summary>
	public struct Vector3int16 : IEquatable<Vector3int16>, IDataType
	{
		/// <summary>The X component.</summary>
		public short x;

		/// <summary>The Y component.</summary>
		public short y;

		/// <summary>The Z component.</summary>
		public short z;

		/// <summary />
		public Vector3int16(short x, short y, short z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		/// <summary />
		public Vector3int16(int x, int y, int z)
		{
			this.x = (short)x;
			this.y = (short)y;
			this.z = (short)z;
		}

		/// <summary>
		/// Shorthand for Vector3int16.new(1, 0, 0)
		/// </summary>
		public static Vector3int16 Right = new Vector3int16(1, 0, 0);

		/// <summary>
		/// Shorthand for Vector3int16.new(0, 1, 0)
		/// </summary>
		public static Vector3int16 Up = new Vector3int16(0, 1, 0);

		/// <summary>
		/// Shorthand for Vector3int16.new(0, 0, 1)
		/// </summary>
		public static Vector3int16 Backward = new Vector3int16(0, 0, 1);

		/// <summary>
		/// Determines if two <see cref="Vector3int16" /> are equal.
		/// </summary>
		public bool Equals(Vector3int16 other)
		{
			return x == other.x && y == other.y && z == other.z;
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is Vector3int16 && Equals((Vector3int16)obj);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = x.GetHashCode();
				hashCode = (hashCode * 397) ^ y.GetHashCode();
				hashCode = (hashCode * 397) ^ z.GetHashCode();
				return hashCode;
			}
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"{x}, {y}, {z}";
		}

		/// <summary />
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(Vector3int16 left, Vector3int16 right)
		{
			return left.Equals(right);
		}

		/// <summary />
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(Vector3int16 left, Vector3int16 right)
		{
			return !left.Equals(right);
		}

		/// <summary />
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator >(Vector3int16 left, Vector3int16 right)
		{
			return left.x > right.x && left.y > right.y && left.z > right.z;
		}

		/// <summary />
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator >=(Vector3int16 left, Vector3int16 right)
		{
			return left.x >= right.x && left.y >= right.y && left.z >= right.z;
		}

		/// <summary />
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator <(Vector3int16 left, Vector3int16 right)
		{
			return left.x < right.x && left.y < right.y && left.z < right.z;
		}

		/// <summary />
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator <=(Vector3int16 left, Vector3int16 right)
		{
			return left.x <= right.x && left.y <= right.y && left.z <= right.z;
		}

		/// <summary>
		/// Adds together two vectors.
		/// </summary>
		public static Vector3int16 operator +(Vector3int16 left, Vector3int16 right)
		{
			return new Vector3int16(left.x + right.x, left.y + right.y, left.z + right.z);
		}

		/// <summary>
		/// Subtracts two vectors.
		/// </summary>
		public static Vector3int16 operator -(Vector3int16 left, Vector3int16 right)
		{
			return new Vector3int16((short)(left.x - right.x), (short)(left.y - right.y), (short)(left.z - right.z));
		}

		/// <summary>
		/// Multiplies two vectors.
		/// </summary>
		public static Vector3int16 operator *(Vector3int16 left, Vector3int16 right)
		{
			return new Vector3int16((short)(left.x * right.x), (short)(left.y * right.y), (short)(left.z * right.z));
		}

		/// <summary>
		/// Divides two vectors.
		/// </summary>
		public static Vector3int16 operator /(Vector3int16 left, Vector3int16 right)
		{
			return new Vector3int16((short)(left.x / right.x), (short)(left.y / right.y), (short)(left.z / right.z));
		}

		/// <summary>
		/// Multiplies two vectors.
		/// </summary>
		public static Vector3int16 operator *(Vector3int16 vec, float scale)
		{
			return new Vector3int16((short)(vec.x * scale), (short)(vec.y * scale), (short)(vec.z * scale));
		}

		/// <summary>
		/// Converts a <see cref="Vector3int16" /> to a <see cref="Vector3" />.
		/// </summary>
		public static explicit operator Vector3(Vector3int16 vector)
		{
			return new Vector3(vector.x, vector.y, vector.z);
		}

		/// <summary>
		/// Converts a <see cref="Vector3" /> to a <see cref="Vector3int16" />.
		/// </summary>
		public static explicit operator Vector3int16(Vector3 vector)
		{
			return new Vector3int16((short)vector.x, (short)vector.y, (short)vector.z);
		}

	    public void Load(BinaryReader reader)
	    {
	        x = reader.ReadInt16();
            y = reader.ReadInt16();
            z = reader.ReadInt16();
        }

	    public void Save(BinaryWriter writer)
	    {
            writer.Write(x);
            writer.Write(y);
            writer.Write(z);
        }
	}
}