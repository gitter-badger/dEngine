// CFrame.cs - dEngine
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
using CSCore.XAudio2.X3DAudio;
using dEngine.Instances;
using Neo.IronLua;

using DxVector3 = SharpDX.Vector3;
using DxMatrix3 = SharpDX.Matrix3x3;
using DxMatrix4 = SharpDX.Matrix;
using BulletMatrix = BulletSharp.Math.Matrix;

// ReSharper disable InconsistentNaming

namespace dEngine
{
	/// <summary>
	/// A type representing a position and orientation in 3D space.
	/// </summary>
	public struct CFrame : IDataType, IEquatable<CFrame>, IListenable
    {
		/// <summary>
		/// The position.
		/// </summary>
		[InstMember(1)] public Vector3 p;

		/// <summary>
		/// The orientation.
		/// </summary>
		[InstMember(2)] private Matrix3 _rotation;

		/// <summary>
		/// A CFrame with an identity matrix.
		/// </summary>
		public static CFrame Identity = new CFrame(Vector3.Zero);

        /// <summary>
        /// A CFrame with a zeroed matrix.
        /// </summary>
        public static CFrame Zero = new CFrame(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

        #region Constructors

        /// <summary>
        /// Creates a CFrame from position (x, y and z).
        /// </summary>
        public CFrame(float x, float y, float z)
		{
			_rotation = Matrix3.Identity;
			p = new Vector3(x, y, z);
		}

		/// <summary>
		/// Creates a CFrame from a quaternion.
		/// </summary>
		public CFrame(float x, float y, float z, float qx, float qy, float qz, float qw) : this(x, y, z)
		{
			p = new Vector3(x, y, z);
			_rotation = Matrix3.FromQuaternion(qx, qy, qz, qw);
		}

		/// <summary>
		/// Creates a CFrame from position.
		/// </summary>
		public CFrame(Vector3 position) : this(position.x, position.y, position.z)
		{
		}

		/// <summary>
		/// Creates a CFrame from position, looking at point.
		/// </summary>
		/// <remarks>
		/// Lua syntax for this method is broken:
		/// https://github.com/neolithos/neolua/issues/40
		/// </remarks>
		public CFrame(Vector3 eye, Vector3 target)
		{
		    var v3Up = Vector3.Up;
			Matrix3 rot;
			Matrix3.LookAt(ref eye, ref target, ref v3Up, out rot);
			p = eye;
			_rotation = rot;
		}

		/// <summary>
		/// Creates a CFrame from matrix components.
		/// </summary>
		public CFrame(float x, float y, float z, float r11, float r12, float r13, float r21, float r22, float r23,
			float r31, float r32, float r33)
		{
			p = new Vector3(x, y, z);
			_rotation = new Matrix3(r11, r12, r13, r21, r22, r23, r31, r32, r33);
		}

		/// <summary>
		/// Creates a CFrame from position with quaternion orientation.
		/// </summary>
		internal CFrame(Vector3 position, Vector4 orientation)
			: this(position.x, position.y, position.z, orientation.x, orientation.y, orientation.z, orientation.w)
		{
		}

		private CFrame(Vector3 position, ref Matrix3 rotation)
		{
			p = position;
			_rotation = rotation;
        }

        #region Factory Methods

        /// <inheritdoc />
        public static CFrame @new(float x, float y, float z)
		{
			return new CFrame(x, y, z);
		}

		/// <summary>
		/// Creates a CFrame from position.
		/// </summary>
		public static CFrame @new(Vector3 position)
		{
			return new CFrame(position);
		}

		/// <summary>
		/// Creates a CFrame from position, look-at point.
		/// </summary>
		public static CFrame @new(Vector3 position, Vector3 target)
		{
			return new CFrame(position, target);
		}

		/// <summary>
		/// Creates a CFrame from a vector position and quaternion orientation.
		/// </summary>
		public static CFrame @new(Vector3 position, Vector4 orientation)
		{
			return new CFrame(position, orientation);
		}

		/// <summary>
		/// Creates a CFrame from x, y, z position and x,y,z,w quaternion rotation.
		/// </summary>
		public static CFrame @new(float x, float y, float z, float qx, float qy, float qz, float qw)
		{
			return new CFrame(x, y, z, qx, qy, qz, qw);
		}

		/// <summary>
		/// Creates a CFrame from x, y, z position and 3x3 matrix components.
		/// </summary>
		public static CFrame @new(float x, float y, float z, float r11, float r12, float r13, float r21, float r22,
			float r23, float r31, float r32, float r33)
		{
			return new CFrame(x, y, z, r11, r12, r13, r21, r22, r23, r31, r32, r33);
		}

		#endregion

		/// <summary>
		/// Determines if the CFrame is an identity matrix.
		/// </summary>
		public bool isIdentity => p.Equals(Vector3.Zero) && _rotation.Equals(Matrix3.Identity);

		/// <summary>
		/// Creates a CFrame from a unit vector rotated in radians.
		/// </summary>
		public static CFrame FromAxisAngle(Vector3 axis, float angle)
		{
			if (Math.Abs(axis.mag2) < float.Epsilon)
				return Identity;

			Matrix3 result;
			Matrix3.RotationAxis(ref axis, ref angle, out result);

			return new CFrame(Vector3.Zero, ref result);
		}

		/// <summary>
		/// Creates a CFrame from euler angles in radians.
		/// </summary>
		public static CFrame Angles(float pitch, float yaw, float roll)
		{
		    Matrix3 rot;
            Matrix3.FromEulerAngles(pitch, yaw, roll, out rot);
            return new CFrame(Vector3.Zero, ref rot);
		    /*
            var xMatrix = new Matrix3x3(1, 0, 0,
				0, Mathf.Cos(pitch), -Mathf.Sin(pitch),
				0, Mathf.Sin(pitch), Mathf.Cos(pitch));

			var yMatrix = new Matrix3x3(Mathf.Cos(yaw), 0, Mathf.Sin(yaw),
				0, 1, 0,
				-Mathf.Sin(yaw), 0, Mathf.Cos(yaw));


			var zMatrix = new Matrix3x3(Mathf.Cos(roll), -Mathf.Sin(roll), 0,
				Mathf.Sin(roll), Mathf.Cos(roll), 0,
				0, 0, 1);

			var rotation = xMatrix * yMatrix * zMatrix;

			return new CFrame(new Vector3(0, 0, 0), rotation);
            */
		}

		#endregion

		/// <summary>
		/// The forward unit vector.
		/// </summary>
		public Vector3 lookVector => new Vector3(-_rotation.M13, -_rotation.M23, -_rotation.M33);

		/// <summary>
		/// The forward unit vector. (the same as lookVector)
		/// </summary>
		public Vector3 forward => new Vector3(-_rotation.M13, -_rotation.M23, -_rotation.M33);

		/// <summary>
		/// The backward unit vector.
		/// </summary>
		public Vector3 backward => new Vector3(_rotation.M13, _rotation.M23, _rotation.M33);

		/// <summary>
		/// The left unit vector.
		/// </summary>
		public Vector3 left => new Vector3(-_rotation.M11, -_rotation.M21, -_rotation.M31);

		/// <summary>
		/// The right unit vector.
		/// </summary>
		public Vector3 right => new Vector3(_rotation.M11, _rotation.M21, _rotation.M31);

		/// <summary>
		/// The downward unit vector.
		/// </summary>
		public Vector3 down => new Vector3(-_rotation.M12, -_rotation.M22, -_rotation.M32);

		/// <summary>
		/// The upward unit vector.
		/// </summary>
		public Vector3 up => new Vector3(_rotation.M12, _rotation.M22, _rotation.M32);

		/// <summary>The X component.</summary>
		public float X => p.x;

		/// <summary> The Y component.</summary>
		public float Y => p.y;

		/// <summary>The Z component.</summary>
		public float Z => p.z;

		/// <summary>The X component.</summary>
		public float x => p.x;

		/// <summary> The Y component.</summary>
		public float y => p.y;

		/// <summary>The Z component.</summary>
		public float z => p.z;

		#region Methods

		/// <summary>
		/// Returns false if the CFrame contain NaN/Infinite values.
		/// </summary>
		public bool validate()
		{
			Func<float, bool> isValid = f => !float.IsNaN(f) && !float.IsInfinity(f);

			return isValid(p.x) && isValid(p.y) && isValid(p.z)
				   && isValid(_rotation.M11) && isValid(_rotation.M12) && isValid(_rotation.M13)
				   && isValid(_rotation.M21) && isValid(_rotation.M22) && isValid(_rotation.M33)
				   && isValid(_rotation.M31) && isValid(_rotation.M32) && isValid(_rotation.M33);
		}

		/// <summary>
		/// Returns the inverse CFrame.
		/// </summary>
		public CFrame inverse()
		{
			var M11 = _rotation.M11;
			var M12 = _rotation.M21;
			var M13 = _rotation.M31;
			var M14 = 0f;
			var M21 = _rotation.M12;
			var M22 = _rotation.M22;
			var M23 = _rotation.M32;
			var M24 = 0f;
			var M31 = _rotation.M13;
			var M32 = _rotation.M23;
			var M33 = _rotation.M33;
			var M34 = 0f;
			var M41 = p.x;
			var M42 = p.y;
			var M43 = p.z;
			var M44 = 1f;

			float num1 = (float)(M31 * (double)M42 - M32 * (double)M41);
			float num2 = (float)(M31 * (double)M43 - M33 * (double)M41);
			float num3 = (float)(M34 * (double)M41 - M31 * (double)M44);
			float num4 = (float)(M32 * (double)M43 - M33 * (double)M42);
			float num5 = (float)(M34 * (double)M42 - M32 * (double)M44);
			float num6 = (float)(M33 * (double)M44 - M34 * (double)M43);
			float num7 = (float)(M22 * (double)num6 + M23 * (double)num5 + M24 * (double)num4);
			float num8 = (float)(M21 * (double)num6 + M23 * (double)num3 + M24 * (double)num2);
			float num9 = (float)(M21 * -(double)num5 + M22 * (double)num3 + M24 * (double)num1);
			float num10 = (float)(M21 * (double)num4 + M22 * -(double)num2 + M23 * (double)num1);
			float num11 = (float)(M11 * (double)num7 - M12 * (double)num8 + M13 * (double)num9 - M14 * (double)num10);

			if (Math.Abs(num11) == 0.0)
			{
				return Identity;
			}

			float num12 = 1f / num11;
			float num13 = (float)(M11 * (double)M22 - M12 * (double)M21);
			float num14 = (float)(M11 * (double)M23 - M13 * (double)M21);
			float num15 = (float)(M14 * (double)M21 - M11 * (double)M24);
			float num16 = (float)(M12 * (double)M23 - M13 * (double)M22);
			float num17 = (float)(M14 * (double)M22 - M12 * (double)M24);
			float num18 = (float)(M13 * (double)M24 - M14 * (double)M23);
			float num19 = (float)(M12 * (double)num6 + M13 * (double)num5 + M14 * (double)num4);
			float num20 = (float)(M11 * (double)num6 + M13 * (double)num3 + M14 * (double)num2);
			float num21 = (float)(M11 * -(double)num5 + M12 * (double)num3 + M14 * (double)num1);
			float num22 = (float)(M11 * (double)num4 + M12 * -(double)num2 + M13 * (double)num1);
			float num23 = (float)(M42 * (double)num18 + M43 * (double)num17 + M44 * (double)num16);
			float num24 = (float)(M41 * (double)num18 + M43 * (double)num15 + M44 * (double)num14);
			float num25 = (float)(M41 * -(double)num17 + M42 * (double)num15 + M44 * (double)num13);
			float num26 = (float)(M41 * (double)num16 + M42 * -(double)num14 + M43 * (double)num13);
			M11 = num7 * num12;
			M12 = -num19 * num12;
			M13 = num23 * num12;
			M21 = -num8 * num12;
			M22 = num20 * num12;
			M23 = -num24 * num12;
			M31 = num9 * num12;
			M32 = -num21 * num12;
			M33 = num25 * num12;
			M41 = -num10 * num12;
			M42 = num22 * num12;
			M43 = -num26 * num12;

			return new CFrame(M41, M42, M43, M11, M21, M31, M12, M22, M32, M13, M23, M33);
		}

		/// <summary>
		/// Returns a CFrame linearly interpolated towards end by amount.
		/// </summary>
		public CFrame lerp(CFrame goal, float alpha)
		{
			if (alpha == 1.0f) return goal;
			if (alpha == 0.0f) return this;
			var rot = _rotation;
			var goalRot = goal._rotation;
            Vector4 q1;
            Vector4 q2;
            Vector4.RotationMatrix(ref rot, out q1);
            Vector4.RotationMatrix(ref goalRot, out q2);
            Vector4 q3;
            Vector4.Slerp(ref q1, ref q2, alpha, out q3);

			return new CFrame(p * (1 - alpha) + goal.p * alpha, q3);
		}

		/// <summary>
		/// Returns a <see cref="CFrame" /> transformed from Object to World coordinates.
		/// </summary>
		public CFrame toWorldSpace(CFrame cf)
		{
			return this * cf;
		}

		/// <summary>
		/// Returns a <see cref="CFrame" /> transformed from World to Object coordinates.
		/// </summary>
		public CFrame toObjectSpace(CFrame cf)
		{
			return inverse() * cf;
		}

		/// <summary>
		/// Returns a <see cref="Vector3" /> transformed from Object to World coordinates.
		/// </summary>
		public Vector3 pointToWorldSpace(Vector3 v3)
		{
			return this * v3;
		}

		/// <summary>
		/// Returns a <see cref="Vector3" />  transformed from World to Object coordinates.
		/// </summary>
		public Vector3 pointToObjectSpace(Vector3 v3)
		{
			return inverse() * v3;
		}

		/// <summary>
		/// Returns a <see cref="Vector3" /> rotated from Object to World coordinates.
		/// </summary>
		public Vector3 vectorToWorldSpace(Vector3 v3)
		{
			return (this - p) * v3;
		}

		/// <summary>
		/// Returns a <see cref="Vector3" />  rotated from World to Object coordinates.
		/// </summary>
		public Vector3 vectorToObjectSpace(Vector3 v3)
		{
			var a = (this - p);
			var b = a.inverse();
			var c = b * v3;

			return c;
		}

		/// <summary>
		/// Returns a <see cref="Ray" /> rotated from Object to World coordinates.
		/// </summary>
		public Ray toWorldSpace(Ray ray)
		{
			return new Ray(pointToWorldSpace(ray.Origin), vectorToWorldSpace(ray.Direction));
		}

		/// <summary>
		/// Returns a <see cref="Ray" /> rotated from World to Object coordinates.
		/// </summary>
		public Ray toObjectSpace(Ray ray)
		{
			return new Ray(pointToObjectSpace(ray.Origin), vectorToObjectSpace(ray.Direction));
		}

		/// <summary>
		/// Returns a tuple of the matrix components that make up the CFrame.
		/// </summary>
		/// <returns> x, y, z, R11, R12, R13, R21, R22, R23, R31, R32, R33</returns>
		public LuaTuple<float, float, float, float, float, float, float, float, float, float, float, float> components()
		{
			var f = GetComponents();
			return new LuaTuple<float, float, float, float, float, float, float, float, float, float, float, float>(f[0], f[1], f[2], f[3], f[4], f[5], f[6], f[7], f[8], f[9], f[10], f[11]);
		}

		/// <summary>
		/// Returns the matrix components as an array.
		/// </summary>
		public float[] GetComponents()
		{
			var m = _rotation;
			return new[]
			{
				x, y, z,
				m.M11, m.M12, m.M13,
				m.M21, m.M22, m.M23,
				m.M31, m.M32, m.M33
			};
		}

		/// <summary>
		/// Returns the rotation matrix components as an array.
		/// </summary>
		public float[] GetRotationComponents()
		{
			var m = _rotation;
			return new[]
			{
				m.M11, m.M21, m.M31,
				m.M12, m.M22, m.M32,
				m.M13, m.M23, m.M33
			};
		}

		/// <summary>
		/// Returns best-guess euler angles.
		/// </summary>
		public Vector3 getEulerAngles()
		{
			var rot = _rotation;
			Vector3 angles;
			Matrix3.GetEulerAngles(ref rot, out angles);
			return angles;
		}

		/// <summary>
		/// Returns a comma-delimited string of the array components.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return string.Join(", ", GetComponents());
		}

