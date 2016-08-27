// Vector3Value.cs - dEngine
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
	/// A <see cref="ValueContainer" /> that holds a <see cref="Vector3" />.
	/// </summary>
	[TypeId(85), ExplorerOrder(3), ToolboxGroup("Values")]
	public sealed class Vector3Value : ValueContainer
	{
		private Vector3 _value;

		/// <inheritdoc />
		public Vector3Value()
		{
			_value = Vector3.Zero;
		}

		/// <summary>
		/// The value that this containers holds.
		/// </summary>
		[InstMember(1), EditorVisible("Data")]
		public Vector3 Value
		{
			get { return _value; }
			set
			{
				_value = value;
				NotifyChanged(nameof(Value));
			}
		}
	}
}