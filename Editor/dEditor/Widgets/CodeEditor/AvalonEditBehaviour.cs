// AvalonEditBehaviour.cs - dEditor
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
using System.Windows.Interactivity;
using ICSharpCode.AvalonEdit;

namespace dEditor.Widgets.CodeEditor
{
    public sealed class AvalonEditBehaviour : Behavior<TextEditor>
    {
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(AvalonEditBehaviour),
                new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    PropertyChangedCallback));

        public static readonly DependencyProperty SelectedTextProperty = DependencyProperty.Register(
            "SelectedText", typeof(string), typeof(AvalonEditBehaviour),
            new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                PropertyChangedCallback));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public string SelectedText
        {
            get { return (string)GetValue(SelectedTextProperty); }
            set { SetValue(SelectedTextProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            if (AssociatedObject != null)
            {
                AssociatedObject.TextChanged += AssociatedObjectOnTextChanged;
                AssociatedObject.TextArea.SelectionChanged += TextAreaOnSelectionChanged;
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            if (AssociatedObject != null)
            {
                AssociatedObject.TextChanged -= AssociatedObjectOnTextChanged;
                AssociatedObject.TextArea.SelectionChanged -= TextAreaOnSelectionChanged;
            }
        }

        private void TextAreaOnSelectionChanged(object sender, EventArgs eventArgs)
        {
            var textEditor = sender as TextEditor;
            if (textEditor?.Document != null)
                Text = textEditor.SelectedText;
        }

        private void AssociatedObjectOnTextChanged(object sender, EventArgs eventArgs)
        {
            var textEditor = sender as TextEditor;
            if (textEditor?.Document != null)
                Text = textEditor.Document.Text;
        }

        private static void PropertyChangedCallback(
            DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var behavior = dependencyObject as AvalonEditBehaviour;
            var editor = behavior?.AssociatedObject;
            if (editor?.Document == null) return;
            var caretOffset = editor.CaretOffset;
            editor.Document.Text = dependencyPropertyChangedEventArgs.NewValue.ToString();
            editor.CaretOffset = caretOffset;
        }
    }
}