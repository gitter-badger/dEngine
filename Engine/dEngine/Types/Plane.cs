// Plane.cs - dEngine
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

using SharpDX;

// ReSharper disable ImpureMethodCallOnReadonlyValueField

namespace dEngine
{
	/// <summary>
	/// A type representing a position and orientation in 3D space.
	/// </summary>
	public struct Plane : IDataType, IEquatable<Plane>
	{
		/// <summary>
		/// The normal vector of the plane.
		/// </summary>
		[InstMember(1)] public Vector3 normal;

		/// <summary>
		/// The distance of the plane long its normal from the origin.
		/// </summary>
		[InstMember(2)] public float d;

		/// <summary>
		/// Creates a plane from the given value.
		/// </summary>
		/// <param name="value">The X, Y, Z and distance components value.</param>
		public Plane(float value)
		{
			normal = new Vector3(value, value, value);
			d = value;
		}

		/// <summary>
		/// Creates a plane from the given normal and distance components.
		/// </summary>
		/// <param name="a">The X component of the normal.</param>
		/// <param name="b">The Y component of the normal.</param>
		/// <param name="c">The Z component of the normal.</param>
		/// <param name="d">The distance.</param>
		public Plane(float a, float b, float c, float d)
		{
			normal = new Vector3(a, b, c);
			this.d = d;
		}

		/// <summary>
		/// Creates a plane.
		/// </summary>
		/// <param name="point">Any point that lies along the plane.</param>
		/// <param name="normal">The normal vector.</param>
		public Plane(Vector3 point, Vector3 normal)
		{
			this.normal = normal;
			d = -normal.dot(point);
		}

		/// <summary>
		/// Creates a plane.
		/// </summary>
		/// <param name="normal">The normal vector.</param>
		/// <param name="d">The distance.</param>
		public Plane(Vector3 normal, float d)
		{
			this.normal = normal;
			this.d = d;
		}

		/// <summary>
		/// Creates a plane.
		/// </summary>
		/// <param name="point1">First point of a triangle defining the plane.</param>
		/// <param name="point2">Second point of a triangle defining the plane.</param>
		/// <param name="point3">Third point of a triangle defining the plane.</param>
		public Plane(Vector3 point1, Vector3 point2, Vector3 point3)
		{
			float x1 = point2.X - point1.X;
			float y1 = point2.Y - point1.Y;
			float z1 = point2.Z - point1.Z;
			float x2 = point3.X - point1.X;
			float y2 = point3.Y - point1.Y;
			float z2 = point3.Z - point1.Z;
			float yz = y1 * z2 - z1 * y2;
			float xz = z1 * x2 - x1 * z2;
			float xy = x1 * y2 - y1 * x2;
			float invPyth = 1.0f / (float)Math.Sqrt(yz * yz + xz * xz + xy * xy);

			var x = yz * invPyth;
			var y = invPyth;
			var z = xy * invPyth;
			normal = new Vector3(x, y, z);
			d = -(x * point1.X + y * point1.Y + z * point1.Z);
		}

		/// <summary>
		/// Creates a plane.
		/// </summary>
		/// <param name="values">The values to assign to the A, B, C, and D components of the plane.</param>
		public Plane(float[] values)
		{
			if (values == null)
				throw new ArgumentNullException(nameof(values));
			if (values.Length != 4)
				throw new ArgumentOutOfRangeException(nameof(values), "There must be four and only four input values for Plane.");

			normal = new Vector3(values[0], values[1], values[2]);
			d = values[3];
		}

		/// //////////////////////////////////////////////////
		/// <summary>
		/// Creates a plane from the given value.
		/// </summary>
		/// <param name="value">The X, Y, Z and distance components value.</param>
		public static Plane @new(float value)
		{
			return new Plane(value);
		}

		/// <summary>
		/// Creates a plane from the given normal and distance components.
		/// </summary>
		/// <param name="a">The X component of the normal.</param>
		/// <param name="b">The Y component of the normal.</param>
		/// <param name="c">The Z component of the normal.</param>
		/// <param name="d">The distance.</param>
		public static Plane @new(float a, float b, float c, float d)
		{
			return new Plane(a, b, c, d);
		}

		/// <summary>
		/// Creates a plane.
		/// </summary>
		/// <param name="point">Any point that lies along the plane.</param>
		/// <param name="normal">The normal vector.</param>
		public static Plane @new(Vector3 point, Vector3 normal)
		{
			return new Plane(point, normal);
		}

