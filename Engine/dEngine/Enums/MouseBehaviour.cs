// MouseBehaviour.cs - dEngine
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
	/// Enum for mouse behaviour.
	/// </summary>
	public enum MouseBehaviour
	{
		/// <summary>
		/// Cursor can move freely.
		/// </summary>
		Default,

		/// <summary>
		/// Cursor is locked to the center of the screen.
		/// </summary>
		LockCenter,

		/// <summary>
		/// Cursor is locked at its current position.
		/// </summary>
		LockCurrentPosition
	}
}