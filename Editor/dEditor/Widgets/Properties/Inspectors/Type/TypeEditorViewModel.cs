// TypeEditorViewModel.cs - dEditor
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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using dEngine;
using dEngine.Serializer.V1;

namespace dEditor.Widgets.Properties.Inspectors.Type
{
	public class TypeEditorViewModel : EditorBase<System.Type>, ILabelled
	{
		public TypeEditorViewModel(object obj, Inst.CachedProperty propDesc, object data) : base(obj, propDesc)
		{
			var filter = (string)data;

			switch (filter)
			{
				case "Enum":
					Items = Inst.TypeDictionary.Values.Where(t => t.IsEnum).Select(t => t.Type);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(filter), "Unsupported type filter.");
			}
		}

		public IEnumerable<System.Type> Items { get; }
	}
}