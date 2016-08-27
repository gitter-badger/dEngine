// StringValue.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System.IO;
using dEngine.Instances.Attributes;


namespace dEngine.Instances
{
	/// <summary>
	/// A <see cref="ValueContainer" /> that holds a <see cref="string" />.
	/// </summary>
	[TypeId(88), ExplorerOrder(3), ToolboxGroup("Values")]
	public sealed class StringValue : ValueContainer
	{
		private string _value;

		/// <inheritdoc />
		public StringValue()
		{
			_value = "Value";
		}

        /// <summary>
        /// The maximum length of <see cref="Value"/>.
        /// </summary>
	    public const int MaxLength = 300000;

        /// <summary>
        /// The value that this container holds.
        /// </summary>
        [InstMember(1), EditorVisible("Data")]
		public string Value
		{
			get { return _value; }
			set
			{
			    value = value ?? string.Empty;

                if (value.Length > MaxLength)
                    throw new InvalidDataException("String value was too long.");

                _value = value;

                NotifyChanged(nameof(Value));
			}
		}
	}
}