// BreakpointMargin.cs - dEditor
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
using System.Windows.Input;
using System.Windows.Media;
using dEditor.Instances;
using dEngine.Instances;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Rendering;

namespace dEditor.Widgets.CodeEditor
{
	public class BreakpointMargin : AbstractMargin
	{
		private const int _margin = 20;
		private readonly ScriptDebugger _debugger;
		private readonly TextEditor _editor;
		private readonly Pen _pen;
		private readonly LuaSourceContainer _script;

		public BreakpointMargin(LuaSourceContainer script, TextEditor editor)
		{
			_script = script;
			_editor = editor;

			_pen = new Pen(Brushes.White, 4);

			_debugger = script.Debugger;

			_debugger.BreakpointAdded.Event += bp =>
			{
				var line = _editor.Document.GetLineByNumber(bp.Line);

				var offset = _editor.Document.GetOffset(bp.Line, 1);
				var anchor = _editor.Document.CreateAnchor(offset);
				anchor.MovementType = AnchorMovementType.AfterInsertion;
				anchor.Deleted += (s, e) => bp.Destroy();
				bp.Tag = anchor;
			};

			_debugger.BreakpointRemoved.Event += bp => { };
		}

		protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
		{
			return new PointHitTestResult(this, hitTestParameters.HitPoint);
		}

		protected override Size MeasureOverride(Size availableSize)
		{
			return new Size(_margin, 0);
		}

		protected override void OnMouseDown(MouseButtonEventArgs e)
		{
			var position = _editor.GetPositionFromPoint(e.GetPosition(_editor));

			if (position.HasValue)
			{
				var lineNum = position.Value.Line;
				DebuggerBreakpoint bp;

				if ((bp = _debugger.GetBreakpoints().FirstOrDefault(x => x.Line == lineNum)) != null)
				{
					bp.Destroy();
				}
				else
				{
					bp = _debugger.SetBreakpoint(lineNum);

					var anchor = _editor.Document.CreateAnchor(position.Value.Line);
					anchor.Deleted += (sender, args) => bp.Destroy();
				}
			}
		}

		private static Point Round(Point point, Size pixelSize)
		{
			return new Point(Round(point.X, pixelSize.Width), Round(point.Y, pixelSize.Height));
		}

		private static Rect Round(Rect rect, Size pixelSize)
		{
			return new Rect(Round(rect.X, pixelSize.Width), Round(rect.Y, pixelSize.Height),
				Round(rect.Width, pixelSize.Width), Round(rect.Height, pixelSize.Height));
		}

		private static double Round(double value, double pixelSize)
		{
			return pixelSize * Math.Round(value / pixelSize, MidpointRounding.AwayFromZero);
		}

		private static Size GetPixelSize(Visual visual)
		{
			if (visual == null)
				throw new ArgumentNullException(nameof(visual));
			PresentationSource source = PresentationSource.FromVisual(visual);
			if (source != null)
			{
				Matrix matrix = source.CompositionTarget.TransformFromDevice;
				return new Size(matrix.M11, matrix.M22);
			}
			return new Size(1, 1);
		}

		/*
        protected override void OnTextViewChanged(TextView oldTextView, TextView newTextView)
        {
            if (oldTextView != null)
            {
                oldTextView.VisualLinesChanged -= OnRedrawRequested;
            }
            base.OnTextViewChanged(oldTextView, newTextView);
            if (newTextView != null)
            {
                newTextView.VisualLinesChanged += OnRedrawRequested;
            }
            InvalidateVisual();
        }

        void OnRedrawRequested(object sender, EventArgs e)
        {
            // Don't invalidate the IconBarMargin if it'll be invalidated again once the
            // visual lines become valid.
            if (this.TextView != null && this.TextView.VisualLinesValid)
            {
                InvalidateVisual();
            }
        }
        */

		protected override void OnRender(DrawingContext drawingContext)
		{
			const float radius = 16;

			var t = IsHitTestVisible;

			var textView = _editor.TextArea.TextView;

			if (textView != null && textView.VisualLinesValid)
			{
				foreach (var bp in _debugger.GetBreakpoints())
				{
					var anchor = (TextAnchor)bp.Tag;

					var line = textView.GetVisualLine(anchor.Line);

					if (line == null)
						continue;

					var pixelSize = GetPixelSize(this);
					var lineMiddle =
						line.GetTextLineVisualYPosition(line.TextLines[0], VisualYPosition.TextMiddle) -
						textView.VerticalOffset;
					var rect = new Rect(0, Round(lineMiddle - 8, pixelSize.Height), radius, radius);

					drawingContext.DrawRoundedRectangle(Brushes.Red, _pen, rect, radius,
						radius);
				}
			}
		}
	}
}