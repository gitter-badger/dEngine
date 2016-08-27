// MemberEntry.cs - dEditor
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
using System.Diagnostics;
using System.Reflection;

namespace dEditor.Widgets.ObjectBrowser
{
	public class MemberEntry : Entry
	{
		protected MemberEntry()
		{
		}

		public MemberEntry(MemberInfo member)
		{
			Name = member.Name;
			Icon = new Uri("/dEditor;component/Content/Icons/Toolbar/Member_16xMD.png", UriKind.Relative);

			switch (member.MemberType)
			{
				case MemberTypes.Field:
					var fieldType = ((FieldInfo)member).FieldType;
					if (fieldType.Name.StartsWith("Signal"))
					{
						Icon = new Uri("/dEditor;component/Content/Icons/Toolbar/Event_orange_16x.png", UriKind.Relative);
					}
					break;
				case MemberTypes.Method:
					Icon = new Uri("/dEditor;component/Content/Icons/Toolbar/Method_purple_16x.png", UriKind.Relative);
					break;
				case MemberTypes.Property:
					Icon = new Uri("/dEditor;component/Content/Icons/Toolbar/properties_16xMD.png", UriKind.Relative);
					break;
			}
		}

		private static bool IsAssignableFrom(Type extendType, Type baseType)
		{
			while (!baseType.IsAssignableFrom(extendType))
			{
				if (extendType == typeof(object))
				{
					return false;
				}
				Debug.Assert(extendType != null, "extendType != null");
				if (extendType.IsGenericType && !extendType.IsGenericTypeDefinition)
				{
					extendType = extendType.GetGenericTypeDefinition();
				}
				else
				{
					extendType = extendType.BaseType;
				}
			}
			return true;
		}
	}
}