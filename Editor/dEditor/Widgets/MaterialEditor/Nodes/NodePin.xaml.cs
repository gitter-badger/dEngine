// NodePin.xaml.cs - dEditor
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
using System.Windows.Media;
using dEngine;

namespace dEditor.Widgets.MaterialEditor.Nodes
{
    /// <summary>
    /// Interaction logic for InputPin.xaml
    /// </summary>
    public partial class NodePin
    {
        public static readonly DependencyProperty PinBrushProperty = DependencyProperty.Register("PinBrush",
            typeof(Brush), typeof(NodePin), new PropertyMetadata(Brushes.White));

        public static readonly DependencyProperty ModeProperty = DependencyProperty.Register("Mode", typeof(InOut?),
            typeof(NodePin), new PropertyMetadata(default(InOut?), PropertyChangedCallback));

        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register("Header", typeof(string),
            typeof(NodePin), new PropertyMetadata(""));

        public NodePin()
        {
            InitializeComponent();
        }

        public Brush PinBrush
        {
            get { return (Brush)GetValue(PinBrushProperty); }
            set { SetValue(PinBrushProperty, value); }
        }

        public InOut? Mode
        {
            get { return (InOut?)GetValue(ModeProperty); }
            set { SetValue(ModeProperty, value); }
        }

        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        private const int Offset = 10;

        private static void PropertyChangedCallback(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var value = (InOut)obj.GetValue(args.Property);
            var nodePin = (NodePin)obj;

            if (value == InOut.In)
            {
                nodePin.PinCircle.RenderTransform = new TranslateTransform(-Offset, 0);
                DockPanel.SetDock(nodePin.PinCircle, System.Windows.Controls.Dock.Left);
                DockPanel.SetDock(nodePin.NodeLabel, System.Windows.Controls.Dock.Right);
            }
            else if (value == InOut.Out)
            {
                nodePin.PinCircle.RenderTransform = new TranslateTransform(Offset, 0);
                DockPanel.SetDock(nodePin.PinCircle, System.Windows.Controls.Dock.Right);
                DockPanel.SetDock(nodePin.NodeLabel, System.Windows.Controls.Dock.Left);
            }
        }
    }
}