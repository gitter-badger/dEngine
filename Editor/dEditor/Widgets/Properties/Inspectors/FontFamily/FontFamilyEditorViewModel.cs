// FontFamilyEditorViewModel.cs - dEditor
// Copyright (C) 2016-2016 DanDevPC (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//  
// You should have received a copy of the GNU General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using dEngine.Serializer.V1;

namespace dEditor.Widgets.Properties.Inspectors.FontFamily
{
    public class FontFamilyEditorViewModel : EditorBase<dEngine.FontFamily>, ILabelled
    {
        public FontFamilyEditorViewModel(object obj, Inst.CachedProperty propDesc) : base(obj, propDesc)
        {
        }

        public System.Windows.Media.FontFamily FontFamilyValue
        {
            get { return new System.Windows.Media.FontFamily(Value); }
            set { Value = new dEngine.FontFamily(value.ToString()); }
        }
    }
}