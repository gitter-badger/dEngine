// CommandBarView.xaml.cs - dEditor
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Caliburn.Micro;

namespace dEditor.Shell.CommandBar
{
    /// <summary>
    /// Interaction logic for CommandBarView.xaml
    /// </summary>
    public partial class CommandBarView
    {
        private string _replacementChar;

        private bool _return;

        public CommandBarView()
        {
            InitializeComponent();
            DataObject.AddPastingHandler(ComboBox, OnPaste);
        }

        private void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            var isText = e.SourceDataObject.GetDataPresent(DataFormats.UnicodeText, true);
            if (!isText) return;
            _replacementChar = " ";
        }

        private void ComboBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !string.IsNullOrWhiteSpace(ComboBox.Text))
            {
                _replacementChar = "";
                IoC.Get<ICommandBar>().Execute(ComboBox.Text);
                _return = true;
            }
        }

        private void ComboBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var text = ComboBox.Text.Replace(Environment.NewLine, _replacementChar);
            ComboBox.Text = text;

            if (_return)
            {
                ComboBox.SetCaret(text.Length);
                _return = false;
            }
        }
    }
}