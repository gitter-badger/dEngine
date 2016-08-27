// ForgeNode.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Reflection;
using dEngine.Instances.Attributes;

namespace dEngine.Instances.Materials
{
    /// <summary>
    /// Base class for material nodes.
    /// </summary>
    [Uncreatable, TypeId(211)]
    public abstract class Node : Instance
    {
        private readonly List<Slot> _inputSlots;
        private readonly List<Slot> _outputSlots;

        private Vector2 _position;
        private Vector2 _size;
        private string _comment;

        internal int InSlotCount;
        internal int OutSlotCount;

        /// <summary>
        /// Fired when a slot is connected.
        /// </summary>
        public readonly Signal<Slot> SlotConnected;

        /// <summary/>
        protected Node()
        {
            _inputSlots = new List<Slot>();
            _outputSlots = new List<Slot>();

            SlotConnected = new Signal<Slot>(this);

            var properties = GetType().GetProperties();
            foreach (var prop in properties)
            {
                var slotAttr = prop.GetCustomAttribute<NodeSlotAttribute>();
                var slotColourAttr = prop.GetCustomAttribute<NodeSlotColourAttribute>();
                if (slotAttr != null)
                {
                    var slot = new Slot(this, slotAttr.DisplayName, slotAttr.Type, slotAttr.Mode, slotColourAttr?.Colour);
                    prop.SetValue(this, slot);
                    if (slotAttr.Mode == InOut.In)
                        _inputSlots.Add(slot);
                    else
                        _outputSlots.Add(slot);
                }
            }
        }

        /// <summary>
        /// Gets the slots of the given mode.
        /// </summary>
        public IEnumerable<Slot> GetSlots(InOut mode)
        {
            return mode == InOut.In ? _inputSlots : _outputSlots;
        }

        /// <summary>
        /// The position of the node.
        /// </summary>
        [EditorVisible("Data")]
        public Vector2 Position
        {
            get { return _position; }
            set
            {
                if (value == _position) return;
                _position = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// The size of the node.
        /// </summary>
        [EditorVisible("Data")]
        public Vector2 Size
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
        /// The comment attached to the node.
        /// </summary>
        [EditorVisible("Data")]
        public string Comment
        {
            get { return _comment; }
            set
            {
                if (value == _comment) return;
                _comment = value;
                NotifyChanged();
            }
        }
    }
}