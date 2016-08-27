// BoundPropertyDescriptor.cs - dEditor
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
using System.ComponentModel;

namespace dEditor.Widgets.Properties
{
	public class BoundPropertyDescriptor
	{
		public BoundPropertyDescriptor(object propertyOwner, PropertyDescriptor propertyDescriptor)
		{
			PropertyOwner = propertyOwner;
			PropertyDescriptor = propertyDescriptor;
		}

		public PropertyDescriptor PropertyDescriptor { get; }
		public object PropertyOwner { get; }

		public object Value
		{
			get { return PropertyDescriptor.GetValue(PropertyOwner); }
			set { PropertyDescriptor.SetValue(PropertyOwner, value); }
		}

		public event EventHandler ValueChanged
		{
			add { PropertyDescriptor.AddValueChanged(PropertyOwner, value); }
			remove { PropertyDescriptor.RemoveValueChanged(PropertyOwner, value); }
		}

		public static BoundPropertyDescriptor FromProperty(object propertyOwner, string propertyName)
		{
			// TODO: ContentCache all this.
			var properties = TypeDescriptor.GetProperties(propertyOwner);
			return new BoundPropertyDescriptor(propertyOwner, properties.Find(propertyName, false));
		}
	}
}