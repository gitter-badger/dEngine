// ServerReplicator.cs - dEngine
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
	/// Represents the server's connection to a client.
	/// </summary>
	/// <remarks>
	/// The server creates this when a client connects.
	/// </remarks>
	[Uncreatable, TypeId(47)]
	public sealed class ServerReplicator : NetworkReplicator
	{
		/// <summary>
		/// Fired when the player's authentication ticket is processed.
		/// </summary>
		public readonly Signal<uint, bool, int> TicketProcessed;

		internal ServerReplicator(NetConnection connection, Player player) : base(connection, player)
		{
			TicketProcessed = new Signal<uint, bool, int>(this);

			Address = connection.RemoteEndPoint.Address.ToString();
			Port = connection.RemoteEndPoint.Port;
		}

		internal CSteamID SteamId { get; private set; }

		/// <inheritdoc />
		public override void CloseConnection(string goodbye = "")
		{
			Player.Disconnected.Fire(goodbye);
			Player.Destroy();
			SteamGameServer.EndAuthSession(SteamId);
			base.CloseConnection(goodbye);
		}
	}
}