// ColourSequenceEditorViewModel.cs - dEditor
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Reflection;
using System.Windows.Media;
using dEngine.Serializer.V1;

namespace dEditor.Widgets.Properties.Inspectors.ColourSequence
{
    public class ColourSequenceEditorViewModel : SequenceEditorBase<dEngine.Colour>, ILabelled
    {
        private readonly dEngine.Instances.Instance _instance;
        private readonly Inst.CachedProperty _propDesc;

        public ColourSequenceEditorViewModel(object obj, Inst.CachedProperty propDesc) : base(obj, propDesc)
        {
            GradientStops = new GradientStopCollection();
            _instance = (dEngine.Instances.Instance)obj;
            _propDesc = propDesc;
        }

        public GradientStopCollection GradientStops { get; set; }

        public void OnClick()
        {
            OpenCurveDialog();
        }

        private void OpenCurveDialog()
        {
            var curveEditor = new CurveDialogViewModel(_instance, _propDesc.Name);
            Editor.Current.Shell.ShowDialog(curveEditor);
        }
    }
}