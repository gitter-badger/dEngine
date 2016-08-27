// CurveEditorViewModel.cs - dEditor
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using Caliburn.Micro;
using dEditor.Framework;

namespace dEditor.Widgets.Properties.Inspectors
{
    public class CurveDialogViewModel : Dialog
    {
        public CurveDialogViewModel(dEngine.Instances.Instance instance, string propertyName)
        {
            DisplayName = $"{instance.GetFullName()}.{propertyName}";
            Width = MinWidth = 800;
            Height = MinHeight = 250;
        }
    }
}