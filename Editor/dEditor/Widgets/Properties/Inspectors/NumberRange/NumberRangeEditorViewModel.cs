// NumberRangeEditorViewModel.cs - dEditor
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System.Reflection;
using System.Windows;
using dEngine.Serializer.V1;

namespace dEditor.Widgets.Properties.Inspectors.NumberRange
{
    public class NumberRangeEditorViewModel : EditorBase<dEngine.NumberRange>, ILabelled
    {
        public NumberRangeEditorViewModel(object obj, Inst.CachedProperty propDesc) : base(obj, propDesc)
        {
        }

        public Visibility MinVisiblity { get; set; }
        public Visibility MaxVisiblity { get; set; }

        public double MinValue
        {
            get { return Value.Min; }
            set { Value = new dEngine.NumberRange((float)value, Value.Max); }
        }

        public double MaxValue
        {
            get { return Value.Max; }
            set { Value = new dEngine.NumberRange(Value.Min, (float)value); }
        }
    }
}