		/// <summary>
		/// Creates a plane.
		/// </summary>
		/// <param name="normal">The normal vector.</param>
		/// <param name="d">The distance.</param>
		public static Plane @new(Vector3 normal, float d)
		{
			return new Plane(normal, d);
		}

		/// <summary>
		/// Creates a plane.
		/// </summary>
		/// <param name="point1">First point of a triangle defining the plane.</param>
		/// <param name="point2">Second point of a triangle defining the plane.</param>
		/// <param name="point3">Third point of a triangle defining the plane.</param>
		public static Plane @new(Vector3 point1, Vector3 point2, Vector3 point3)
		{
			return new Plane(point1, point2, point3);
		}

		/// <summary>
		/// Determines whether the point intersects the plane.
		/// </summary>
		public PlaneIntersection Intersects(Vector3 point)
		{
			var distance = normal.dot(point) + d;

			return distance > 0f
				? PlaneIntersection.Front
				: (distance < 0f)
					? PlaneIntersection.Back
					: PlaneIntersection.Intersecting;
		}

		/// <summary>
		/// Determines whether the ray intersects the plane.
		/// </summary>
		/// <returns>
		/// If intersected, the distance to the intersection, otherwise 0.
		/// </returns>
		public float Intersects(Ray ray)
		{
			var direction = normal.dot(ray.Direction);

			if (MathUtil.IsZero(direction))
			{
				return 0f;
			}

			var position = normal.dot(ray.Origin);
			var distance = (-d - position) / direction;

			return distance;
		}

		/// <summary>
		/// Determines if this plane intersects another plane.
		/// </summary>
		public bool Intersects(Plane plane)
		{
			var direction = normal.cross(plane.normal);

			float denominator;
			Vector3.Dot(ref direction, ref direction, out denominator);

			return !MathUtil.IsZero(denominator);
		}

		/// <summary>
		/// Determines if the plane intersects the region.
		/// </summary>
		public PlaneIntersection Intersects(Region3 region)
		{
			var maxX = (normal.X >= 0.0f) ? region.Min.X : region.Max.X;
			var maxY = (normal.Y >= 0.0f) ? region.Min.Y : region.Max.Y;
			var maxZ = (normal.Z >= 0.0f) ? region.Min.Z : region.Max.Z;
			var minX = (normal.X >= 0.0f) ? region.Max.X : region.Min.X;
			var minY = (normal.Y >= 0.0f) ? region.Max.Y : region.Min.Y;
			var minZ = (normal.Z >= 0.0f) ? region.Max.Z : region.Min.Z;
			var max = new Vector3(maxX, maxY, maxZ);
			var min = new Vector3(minX, minY, minZ);

			var distance = normal.dot(max);

			if (distance + d > 0.0f)
				return PlaneIntersection.Front;

			distance = normal.dot(min);

			return distance + d < 0.0f ? PlaneIntersection.Back : PlaneIntersection.Intersecting;
		}

		internal float[] ToArray()
		{
			return new[] {normal.x, normal.y, normal.z, d};
		}

		internal static void Normalize(ref Plane plane, out Plane result)
		{
			float magnitude = 1.0f /
							  (float)
								  (Math.Sqrt(plane.normal.x * plane.normal.x + plane.normal.y * plane.normal.y +
											 (plane.normal.z * plane.normal.z)));
			result = new Plane(plane.normal * magnitude, plane.d * magnitude);
		}

		/// <summary>
		/// Determines if two planes are equal.
		/// </summary>
		public bool Equals(Plane other)
		{
			return normal.Equals(other.normal) && d.Equals(other.d);
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is Plane && Equals((Plane)obj);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			unchecked
			{
				return (normal.GetHashCode() * 397) ^ d.GetHashCode();
			}
		}

		/// <summary>
		/// Returns true if two Planes are equal.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(Plane left, Plane right)
		{
			return left.Equals(right);
		}

		/// <summary>
		/// Returns false if two Planes are equal.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(Plane left, Plane right)
		{
			return !left.Equals(right);
		}

        /// <summary/>
	    public void Load(BinaryReader reader)
	    {
            var n = new Vector3();
            n.Load(reader);
	        normal = n;
	        d = reader.ReadSingle();
	    }

        /// <summary/>
        public void Save(BinaryWriter writer)
	    {
            normal.Save(writer);
            writer.Write(d);
	    }
	}
}