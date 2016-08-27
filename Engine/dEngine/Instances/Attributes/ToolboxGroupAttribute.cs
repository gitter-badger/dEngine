﻿// ToolboxGroupAttribute.cs - dEngine
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

namespace dEngine.Instances.Attributes
{
	/// <summary>
	/// Attribute for group names in the toolbox.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
	public class ToolboxGroupAttribute : Attribute, IValueAttribute
	{
		/// <summary>
		/// Sets the group name.
		/// </summary>
		public ToolboxGroupAttribute(string groupName)
		{
			GroupName = groupName;
		}

		/// <summary>
		/// The group name.
		/// </summary>
		public string GroupName { get; }

	    string IValueAttribute.GetValue()
	    {
	        return GroupName;
	    }
	}
}