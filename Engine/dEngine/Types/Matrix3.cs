// Matrix3.cs - dEngine
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
using dEngine.Utility;
using SharpDX;

namespace dEngine
{
    internal struct Matrix3 : IDataType, IEquatable<Matrix3>
    {
        [InstMember(1)] public readonly float M11;
        [InstMember(2)] public readonly float M12;
        [InstMember(3)] public readonly float M13;
        [InstMember(4)] public readonly float M21;
        [InstMember(5)] public readonly float M22;
        [InstMember(6)] public readonly float M23;
        [InstMember(7)] public readonly float M31;
        [InstMember(8)] public readonly float M32;
        [InstMember(9)] public readonly float M33;

        public Matrix3(float m11, float m12, float m13, float m21, float m22, float m23, float m31, float m32,
            float m33)
        {
            M11 = m11;
            M12 = m12;
            M13 = m13;
            M21 = m21;
            M22 = m22;
            M23 = m23;
            M31 = m31;
            M32 = m32;
            M33 = m33;
        }

        public Matrix3(float[] rotation) : this()
        {
            M11 = rotation[0];
            M12 = rotation[1];
            M13 = rotation[2];
            M21 = rotation[3];
            M22 = rotation[4];
            M23 = rotation[5];
            M31 = rotation[6];
            M32 = rotation[7];
            M33 = rotation[8];
        }

        public static Matrix3 Zero = new Matrix3(0, 0, 0, 0, 0, 0, 0, 0, 0);
        public static Matrix3 Identity = new Matrix3(1, 0, 0, 0, 1, 0, 0, 0, 1);

        public static void LookAt(ref Vector3 eye, ref Vector3 target, ref Vector3 up, out Matrix3 result)
        {
            Vector3 result1;
            Vector3.Subtract(ref eye, ref target, out result1);
            Vector3.Normalize(ref result1, out result1);
            Vector3 result2;
            Vector3.Cross(ref up, ref result1, out result2);
            Vector3.Normalize(ref result2, out result2);
            Vector3 result3;
            Vector3.Cross(ref result1, ref result2, out result3);

            var m11 = result2.x;
            var m12 = result2.y;
            var m13 = result2.z;

            var m21 = result3.x;
            var m22 = result3.y;
            var m23 = result3.z;

            var m31 = result1.x;
            var m32 = result1.y;
            var m33 = result1.z;

            result = new Matrix3(m11, m21, m31, m12, m22, m32, m13, m23, m33);
        }

        public static Matrix3 FromQuaternion(float qx, float qy, float qz, float qw)
        {
            float num1 = qx * qx;
            float num2 = qy * qy;
            float num3 = qz * qz;
            float num4 = qx * qy;
            float num5 = qz * qw;
            float num6 = qz * qx;
            float num7 = qy * qw;
            float num8 = qy * qz;
            float num9 = qx * qw;

            return Matrix3.Transposed((float)(1.0 - 2.0 * (num2 + (double)num3)),
                (float)(2.0 * (num4 + (double)num5)),
                (float)(2.0 * (num6 - (double)num7)),
                (float)(2.0 * (num4 - (double)num5)),
                (float)(1.0 - 2.0 * (num3 + (double)num1)),
                (float)(2.0 * (num8 + (double)num9)),
                (float)(2.0 * (num6 + (double)num7)),
                (float)(2.0 * (num8 - (double)num9)),
                (float)(1.0 - 2.0 * (num2 + (double)num1)));
        }

