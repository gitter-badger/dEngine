// NodeViewModel.cs - dEditor
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
using Caliburn.Micro;
using dEngine;
using dEngine.Instances.Materials;
using dEngine.Instances.Materials.Nodes;

namespace dEditor.Widgets.MaterialEditor.Nodes
{
    public class NodeViewModel : Screen
    {
        private bool _isFolded;
        private string _subName;
        private Point _position;
        private NodeContent _nodeContent;

        public Node Node { get; }

        public NodeViewModel(Node node)
        {
            Node = node;
            Position = new Point(node.Position.X, node.Position.Y);
            node.Changed.Connect(NodeChanged);

            switch (node.GetType().Name)
            {
                case nameof(TextureParameterNode):
                    NodeContent = new Views.TextureNodeView();
                    break;
                case nameof(FinalNode):
                    NodeContent = new Views.MainNodeView();
                    break;
            }
        }

        public override string DisplayName => Node.Name;

        public string SubName
        {
            get { return _subName; }
            set
            {
                if (value == _subName) return;
                _subName = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(SubNameVisible));
            }
        }

        public bool IsFolded
        {
            get { return _isFolded; }
            set
            {
                if (value == _isFolded) return;
                _isFolded = value;
                NotifyOfPropertyChange();
            }
        }

        public Point Position
        {
            get { return _position; }
            set
            {
                if (value.Equals(_position)) return;
                _position = value;
                NotifyOfPropertyChange();
            }
        }

        public NodeContent NodeContent
        {
            get { return _nodeContent; }
            set
            {
                if (Equals(value, _nodeContent)) return;
                _nodeContent = value;
                NotifyOfPropertyChange();
            }
        }

        public bool SubNameVisible => !string.IsNullOrEmpty(_subName);

        private void NodeChanged(string prop)
        {
            switch (prop)
            {
                case nameof(Node.Name):
                    NotifyOfPropertyChange(nameof(DisplayName));
                    break;
                case nameof(Node.Position):
                    Position = new Point(Node.Position.X, Node.Position.Y);
                    break;
            }
        }
    }
}