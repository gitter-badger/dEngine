// MaterialInstance.cs - dEngine
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
using System.Collections.Generic;
using dEngine.Instances.Attributes;

namespace dEngine.Instances.Materials
{
    /// <summary>
    /// An instance of a <see cref="Material"/> which can be used to modify paramaters on the fly.
    /// </summary>
    [Uncreatable, TypeId(42)]
    public class MaterialInstance : MaterialBase
    {
        private Dictionary<string, object> _parameters;

        internal MaterialInstance(Material material)
        {
            Material = material;
            _parameters = new Dictionary<string, object>();
        }

        /// <summary>
        /// Sets the value of a material paramater.
        /// </summary>
        public void SetParameter(string name, object value)
        {
            _parameters[name] = value;
        }

        [InstMember(1)]
        public MaterialBase Material { get; internal set; }

        /// <summary>
        /// The <see cref="MaterialDomain"/> of the parent material.
        /// </summary>
        public override MaterialDomain Domain { get { return Material.Domain; } set {throw new InvalidOperationException("The domain of a material instance cannot be set.");} }

        /// <summary>
        /// The <see cref="dEngine.ShadingModel"/> of the parent material.
        /// </summary>
        public override ShadingModel ShadingModel { get { return Material.ShadingModel; } set { throw new InvalidOperationException("The shading model of a material instance cannot be set."); } }
    }
}