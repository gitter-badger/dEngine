// Folder.cs - dEngine
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
using dEngine.Instances.Attributes;


namespace dEngine.Instances
{
	/// <summary>
	/// A container for organizing objects.
	/// </summary>
	/// <seealso cref="Model" />
	[TypeId(14), ToolboxGroup("Containers"), ExplorerOrder(2)]
	public class Folder : Instance
	{
		private int _hue;

		/// <summary>
		/// The hue of the folder.
		/// </summary>
		[InstMember(1), EditorVisible("Hue"), Range(0, 360)]
		public int Hue
		{
			get { return _hue; }
			set
			{
				if (value == _hue) return;
				value = Math.Max(0, Math.Min(value, 360));
				_hue = value;
				NotifyChanged();
			}
		}
	}
}