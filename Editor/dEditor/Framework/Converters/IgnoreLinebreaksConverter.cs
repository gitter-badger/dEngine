// IgnoreLinebreaksConverter.cs - dEditor
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
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace dEditor.Framework.Converters
{
    public class IgnoreLinebreaksConverter : IValueConverter
    {
        private const string Sub = " \u200B";
        private const string Lb = "\r\n";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var s = (string)value;
            return string.IsNullOrEmpty(s) ? s : Regex.Replace(s, Lb, Sub);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var s = (string)value;
            return string.IsNullOrEmpty(s) ? s : Regex.Replace(s, Sub, Lb);
        }
    }
}