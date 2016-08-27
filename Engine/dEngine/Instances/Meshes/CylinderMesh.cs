// CylinderMesh.cs - dEngine
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
	/// A CylinderMesh changes the appearance of its parent <see cref="Part" />, regardless of <see cref="Part.Size" /> and
	/// <see cref="Part.Shape" /> properties.
	/// </summary>
	[TypeId(166), ExplorerOrder(3)]
	public sealed class CylinderMesh : Mesh
	{
		/// <inheritdoc />
		public CylinderMesh()
		{
			_geometry = Primitives.CylinderGeometry;
			UsePartSize = true;
		}
	}
}