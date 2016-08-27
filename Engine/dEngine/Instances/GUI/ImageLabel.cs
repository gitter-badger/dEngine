// ImageLabel.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using dEngine.Data;
using dEngine.Graphics;
using dEngine.Instances.Attributes;
using SharpDX;
using SharpDX.Direct2D1;

namespace dEngine.Instances
{
	/// <summary>
	/// A gui element which can draw an image.
	/// </summary>
	[TypeId(69), ToolboxGroup("GUI"), ExplorerOrder(18)]
	public class ImageLabel : GuiElement
	{
		private Bitmap1 _bitmap;
		private BitmapBrush _bitmapBrush;
		private ExtendMode _extendMode;
		private Content<Texture> _imageContent;
		private float _imageTransparency;
		private ScalingMode _scalingMode;

		/// <inheritdoc />
		public ImageLabel()
		{
			ScalingMode = ScalingMode.Linear;
			ExtendMode = ExtendMode.Stretch;
			ImageId = "internal://content/textures/missing.png";
		}

		/// <summary>
		/// The image file.
		/// </summary>
		[InstMember(2), EditorVisible("Data")]
		public Content<Texture> ImageId
		{
			get { return _imageContent; }
			set
			{
				_imageContent = value;
				value.Subscribe(value, OnImageContentUpdated);
				NotifyChanged();
			}
		}

		/// <summary>
		/// The image extend mode to use when the size of the image is bigger than the source image.
		/// </summary>
		[InstMember(3), EditorVisible("Appearance")]
		public ExtendMode ExtendMode
		{
			get { return _extendMode; }
			set
			{
				if (value == _extendMode) return;
				_extendMode = value;
				NotifyChanged();
			}
		}

		/// <summary>
		/// The bitmap scaling mode to use.
		/// </summary>
		[InstMember(4), EditorVisible("Appearance")]
		public ScalingMode ScalingMode
		{
			get { return _scalingMode; }
			set
			{
				if (value == _scalingMode) return;
				_scalingMode = value;
				NotifyChanged();
			}
		}

		/// <summary>
		/// The transparency of the image.
		/// </summary>
		[InstMember(5), EditorVisible("Appearance")]
		public float ImageTransparency
		{
			get { return _imageTransparency; }
			set
			{
				if (value == _imageTransparency)
					return;

				_imageTransparency = value;
				NotifyChanged(nameof(ImageTransparency));
			}
		}

		private void OnImageContentUpdated(string url, Texture texture)
		{
			if (url != _imageContent)
				return;

			_bitmap?.Dispose();
			_bitmap = null;
			_bitmapBrush?.Dispose();

            if (texture == null)
                return;

            if (texture.IsLoaded)
				_bitmap = texture.GetD2DBitmap();
			_bitmapBrush = null;
		}

		private void UpdateBitmapBrush()
		{
			_bitmapBrush?.Dispose();

			var extendMode = _extendMode >= (ExtendMode)3 ? ExtendMode.Clamp : _extendMode;

			_bitmapBrush = new BitmapBrush1(Renderer.Context2D, _bitmap, new BitmapBrushProperties1
			{
				ExtendModeX = (SharpDX.Direct2D1.ExtendMode)extendMode,
				ExtendModeY = (SharpDX.Direct2D1.ExtendMode)extendMode,
				InterpolationMode = (InterpolationMode)_scalingMode
			});
		}

		/// <inheritdoc />
		public override void Destroy()
		{
			base.Destroy();

			lock (Renderer.Locker)
			{
				Utilities.Dispose(ref _bitmapBrush);
				Utilities.Dispose(ref _bitmap);
			}
		}

		internal override void Draw()
		{
			base.Draw();

			lock (Renderer.Locker)
			{
				var context = Renderer.Context2D;

				if (_bitmap == null)
					return;

				if (((_extendMode == ExtendMode.Stretch || _extendMode == ExtendMode.Stretch) && !(CornerRadius > 0)))
				{
					var interpMode = (BitmapInterpolationMode)ScalingMode;
					interpMode = interpMode >= (BitmapInterpolationMode)3 ? BitmapInterpolationMode.Linear : interpMode;
                    
                    context.DrawBitmap(_bitmap, _roundedRectangle.Rect, 1.0f - _imageTransparency, interpMode);
				}
				else
				{
					if (_bitmapBrush == null) UpdateBitmapBrush();
					context.FillRoundedRectangle(ref _roundedRectangle, _bitmapBrush);
				}
			}
		}
	}
}