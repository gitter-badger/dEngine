using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using dEngine;
using dEngine.Instances.Materials;

namespace dEditor.Widgets.MaterialEditor.Nodes
{
    /// <summary>
    /// Interaction logic for NodeView.xaml
    /// </summary>
    public partial class NodeView
    {
        private bool _dragging;
        private Point _clickPos;
        private Dictionary<Slot, ConnectionLine> _lines;
        private Node _node;

        public NodeViewModel ViewModel => (NodeViewModel)DataContext;
        public MaterialEditorViewModel MaterialEditor => (MaterialEditorViewModel)Editor.Current.Shell.ActiveDocument;
        public Canvas Canvas => MaterialEditor.Canvas;

        private static int CurrentZIndex;

        public NodeView()
        {
            _lines = new Dictionary<Slot, ConnectionLine>();
            DataContextChanged += OnDataContextChanged;
            InitializeComponent();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            var vm = (NodeViewModel)args.NewValue;
            _node = vm.Node;
            _node.SlotConnected.Connect(NodeOnSlotConnected);
            _node.Destroyed.Connect(NodeOnDestroyed);
            _node.Changed.Connect(NodeOnChanged);
        }

        private void NodeOnChanged(string prop)
        {
            if (prop == nameof(Node.Position))
            {
                UpdateLines();
                foreach (var slot in _node.GetSlots(InOut.In))
                    slot.Node.Changed.Fire(nameof(Node.Position));
            }
        }

        private void NodeOnDestroyed()
        {
            foreach (var line in _lines)
            {
                line.Value.Dispose();
            }
            _lines.Clear();
        }

        private void NodeOnSlotConnected(Slot slot)
        {
            Dispatcher.Invoke(() =>
            {
                ConnectionLine line;
                if (_lines.TryGetValue(slot, out line))
                {
                    line.Dispose();
                }
                if (slot.Mode == InOut.In)
                {
                    if (slot.Output != null)
                    {
                        line = new ConnectionLine(Canvas, slot, slot.Output);
                        _lines[slot] = line;
                    }
                }
            });
        }

        private void UpdateLines()
        {
            foreach (var line in _lines.Values)
            {
                line.Update();
            }
        }

        public int ZIndex { get; set; }

        private void NodeView_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            _dragging = true;
            _clickPos = e.GetPosition(this);
            ZIndex = ++CurrentZIndex;
            CaptureMouse();
        }

        private void NodeView_OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_dragging)
            {
                var delta = e.GetPosition(Canvas) - _clickPos;
                ViewModel.Node.Position = new Vector2(RoundOff10((float)delta.X), RoundOff10((float)delta.Y));
            }
        }

        private void NodeView_OnPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            _dragging = false;
            ReleaseMouseCapture();
        }

        public static float RoundOff10(float i)
        {
            return ((float)Math.Round(i / 10.0)) * 10;
        }
    }
}
