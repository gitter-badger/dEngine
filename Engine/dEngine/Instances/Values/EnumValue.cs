// EnumValue.cs - dEngine
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
using System.Collections.Concurrent;
using System.Diagnostics;
using dEngine.Instances.Attributes;


namespace dEngine.Instances
{
	/// <summary>
	/// A <see cref="ValueContainer" /> that holds an enum value.
	/// </summary>
	[TypeId(195), ExplorerOrder(3), ToolboxGroup("Values")]
	public sealed class EnumValue : ValueContainer
	{
		private static readonly ConcurrentDictionary<string, Enum[]> _values = new ConcurrentDictionary<string, Enum[]>();

		[InstMember(1)] private string _typeFullName;

		[InstMember(2)] private int _value;

		/// <inheritdoc />
		public EnumValue()
		{
			Type = typeof(Axis);
			Value = AlignmentX.Right;
		}

		/// <summary>
		/// The type of enum to store.
		/// </summary>
		[EditorVisible("Data", null, "Enum")]
		public Type Type
		{
			get { return Type.GetType(_typeFullName); }
			set
			{
				_typeFullName = value.FullName;

				var type = Type;
				Debug.Assert(type != null, "Type was non-existent.");

				if (!_values.ContainsKey(_typeFullName))
				{
					var values = Enum.GetValues(type);
					var valueList = new Enum[values.Length];

					for (int i = 0; i < values.Length; i++)
						valueList[i] = (Enum)values.GetValue(i);

					_values[_typeFullName] = valueList;
				}

				Value = _values[_typeFullName][0];
			}
		}

		/// <summary>
		/// The value that the container holds.
		/// </summary>
		[EditorVisible("Data")]
		public Enum Value
		{
			get { return _values[_typeFullName][_value]; }
			set
			{
				_value = (int)(object)value;
				NotifyChanged();
			}
		}
	}
}