// BoundPropertyInfo.cs - dEditor
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
using System.Reflection;
using dEngine.Instances;
using dEngine.Instances.Attributes;
using dEngine.Serializer.V1;

namespace dEditor.Framework
{
	public class BoundPropertyInfo : IEquatable<BoundPropertyInfo>
	{
		private object _value;

		public BoundPropertyInfo(object o, Inst.CachedProperty property, string displayName = null)
		{
			Object = o;
			Property = property;

		    IsReadOnly = property.Set == null || !property.IsSetterPublic;

            DisplayName = displayName ?? property.EditorVisible?.DisplayName ?? property.Name;

			UpdateValue();

			var instance = o as Instance;
			if (instance != null)
				instance.Changed.Event += OnInstanceChanged;
		}

		public object Object { get; }

		public Inst.CachedProperty Property { get; }

		public object Value
		{
			get { return _value; }
			set { Property?.FastSet(Object, value); }
		}

		public bool IsReadOnly { get; private set; }
		public string DisplayName { get; private set; }

		public bool Equals(BoundPropertyInfo other)
		{
			return Property == other.Property;
		}

		~BoundPropertyInfo()
		{
			var instance = Object as Instance;
			if (instance != null)
				instance.Changed.Event -= OnInstanceChanged;
		}

		private void OnInstanceChanged(string s)
		{
			if (s == Property.Name)
				UpdateValue();
		}

		private void UpdateValue()
		{
			_value = Property.FastGet(Object);
		}
	}
}