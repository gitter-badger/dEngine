// IEditor.cs - dEditor
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Reflection;
using dEditor.Framework;
using dEngine.Serializer.V1;

namespace dEditor.Widgets.Properties
{
	public interface IEditor
	{
		string DisplayName { get; set; }
		string Description { get; }
		bool IsReadOnly { get; }
		PropertiesViewModel PropertiesWidget { get; set; }
		Dictionary<object, BoundPropertyInfo> Objects { get; }
		BoundPropertyInfo AddObject(object obj, Inst.CachedProperty descriptor);
		void RemoveObject(object obj);
	}
}