		#endregion

		#region Operators

		/// <summary>
		/// Determines if two CFrames are equal.
		/// </summary>
		public bool Equals(CFrame other)
		{
			return p.Equals(other.p) && _rotation.Equals(other._rotation);
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is CFrame && Equals((CFrame)obj);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			unchecked
			{
				return (p.GetHashCode() * 397) ^ _rotation.GetHashCode();
			}
		}

	    void IListenable.UpdateListener(ref Listener listener)
        {
            listener.Velocity = new CSCore.Utils.Vector3(0, 0, 0);
            listener.Position = new CSCore.Utils.Vector3(-p.x, -p.y, -p.z);
            listener.OrientTop = (CSCore.Utils.Vector3)up;
            listener.OrientFront = (CSCore.Utils.Vector3)forward;
        }

	    /// <summary>
		/// Returns an inverse CFrame.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static CFrame operator -(CFrame cf)
		{
			return cf.inverse();
		}

		/// <summary>
		/// Returns true if two CFrame are equal.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(CFrame left, CFrame right)
		{
			return left.Equals(right);
		}

		/// <summary>
		/// Returns true if two CFrame are not equal.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(CFrame left, CFrame right)
		{
			return !left.Equals(right);
		}

		/// <summary>
		/// Converts CFrame to a SharpDX Matrix4x4.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator DxMatrix4(CFrame cf)
		{
			return cf.GetModelMatrix();
		}

