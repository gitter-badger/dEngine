// InspectorVisibleAttribute.cs - dEngine
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
	/// Attribute for defining inspector property group.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
	public class EditorVisibleAttribute : Attribute
	{
		/// <summary>
		/// Sets the group name in the inspector.
		/// </summary>
		/// <param name="group">The group name.</param>
		/// <param name="displayName">The name to display in the inspector.</param>
		/// <param name="unknown"></param>
		public EditorVisibleAttribute(string group = "Data", string displayName = null, object data = null)
		{
			Group = group;
			DisplayName = displayName;
			Data = data;
		}

		/// <summary>
		/// The group name.
		/// </summary>
		public string Group { get; }

		/// <summary>
		/// The group name.
		/// </summary>
		public string DisplayName { get; }

		public object Data { get; }
	}
}