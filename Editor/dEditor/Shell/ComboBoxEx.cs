// ComboBoxEx.cs - dEditor
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System.Windows;
using System.Windows.Controls;

namespace dEditor.Shell
{
    public class ComboBoxEx : ComboBox
    {
        private TextBox _textBox;

        public void SetCaret(int position)
        {
            _textBox.SelectionStart = position;
            _textBox.SelectionLength = 0;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var textBox = GetTemplateChild("PART_EditableTextBox") as TextBox;
            if (textBox != null)
            {
                _textBox = textBox;
            }

            _textBox.Padding = new Thickness(2);
        }
    }
}