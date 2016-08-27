// ConnectionLine.cs - dEditor
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
using System.Windows.Controls;
using dEditor.Widgets.MaterialEditor.Nodes;
using dEngine;
using dEngine.Instances.Materials;

namespace dEditor.Widgets.MaterialEditor
{
    public class ConnectionLine : IDisposable
    {
        private readonly Canvas _canvas;
        private readonly BezierLine _bezier;
        private readonly Slot _input;
        private readonly Slot _output;

        public ConnectionLine(Canvas canvas, Slot input, Slot output)
        {
            _canvas = canvas;
            _bezier = new BezierLine();
            canvas.Children.Add(_bezier);
            _input = input;
            _output = output;
        }

        public void Update()
        {
            var inputPosition = _input.Node.Position + new Vector2(_input.Node.Size.x, 0);
            var outputPosition = _output.Node.Position + new Vector2(0, 0);

            _bezier.X1 = inputPosition.X;
            _bezier.Y1 = inputPosition.Y;

            _bezier.X2 = outputPosition.X;
            _bezier.Y2 = outputPosition.Y;
        }

        public void Dispose()
        {
            _canvas.Children.Remove(_bezier);
        }
    }
}