// TerrainRegion.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System.IO;
using dEngine.Instances.Attributes;


namespace dEngine.Instances
{
	/// <summary>
	/// A TerrainRegion stores the data for a region of <see cref="Terrain" />.
	/// </summary>
	[TypeId(136), Uncreatable]
	public class TerrainRegion : Instance
	{
		internal Stream Data { get; set; }

		/// <summary>
		/// The size of the region in cells.
		/// </summary>
		public Vector3 SizeInCells { get; }
	}
}