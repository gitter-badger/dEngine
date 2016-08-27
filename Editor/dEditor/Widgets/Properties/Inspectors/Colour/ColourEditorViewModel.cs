// ColourEditorViewModel.cs - dEditor
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
using System.Windows.Data;
using System.Windows.Media;
using dEngine.Serializer.V1;

namespace dEditor.Widgets.Properties.Inspectors.Colour
{
    public class ColourEditorViewModel : EditorBase<dEngine.Colour>, ILabelled
    {
        private double _alpha;
        private SolidColorBrush _brush;
        private double _hue;
        private double _luminance;
        private double _saturation;

        public ColourEditorViewModel(object obj, Inst.CachedProperty propDesc) : base(obj, propDesc)
        {
            NotifyOfPropertyChange(nameof(StringValue));
        }

        public string StringValue
        {
            get
            {
                if (AreMultipleValuesSame)
                    return
                        $"{Math.Round(255 * Value.r)}, {Math.Round(255 * Value.g)}, {Math.Round(255 * Value.b)}, {Math.Round(Value.a, 3)}";
                return null;
            }
            set { Value = dEngine.Colour.parseRGBA(value); }
        }
    }

    public class ColourConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            try
            {
                var c = (dEngine.Colour)value;
                var color = Color.FromArgb((byte)(c.a * 255), (byte)(c.r * 255), (byte)(c.g * 255), (byte)(c.b * 255));
                return color;
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
                var color = (Color)value;
                return dEngine.Colour.fromRGBA(color.R, color.G, color.B, color.A);
            }
            catch
            {
                return null;
            }
        }
    }
}