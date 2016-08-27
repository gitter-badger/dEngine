using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Caliburn.Micro;
using dEditor.Widgets.CodeEditor;
using dEditor.Widgets.StartPage;
using dEngine;
using dEngine.Instances;
using Key = System.Windows.Input.Key;

namespace dEditor.Widgets.Explorer
{
    /// <summary>
    /// Interaction logic for ExplorerView.xaml
    /// </summary>
    public partial class ExplorerView
    {
        private int _startClickY;
        private int _dist;
        internal const int ItemHeight = 23;

        public ExplorerView()
        {
            InitializeComponent();
        }

        private void TreeView_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void TreeViewItem_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is ItemsPresenter)
                return;

            var treeViewItem = (TreeViewItem)sender;
            var item = (ExplorerItem)treeViewItem.DataContext;

            if (item == _startingItem && item.IsSelected)
                return;

            var controlHeld = Keyboard.IsKeyDown(Key.LeftCtrl);
            var shiftHeld = Keyboard.IsKeyDown(Key.LeftShift);

            var y = (int)treeViewItem.PointToScreen(new Point()).Y;

            if (shiftHeld)
            {
                Game.Selection.Select(_startingItem.Instance);
                ShiftSelect(item, y);
            }
            else
            {
                if (!item.Instance.IsSelected)
                    Game.Selection.Select(item.Instance, !controlHeld);
                else if (controlHeld)
                    Game.Selection.Deselect(item.Instance);
                else
                    Game.Selection.Select(item.Instance, true);
            }

            if (!shiftHeld)
            {
                _startClickY = y;
                _startingItem = item;
                ((ExplorerViewModel)DataContext).LastClickedInstance = item.Instance;
            }
        }

        private void ShiftSelect(ExplorerItem item, int clickY)
        {
            if (_shiftSelection != null)
                foreach (var inst in _shiftSelection)
                    Game.Selection.Deselect(inst, false);

            _shiftSelection = new List<Instance>();

            int cur = ItemHeight;
            _dist = Math.Abs(clickY - _startClickY);

            Traverse(_startingItem, ref cur, true);

            foreach (var inst in _shiftSelection)
                Game.Selection.Select(inst);
        }

        private bool Traverse(ExplorerItem item, ref int cur, bool root = false, int offset = 0, ExplorerItem test = null)
        {
            if (offset == 0 && !root)
            {
                cur += ItemHeight;
                _shiftSelection.Add(item.Instance);
            }

            if (cur >= _dist)
                return true;

            if (item.IsExpanded)
            {
                var items = item.Items;
                var count = items.Count;
                for (int i = offset; i < count; i++)
                {
                    var child = items[i];
                    if (child == test)
                        continue;
                    if (Traverse(child, ref cur))
                        return true;
                }
            }

            var parent = item.Parent;

            if (parent != null)
            {
                if (parent.Parent != null && item.IsLastChild)
                    Traverse(parent.Parent, ref cur, true, parent.ChildIndex + 1, parent);
                else
                    Traverse(parent, ref cur, true, item.ChildIndex + 1, parent);
            }

            return false;
        }

        private List<Instance> _shiftSelection;
        private ExplorerItem _startingItem;

        private void TreeViewItem_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
        }

        private void TreeView_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is ItemsPresenter) return;
            var item = (ExplorerItem)((FrameworkElement)e.OriginalSource).DataContext;

            Script script;
            if ((script = item.Instance as Script) != null)
            {
                CodeEditorViewModel.TryOpenScript(script);
            }
        }
    }
}
