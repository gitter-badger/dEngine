// Vector3.cs - dEngine
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
    /// A vector with three components.
    /// </summary>
    public struct Vector3 : IDataType, IEquatable<Vector3>
    {
        /// <summary>Shorthand for Vector3.new(0, 1, 0)</summary>
        public static readonly Vector3 Up = new Vector3(0, 1, 0);

        /// <summary>Shorthand for Vector3.new(0, -1, 0)</summary>
        public static readonly Vector3 Down = new Vector3(0, -1, 0);

        /// <summary>Shorthand for Vector3.new(0, 0, 1)</summary>
        public static readonly Vector3 Backward = new Vector3(0, 0, 1);

        /// <summary>Shorthand for Vector3.new(0, 0, -1)</summary>
        public static readonly Vector3 Forward = new Vector3(0, 0, -1);

        /// <summary>Shorthand for Vector3.new(-1, 0, 0)</summary>
        public static readonly Vector3 Left = new Vector3(-1, 0, 0);

        /// <summary>Shorthand for Vector3.new(1, 0, 0)</summary>
        public static readonly Vector3 Right = new Vector3(1, 0, 0);

        /// <summary>Shorthand for Vector3.new(0, 0, 0)</summary>
        public static readonly Vector3 Zero = new Vector3(0, 0, 0);

        /// <summary>Shorthand for Vector3.new(1, 1, 1)</summary>
        public static readonly Vector3 One = new Vector3(1, 1, 1);

        /// <summary>
        /// X component of the vector.
        /// </summary>
        [InstMember(1)] public float x;

        /// <summary>
        /// Y component of the vector.
        /// </summary>
        [InstMember(2)] public float y;

        /// <summary>
        /// Z component of the vector.
        /// </summary>
        [InstMember(3)] public float z;

        /// <summary>
        /// X component of the vector.
        /// </summary>
        public float X => x;

        /// <summary>
        /// Y component of the vector.
        /// </summary>
        public float Y => y;

        /// <summary>
        /// Z component of the vector.
        /// </summary>
        public float Z => z;

        /// <summary>
        /// Gets the length of this vector.
        /// </summary>
        public float magnitude => Mathf.Sqrt(x * x + y * y + z * z);

        /// <summary>
        /// Gets the length squared of this vector.
        /// </summary>
        public float mag2 => x * x + y * y + z * z;

        /// <summary>
        /// Gets a normalized version of the vector.
        /// </summary>
        public Vector3 unit => this / magnitude;

        /// <summary>
        /// Returns a Vector2 of the x and y components.
        /// </summary>
        public Vector2 xy => new Vector2(X, Y);

        /// <summary>
        /// Returns a Vector2 of the x and y components.
        /// </summary>
        public Vector2 XY => new Vector2(X, Y);

        /// <summary>
        /// Creates a new Vector3 with each component set to value.
        /// </summary>
        public Vector3(float value)
        {
            x = y = z = value;
        }

        /// <summary>
        /// Creates a new Vector3 with the given component values.
        /// </summary>
        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        /// <summary>
        /// Creates a new Vector3 from a Vector2 and Z value.
        /// </summary>
        public Vector3(Vector2 vector, float z)
        {
            x = vector.x;
            y = vector.y;
            this.z = z;
        }

        /// <summary />
        public static Vector3 @new(float value)
        {
            return new Vector3(value);
        }

        /// <summary />
        public static Vector3 @new(float x, float y, float z)
        {
            return new Vector3(x, y, z);
        }

        /// <summary />
        public static Vector3 @new(Vector2 vector, float z)
        {
            return new Vector3(vector, z);
        }

        internal void Set(float xx, float yy, float zz)
        {
            x = xx;
            y = yy;
            z = zz;
        }

        internal void Normalize()
        {
            Normalize(ref this, out this);
        }

        /// <summary>
        /// Creates a unit Vector3 from a NormalId.
        /// </summary>
        public static Vector3 FromNormalId(NormalId normalId)
        {
            switch (normalId)
            {
                case NormalId.Right:
                    return Right;
                case NormalId.Top:
                    return Up;
                case NormalId.Back:
                    return Backward;
                case NormalId.Left:
                    return Left;
                case NormalId.Bottom:
                    return Down;
                case NormalId.Front:
                    return Forward;
                default:
                    throw new ArgumentOutOfRangeException(nameof(normalId));
            }
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

        #region Conversions

#pragma warning disable 1591
        /// <summary>
        /// Converts a string to a vector.
        /// </summary>
        /// <param name="s"></param>
        public static explicit operator Vector3(string s)
        {
            var vals = s.Split(',').Select(float.Parse).ToArray();
            return new Vector3(vals[0], vals[1], vals[2]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Vector3(Assimp.Vector3D v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Vector3(System.Numerics.Vector3 v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Vector3(SharpDX.Vector3 v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator SharpDX.Vector3(Vector3 v)
        {
            return new SharpDX.Vector3(v.x, v.y, v.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Vector3(BulletSharp.Math.Vector3 v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator BulletSharp.Math.Vector3(Vector3 v)
        {
            return new BulletSharp.Math.Vector3(v.x, v.y, v.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator CSCore.Utils.Vector3(Vector3 v)
        {
            return new CSCore.Utils.Vector3(v.x, v.y, v.z);
        }

#pragma warning restore 1591

        #endregion

        /// <summary>
        /// Returns true if the Vector3 does not contain NaN/Infinite values.
        /// </summary>
        public bool validate()
        {
            return !float.IsNaN(x) && !float.IsNaN(y) && !float.IsNaN(z) &&
                   !float.IsInfinity(x) && !float.IsInfinity(y) && !float.IsInfinity(z);
        }

        /// <summary>
        /// Returns an inverted vector.
        /// </summary>
        public Vector3 inverse()
        {
            return -this;
        }

        /// <summary>
        /// Returns a vector lerped towards target.
        /// </summary>
        public Vector3 lerp(Vector3 target, float delta)
        {
            Vector3 l;
            Lerp(ref this, ref target, ref delta, out l);
            return l;
        }

        /// <summary>
        /// Returns the dot product of two vectors.
        /// </summary>
        public float dot(Vector3 other)
        {
            float d;
            Dot(ref this, ref other, out d);
            return d;
        }

        /// <summary>
        /// Returns the cross product of two vectors.
        /// </summary>
        public Vector3 cross(Vector3 other)
        {
            Vector3 c;
            Cross(ref this, ref other, out c);
            return c;
        }

        /// <summary>
        /// Returns the distance between two vectors.
        /// </summary>
        public float distance(Vector3 other)
        {
            float d;
            Distance(ref this, ref other, out d);
            return d;
        }

        /// <summary>
        /// Inverts a vector.
        /// </summary>
        public static void Inverse(ref Vector3 v3, out Vector3 result)
        {
            result = -v3;
        }

        /// <summary>
        /// Lerps a vector.
        /// </summary>
        public static void Lerp(ref Vector3 from, ref Vector3 target, ref float delta, out Vector3 result)
        {
            result = from * (1f - delta) + target * delta;
        }


        /// <summary>
        /// Returns the dot product of two vectors.
        /// </summary>
        public static void Dot(ref Vector3 lhs, ref Vector3 rhs, out float result)
        {
            result = (float)(lhs.x * (double)rhs.x + lhs.y * (double)rhs.y + lhs.z * (double)rhs.z);
        }

        /// <summary>
        /// Returns the distance between two vectors.
        /// </summary>
        public static void Distance(ref Vector3 lhs, ref Vector3 rhs, out float result)
        {
            var v = lhs - rhs;
            float d;
            Dot(ref v, ref v, out d);
            result = (float)Math.Sqrt(d);
        }

        internal static void Multiply(ref Vector3 lhs, float n, out Vector3 result)
        {
            result = lhs * n;
        }

        internal static void Cross(ref Vector3 lhs, ref Vector3 rhs, out Vector3 result)
        {
            result = new Vector3((float)(lhs.y * (double)rhs.z - lhs.z * (double)rhs.y),
                (float)(lhs.z * (double)rhs.x - lhs.x * (double)rhs.z),
                (float)(lhs.x * (double)rhs.y - lhs.y * (double)rhs.x));
        }

        internal static void Add(ref Vector3 left, ref Vector3 right, out Vector3 result)
        {
            result = new Vector3(left.x + right.x, left.y + right.y, left.z + right.z);
        }

        internal static void Subtract(ref Vector3 left, ref Vector3 right, out Vector3 result)
        {
            result = new Vector3(left.x - right.x, left.y - right.y, left.z - right.z);
        }

        internal static void Subtract(ref Vector3 left, ref BulletSharp.Math.Vector3 right, out Vector3 result)
        {
            result = new Vector3(left.x - right.X, left.y - right.Y, left.z - right.Z);
        }

        internal static void Subtract(ref BulletSharp.Math.Vector3 left, ref BulletSharp.Math.Vector3 right, out Vector3 result)
        {
            result = new Vector3(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
        }

        internal static void Normalize(ref Vector3 left, out Vector3 result)
        {
            if (Math.Abs(left.mag2 - 0) < float.Epsilon)
            {
                result = left;
                return;
            }

            float mag;
            Dot(ref left, ref left, out mag);
            mag = (float)Math.Sqrt(mag);
            Divide(ref left, ref mag, out result);
        }

        internal static void Divide(ref Vector3 v, ref float n, out Vector3 result)
        {
            result = new Vector3(v.x / n, v.y / n, v.z / n);
        }

        #region Operators

        /// <summary />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Vector3 other)
        {
            return MathUtil.NearEqual(x, other.x) && MathUtil.NearEqual(y, other.y) && MathUtil.NearEqual(z, other.z);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Vector3 && Equals((Vector3)obj);
        }

        /// <summary />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Vector3 left, Vector3 right)
        {
            return left.Equals(right);
        }

        /// <summary />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Vector3 left, Vector3 right)
        {
            return !left.Equals(right);
        }

        /// <summary />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Vector3 left, Vector3 right)
        {
            return left.x > right.x && left.y > right.y && left.z > right.z;
        }

        /// <summary />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Vector3 left, Vector3 right)
        {
            return left.x >= right.x && left.y >= right.y && left.z >= right.z;
        }

        /// <summary />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Vector3 left, Vector3 right)
        {
            return left.x < right.x && left.y < right.y && left.z < right.z;
        }

        /// <summary />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Vector3 left, Vector3 right)
        {
            return left.x <= right.x && left.y <= right.y && left.z <= right.z;
        }

        /// <summary>
        /// Adds two vectors.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator +(Vector3 left, Vector3 right)
        {
            Vector3 result;
            Add(ref left, ref right, out result);
            return result;
        }

        /// <summary>
        /// Subtracts two vectors.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator -(Vector3 left, Vector3 right)
        {
            Vector3 result;
            Subtract(ref left, ref right, out result);
            return result;
        }

        /// <summary>
        /// Muliplies two vectors.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator *(Vector3 lhs, Vector3 rhs)
        {
            return new Vector3(lhs.x * rhs.x, lhs.y * rhs.y, lhs.z * rhs.z);
        }

        /// <summary>
        /// Returns a negated vector.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator -(Vector3 v)
        {
            return new Vector3(-v.x, -v.y, -v.z);
        }

        /// <summary>
        /// Returns a vector multiplied by number.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator *(Vector3 v, float n)
        {
            return new Vector3(v.x * n, v.y * n, v.z * n);
        }

        /// <summary>
        /// Returns a vector multiplied by number.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator *(float number, Vector3 vector)
        {
            return vector * number;
        }

        /// <summary>
        /// Adds num to each component of vector.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator +(Vector3 v, float n)
        {
            return new Vector3(v.x + n, v.y + n, v.z + n);
        }

        /// <summary>
        /// Subtracts num from each component of vector.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator -(Vector3 v, float n)
        {
            return new Vector3(v.x - n, v.y - n, v.z - n);
        }

        /// <summary>
        /// Divides each component of vector by num.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator /(Vector3 v, float n)
        {
            Vector3 result;
            Divide(ref v, ref n, out result);
            return result;
        }

        #endregion

        internal static void TransformCoordinate(ref Vector3 coordinate, ref SharpDX.Matrix transform, out Vector3 result)
        {
            var x = (coordinate.x * transform.M11 + coordinate.y * transform.M21 + coordinate.z * transform.M31) + transform.M41;
            var y = (coordinate.x * transform.M12 + coordinate.y * transform.M22 + coordinate.z * transform.M32) + transform.M42;
            var z = (coordinate.x * transform.M13 + coordinate.y * transform.M23 + coordinate.z * transform.M33) + transform.M43;
            var w = (1.0f / (coordinate.x * transform.M14 + coordinate.y * transform.M24 + coordinate.z * transform.M34 + transform.M44));
            result = new Vector3(x * w, y * w, z * w);
        }

        internal static void TransformCoordinate(ref Vector3 coordinate, ref Matrix3 transform, out Vector3 result)
        {
            result = new Vector3(
                (float)
                    (coordinate.x * (double)transform.M11 + coordinate.y * (double)transform.M21 +
                     coordinate.z * (double)transform.M31),
                (float)
                    (coordinate.x * (double)transform.M12 + coordinate.y * (double)transform.M22 +
                     coordinate.z * (double)transform.M32),
                (float)
                    (coordinate.x * (double)transform.M13 + coordinate.y * (double)transform.M23 +
                     coordinate.z * (double)transform.M33));
        }

        /// <summary>
        /// Returns the vector components as an array of floats.
        /// </summary>
        /// <returns></returns>
        public float[] ToArray()
        {
            return new[] {x, y, z};
        }

        /// <summary>
        /// Returns a vector containing the smallest components of the two vectors.
        /// </summary>
        public static void Min(ref Vector3 left, ref Vector3 right, out Vector3 result)
        {
            result = new Vector3(left.x < right.x ? left.x : right.x,
                left.y < right.y ? left.y : right.y,
                left.z < right.z ? left.z : right.z);
        }

        /// <summary>
        /// Returns a vector containing the biggest components of the two vectors.
        /// </summary>
        public static void Max(ref Vector3 left, ref Vector3 right, out Vector3 result)
        {
            result = new Vector3(left.x > right.x ? left.x : right.x,
                left.y > right.y ? left.y : right.y,
                left.z > right.z ? left.z : right.z);
        }

        /// <summary>
        /// Returns the max component of the given vector.
        /// </summary>
        public static float Max(ref Vector3 vector)
        {
            return Math.Max(vector.z, Math.Max(vector.x, vector.y));
        }

        /// <summary>
        /// Returns the largest component of the vector.
        /// </summary>
        public float max()
        {
            var vec = this;
            return Max(ref vec);
        }
        
        internal static void Unproject(float vectorX, float vectorY, float z, float x, float y, float width, float height, float minZ, float maxZ, ref Matrix worldViewProjection, out Vector3 result)
        {
            Matrix result1;
            Matrix.Invert(ref worldViewProjection, out result1);
            var coordinate = new Vector3(
            ((vectorX - x) / width * 2.0f - 1.0f),
            -((vectorY - y) / height * 2.0f - 1.0f),
           ((z - minZ) / (maxZ - minZ)));
            TransformCoordinate(ref coordinate, ref result1, out result);
        }

        /// <summary/>
        public void Load(BinaryReader reader)
        {
            x = reader.ReadSingle();
            y = reader.ReadSingle();
            z = reader.ReadSingle();
        }

        /// <summary/>
        public void Save(BinaryWriter writer)
        {
            writer.Write(x);
            writer.Write(y);
            writer.Write(z);
        }
    }
}