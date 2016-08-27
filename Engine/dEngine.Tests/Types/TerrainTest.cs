// TerrainTest.cs - dEditor
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using dEngine.Instances;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace dEngine.Tests
{
	[TestClass]
	public class TerrainTest
	{
		private readonly Terrain _terrain;

		public TerrainTest()
		{
			_terrain = new Terrain(null);
		}

		[TestMethod]
		public void MaxExtents()
		{
			var extents = _terrain.MaxExtents;
			var target = new Region3int16(new Vector3int16(-32000, -32000, -32000),
				new Vector3int16(32000, 32000, 32000));
			Assert.AreEqual(extents, target);
		}

		[TestMethod]
		public void CellCoords()
		{
			var wtc0 = _terrain.WorldToCell(new Vector3(8, 4, 4));
			Assert.AreEqual(wtc0, new Vector3int16(2, 1, 1));

			var wtc1 = _terrain.WorldToCell(new Vector3(8, -8, 4));
			Assert.AreEqual(wtc1, new Vector3int16(2, -2, 1));
		}
	}
}