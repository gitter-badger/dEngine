// BloomEffect.cs - dEngine
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
using dEngine.Instances.Attributes;
using SharpDX.Direct3D11;

namespace dEngine.Instances
{
	/// <summary>
	/// An effect which makes bright parts of the screen glow.
	/// </summary>
	[TypeId(174), ExplorerOrder(0)]
	public sealed class BloomEffect : PostEffect
	{
		private Colour _dirtColour;
		private Content<Texture> _dirtMask;
        private float _dirtIntensity;
        private float _intensity;
		private float _size;
		private float _threshold;

		/// <inheritdoc />
		public BloomEffect()
		{
			_dirtMask = new Content<Texture>();
		    _effectOrder = (int)EffectPrority.Bloom;
		}

		/// <summary>
		/// The intensity of the bloom.
		/// </summary>
		[InstMember(1), EditorVisible("Data")]
		public float Intensity
		{
			get { return _intensity; }
			set
			{
				if (value == _intensity) return;
				_intensity = value;
				NotifyChanged();
			}
		}

		/// <summary>
		/// The size in percent of the screen width.
		/// </summary>
		[InstMember(2), EditorVisible("Data")]
		public float Size
		{
			get { return _size; }
			set
			{
				if (value == _size) return;
				_size = value;
				NotifyChanged();
			}
		}

		/// <summary>
		/// The size in percent of the screen width.
		/// </summary>
		[InstMember(3), EditorVisible("Data")]
		public float Threshold
		{
			get { return _threshold; }
			set
			{
				if (value == _threshold) return;
				_threshold = value;
				NotifyChanged();
			}
		}

		/// <summary>
		/// The texture to use for the dirty lens effect.
		/// </summary>
		[InstMember(4), EditorVisible("Dirt")]
		public Content<Texture> DirtMask
		{
			get { return _dirtMask; }
			set
			{
				_dirtMask = value;
				value.Subscribe(this, OnDirtMaskFetched);
				NotifyChanged();
			}
		}

		/// <summary>
		/// The intensity of the dirty lens effect.
		/// </summary>
		[InstMember(5), EditorVisible("Data")]
		public float DirtIntensity
		{
			get { return _intensity; }
			set
			{
				if (value == _dirtIntensity) return;
				_dirtIntensity = value;
				NotifyChanged();
			}
		}

		/// <summary>
		/// The intensity of the dirty lens effect.
		/// </summary>
		[InstMember(6), EditorVisible("Data")]
		public Colour DirtColour
		{
			get { return _dirtColour; }
			set
			{
				if (value == _dirtColour) return;
				_dirtColour = value;
				NotifyChanged();
			}
		}

		private void OnDirtMaskFetched(string id, Texture texture)
		{
			throw new NotImplementedException();
		}

	    internal override void Render(ref DeviceContext context)
	    {
	        //throw new NotImplementedException();
	    }
	}
}