        public static void Inverse(ref Matrix3 value, out Matrix3 result)
        {
            float num1 = (float)(value.M22 * (double)value.M33 + value.M23 * -(double)value.M32);
            float num2 = (float)(value.M21 * (double)value.M33 + value.M23 * -(double)value.M31);
            float num3 = (float)(value.M21 * (double)value.M32 + value.M22 * -(double)value.M31);
            float num4 = (float)(value.M11 * (double)num1 - value.M12 * (double)num2 + value.M13 * (double)num3);
            if (Math.Abs(num4) == 0.0)
            {
                result = Matrix3.Zero;
            }
            else
            {
                float num5 = 1f / num4;
                float num6 = (float)(value.M12 * (double)value.M33 + value.M13 * -(double)value.M32);
                float num7 = (float)(value.M11 * (double)value.M33 + value.M13 * -(double)value.M31);
                float num8 = (float)(value.M11 * (double)value.M32 + value.M12 * -(double)value.M31);
                float num9 = (float)(value.M12 * (double)value.M23 - value.M13 * (double)value.M22);
                float num10 = (float)(value.M11 * (double)value.M23 - value.M13 * (double)value.M21);
                float num11 = (float)(value.M11 * (double)value.M22 - value.M12 * (double)value.M21);
                result = new Matrix3(num1 * num5,
                    -num6 * num5,
                    num9 * num5,
                    -num2 * num5,
                    num7 * num5,
                    -num10 * num5,
                    num3 * num5,
                    -num8 * num5,
                    num11 * num5);
            }
        }

        public static void GetEulerAngles(ref Matrix3 matrix, out Vector3 result)
        {
            float X, Y, Z;

            if (matrix.M13 > -1 && matrix.M13 < 1)
            {
                X = Mathf.Atan2(-matrix.M23, matrix.M33);
                Y = Mathf.Asin(matrix.M13);
                Z = Mathf.Atan2(-matrix.M12, matrix.M11);
            }
            else
            {
                Y = Mathf.Pi / 2;

                if (matrix.M13 <= -1)
                    Y = -Y;

                X = -Mathf.Atan2(-matrix.M21, matrix.M22);
                Z = 0;
            }

            result = new Vector3(X, Y, Z);
        }

        public static Matrix3 operator -(Matrix3 value)
        {
            Matrix3 result;
            Negate(ref value, out result);
            return result;
        }

        private static void Negate(ref Matrix3 value, out Matrix3 result)
        {
            result = new Matrix3(-value.M11,
                -value.M12,
                -value.M13,
                -value.M21,
                -value.M22,
                -value.M23,
                -value.M31,
                -value.M32,
                -value.M33);
        }

        public static Matrix3 operator *(Matrix3 value, float scale)
        {
            Matrix3 result;
            Multiply(ref value, scale, out result);
            return result;
        }

        public static Matrix3 operator *(Matrix3 lhs, Matrix3 rhs)
        {
            Matrix3 result;
            Multiply(ref lhs, ref rhs, out result);
            return result;
        }

        public static void Multiply(ref Matrix3 left, ref Matrix3 right, out Matrix3 result)
        {
            result = new Matrix3(
                (float)(left.M11 * (double)right.M11 + left.M12 * (double)right.M21 + left.M13 * (double)right.M31),
                (float)(left.M11 * (double)right.M12 + left.M12 * (double)right.M22 + left.M13 * (double)right.M32),
                (float)(left.M11 * (double)right.M13 + left.M12 * (double)right.M23 + left.M13 * (double)right.M33),
                (float)(left.M21 * (double)right.M11 + left.M22 * (double)right.M21 + left.M23 * (double)right.M31),
                (float)(left.M21 * (double)right.M12 + left.M22 * (double)right.M22 + left.M23 * (double)right.M32),
                (float)(left.M21 * (double)right.M13 + left.M22 * (double)right.M23 + left.M23 * (double)right.M33),
                (float)(left.M31 * (double)right.M11 + left.M32 * (double)right.M21 + left.M33 * (double)right.M31),
                (float)(left.M31 * (double)right.M12 + left.M32 * (double)right.M22 + left.M33 * (double)right.M32),
                (float)(left.M31 * (double)right.M13 + left.M32 * (double)right.M23 + left.M33 * (double)right.M33)
                );
        }

        public static void Multiply(ref Matrix3 left, float right, out Matrix3 result)
        {
            result = new Matrix3(left.M11 * right,
                left.M12 * right,
                left.M13 * right,
                left.M21 * right,
                left.M22 * right,
                left.M23 * right,
                left.M31 * right,
                left.M32 * right,
                left.M33 * right);
        }

