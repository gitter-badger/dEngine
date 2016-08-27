// Vector4.cs - dEngine
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
using dEngine.Utility;
using SharpDX;

#pragma warning disable 1591

namespace dEngine
{
    /// <summary>
    /// A quaternion type.
    /// </summary>
    public struct Vector4 : IDataType, IEquatable<Vector4>
    {
        /// <summary>Shorthand for Vector4.new(1, 0, 0, 0)</summary>
        public static Vector4 UnitX = new Vector4(1, 0, 0, 0);

        /// <summary>Shorthand for Vector4.new(0, 1, 0, 0)</summary>
        public static Vector4 UnitY = new Vector4(0, 1, 0, 0);

        /// <summary>Shorthand for Vector4.new(0, 0, 1, 0)</summary>
        public static Vector4 UnitZ = new Vector4(0, 0, 1, 0);

        /// <summary>Shorthand for Vector4.new(0, 0, 0, 1)</summary>
        public static Vector4 UnitW = new Vector4(0, 0, 0, 1);

        /// <summary>Shorthand for Vector4.new(0, 0, 0, 0)</summary>
        public static Vector4 Zero = new Vector4(0, 0, 0, 0);

        /// <summary>Shorthand for Vector4.new(1, 1, 1, 1)</summary>
        public static Vector4 One = new Vector4(1, 1, 1, 1);

        /// <summary>Shorthand for Vector4.new(0, 0, 0, 1)</summary>
        public static Vector4 Identity = new Vector4(0, 0, 0, 1);

        /// <summary>
        /// The X component.
        /// </summary>
        [InstMember(1)] public float x;

        /// <summary>
        /// The Y component.
        /// </summary>
        [InstMember(2)] public float y;

        /// <summary>
        /// The Z component.
        /// </summary>
        [InstMember(3)] public float z;

        /// <summary>
        /// The W component.
        /// </summary>
        [InstMember(4)] public float w;

        /// <summary>X component of the vector.</summary>
        public float X => x;

        /// <summary>Y component of the vector.</summary>
        public float Y => y;

        /// <summary>Z component of the vector.</summary>
        public float Z => z;

        /// <summary>W component of the vector.</summary>
        public float W => w;

        /// <summary>
        /// Gets the length of this vector.
        /// </summary>
        public float magnitude => Mathf.Sqrt(x*x + y*y + z*z + w*w);

        /// <summary>
        /// Gets the length of this vector.
        /// </summary>
        public float mag2 => x*x + y*y + z*z + w*w;

        /// <summary>
        /// Returns the x, y and z components as a <see cref="Vector3" />.
        /// </summary>
        public Vector3 xyz => new Vector3(x, y, z);

        public Vector4(float value)
        {
            x = y = z = w = value;
        }

        public Vector4(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            w = 1;
        }

