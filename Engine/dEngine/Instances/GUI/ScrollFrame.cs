// ScrollFrame.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System;
using dEngine.Data;
using dEngine.Graphics;
using dEngine.Instances.Attributes;
using SharpDX;

namespace dEngine.Instances
{
	/// <summary>
	/// A frame with a scrollbar.
	/// </summary>
	[TypeId(182)]
	public class ScrollFrame : Frame
	{
		private Content<Texture> _bottomImage;
		private Vector2 _canvasSize;
		private Content<Texture> _middleImage;
		private RectangleF _scrollBarBorderRectangleX;
		private RectangleF _scrollBarBorderRectangleY;

		private RectangleF _scrollBarRectangleX;
		private RectangleF _scrollBarRectangleY;
		private int _scrollBarThickness;
		private Vector2 _scrollOffset;
		private ScrollBarVisibility _scrollVisX;
		private ScrollBarVisibility _scrollVisY;
		private Content<Texture> _topImage;

		/// <inheritdoc />
		public ScrollFrame()
		{
			_scrollBarThickness = 12;
			_scrollVisX = ScrollBarVisibility.Auto;
			_scrollVisY = ScrollBarVisibility.Auto;
			_canvasSize = new Vector2(200, 200);
		}

		/// <summary>
		/// The image for the top of the scrollbar.
		/// </summary>
		[InstMember(1), EditorVisible("Scrolling")]
		public Content<Texture> TopImage
		{
			get { return _topImage; }
			set
			{
				_topImage = value;
				value.Subscribe(this, (id, a) => OnScrollbarTextureFetched(a, AlignmentY.Top));
				NotifyChanged();
			}
		}

		/// <summary>
		/// The image for the middle of the scrollbar.
		/// </summary>
		[InstMember(2), EditorVisible("Scrolling")]
		public Content<Texture> MiddleImage
		{
			get { return _middleImage; }
			set
			{
				_middleImage = value;
				value.Subscribe(this, (id, a) => OnScrollbarTextureFetched(a, AlignmentY.Middle));
				NotifyChanged();
			}
		}

		/// <summary>
		/// The texture for the bottom of the scrollbar.
		/// </summary>
		[InstMember(3), EditorVisible("Scrolling")]
		public Content<Texture> BottomImage
		{
			get { return _bottomImage; }
			set
			{
				_bottomImage = value;
				value.Subscribe(this, (id, a) => OnScrollbarTextureFetched(a, AlignmentY.Bottom));
				NotifyChanged();
			}
		}

		/// <summary>
		/// The visiblity of the horizontal scrollbar.
		/// </summary>
		[InstMember(4), EditorVisible("Scrolling")]
		public ScrollBarVisibility ScrollBarVisibilityX
		{
			get { return _scrollVisX; }
			set
			{
				if (value == _scrollVisX) return;
				_scrollVisX = value;
				NotifyChanged();
			}
		}

		/// <summary>
		/// The visiblity of the vertical scrollbar.
		/// </summary>
		[InstMember(5), EditorVisible("Scrolling")]
		public ScrollBarVisibility ScrollBarVisibilityY
		{
			get { return _scrollVisY; }
			set
			{
				if (value == _scrollVisY) return;
				_scrollVisY = value;
				NotifyChanged();
			}
		}

		/// <summary>
		/// The thickness of the scrollbars.
		/// </summary>
		[InstMember(6), EditorVisible("Scrolling")]
		public int ScrollBarThickness
		{
			get { return _scrollBarThickness; }
			set
			{
				if (value == _scrollBarThickness) return;
				_scrollBarThickness = value;
				NotifyChanged();
			}
		}

		/// <summary>
		/// The size of the scrollable area.
		/// </summary>
		[InstMember(7), EditorVisible("Scrolling")]
		public Vector2 CanvasSize
		{
			get { return _canvasSize; }
			set
			{
				if (value == _canvasSize) return;
				_canvasSize = value;
				Measure();
				NotifyChanged();
			}
		}

