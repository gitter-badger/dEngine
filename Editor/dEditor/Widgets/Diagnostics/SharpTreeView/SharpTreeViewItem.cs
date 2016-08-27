// SharpTreeViewItem.cs - dEditor
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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace dEditor.Widgets.Diagnostics.SharpTreeView
{
    public class SharpTreeViewItem : ListViewItem
    {
        static SharpTreeViewItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SharpTreeViewItem),
                                                     new FrameworkPropertyMetadata(typeof(SharpTreeViewItem)));
        }

        public SharpTreeNode Node
        {
            get { return DataContext as SharpTreeNode; }
        }

        public SharpTreeNodeView NodeView { get; internal set; }
        public SharpTreeView ParentTreeView { get; internal set; }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.F2:
                    //					if (SharpTreeNode.ActiveNodes.Count == 1 && Node.IsEditable) {
                    //						Node.IsEditing = true;
                    //						e.Handled = true;
                    //					}
                    break;
                case Key.Escape:
                    Node.IsEditing = false;
                    break;
            }
        }

        #region Mouse

        Point startPoint;
        bool wasSelected;
        bool wasDoubleClick;

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            wasSelected = IsSelected;
            if (!IsSelected)
            {
                base.OnMouseLeftButtonDown(e);
            }

            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                startPoint = e.GetPosition(null);
                CaptureMouse();

                if (e.ClickCount == 2)
                {
                    wasDoubleClick = true;
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (IsMouseCaptured)
            {
                var currentPoint = e.GetPosition(null);
                if (Math.Abs(currentPoint.X - startPoint.X) >= SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(currentPoint.Y - startPoint.Y) >= SystemParameters.MinimumVerticalDragDistance)
                {

                    var selection = ParentTreeView.GetTopLevelSelection().ToArray();
                    if (Node.CanDrag(selection))
                    {
                        Node.StartDrag(this, selection);
                    }
                }
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (wasDoubleClick)
            {
                wasDoubleClick = false;
                Node.ActivateItem(e);
                if (!e.Handled)
                {
                    if (!Node.IsRoot || ParentTreeView.ShowRootExpander)
                    {
                        Node.IsExpanded = !Node.IsExpanded;
                    }
                }
            }

            ReleaseMouseCapture();
            if (wasSelected)
            {
                base.OnMouseLeftButtonDown(e);
            }
        }

        #endregion

        #region Drag and Drop

        protected override void OnDragEnter(DragEventArgs e)
        {
            ParentTreeView.HandleDragEnter(this, e);
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            ParentTreeView.HandleDragOver(this, e);
        }

        protected override void OnDrop(DragEventArgs e)
        {
            ParentTreeView.HandleDrop(this, e);
        }

        protected override void OnDragLeave(DragEventArgs e)
        {
            ParentTreeView.HandleDragLeave(this, e);
        }

        #endregion
    }
}