// LinesRenderer.cs - dEditor
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
using System.Windows.Media;

namespace dEditor.Widgets.Diagnostics.SharpTreeView
{
    class LinesRenderer : FrameworkElement
    {
        static LinesRenderer()
        {
            pen = new Pen(Brushes.LightGray, 1);
            pen.Freeze();
        }

        static Pen pen;

        SharpTreeNodeView NodeView
        {
            get { return TemplatedParent as SharpTreeNodeView; }
        }

        protected override void OnRender(DrawingContext dc)
        {
            var indent = NodeView.CalculateIndent();
            var p = new Point(indent + 4.5, 0);

            if (!NodeView.Node.IsRoot || NodeView.ParentTreeView.ShowRootExpander)
            {
                dc.DrawLine(pen, new Point(p.X, ActualHeight / 2), new Point(p.X + 10, ActualHeight / 2));
            }

            if (NodeView.Node.IsRoot) return;

            if (NodeView.Node.IsLast)
            {
                dc.DrawLine(pen, p, new Point(p.X, ActualHeight / 2));
            }
            else
            {
                dc.DrawLine(pen, p, new Point(p.X, ActualHeight));
            }

            var current = NodeView.Node;
            while (true)
            {
                p.X -= 19;
                current = current.Parent;
                if (p.X < 0) break;
                if (!current.IsLast)
                {
                    dc.DrawLine(pen, p, new Point(p.X, ActualHeight));
                }
            }
        }
    }
}