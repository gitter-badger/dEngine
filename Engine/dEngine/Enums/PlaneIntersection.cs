// PlaneIntersection.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

namespace dEngine
{
	/// <summary>
	/// Describes the result of a plane intersection.
	/// </summary>
	public enum PlaneIntersection
	{
		/// <summary>
		/// The object is behind the plane.
		/// </summary>
		Back,

		/// <summary>
		/// The object is in front of the plane.
		/// </summary>
		Front,

		/// <summary>
		/// The object is intersecting the plane.
		/// </summary>
		Intersecting
	}
}