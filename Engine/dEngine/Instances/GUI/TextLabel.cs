// TextLabel.cs - dEngine
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
	/// A gui element which draws text.
	/// </summary>
	[TypeId(70), ToolboxGroup("GUI"), ExplorerOrder(19)]
	public class TextLabel : TextElement
	{
		/// <inheritdoc />
		public TextLabel()
		{
			Size = new UDim2(0, 200, 0, 50);
		}
	}
}