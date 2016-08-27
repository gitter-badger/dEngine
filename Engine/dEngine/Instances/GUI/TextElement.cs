// TextElement.cs - dEngine
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
using System.Diagnostics;
using dEngine.Graphics;
using dEngine.Instances.Attributes;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct2D1.Effects;
using SharpDX.DirectWrite;

namespace dEngine.Instances
{
	/// <summary>
	/// Base class for text elements.
	/// </summary>
	[TypeId(157)]
	public abstract class TextElement : GuiElement
	{
		private FontFamily _font;
		private float _fontSize;
		private SharpDX.DirectWrite.FontWeight _fontWeight;
	    private Shadow _shadowEffect;
	    private CustomTextRenderer _textRenderer;
		private AlignmentX _textAlignmentX;
		private AlignmentY _textAlignmentY;
		private Colour _textColour;
		private Colour _textStrokeColour;
	    private WordWrapping _wordWrapping;
        internal TextFormat _textFormat;
        internal TextLayout _textLayout;
        protected string _text;
	    private bool _textReady;

	    /// <inheritdoc />
        protected TextElement()
		{
			_font = "Roboto";
			_text = "Label";
			_fontSize = 16.0f;
			_fontWeight = SharpDX.DirectWrite.FontWeight.Normal;
			_textAlignmentX = AlignmentX.Center;
			_textAlignmentY = AlignmentY.Middle;
			_wordWrapping = WordWrapping.NoWrap;
            TextColour = Colour.Black;
			TextStrokeColour = new Colour(0, 0, 0, 0);
		    _textReady = true;
		}

	    private void UpdateTextBounds()
	    {
            TextBounds = new Vector2(_textLayout.Metrics.WidthIncludingTrailingWhitespace, _textLayout.Metrics.Height);
        }

		/// <summary>
		/// The size of the space the text takes up.
		/// </summary>
		[EditorVisible("Text")]
		public Vector2 TextBounds { get; private set; }

		/// <summary>
		/// The text to draw.
		/// </summary>
		[InstMember(1), EditorVisible("Text")]
		public string Text
		{
			get { return _text; }
			set
			{
				_text = value ?? string.Empty;
				UpdateTextLayout();
				NotifyChanged();
			}
		}

		/// <summary>
		/// The colour of the text.
		/// </summary>
		[InstMember(2), EditorVisible("Text")]
		public Colour TextColour
		{
			get { return _textColour; }
			set
			{
				if (_textColour == value) return;
				_textColour = value;
				if (Renderer.IsInitialized)
				{
				    _textRenderer.FillBrush = Renderer.Brushes.Get(value);
				}
				NotifyChanged();
			}
		}

		/// <summary>
		/// The size of the text in points.
		/// </summary>
		[InstMember(3), EditorVisible("Text")]
		public float FontSize
		{
			get { return _fontSize; }
			set
			{
				_fontSize = Math.Min(Math.Max(8, value), 1000);
				UpdateTextLayout();
				NotifyChanged();
			}
		}

		/// <summary>
		/// The font to use.
		/// </summary>
		[InstMember(4), EditorVisible("Text")]
		public FontFamily Font
		{
			get { return _font; }
			set
			{
				_font = value;
				UpdateTextLayout();
				NotifyChanged();
			}
		}

		/// <summary>
		/// The horizontal alignment of the text.
		/// </summary>
		[InstMember(5), EditorVisible("Text")]
		public AlignmentX TextAlignmentX
		{
			get { return _textAlignmentX; }
			set
			{
				_textAlignmentX = value;
				UpdateTextLayout();
				NotifyChanged();
			}
		}

		/// <summary>
		/// The vertical alignment of the text.
		/// </summary>
		[InstMember(6), EditorVisible("Text")]
		public AlignmentY TextAlignmentY
		{
			get { return _textAlignmentY; }
			set
			{
				_textAlignmentY = value;
				UpdateTextLayout();
				NotifyChanged();
			}
		}

		/// <summary>
		/// The weight of the text.
		/// </summary>
		[InstMember(7), EditorVisible("Text")]
		public FontWeight FontWeight
		{
			get { return (FontWeight)_fontWeight; }
			set
			{
				_fontWeight = (SharpDX.DirectWrite.FontWeight)value;
				UpdateTextLayout();
				NotifyChanged();
			}
		}

