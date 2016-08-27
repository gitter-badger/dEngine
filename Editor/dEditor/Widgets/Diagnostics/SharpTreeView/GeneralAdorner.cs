// GeneralAdorner.cs - dEditor
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
using System.Windows.Documents;
using System.Windows.Media;

namespace dEditor.Widgets.Diagnostics.SharpTreeView
{
    public class GeneralAdorner : Adorner
    {
        public GeneralAdorner(UIElement target)
            : base(target)
        {
        }

        FrameworkElement child;

        public FrameworkElement Child
        {
            get
            {
                return child;
            }
            set
            {
                if (child != value)
                {
                    RemoveVisualChild(child);
                    RemoveLogicalChild(child);
                    child = value;
                    AddLogicalChild(value);
                    AddVisualChild(value);
                    InvalidateMeasure();
                }
            }
        }

        protected override int VisualChildrenCount
        {
            get { return child == null ? 0 : 1; }
        }

        protected override Visual GetVisualChild(int index)
        {
            return child;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            if (child != null)
            {
                child.Measure(constraint);
                return child.DesiredSize;
            }
            return new Size();
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (child != null)
            {
                child.Arrange(new Rect(finalSize));
                return finalSize;
            }
            return new Size();
        }
    }
}