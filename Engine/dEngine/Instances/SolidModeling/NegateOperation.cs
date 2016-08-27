// NegateOperation.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using dEngine.Graphics;
using dEngine.Instances.Attributes;

namespace dEngine.Instances
{
	/// <summary>
	/// A part representing a CSG Negate operation - a subtraction of one object from another.
	/// </summary>
	[TypeId(143), Uncreatable, ExplorerOrder(9)]
	public sealed class NegateOperation : PartOperation
	{
		/// <inheritdoc />
		public NegateOperation()
		{
			Renderer.TransparentParts.Add(this);
		}

		/// <inheritdoc />
		public override void Destroy()
		{
			base.Destroy();
			Renderer.TransparentParts.Remove(this);
		}
	}
}