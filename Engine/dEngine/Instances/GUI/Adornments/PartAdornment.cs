// PartAdornment.cs - dEngine
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
	/// A base class for adornments which can be attached to a <see cref="Part" />.
	/// </summary>
	[TypeId(128)]
	public abstract class PartAdornment : GuiBase3D
	{
		private Part _adornee;

		/// <summary>
		/// The object to adorn to.
		/// </summary>
		[InstMember(1), EditorVisible("Data")]
		public Part Adornee
		{
			get { return _adornee; }
			set
			{
				if (_adornee == value) return;
				_adornee = value;

				if (value != null)
					_adornee.Destroyed.Event += OnAdorneeDestroyed;

				NotifyChanged(nameof(Adornee));
			}
		}

		private void OnAdorneeDestroyed()
		{
			if (_adornee != null)
				_adornee.Destroyed.Event -= OnAdorneeDestroyed;
			Adornee = null;
		}

		/// <inheritdoc />
		public override void Destroy()
		{
			base.Destroy();

			Adornee = null;
		}
	}
}