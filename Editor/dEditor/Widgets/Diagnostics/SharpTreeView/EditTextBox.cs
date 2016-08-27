// EditTextBox.cs - dEditor
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System.Net.Mime;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace dEditor.Widgets.Diagnostics.SharpTreeView
{
    public class EditTextBox : TextBox
    {
        static EditTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(EditTextBox),
                new FrameworkPropertyMetadata(typeof(EditTextBox)));
        }

        public EditTextBox()
        {
            Loaded += delegate { Init(); };
        }

        public SharpTreeViewItem Item { get; set; }

        public SharpTreeNode Node
        {
            get { return Item.Node; }
        }

        void Init()
        {
            Text = Node.LoadEditText();
            Focus();
            SelectAll();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Commit();
            }
            else if (e.Key == Key.Escape)
            {
                Node.IsEditing = false;
            }
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            if (Node.IsEditing)
            {
                Commit();
            }
        }

        bool commiting;

        void Commit()
        {
            if (!commiting)
            {
                commiting = true;

                Node.IsEditing = false;
                if (!Node.SaveEditText(Text))
                {
                    Item.Focus();
                }
                Node.RaisePropertyChanged("Text");

                //if (Node.SaveEditText(Text)) {
                //    Node.IsEditing = false;
                //    Node.RaisePropertyChanged("Text");
                //}
                //else {
                //    Init();
                //}

                commiting = false;
            }
        }
    }
}