		/// <summary>
		/// Converts SharpDX Matrix4x4 to a CFrame.
		/// </summary>
		/// <param name="mat"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator CFrame(DxMatrix4 mat)
		{
			// TODO: check if correct order
			return new CFrame(mat.M41, mat.M42, mat.M43,
				mat.M11, mat.M12, mat.M13,
				mat.M21, mat.M22, mat.M23,
				mat.M31, mat.M32, mat.M33);
		}

		/// <summary>
		/// Converts CFrame to a BulletSharp Matrix4x4.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator BulletMatrix(CFrame cf)
		{
			// TODO: check if correct order
			return new BulletMatrix(cf._rotation.M11, cf._rotation.M12, cf._rotation.M13, 0,
				cf._rotation.M21, cf._rotation.M22, cf._rotation.M23, 0,
				cf._rotation.M31, cf._rotation.M32, cf._rotation.M33, 0,
				cf.p.x, cf.p.y, cf.p.z, 1);
		}

		/// <summary>
		/// Converts BulletSharp Matrix4x4 to a CFrame.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator CFrame(BulletMatrix mat)
		{
			// TODO: check if correct order
			return new CFrame(mat.M41, mat.M42, mat.M43,
				mat.M11, mat.M12, mat.M13,
				mat.M21, mat.M22, mat.M23,
				mat.M31, mat.M32, mat.M33);
		}

