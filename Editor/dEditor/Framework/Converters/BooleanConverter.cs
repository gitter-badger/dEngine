// BooleanConverter.cs - dEditor
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
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace dEditor.Framework.Converters
{
	public class BooleanConverter<T> : IValueConverter
	{
		public BooleanConverter(T trueValue, T falseValue)
		{
			True = trueValue;
			False = falseValue;
		}

		public T True { get; set; }
		public T False { get; set; }

		public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value is bool && ((bool)value) ? True : False;
		}

		public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value is T && EqualityComparer<T>.Default.Equals((T)value, True);
		}
	}

	public sealed class BooleanToVisibilityConverter : BooleanConverter<Visibility>
	{
		public BooleanToVisibilityConverter() :
			base(Visibility.Visible, Visibility.Collapsed)
		{
		}
	}

	public sealed class BooleanInverter : BooleanConverter<bool>
	{
		public BooleanInverter() :
			base(true, false)
		{
		}
	}
}