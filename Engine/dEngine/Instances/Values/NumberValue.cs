// NumberValue.cs - dEngine
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
	/// A <see cref="ValueContainer" /> that holds a <see cref="double" />.
	/// </summary>
	[TypeId(86), ExplorerOrder(3), ToolboxGroup("Values")]
	public sealed class NumberValue : ValueContainer
	{
		private double _value;

		/// <inheritdoc />
		public NumberValue()
		{
			_value = 0;
		}

		/// <summary>
		/// The value this container holds.
		/// </summary>
		[InstMember(1), EditorVisible("Data")]
		public double Value
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