        public float this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return M11;
                    case 1:
                        return M12;
                    case 2:
                        return M13;
                    case 3:
                        return M21;
                    case 4:
                        return M22;
                    case 5:
                        return M23;
                    case 6:
                        return M31;
                    case 7:
                        return M32;
                    case 8:
                        return M33;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(index),
                            "Indices for Matrix3x3 run from 0 to 8, inclusive.");
                }
            }
        }

        public float this[int row, int column]
        {
            get
            {
                if (row < 0 || row > 2)
                    throw new ArgumentOutOfRangeException(nameof(row),
                        "Rows and columns for matrices run from 0 to 2, inclusive.");
                if (column < 0 || column > 2)
                    throw new ArgumentOutOfRangeException(nameof(column),
                        "Rows and columns for matrices run from 0 to 2, inclusive.");
                return this[row * 3 + column];
            }
        }

        public static void RotationAxis(ref Vector3 axis, ref float angle, out Matrix3 result)
        {
            float num1 = axis.X;
            float num2 = axis.Y;
            float num3 = axis.Z;
            float num4 = (float)Math.Cos(angle);
            float num5 = (float)Math.Sin(angle);
            float num6 = num1 * num1;
            float num7 = num2 * num2;
            float num8 = num3 * num3;
            float num9 = num1 * num2;
            float num10 = num1 * num3;
            float num11 = num2 * num3;

            result = Matrix3.Transposed(
                num6 + num4 * (1f - num6),
                (float)(num9 - num4 * (double)num9 + num5 * (double)num3),
                (float)(num10 - num4 * (double)num10 - num5 * (double)num2),
                (float)(num9 - num4 * (double)num9 - num5 * (double)num3),
                num7 + num4 * (1f - num7),
                (float)(num11 - num4 * (double)num11 + num5 * (double)num1),
                (float)(num10 - num4 * (double)num10 + num5 * (double)num2),
                (float)(num11 - num4 * (double)num11 - num5 * (double)num1),
                num8 + num4 * (1f - num8));
        }

        private static Matrix3 Transposed(float m11, float m12, float m13, float m21, float m22, float m23, float m31,
            float m32, float m33)
        {
            return new Matrix3(m11, m21, m31, m12, m22, m32, m13, m23, m33);
        }

        /// <summary />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Matrix3 other)
        {
            return MathUtil.NearEqual(M11, other.M11) && MathUtil.NearEqual(M12, other.M12) &&
                   MathUtil.NearEqual(M13, other.M13) &&
                   MathUtil.NearEqual(M21, other.M21) && MathUtil.NearEqual(M22, other.M22) &&
                   MathUtil.NearEqual(M33, other.M33) &&
                   MathUtil.NearEqual(M31, other.M31) && MathUtil.NearEqual(M32, other.M32) &&
                   MathUtil.NearEqual(M33, other.M33);
        }

        public void Load(BinaryReader reader)
        {
            throw new NotImplementedException();
        }

        public void Save(BinaryWriter writer)
        {
            for (int i = 0; i < 9; i++)
                writer.Write(this[i]);
        }

        public static void FromEulerAngles(float pitch, float yaw, float roll, out Matrix3 output)
        {
            float fCos, fSin;

            fCos = Mathf.Cos(yaw);
            fSin = Mathf.Sin(yaw);
            var yMat = new Matrix3(fCos, 0.0f, fSin, 0.0f, 1.0f, 0.0f, -fSin, 0.0f, fCos);

            fCos = Mathf.Cos(pitch);
            fSin = Mathf.Sin(pitch);
            var xMat = new Matrix3(1.0f, 0.0f, 0.0f, 0.0f, fCos, -fSin, 0.0f, fSin, fCos);

            fCos = Mathf.Cos(roll);
            fSin = Mathf.Sin(roll);
            var zMat = new Matrix3(fCos, -fSin, 0.0f, fSin, fCos, 0.0f, 0.0f, 0.0f, 1.0f);

            Matrix3.Multiply(ref xMat, ref zMat, out output);
            Matrix3.Multiply(ref yMat, ref output, out output);
        }
    }
}