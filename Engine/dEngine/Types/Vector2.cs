// Vector2.cs - dEngine
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
using System.Linq;
using System.Runtime.CompilerServices;
using dEngine.Utility;

using SharpDX;

namespace dEngine
{
	/// <summary>
	/// A vector with two components.
	/// </summary>
	public struct Vector2 : IDataType, IEquatable<Vector2>
	{
		/// <summary>Shorthand for Vector2.new(1, 1)</summary>
		public static Vector2 One = new Vector2(1, 1);

		/// <summary>Shorthand for Vector2.new(0, 0)</summary>
		public static Vector2 Zero = new Vector2(0, 0);

		/// <summary>
		/// X component of the vector.
		/// </summary>
		[InstMember(1)] public float x;

		/// <summary>
		/// Y component of the vector.
		/// </summary>
		[InstMember(2)] public float y;

		/// <summary>
		/// X component of the vector.
		/// </summary>
		public float X => x;

		/// <summary>
		/// Y component of the vector.
		/// </summary>
		public float Y => y;

		/// <summary>
		/// Gets the length of this vector.
		/// </summary>
		public float magnitude
		{
			get
			{
				float d;
				Dot(ref this, ref this, out d);
				return (float)Math.Sqrt(d);
			}
		}

		/// <summary>
		/// Gets a copy of this vector with a magnitude of 1.
		/// </summary>
		public Vector2 unit => this / magnitude;

		/// <summary>
		/// Shorthand for Vector2.new(1, 0).
		/// </summary>
		public static Vector2 Right = new Vector2(1, 0);

		/// <summary>
		/// Shorthand for Vector2.new(0, 1).
		/// </summary>
		public static Vector2 Down = new Vector2(0, 1);

		/// <summary>
		/// Initializes a Vector2 with all components set to value.
		/// </summary>
		public Vector2(float value)
		{
			x = y = value;
		}

		/// <summary>
		/// Initializes a Vector2 with the given x and y components.
		/// </summary>
		public Vector2(float x, float y)
		{
			this.x = x;
			this.y = y;
		}
        
        /// <summary>
        /// Initializes a Vector2 with all components set to value.
        /// </summary>
        public static Vector2 @new(float value)
        {
            return new Vector2(value);
        }

        /// <summary>
        /// Initializes a Vector2 with the given x and y components.
        /// </summary>
        public static Vector2 @new(float x, float y)
        {
            return new Vector2(x, y);
        }

        /// <summary>
        /// Returns true if the Vector2 does not contain NaN/Infinite values.
        /// </summary>
        public bool validate()
		{
			return !float.IsNaN(x) && !float.IsNaN(y) &&
				   !float.IsInfinity(x) && !float.IsInfinity(y);
		}

		/// <summary>
		/// Returns an inverted vector.
		/// </summary>
		public Vector2 inverse()
		{
			return -this;
		}

		/// <summary>
		/// Returns a vector lerped towards target.
		/// </summary>
		public Vector2 lerp(Vector2 target, float delta)
		{
			Vector2 v;
			Lerp(ref this, ref target, ref delta, out v);
			return v;
		}

		/// <summary>
		/// Returns the dot product of two vectors.
		/// </summary>
		public float dot(Vector2 other)
		{
			float d;
			Dot(ref this, ref other, out d);
			return d;
		}

		/// <summary>
		/// Returns the distance between two vectors.
		/// </summary>
		public float distance(Vector2 other)
		{
			float d;
			Distance(ref this, ref other, out d);
			return d;
		}

		/// <summary>
		/// Returns an inverted vector.
		/// </summary>
		public static Vector2 inverse(Vector2 v3)
		{
			return -v3;
		}

		/// <summary>
		/// Returns a copy of the vector with its components rounded.
		/// </summary>
		public Vector2 round()
		{
			return new Vector2(Mathf.Round(x), Mathf.Round(y));
		}

		/// <summary>
		/// Returns a vector lerped towards target.
		/// </summary>
		internal static void Lerp(ref Vector2 from, ref Vector2 target, ref float delta, out Vector2 result)
		{
			var d = Mathf.Clamp(delta, 0, 1);
			result = new Vector2(from.x + (target.x - from.x) * delta, from.y + (target.y - from.y) * delta);
		}

		/// <summary>
		/// Returns the dot product of two vectors.
		/// </summary>
		internal static void Dot(ref Vector2 lhs, ref Vector2 rhs, out float result)
		{
			result = (lhs.X * rhs.X + lhs.Y * rhs.Y);
		}

		/// <summary>
		/// Returns the distance between two vectors.
		/// </summary>
		internal static void Distance(ref Vector2 lhs, ref Vector2 rhs, out float result)
		{
			var v = lhs - rhs;
			float d;
			Dot(ref v, ref v, out d);
			result = (float)Math.Sqrt(d);
		}

		/// <summary>
		/// Returns a string representation of the vector.
		/// </summary>
		public override string ToString()
		{
			return $"{x}, {y}";
		}

		/// <summary>
		/// Determines if two vectors are equal.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(Vector2 other)
		{
			return MathUtil.NearEqual(x, other.x) && MathUtil.NearEqual(y, other.y);
		}

		/// <inheritdoc />
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override bool Equals(object obj)
		{
			var flag = false;
			if (obj is Vector2)
				flag = Equals((Vector2)obj);
			return flag;
		}

