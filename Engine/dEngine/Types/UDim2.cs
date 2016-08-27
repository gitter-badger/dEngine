// UDim2.cs - dEngine
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
	/// A type representing 2D universal dimensions.
	/// </summary>
	/// <remarks>
	/// UDim2 has two vectors, Scale is relative to the parent element's size and Absolute is absolute.
	/// A size of UDim2(0.5, 0.5, 0, 0) would be half the width and half the height of the parent.
	/// The final sum of UDim2 is (Scale * Parent's Size + Absolute).
	/// </remarks>
	public struct UDim2 : IDataType, IEquatable<UDim2>
	{
		[InstMember(1)] private float _scaleX;
		[InstMember(2)] private float _scaleY;
		[InstMember(3)] private int _offsetX;
		[InstMember(4)] private int _offsetY;

		/// <summary>
		/// Shorthand for UDim2(0, 0, 0, 0).
		/// </summary>
		public static UDim2 Zero = new UDim2();

		/// <summary>
		/// Shorthand for UDim2(1, 0, 1, 0).
		/// </summary>
		public static UDim2 ScaleOne = new UDim2(1, 0, 1, 0);

		/// <summary>
		/// A 2D unit which scales the parent's size.
		/// </summary>
		public Vector2 Scale => new Vector2(_scaleX, _scaleY);

		/// <summary>
		/// A 2D unit which is added to the scale.
		/// </summary>
		public Vector2 Absolute => new Vector2(_offsetX, _offsetY);

		/// <summary>
		/// Creates a UDim2 with scale and offset values.
		/// </summary>
		public UDim2(float scaleX, int offsetX, float scaleY, int offsetY)
		{
			_scaleX = scaleX;
			_scaleY = scaleY;
			_offsetX = offsetX;
			_offsetY = offsetY;
		}

		/// <summary>
		/// Creates a UDim2 with scale and offset values.
		/// </summary>
		public UDim2(float scaleX, float offsetX, float scaleY, float offsetY)
		{
			_scaleX = scaleX;
			_scaleY = scaleY;
			_offsetX = (int)offsetX;
			_offsetY = (int)offsetY;
		}

		/// <summary>
		/// Creates a UDim2 with scale and offset vectors.
		/// </summary>
		public UDim2(Vector2 scale, Vector2 offset)
		{
			_scaleX = scale.x;
			_scaleY = scale.y;
			_offsetX = (int)offset.x;
			_offsetY = (int)offset.y;
		}

		/// <summary>
		/// Creates a UDim2 with scale and offset values.
		/// </summary>
		public static UDim2 @new(float scaleX, int absoluteX, float scaleY, int absoluteY)
		{
			return new UDim2(scaleX, absoluteX, scaleY, absoluteY);
		}

		/// <summary>
		/// Creates a UDim2 with scale and offset vectors.
		/// </summary>
		public static UDim2 @new(Vector2 scale, Vector2 absolute)
		{
			return new UDim2(scale, absolute);
		}

		/// <summary>
		/// Returns the absolute value of this UDim2.
		/// </summary>
		public Vector2 toAbsolute(Vector2 parentSize)
		{
			return Scale * parentSize + Absolute;
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"{_scaleX}, {_offsetX}, {_scaleY}, {_offsetY}";
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = _scaleX.GetHashCode();
				hashCode = (hashCode * 397) ^ _scaleY.GetHashCode();
				hashCode = (hashCode * 397) ^ _offsetX;
				hashCode = (hashCode * 397) ^ _offsetY;
				return hashCode;
			}
		}

		/// <summary />
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(UDim2 other)
		{
			return _scaleX.Equals(other._scaleX) && _scaleY.Equals(other._scaleY) && _offsetX == other._offsetX &&
				   _offsetY == other._offsetY;
		}

		/// <inheritdoc />
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is UDim2 && Equals((UDim2)obj);
		}

		/// <summary />
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(UDim2 left, UDim2 right)
		{
			return left.Equals(right);
		}

		/// <summary />
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(UDim2 left, UDim2 right)
		{
			return !left.Equals(right);
		}

		/// <summary />
		public static UDim2 operator +(UDim2 left, UDim2 right)
		{
			return new UDim2(left.Scale + right.Scale, left.Absolute + right.Absolute);
		}

		/// <summary />
		public static UDim2 operator -(UDim2 left, UDim2 right)
		{
			return new UDim2(left.Scale - right.Scale, left.Absolute - right.Absolute);
		}

		/// <summary />
		public static UDim2 operator *(UDim2 left, UDim2 right)
		{
			return new UDim2(left.Scale * right.Scale, left.Absolute * right.Absolute);
		}

	    public void Load(BinaryReader reader)
	    {
	        _scaleX = reader.ReadSingle();
            _scaleY = reader.ReadSingle();
            _offsetX = reader.ReadInt32();
            _offsetY = reader.ReadInt32();
        }

	    public void Save(BinaryWriter writer)
	    {
            writer.Write(_scaleX);
            writer.Write(_scaleY);
            writer.Write(_offsetX);
            writer.Write(_offsetY);
        }
	}
}