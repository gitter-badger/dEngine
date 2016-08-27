// StandardFinalNode.cs - dEngine
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
// ReSharper disable UnassignedGetOnlyAutoProperty

namespace dEngine.Instances.Materials.Nodes
{
    /// <summary>
    /// The final node of the material graph.
    /// </summary>
    [TypeId(36)]
    public class FinalNode : Node
    {
        /// <summary>
        /// The colour of the material.
        /// </summary>
        [NodeSlot("Base Colour", SlotType.Float3, InOut.In, DisableDomains = new[] { MaterialDomain.LightFunction })]
        public Slot BaseColour { get; private set; }

        /// <summary>
        /// Determiens how metallic the material is.
        /// </summary>
        [NodeSlot("Metallic", SlotType.Float, InOut.In, DisableDomains = new[] { MaterialDomain.LightFunction })]
        public Slot Metallic { get; private set; }

        /// <summary>
        /// Determines how smooth the material is.
        /// </summary>
        [NodeSlot("Smoothness", SlotType.Float, InOut.In, DisableDomains = new[] { MaterialDomain.LightFunction })]
        public Slot Smoothness { get; private set; }

        /// <summary>
        /// Determines the colour/power of the glow.
        /// </summary>
        [NodeSlot("Emissive Colour", SlotType.Float3, InOut.In)]
        public Slot EmissiveColour { get; private set; }

        /// <summary>
        /// The opacity of the material.
        /// </summary>
        [NodeSlot("Opacity", SlotType.Float3, InOut.In, DisableDomains = new[] {MaterialDomain.LightFunction, MaterialDomain.GUI})]
        public Slot Opacity { get; private set; }

        /// <summary>
        /// Determines whether the pixel is clipped or not.
        /// </summary>
        [NodeSlot("Opacity Mask", SlotType.Float3, InOut.In,
            DisableDomains = new[] {MaterialDomain.LightFunction, MaterialDomain.Decal, MaterialDomain.GUI})]
        public Slot OpacityMask { get; private set; }

        /// <summary>
        /// The colour of the subsurface scattering.
        /// </summary>
        [NodeSlot("Subsurface Colour", SlotType.Float3, InOut.In, DisableDomains = new[] {MaterialDomain.Decal})]
        public Slot SubsurfaceColour { get; private set; }

        /// <summary>
        /// The colour of the refraction.
        /// </summary>
        [NodeSlot("Refraction", SlotType.Float3, InOut.In, DisableDomains = new[] {MaterialDomain.Decal})]
        public Slot Refraction { get; private set; }

        /// <summary>
        /// The world-space offset of the vertex.
        /// </summary>
        [NodeSlot("Vertex Offset", SlotType.Float3, InOut.In, DisableDomains = new[] {MaterialDomain.Decal})]
        public Slot VertexOffset { get; private set; }
    }
}