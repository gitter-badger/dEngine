// CellMaterial.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using dEngine.Instances;

#pragma warning disable 1591

namespace dEngine
{
	/// <summary>
	/// The material for a <see cref="Terrain" /> cell.
	/// </summary>
	public enum CellMaterial
	{
		Air,
		Water,
		WoodPlanks,
		Slate,
		Concrete,
		Brick,
		Grass,
		Sand,
		Rock,
		Glacier,
		Snow,
		Sandstone,
		Mud,
		Basalt,
		Ground,
		CrackedLava
	}
}