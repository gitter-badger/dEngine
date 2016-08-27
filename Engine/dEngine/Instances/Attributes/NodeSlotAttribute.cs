// NodeSlotAttribute.cs - dEngine
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
using dEngine.Instances.Materials;
#pragma warning disable 1591

namespace dEngine.Instances.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class NodeSlotAttribute : Attribute
    {
        public string DisplayName { get; set; }
        public SlotType Type { get; set; }
        public InOut Mode { get; set; }
        public MaterialDomain[] DisableDomains { get; set; }
        public ShadingModel[] DisabledModels { get; set; }

        public NodeSlotAttribute(string displayName, SlotType type, InOut mode)
        {
            DisplayName = displayName;
            Type = type;
            Mode = mode;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class NodeSlotColourAttribute : Attribute
    {
        public Colour Colour { get; set; }

        public NodeSlotColourAttribute(int r, int g, int b)
        {
            Colour = Colour.fromRGB(r, g, b);
        }
    }
}