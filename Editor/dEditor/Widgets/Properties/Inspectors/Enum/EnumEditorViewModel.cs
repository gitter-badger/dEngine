// EnumEditorViewModel.cs - dEditor
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
using System.Linq;
using System.Reflection;
using Caliburn.Micro;
using dEngine.Serializer.V1;

namespace dEditor.Widgets.Properties.Inspectors.Enum
{
	public class EnumEditorViewModel : EditorBase<System.Enum>, ILabelled
	{
		private readonly List<EnumItem> _items;

		public EnumEditorViewModel(object obj, Inst.CachedProperty propDesc) : base(obj, propDesc)
		{
			_items = System.Enum.GetValues(propDesc.PropertyType).Cast<object>().Select(x => new EnumItem
			{
				Value = x,
				Text = System.Enum.GetName(propDesc.PropertyType, x)
			}).ToList();
		}

		public IEnumerable<EnumItem> Items => _items;
	}

	public class EnumItem : PropertyChangedBase
	{
		private string _text;
		public object Value { get; set; }

		public string Text
		{
			get { return _text; }
			set
			{
				if (value == _text) return;
				_text = value;
				NotifyOfPropertyChange();
			}
		}
	}
}