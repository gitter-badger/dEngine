// WordWrapping.cs - dEngine
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
	/// Word wrapping modes.
	/// </summary>
	public enum WordWrapping
	{
		/// <summary>
		/// Indicates that words are broken across lines to avoid text overflowing the layout box.
		/// </summary>
		Wrap = 0,

		/// <summary>
		/// Indicates that words are kept within the same line even when it overflows the layout box. This option is often used
		/// with scrolling to reveal overflow text.
		/// </summary>
		NoWrap = 1,

		/// <summary>
		/// Words are broken across lines to avoid text overflowing the layout box. Emergency wrapping occurs if the word is larger
		/// than the maximum width.
		/// </summary>
		EmergencyBreak = 2,

		/// <summary>
		/// When emergency wrapping, only wrap whole words, never breaking words when the layout width is too small for even a
		/// single word.
		/// </summary>
		WholeWord = 3,

		/// <summary>
		/// Wrap between any valid character clusters.
		/// </summary>
		Character = 4
	}
}