// NetworkServer.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using dEngine.Instances;
using dEngine.Instances.Attributes;
using dEngine.Settings.Global;
using Lidgren.Network;

using Steamworks;

namespace dEngine.Services.Networking
{
	/// <summary>
	/// Service for handling networking and replication on the server.
	/// </summary>
	[TypeId(77), ExplorerOrder(6)]
	public sealed class NetworkServer : NetworkPeer
	{
		private NetServer _netServer;
	    private bool _authenticationRequired;

	    /// <inheritdoc />
		public NetworkServer()
		{
			Replicators = new ConcurrentDictionary<Player, ServerReplicator>();
		}

		/// <summary>
		/// The port the server is hosted on.
		/// </summary>
		[EditorVisible("Data")]
		public int Port { get; private set; }

		/// <summary>
		/// The address the server is hosted on.
		/// </summary>
		[EditorVisible("Data")]
		public string Address { get; private set; }

		/// <summary>
		/// The number of clients connected to the server.
		/// </summary>
		[EditorVisible("Data")]
		public int ClientCount => _netServer?.ConnectionsCount ?? 0;

		/// <summary>
		/// A dictionary of replicators indexed by player.
		/// </summary>
		public ConcurrentDictionary<Player, ServerReplicator> Replicators { get; }

		/// <summary>
		/// A custom handler for messages that are not handled in the base implementation.
		/// </summary>
		[LevelEditorRelated]
		public Func<byte, NetIncomingMessage, bool> CustomMessageHandler { get; set; }

		internal static object GetExisting()
		{
			return DataModel.GetService<NetworkServer>();
		}

		/// <summary>
		/// Starts the server.
		/// </summary>
		/// <param name="port">The port number to host on.</param>
		public void Start(int port = 0)
		{
		    _peerConfig = NetworkSettings.GetNetPeerConfig(this);
            _peerConfig.Port = port > 0 ? port : _peerConfig.Port;

            if (_authenticationRequired)
			    _peerConfig.EnableMessageType(NetIncomingMessageType.ConnectionApproval);

			Port = port;
			Address = _peerConfig.BroadcastAddress.ToString();

			_netServer = new NetServer(_peerConfig);
			_peer = _netServer;

			SteamGameServer.LogOnAnonymous();

		    Logger.Trace($"NetworkServer starting at {Address}:{Port}");

			try
			{
				_netServer.Start();

				IsRunning = true;

				Logger.Trace("NetworkServer started.");

				IsRunning = true;
			}
			catch (SocketException e)
			{
				Logger.Warn("NetworkServer threw an exception while starting.");
				Logger.Warn(e.Message);
			}
		}

        /// <summary>
        /// Sets whether or not authentication is required.
        /// </summary>
        /// <param name="required"></param>
        [ScriptSecurity(ScriptIdentity.CoreScript | ScriptIdentity.Server | ScriptIdentity.Editor)]
	    public void SetIsPlayerAuthenticationRequired(bool required)
        {
            ScriptService.AssertIdentity(ScriptIdentity.CoreScript | ScriptIdentity.Server | ScriptIdentity.Editor);
            _authenticationRequired = true;
	    }

        /// <summary>
        /// Stops the server.
        /// </summary>
        public void Stop()
		{
			if (!IsRunning)
			{
				return;
			}

			ClearChildren();

			IsRunning = false;

			_netServer?.Shutdown("NetworkServer.Stop()");
		}

		internal override void ProcessMessages()
		{
			if (_netServer == null)
				return;

			NetIncomingMessage msg;

			while ((msg = _netServer.ReadMessage()) != null)
			{
				switch (msg.MessageType)
				{
					case NetIncomingMessageType.ConnectionApproval:
						AuthorizeConnection(msg);
						break;
					case NetIncomingMessageType.Data:
						ProcessCustomMessage(msg);
						break;
					default:
						Logger.Warn($"Unhandled MessageType \"{msg.MessageType}\"");
						break;
				}

				_netServer.Recycle(msg);
			}
		}

		private void ProcessCustomMessage(NetIncomingMessage msg)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Checks if the given <see cref="NetIncomingMessageType.ConnectionApproval" /> request is authorized.
		/// </summary>
		/// <param name="msg"></param>
		private void AuthorizeConnection(NetIncomingMessage msg)
		{
			var authTicket = msg.ReadBytes(1024);
			var pcbTicket = msg.ReadUInt32();
			var steamId = new CSteamID(msg.ReadUInt64());

			var result = SteamGameServer.BeginAuthSession(authTicket, (int)pcbTicket, steamId);

			var connection = msg.SenderConnection;

			if (result == EBeginAuthSessionResult.k_EBeginAuthSessionResultOK)
			{
				connection.Approve();
				Logger.Info($"Connection {connection.RemoteEndPoint} approved.");
			}
			else
			{
				connection.Deny(result.ToString());
				Logger.Warn($"Connection {connection.RemoteEndPoint} was not approved: {result}");
			}
		}

		/// <summary>
		/// Creates a <see cref="Player" /> and <see cref="ServerReplicator" /> for the client.
		/// </summary>
		private void AddClient(string persona, NetConnection connection)
		{
			var player = new Player(LoginService.SteamId.GetAccountID().m_AccountID) {Name = persona};

			var replicator = new ServerReplicator(connection, player);
			replicator.Parent = this;

			player.Parent = Game.Players;
			Game.Players.PlayerAdded.Fire(player);
		}

		internal void ConfigureAsTeamBuildServer(bool b)
		{
		}
    }
}