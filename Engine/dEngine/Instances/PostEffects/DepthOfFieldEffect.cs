// DepthOfFieldEffect.cs - dEngine
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
using SharpDX.Direct3D11;

namespace dEngine.Instances
{
    /// <summary>
    /// Blurs parts of the scene outside a specified range.
    /// </summary>
	[TypeId(175), ExplorerOrder(0)]
	public sealed class DepthOfField : PostEffect
	{
        /// <summary/>
	    public DepthOfField()
	    {
            _effectOrder = (int)EffectPrority.DepthOfField;
        }

		internal override void Render(ref DeviceContext context)
		{
			throw new System.NotImplementedException();
		}
	}
}