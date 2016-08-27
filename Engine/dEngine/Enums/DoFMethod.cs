// DoFMethod.cs - dEngine
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
	/// Depth of Field methods.
	/// </summary>
	public enum DoFMethod
	{
		/// <summary>
		/// Gaussian DoF blurs the scene using a standard Guassian blur.
		/// </summary>
		Gaussian,

		/// <summary>
		/// Bokeh is the name of the shape that can be seen in photos or movies when objects are out of focus.
		/// </summary>
		Bokeh
	}
}