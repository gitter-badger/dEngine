// UnionOperation.cs - dEngine
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
using dEngine.Services;
using dEngine.Utility;

namespace dEngine.Instances
{
	/// <summary>
	/// A part representing a Union CSG operation - a merger of two objects into one.
	/// </summary>
	[TypeId(142), Uncreatable, ExplorerOrder(3)]
	public sealed class UnionOperation : PartOperation
	{
		/// <inheritdoc />
		public UnionOperation()
		{
			SolidModelingManager.Operations.TryAdd(this);
		}

		/// <inheritdoc />
		public override void Destroy()
		{
			base.Destroy();
			SolidModelingManager.Operations.TryRemove(this);
		}
	}
}