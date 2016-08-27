// Slot.cs - dEngine
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
using JetBrains.Annotations;

#pragma warning disable 1591
namespace dEngine.Instances.Materials
{
    public class Slot
    {
        private static readonly Colour DefaultColour = new Colour(0, 0, 0);

        internal Slot(Node node, string name, SlotType type, InOut mode, Colour? colour = null)
        {
            Name = name;
            Colour = colour ?? DefaultColour;
            Type = type;
            Mode = mode;
            Node = node;

            Index = mode == InOut.In ? node.InSlotCount++ : node.OutSlotCount++;
        }

        /// <summary>
        /// The node this slot is attached to.
        /// </summary>
        public Node Node { get; }

        internal int Index { get; }

        internal Slot Input { get; set; }
        internal Slot Output { get; set; }

        public bool ConnectTo([CanBeNull] Slot output)
        {
            if (Mode != InOut.Out)
                throw new InvalidOperationException("Input slots cannot be connected.");

            if (output != null)
            {
                if (output.Mode != InOut.In)
                    return false;
                output.Input = this;
                output.Node.SlotConnected.Fire(output);
            }

            Output = output;
            Node.SlotConnected.Fire(this);

            return true;
        }

        public string Name { get; }
        public Colour Colour { get;}
        public SlotType Type { get; }
        public InOut Mode { get; }
    }
}