		/// <summary>
		/// Scales the CFrame.
		/// </summary>
		public static CFrame operator *(CFrame lhs, float rhs)
		{
		    Matrix3 rot;
            Matrix3.Multiply(ref lhs._rotation, rhs, out rot);
            return new CFrame(lhs.p * rhs, ref rot);
		}

		/// <summary>
		/// Multiplies two CFrames.
		/// </summary>
		public static CFrame operator *(CFrame lhs, CFrame rhs)
        {
            Matrix3 rot;
            Matrix3.Multiply(ref lhs._rotation, ref rhs._rotation, out rot);
            return new CFrame(lhs.pointToWorldSpace(rhs.p), ref rot);
		}

		/// <summary>
		/// Adds Vector3 to CFrame Position.
		/// </summary>
		public static CFrame operator +(CFrame value, Vector3 offset)
		{
			return new CFrame(value.p + offset, ref value._rotation);
		}

		/// <summary>
		/// Subtracts Vector3 to CFrame Position.
		/// </summary>
		public static CFrame operator -(CFrame value, Vector3 offset)
		{
			return new CFrame(value.p - offset, ref value._rotation);
		}

		/// <summary>
		/// Transforms a Vector3 from Object to World coordinates.
		/// </summary>
		public static Vector3 operator *(CFrame cf, Vector3 vector)
		{
			/*
			var rot = cf._rotation;
			Vector3 result;
			Vector3.TransformCoordinate(ref vector, ref rot, out result);
			return result;
			*/
			var rotation = cf._rotation;
			return
				new Vector3(rotation.M11 * vector.x + rotation.M12 * vector.y + rotation.M13 * vector.z + cf.p.x,
					rotation.M21 * vector.x + rotation.M22 * vector.y + rotation.M23 * vector.z + cf.p.y,
					rotation.M31 * vector.x + rotation.M32 * vector.y + rotation.M33 * vector.z + cf.p.z);
		}