		/// <inheritdoc />
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = x.GetHashCode();
				hashCode = (hashCode * 397) ^ y.GetHashCode();
				return hashCode;
			}
		}

		#region Operators

		/// <summary />
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(Vector2 left, Vector2 right)
		{
			return left.Equals(right);
		}

		/// <summary />
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(Vector2 left, Vector2 right)
		{
			return !left.Equals(right);
		}

		/// <summary />
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator >(Vector2 left, Vector2 right)
		{
			return left.X > right.X && left.Y > right.Y;
		}

		/// <summary />
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator <(Vector2 left, Vector2 right)
		{
			return left.X < right.X && left.Y < right.Y;
		}
        
        /// <summary>
        /// Converts a string to a vector.
        /// </summary>
        /// <param name="s"></param>
        public static explicit operator Vector2(string s)
        {
            var vals = s.Split(',').Select(float.Parse).ToArray();
            return new Vector2(vals[0], vals[1]);
        }

        /// <summary>
        /// Converts SharpDX.Vector2 to dEngine.Vector2
        /// </summary>
        /// <param name="v">A SharpDX Vector2</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator Vector2(SharpDX.Vector2 v)
		{
			return new Vector2(v.X, v.Y);
		}

		/// <summary>
		/// Converts dEngine.Vector2 to SharpDX.Vector2
		/// </summary>
		/// <param name="v">A dEngine Vector2</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator SharpDX.Vector2(Vector2 v)
		{
			return new SharpDX.Vector2(v.x, v.y);
		}

		/// <summary>
		/// Converts SharpDX.Point to dEngine.Vector2
		/// </summary>
		/// <param name="v">A SharpDX Vector2</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator Vector2(Point v)
		{
			return new Vector2(v.X, v.Y);
		}

		/// <summary>
		/// Converts dEngine.Vector2 to SharpDX.Point
		/// </summary>
		/// <param name="v">A dEngine Vector2</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator Point(Vector2 v)
		{
			return new Point((int)v.x, (int)v.y);
		}

		/// <summary>
		/// Adds two vectors.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2 operator +(Vector2 lhs, Vector2 rhs)
		{
			return new Vector2(lhs.x + rhs.x, lhs.y + rhs.y);
		}

		/// <summary>
		/// Subtracts two vectors.
		/// </summary>
		/// <returns>The sum of the vectors.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2 operator -(Vector2 lhs, Vector2 rhs)
		{
			return new Vector2(lhs.x - rhs.x, lhs.y - rhs.y);
		}

		/// <summary>
		/// Muliplies two vectors.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2 operator *(Vector2 lhs, Vector2 rhs)
		{
			return new Vector2(lhs.x * rhs.x, lhs.y * rhs.y);
		}

		/// <summary>
		/// Returns a negated vector.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2 operator -(Vector2 v)
		{
			return new Vector2(-v.x, -v.y);
		}

		/// <summary>
		/// Returns a vector multiplied by number.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2 operator *(Vector2 v, float n)
		{
			return new Vector2(v.x * n, v.y * n);
		}

		/// <summary>
		/// Returns a vector multiplied by number.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2 operator *(float number, Vector2 vector)
		{
			return vector * number;
		}

		/// <summary>
		/// Adds num to each component of vector.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2 operator +(Vector2 v, float n)
		{
			return new Vector2(v.x + n, v.y + n);
		}

		/// <summary>
		/// Subtracts num from each component of vector.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2 operator -(Vector2 v, float n)
		{
			return new Vector2(v.x - n, v.y - n);
		}

		/// <summary>
		/// Divides each component of vector by num.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2 operator /(Vector2 v, float n)
		{
			return new Vector2(v.x / n, v.y / n);
		}

		#endregion

		/// <summary>
		/// Returns a vector containing the smallest components of the two vectors.
		/// </summary>
		public static void Min(ref Vector2 left, ref Vector2 right, out Vector2 result)
		{
			result = new Vector2(left.X < right.X ? left.X : right.X,
				left.Y < right.Y ? left.Y : right.Y);
		}

		/// <summary>
		/// Returns a vector containing the biggest components of the two vectors.
		/// </summary>
		public static void Max(ref Vector2 left, ref Vector2 right, out Vector2 result)
		{
			result = new Vector2(left.X > right.X ? left.X : right.X,
				left.Y > right.Y ? left.Y : right.Y);
		}

		/// <summary>
		/// Returns a vector containing the smallest components of the two vectors.
		/// </summary>
		public static Vector2 Min(Vector2 left, Vector2 right)
		{
			Vector2 result;
			Min(ref left, ref right, out result);
			return result;
		}

		/// <summary>
		/// Returns a vector containing the biggest components of the two vectors.
		/// </summary>
		public static Vector2 Max(Vector2 left, Vector2 right)
		{
			Vector2 result;
			Max(ref left, ref right, out result);
			return result;
		}

        /// <summary/>
        public void Load(BinaryReader reader)
        {
            x = reader.ReadSingle();
            y = reader.ReadSingle();
        }

        /// <summary/>
        public void Save(BinaryWriter writer)
        {
            writer.Write(x);
            writer.Write(y);
	    }

	    internal static void Multiply(ref Vector2 left, float scalar, out Vector2 result)
	    {
            result = new Vector2(left.x * scalar, left.y * scalar);
	    }
	}
}