// BrushConverter.cs - dEditor
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System.Windows.Data;
using System.Windows.Media;

namespace dEditor.Utility
{

    public class BrushConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            try
            {
                var c = (dEngine.Colour)value;
                var color = Color.FromArgb((byte)(c.a * 255), (byte)(c.r * 255), (byte)(c.g * 255), (byte)(c.b * 255));
                var brush = new SolidColorBrush(color);
                return brush;
            }
            catch
            {
                return null;
            }
        }

        public object ConvertBack(object value, System.Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            try
            {
                var brush = (SolidColorBrush)value;
                var color = brush.Color;
                return dEngine.Colour.fromRGBA(color.R, color.G, color.B, color.A);
            }
            catch
            {
                return null;
            }
        }
    }
}