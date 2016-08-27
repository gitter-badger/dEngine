// BoolValue.cs - dEngine
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
    /// A <see cref="ValueContainer" /> that holds a boolean.
    /// </summary>
    [TypeId(50), ToolboxGroup("Values"), ExplorerOrder(3)]
    public sealed class BoolValue : ValueContainer
    {
        private bool _value;

        /// <inheritdoc />
        public BoolValue()
        {
            _value = false;
        }

        /// <summary>
        /// The value this container holds.
        /// </summary>
        [InstMember(1), EditorVisible("Data")]
        public bool Value
        {
            get { return _value; }
            set
            {
                _value = value;
                NotifyChanged();
            }
        }
    }
}