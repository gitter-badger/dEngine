// InspectorGrid.cs - dEditor
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
using System.Windows;

namespace dEditor.Widgets.Properties
{
	public class InspectorGrid
	{
		private static GridLength _propertyNameColumnWidth = new GridLength(1, GridUnitType.Star);


		private static GridLength _propertyValueColumnWidth = new GridLength(1.5, GridUnitType.Star);

		public static GridLength PropertyNameColumnWidth
		{
			get { return _propertyNameColumnWidth; }
			set
			{
				_propertyNameColumnWidth = value;
				var handler = PropertyNameColumnWidthChanged;
				if (handler != null)
					handler(null, EventArgs.Empty);
			}
		}

		public static GridLength PropertyValueColumnWidth
		{
			get { return _propertyValueColumnWidth; }
			set
			{
				_propertyValueColumnWidth = value;
				var handler = PropertyValueColumnWidthChanged;
				if (handler != null)
					handler(null, EventArgs.Empty);
			}
		}

		public static event EventHandler PropertyNameColumnWidthChanged;
		public static event EventHandler PropertyValueColumnWidthChanged;
	}
}