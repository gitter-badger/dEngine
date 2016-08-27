// ClientReplicator.cs - dEngine
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
using Lidgren.Network;
using Steamworks;

namespace dEngine.Instances
{
	/// <summary>
	/// Represents the client's connection to a server.
	/// </summary>
	/// <remarks>
	/// The client creates this when it connects to a server.
	/// </remarks>
	[TypeId(43), Uncreatable]
	public sealed class ClientReplicator : NetworkReplicator
	{
		internal ClientReplicator(NetConnection connection, HAuthTicket steamTicket, Player player)
			: base(connection, player)
		{
			AuthTicket = steamTicket;

			Address = connection.RemoteEndPoint.Address.ToString();
			Port = connection.RemoteEndPoint.Port;
		}

		internal HAuthTicket AuthTicket { get; }

		/// <inheritdoc />
		public override void Destroy()
		{
			base.Destroy();
			CloseConnection("Replicator was destroyed.");
		}
	}
}