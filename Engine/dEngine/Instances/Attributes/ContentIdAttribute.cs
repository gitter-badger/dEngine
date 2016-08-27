// ContentIdAttribute.cs - dEngine
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
	/// An attribute for telling the level editor what this string property represents a content url.
	/// </summary>
	[LevelEditorRelated]
	public class ContentIdAttribute : Attribute
	{
		/// <summary>
		/// </summary>
		public ContentIdAttribute(ContentType type)
		{
			ContentType = type;
		}

		/// <summary>
		/// The type of content this string represents.
		/// </summary>
		public ContentType ContentType { get; }
	}
}