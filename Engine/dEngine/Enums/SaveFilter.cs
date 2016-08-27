// SaveFilter.cs - dEngine
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
using dEngine.Services;

namespace dEngine
{
	/// <summary>
	/// Enum for filtering the save of <see cref="DataModel" /> and <see cref="Workspace" />
	/// </summary>
	public enum SaveFilter
	{
		/// <summary>
		/// Only save the Workspace.
		/// </summary>
		SaveWorld,

		/// <summary>
		/// Only save the DataModel.
		/// </summary>
		SaveGame,

		/// <summary>
		/// Save DataModel with Workspace to the same stream. (No callbacks invoked.)
		/// </summary>
		SaveTogether
	}
}