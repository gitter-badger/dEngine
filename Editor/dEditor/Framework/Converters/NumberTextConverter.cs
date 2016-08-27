// NumberTextConverter.cs - dEditor
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
using System.Globalization;
using System.Windows.Data;

namespace dEditor.Framework.Converters
{
	public class NumberTextConverter : IValueConverter
	{
	    public int DecimalCount { get; set; } = 32;

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return ((double)value).ToString("0." + new string('#', DecimalCount));
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			string s = value as string;
			double result;
			var didParse = double.TryParse(s, out result);
			return string.IsNullOrEmpty(s) || !didParse ? 0 : result;
		}
	}
}