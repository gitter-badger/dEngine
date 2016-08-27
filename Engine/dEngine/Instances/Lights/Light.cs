// Light.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using dEngine.Instances.Attributes;
using dEngine.Utility;


namespace dEngine.Instances
{
	/// <summary>
	/// Base class for lights.
	/// </summary>
	[TypeId(30)]
	public abstract class Light : Instance
	{
		private bool _castsShadows;
		private Colour _colour;
		private bool _enabled;
		private Part _parentParent;

		/// <inheritdoc />
		protected Light()
		{
			_enabled = true;
			_castsShadows = false;
		}

		/// <summary>
		/// The position of the light.
		/// </summary>
		protected Vector3 Position => _parentParent?.CFrame.p ?? Vector3.Zero;

		internal int LightIndex { get; set; } = -1;

		/// <summary>
		/// Determines if the light is enabled.
		/// </summary>
		[InstMember(1), EditorVisible("Data")]
		public bool IsEnabled
		{
			get { return _enabled; }
			set
			{
				_enabled = value;
				NotifyChanged(nameof(IsEnabled));
			}
		}

		/// <summary>
		/// Determines if the light casts shadows.
		/// </summary>
		[InstMember(2), EditorVisible("Data")]
		public bool CastsShadows
		{
			get { return _castsShadows; }
			set
			{
				_castsShadows = value;
				NotifyChanged(nameof(CastsShadows));
			}
		}

		/// <summary>
		/// The colour of the light.
		/// </summary>
		[InstMember(3), EditorVisible("Data")]
		public Colour Colour
		{
			get { return _colour; }
			set
			{
				if (value == _colour) return;
				_colour = value;
				NotifyChanged(nameof(Colour));
			}
		}

		/// <inheritdoc />
		protected override void OnAncestryChanged(Instance child, Instance parent)
		{
			if (child == this)
			{
				Part part = _parentParent;
				if (part != null)
				{
					//part.PositionChanged.Event -= PositionChangedOnEvent;
				}

				if ((_parentParent = part = parent as Part) != null)
				{
					//part.PositionChanged.Event += PositionChangedOnEvent;
				}
			}
		}

		internal abstract void UpdateLightData();

		private Colour TempatureToColour(float temperature)
		{
			temperature /= 100;

			var r = temperature < 66 ? 255 : 329.698727446f * Mathf.Pow((temperature - 60), -0.1332047592f);

			var g = temperature <= 66
				? 99.4708025861f * Mathf.Log(temperature) - 161.1195681661f
				: 288.1221695283f * Mathf.Pow(temperature - 60, -0.0755148492f);

			var b = temperature >= 66
				? 255
				: temperature <= 19
					? 0
					: 138.5177312231f * Mathf.Log(temperature - 10) - 305.0447927307f;

			return Colour.fromRGB((int)Mathf.Clamp(r, 0, 255), (int)Mathf.Clamp(g, 0, 255), (int)Mathf.Clamp(b, 0, 255));
		}
	}
}