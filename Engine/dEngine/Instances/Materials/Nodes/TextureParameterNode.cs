// TextureNode.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using dEngine.Data;
using dEngine.Instances.Attributes;
// ReSharper disable UnassignedGetOnlyAutoProperty

#pragma warning disable 1591

namespace dEngine.Instances.Materials.Nodes
{
    /// <summary>
    /// A node which samples a static texture.
    /// </summary>
    [TypeId(213)]
    public class TextureParameterNode : ParameterNode
    {
        private Content<Texture> _textureId;

        public TextureParameterNode()
        {

        }

        #region Properties
        /// <summary>
        /// The texture to sample.
        /// </summary>
        [InstMember(1), EditorVisible("Data")]
        public Content<Texture> TextureId
        {
            get { return _textureId; }
            set
            {
                if (value == _textureId) return;
                _textureId = value;
                NotifyChanged();
            }
        }
        #endregion

        #region Inputs
        /// <summary>
        /// The UV input.
        /// </summary>
        [NodeSlot("UVs", SlotType.Float, InOut.In)]
        public Slot TexCoord { get; private set; }
        #endregion

        #region Outputs
        /// <summary>
        /// Outputs the R channel.
        /// </summary>
        [NodeSlot("R", SlotType.Float, InOut.Out)]
        public Slot R { get; private set; }

        /// <summary>
        /// Outputs the G channel.
        /// </summary>
        [NodeSlot("G", SlotType.Float, InOut.Out)]
        public Slot G { get; private set; }

        /// <summary>
        /// Outputs the B channel.
        /// </summary>
        [NodeSlot("B", SlotType.Float, InOut.Out)]
        public Slot B { get; private set; }

        /// <summary>
        /// Outputs the A channel.
        /// </summary>
        [NodeSlot("A", SlotType.Float, InOut.Out)]
        public Slot A { get; private set; }

        /// <summary>
        /// Outputs the A channel.
        /// </summary>
        [NodeSlot("RGB", SlotType.Float3, InOut.Out)]
        public Slot RGB { get; private set; }
        #endregion
    }
}