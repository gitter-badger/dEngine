// ServerScriptService.cs - dEngine
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
	/// A container service who executes scripts on the server.
	/// </summary>
	/// <remarks>
	/// Objects stored in this container will not be baked into the client.
	/// </remarks>
	/// <seealso cref="Workspace" />
	[TypeId(79), ExplorerOrder(6), ToolboxGroup("Containers")]
	public sealed class ServerScriptService : Service
	{
		public static ServerScriptService Service;

		public ServerScriptService()
		{
			Service = this;
		}

		internal static object GetExisting()
		{
			return DataModel.GetService<ServerScriptService>();
		}
	}
}