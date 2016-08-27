// BinaryValue.cs - dEngine
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
	/// A <see cref="ValueContainer" /> for holding binary data.
	/// </summary>
	[TypeId(145), ExplorerOrder(3)]
	public sealed class BinaryValue : ValueContainer
	{
	    /// <inheritdoc />
		public BinaryValue()
		{
		}

	    public BinaryData GetValue()
	    {
	        return Value;
	    }

        internal void SetValue(Stream stream)
	    {
	        Value = new BinaryData(stream);
        }

        internal void SetValue(byte[] bytes)
        {
            Value = new BinaryData(bytes);
        }

        [InstMember(1), EditorVisible("Data")]
		internal BinaryData Value { get; set; }
	}
}