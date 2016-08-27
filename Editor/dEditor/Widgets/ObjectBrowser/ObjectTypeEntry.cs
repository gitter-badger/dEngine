// ObjectTypeEntry.cs - dEditor
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System;
using dEditor.Framework.Services;
using dEngine.Instances;

namespace dEditor.Widgets.ObjectBrowser
{
	public class ObjectTypeEntry : Entry
	{
		public ObjectTypeEntry(Type type)
		{
			Name = type.Name;
			Type = type;

			if (typeof(Instance).IsAssignableFrom(type))
			{
				Icon = IconProvider.GetIconUri(type);
			}
			else if (type.IsEnum)
			{
				Icon = new Uri("/dEditor;component/Content/Icons/Toolbar/Enumerator_orange_16x.png", UriKind.Relative);
			}
			else
			{
				Icon = new Uri("/dEditor;component/Content/Icons/Toolbar/Class_yellow_16x.png", UriKind.Relative);
			}
		}

		public Type Type { get; }
	}
}