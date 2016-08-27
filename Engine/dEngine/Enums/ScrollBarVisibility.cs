// ScrollBarVisibility.cs - dEngine
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
	/// Enumeration for ScrollBar visiblity.
	/// </summary>
	public enum ScrollBarVisibility
	{
		/// <summary>
		/// The scrollbar is only visible when content overflows.
		/// </summary>
		Auto = 0,

		/// <summary>
		/// Unused.
		/// </summary>
		Disabled = 1,

		/// <summary>
		/// The scroll bar is never visible, and margins are not applied to the content.
		/// </summary>
		Hidden = 2,

		/// <summary>
		/// The scrollbar is always visible.
		/// </summary>
		Visible = 3
	}
}