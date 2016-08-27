// GuiBase2D.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using dEngine.Instances.Attributes;

namespace dEngine.Instances
{
	/// <summary>
	/// The base class of all gui screens and elements.
	/// </summary>
	[TypeId(95), ToolboxGroup("GUI")]
	public abstract class GuiBase2D : GuiBase
	{
		protected internal Vector2 _bbMax;
		protected internal Vector2 _bbMin;

		/// <summary>
		/// Gets the absolute Position of the gui object.
		/// </summary>
		public abstract Vector2 AbsolutePosition { get; internal set; }

		/// <summary>
		/// Gets the absolute size of the gui object.
		/// </summary>
		public abstract Vector2 AbsoluteSize { get; internal set; }

		/// <summary>
		/// Returns true if the given point is within the bounds of this element,
		/// </summary>
		public bool BoundsCheck(Vector2 pos)
		{
			return pos.x > AbsolutePosition.x &&
				   pos.y > AbsolutePosition.y &&
				   pos.x < AbsolutePosition.x + AbsoluteSize.x &&
				   pos.y < AbsolutePosition.y + AbsoluteSize.y;
		}

		internal abstract bool CheckCanDraw();
	}
}