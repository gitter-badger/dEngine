// SurfaceGui.cs - dEngine
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

namespace dEngine.Instances
{
	/// <summary>
	/// A GUI which is rendered on the face of a <see cref="Part" />.
	/// </summary>
	[TypeId(113), ToolboxGroup("GUI")]
	public class SurfaceGui : LayerCollector
	{
		private Part _adornee;
		private Axis _face;

		/// <summary>
		/// The face of the <see cref="Adornee" /> which this gui will be rendered to.
		/// </summary>
		public Axis Face
		{
			get { return _face; }
			set
			{
				if (value == _face) return;
				_face = value;
				NotifyChanged();
			}
		}

		/// <summary>
		/// The <see cref="Part" /> to render on.
		/// </summary>
		public Part Adornee
		{
			get { return _adornee; }
			set
			{
				if (value == _adornee)
					return;

				_adornee = value;
				NotifyChanged(nameof(Adornee));
			}
		}

		/// <inheritdoc />
		public override Vector2 AbsolutePosition { get; internal set; }

		/// <inheritdoc />
		public override Vector2 AbsoluteSize { get; internal set; }
	}
}