        public Vector4(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public Vector4(Vector3 vec, float w)
        {
            x = vec.x;
            y = vec.y;
            z = vec.z;
            this.w = w;
        }

        public Vector4 unit
        {
            get
            {
                var num = 1f/w;
                return this*num;
            }
        }

        /// <summary>
        /// Determines if two vectors are equal.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(Vector4 other)
        {
            float d;
            Dot(ref this, ref other, out d);
            return d > 0.999999f;
        }

        internal static void Multiply(ref Vector4 left, ref Vector4 right, out Vector4 result)
        {
            result = new Vector4(left.w*right.x + left.x*right.w + left.y*right.z - left.z*right.y,
                left.w*right.y + left.y*right.w + left.z*right.x - left.x*right.z,
                left.w*right.z + left.z*right.w + left.x*right.y - left.y*right.x,
                left.w*right.w - left.x*right.x - left.y*right.y - left.z*right.z);
        }

        internal static void Multiply(ref Vector4 value, ref float scale, out Vector4 result)
        {
            result = new Vector4(value.x*scale, value.y*scale, value.z*scale, value.w*scale);
        }

        public static Vector4 operator *(Vector4 left, Vector4 right)
        {
            Vector4 result;
            Multiply(ref left, ref right, out result);
            return result;
        }

        public static Vector4 operator *(Vector4 value, float scale)
        {
            Vector4 result;
            Multiply(ref value, ref scale, out result);
            return result;
        }

        public static Vector3 operator *(Vector4 rotation, Vector3 point)
        {
            var num = rotation.x*2f;
            var num2 = rotation.y*2f;
            var num3 = rotation.z*2f;
            var num4 = rotation.x*num;
            var num5 = rotation.y*num2;
            var num6 = rotation.z*num3;
            var num7 = rotation.x*num2;
            var num8 = rotation.x*num3;
            var num9 = rotation.y*num3;
            var num10 = rotation.w*num;
            var num11 = rotation.w*num2;
            var num12 = rotation.w*num3;
            var x = (1f - (num5 + num6))*point.x + (num7 - num12)*point.y + (num8 + num11)*point.z;
            var y = (num7 + num12)*point.x + (1f - (num4 + num6))*point.y + (num9 - num10)*point.z;
            var z = (num8 - num11)*point.x + (num9 + num10)*point.y + (1f - (num4 + num5))*point.z;
            return new Vector3(x, y, z);
        }

        public static Vector4 operator -(Vector4 rotation)
        {
            var a = rotation.unit2;
            if (MathUtil.IsZero(a))
                return Zero;
            var num = 1f/a;
            var x = -rotation.x*num;
            var y = -rotation.y*num;
            var z = -rotation.z*num;
            var w = rotation.w*num;
            return new Vector4(x, y, z, w);
        }

        /// <summary>
        /// Returns the length of the quaternion squared.
        /// </summary>
        public float unit2 => x*x + y*y + z*z + w*w;

        internal static void RotationMatrix(ref Matrix3 matrix, out Vector4 result)
        {
            var num1 = matrix.M11 + matrix.M22 + matrix.M33;
            if (num1 > 0.0)
            {
                var num2 = (float)Math.Sqrt(num1 + 1.0);
                var num3 = 0.5f/num2;

                result = new Vector4((matrix.M32 - matrix.M23)*num3,
                    (matrix.M13 - matrix.M31)*num3,
                    (matrix.M21 - matrix.M12)*num3,
                    num2*0.5f);
            }
            else if ((matrix.M11 >= matrix.M22) && (matrix.M11 >= matrix.M33))
            {
                var num2 = (float)Math.Sqrt(1.0 + matrix.M11 - matrix.M22 - matrix.M33);
                var num3 = 0.5f/num2;

                result = new Vector4(0.5f*num2,
                    (matrix.M21 + matrix.M12)*num3,
                    (matrix.M31 + matrix.M13)*num3,
                    (matrix.M32 - matrix.M23)*num3);
            }
            else if (matrix.M22 > matrix.M33)
            {
                var num2 = (float)Math.Sqrt(1.0 + matrix.M22 - matrix.M11 - matrix.M33);
                var num3 = 0.5f/num2;
                result = new Vector4((matrix.M12 + matrix.M21)*num3,
                    0.5f*num2,
                    (matrix.M23 + matrix.M32)*num3,
                    (matrix.M13 - matrix.M31)*num3
                );
            }
            else
            {
                var num2 = (float)Math.Sqrt(1.0 + matrix.M33 - matrix.M11 - matrix.M22);
                var num3 = 0.5f/num2;
                result = new Vector4((matrix.M13 + matrix.M31)*num3,
                    (matrix.M23 + matrix.M32)*num3,
                    0.5f*num2,
                    (matrix.M21 - matrix.M12)*num3);
            }
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Vector4 && Equals((Vector4)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = x.GetHashCode();
                hashCode = (hashCode*397) ^ y.GetHashCode();
                hashCode = (hashCode*397) ^ z.GetHashCode();
                hashCode = (hashCode*397) ^ w.GetHashCode();
                return hashCode;
            }
        }

        internal static void Dot(ref Vector4 left, ref Vector4 right, out float result)
        {
            result =
                left.x*right.x + left.y*right.y + left.z*right.z +
                left.w*right.w;
        }

        internal static void Slerp(ref Vector4 startRot, ref Vector4 endRot, float amount,
            out Vector4 resultRot)
        {
            float num1;
            Dot(ref startRot, ref endRot, out num1);
            float num2;
            float num3;
            if (Math.Abs(num1) > 0.999998986721039)
            {
                num2 = 1f - amount;
                num3 = amount*Math.Sign(num1);
            }
            else
            {
                var num4 = (float)Math.Acos(Math.Abs(num1));
                var num5 = (float)(1.0/Math.Sin(num4));
                num2 = (float)Math.Sin((1.0 - amount)*num4)*num5;
                num3 = (float)Math.Sin(amount*num4)*num5*Math.Sign(num1);
            }
            resultRot = new Vector4(num2*startRot.x + num3*endRot.x,
                num2*startRot.y + num3*endRot.y,
                num2*startRot.z + num3*endRot.z,
                num2*startRot.w + num3*endRot.w);
        }

        public static explicit operator SharpDX.Vector4(Vector4 q)
        {
            return new SharpDX.Vector4(q.x, q.y, q.z, q.w);
        }

        public void Load(BinaryReader reader)
        {
            x = reader.ReadSingle();
            y = reader.ReadSingle();
            z = reader.ReadSingle();
            w = reader.ReadSingle();
        }

        public void Save(BinaryWriter writer)
        {
            writer.Write(x);
            writer.Write(y);
            writer.Write(z);
            writer.Write(w);
        }

        public static Vector4 Angles(float pitch, float yaw, float roll)
        {
            var num1 = roll*0.5f;
            var num2 = pitch*0.5f;
            var num3 = yaw*0.5f;
            var num4 = Mathf.Sin(num1);
            var num5 = Mathf.Cos(num1);
            var num6 = Mathf.Sin(num2);
            var num7 = Mathf.Cos(num2);
            var num8 = Mathf.Sin(num3);
            var num9 = Mathf.Cos(num3);
            return
                new Vector4(
                    num9*num6*num5 + num8*num7*num4,
                    num8*num7*num5 - num9*num6*num4,
                    num9*num7*num4 - num8*num6*num5,
                    num9*num7*num5 + num8*num6*num4);
        }
    }
}