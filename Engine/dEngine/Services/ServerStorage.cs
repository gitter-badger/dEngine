// ServerStorage.cs - dEngine
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
using dEngine.Instances.Attributes;


namespace dEngine.Services
{
	/// <summary>
	/// A service for storing objects that are not replicated to the client.
	/// </summary>
	/// <remarks>
	/// Objects stored in this container will not be baked into the client.
	/// </remarks>
	[TypeId(89), ExplorerOrder(6), ToolboxGroup("Containers")]
	public sealed class ServerStorage : Service
	{
		internal static object GetExisting()
		{
			return DataModel.GetService<ServerStorage>();
		}
	}
}