		#endregion

		/// <summary>
		/// Gets the rotation of the CFrame as a quaternion.
		/// </summary>
		/// <returns></returns>
		public Vector4 getQuaternion()
		{
			var rot = _rotation;
            Vector4 quat;
            Vector4.RotationMatrix(ref rot, out quat);
			return quat;
		}

		/// <summary>
		/// Returns a model matrix for ray testing..
		/// </summary>
		/// <returns></returns>
		internal DxMatrix4 GetMatrix()
		{
			return new DxMatrix4(_rotation.M11, _rotation.M12, _rotation.M13, 0,
				_rotation.M21, _rotation.M22, _rotation.M23, 0,
				_rotation.M31, _rotation.M32, _rotation.M33, 0,
				p.x, p.y, p.z, 1);
		}

		/// <summary>
		/// Returns a render model matrix for Direct3D.
		/// </summary>
		/// <returns></returns>
		internal DxMatrix4 GetModelMatrix()
		{
			return new DxMatrix4(_rotation.M11, _rotation.M21, _rotation.M31, 0,
				_rotation.M12, _rotation.M22, _rotation.M32, 0,
				_rotation.M13, _rotation.M23, _rotation.M33, 0,
				p.x, p.y, p.z, 1);
		}

		/// <summary>
		/// Returns a render view matrix for Direct3D.
		/// </summary>
		internal void GetViewMatrix(out DxMatrix4 result)
		{
			var m = (DxMatrix4)this;
		    DxMatrix4.Invert(ref m, out result);
		}

		internal Vector3 this[NormalId normalId]
		{
			get
			{
				switch (normalId)
				{
					case NormalId.Right:
						return right;
					case NormalId.Top:
						return up;
					case NormalId.Back:
						return backward;
					case NormalId.Left:
						return left;
					case NormalId.Bottom:
						return down;
					case NormalId.Front:
						return forward;
					default:
						throw new ArgumentOutOfRangeException(nameof(normalId), normalId, null);
				}
			}
		}

        /// <summary/>
	    public void Load(BinaryReader reader)
        {
            var x = reader.ReadSingle();
	        var y = reader.ReadSingle();
            var z = reader.ReadSingle();
            p = new Vector3(x, y, z);

	        var rotation = new float[9];
	        for (int i = 0; i < 9; i++)
	            rotation[i] = reader.ReadSingle();
            _rotation = new Matrix3(rotation);
        }

        /// <summary/>
        public void Save(BinaryWriter writer)
        {
            writer.Write(x);
            writer.Write(y);
            writer.Write(z);
            for (int i = 0; i < 9; i++)
                writer.Write(_rotation[i]);
        }
    }
}