		/// <summary>
		/// Determines the method of word wrapping to use.
		/// </summary>
		[InstMember(8), EditorVisible("Text")]
		public WordWrapping WordWrapping
		{
			get { return _wordWrapping; }
			set
			{
				_wordWrapping = value;
				UpdateTextLayout();
				NotifyChanged();
			}
		}

		/*
		/// <summary>
		/// Determines if coloured fonts should be used. (i.e. yellow emojis)
		/// </summary>
		[Member(9), InspectorVisible("Text")]
		public bool UseColouredFonts
		{
			get { return _useColouredFonts; }
			set
			{
				if (value == _useColouredFonts)
					return;

				_useColouredFonts = value;

				NotifyChanged(nameof(UseColouredFonts));
			}
		}
		*/

		/// <summary>
		/// The colour of the text stroke.
		/// </summary>
		[InstMember(10), EditorVisible("Text")]
		public Colour TextStrokeColour
		{
			get { return _textStrokeColour; }
			set
			{
				if (value == _textStrokeColour) return;
				_textStrokeColour = value;

				if (Renderer.IsInitialized)
				{
				    _textRenderer.OutlineBrush = Renderer.Brushes.Get(value);
					_shadowEffect.Color = value;
				}

				NotifyChanged(nameof(TextColour));
			}
		}

		protected override void CreateResources()
		{
			base.CreateResources();
		    _textRenderer = new CustomTextRenderer(Renderer.Factory2D)
		    {
		        FillBrush = Renderer.Brushes.Get(Colour.Black),
		        OutlineBrush = Renderer.Brushes.Get(Colour.Transparent),
                StrokeWidth = 2.0f,
		    };
		    _shadowEffect = new Shadow(Renderer.Context2D)
			{
				Optimization = ShadowOptimization.Speed,
				Color = Colour.Black
			};
		}

		/// <inheritdoc />
		public override void Arrange()
		{
			base.Arrange();
			if (_textReady && !IsDestroyed)
				UpdateTextLayout();
		}

		internal void UpdateTextLayout()
		{
			lock (Locker)
			{
				if (Renderer.DirectWriteFactory?.IsDisposed != false)
					return;

				UpdateTextFormat();

                 _textLayout?.Dispose();
				_textLayout = new TextLayout(Renderer.DirectWriteFactory, _text, _textFormat,
					Math.Max(RenderRect.Width - 0.5f, 1), Math.Max(RenderRect.Height - 0.5f, 1));

			    UpdateTextBounds();

			}
		}

		private void UpdateTextFormat()
		{
			Utilities.Dispose(ref _textFormat);

			if (Renderer.DirectWriteFactory?.IsDisposed != false)
				return;
            
            _textFormat = new TextFormat(Renderer.DirectWriteFactory, _font, _fontWeight, FontStyle.Normal, _fontSize)
			{
				WordWrapping = (SharpDX.DirectWrite.WordWrapping)WordWrapping,
				TextAlignment = (TextAlignment)TextAlignmentX,
				ParagraphAlignment = (ParagraphAlignment)TextAlignmentY,
			};
		}

		internal override void Draw()
		{
			base.Draw();

			lock (Locker)
			{
				if (_textLayout == null)
					UpdateTextLayout();

				Debug.Assert(_textLayout != null, "_textLayout != null");

			    if (_textRenderer.OutlineBrush.Opacity > 0)
			    {
			        _textRenderer.Location = RenderRect.Location;
			        _textLayout.Draw(_textRenderer, 0, 0);
			    }
			    else
			    {
			        Renderer.Context2D.DrawTextLayout(RenderRect.Location, _textLayout, _textRenderer.FillBrush, DrawTextOptions.EnableColorFont);
			    }
			}
		}

		/// <inheritdoc />
		public override void Destroy()
		{
			base.Destroy();

			lock (Locker)
			{
				Utilities.Dispose(ref _textRenderer);
				Utilities.Dispose(ref _textLayout);
				Utilities.Dispose(ref _textFormat);
				Utilities.Dispose(ref _shadowEffect);
			}
		}
	}
}