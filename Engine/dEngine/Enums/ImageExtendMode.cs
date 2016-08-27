// ImageExtendMode.cs - dEngine
// Copyright (C) 2015-2016 DanDev (dandev.sco@gmail.com)
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
	/// Enum for bitmap extend modes.
	/// </summary>
	public enum ExtendMode
	{
		/// <summary>
		/// Clamps to the border of the image.
		/// </summary>
		Clamp = 0,

		/// <summary>
		/// Wraps the image.
		/// </summary>
		Wrap = 1,

		/// <summary>
		/// Mirrors the image.
		/// </summary>
		Mirror = 2,

		/// <summary>
		/// Stretches the image.
		/// </summary>
		Stretch = 3
	}
}