		/// <summary>
		/// The scroll offset of the canvas.
		/// </summary>
		[InstMember(8), EditorVisible("Scrolling")]
		public Vector2 ScrollOffset
		{
			get { return _scrollOffset; }
			set
			{
				if (value == _scrollOffset) return;
				_scrollOffset = value;
				NotifyChanged();
			}
		}

		/// <summary>
		/// Scrolls the canvas to the given coordinates.
		/// </summary>
		public void ScrollTo(int x, int y)
		{
			ScrollOffset = new Vector2(x, y);
		}

		/// <summary>
		/// Scrolls the canvas by the given coordinates.
		/// </summary>
		public void ScrollBy(int x, int y)
		{
			ScrollOffset += new Vector2(x, y);
		}

		private bool ShouldScrollBarBeVisible(Axis axis)
		{
			switch (axis)
			{
				case Axis.X:
					if (_scrollVisX == ScrollBarVisibility.Visible)
						return true;
					if (_scrollVisX == ScrollBarVisibility.Auto)
						return CanvasSize.X > AbsoluteSize.X;
					return false;
				case Axis.Y:
					if (_scrollVisY == ScrollBarVisibility.Visible)
						return true;
					if (_scrollVisY == ScrollBarVisibility.Auto)
						return CanvasSize.Y > AbsoluteSize.Y;
					return false;
				default:
					throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
			}
		}

		/// <inheritdoc />
		public override void Measure()
		{
			base.Measure();

			var left = _roundedRectangle.Rect.Left;
			var right = _roundedRectangle.Rect.Right;
			var top = _roundedRectangle.Rect.Top;
			var bottom = _roundedRectangle.Rect.Bottom;

			var xVis = ShouldScrollBarBeVisible(Axis.X);
			var yVis = ShouldScrollBarBeVisible(Axis.Y);

			if (yVis)
			{
				var offset = xVis ? _scrollBarThickness : 0;
				var p0 = new SharpDX.Vector2(right - _scrollBarThickness, top);
				var p1 = new SharpDX.Vector2(right, bottom - offset);
				_scrollBarRectangleY = new RectangleF(p0.X, p0.Y, p1.X - p0.X, p1.Y - p0.Y);
				_scrollBarBorderRectangleY = _scrollBarRectangleY;
				_scrollBarBorderRectangleY.Inflate(1, 1);
			}

			if (xVis)
			{
				var offset = yVis ? _scrollBarThickness : 0;
				var p0 = new Point((int)left, (int)(bottom - _scrollBarThickness));
				var p1 = new Point((int)right - offset, (int)(bottom));
				_scrollBarRectangleX = new RectangleF(p0.X, p0.Y, p1.X - p0.X, p1.Y - p0.Y);
				_scrollBarBorderRectangleX = _scrollBarRectangleX;
				_scrollBarBorderRectangleX.Inflate(1, 1);
			}
		}

		internal override void Draw()
		{
			base.Draw();

			var context = Renderer.Context2D;

			var xVis = ShouldScrollBarBeVisible(Axis.X);
			var yVis = ShouldScrollBarBeVisible(Axis.Y);

			var bgBrush = Renderer.Brushes.Get(BackgroundColour);
			var borderBrush = Renderer.Brushes.Get(BorderColour);

			if (yVis)
			{
				context.FillRectangle(_scrollBarBorderRectangleY, borderBrush);
				context.FillRectangle(_scrollBarRectangleY, bgBrush);
			}

			if (xVis)
			{
				context.FillRectangle(_scrollBarBorderRectangleX, borderBrush);
				context.FillRectangle(_scrollBarRectangleX, bgBrush);
			}
		}

		private void OnScrollbarTextureFetched(Texture texture, AlignmentY alignment)
		{
			throw new NotImplementedException();
		}
	}
}