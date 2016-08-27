// NetworkReplicator.cs - dEngine
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


#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace dEngine.Instances
{
	/// <summary>
	/// Base class for the client and server replicator classes.
	/// </summary>
	[TypeId(44), Uncreatable]
	public abstract class NetworkReplicator : Instance
	{
		private readonly NetConnection _connection;
		protected bool _replicationEnabled;

		/// <inheritdoc />
		protected NetworkReplicator(NetConnection connection, Player player)
		{
			_connection = connection;
			Player = player;
		}

		/// <summary>
		/// Gets whether or not the connection is connected.
		/// </summary>
		public bool IsConnected => _connection.Status == NetConnectionStatus.Connected;

		/// <summary>
		/// The player that this replicator is connected to.
		/// </summary>
		[EditorVisible("Data")]
		public Player Player { get; }

		/// <summary>
		/// Determines if replication packets should be sent/received.
		/// </summary>
		public bool ReplicationEnabled
		{
			get { return _replicationEnabled; }
			set
			{
				if (value == _replicationEnabled) return;
				_replicationEnabled = value;
				NotifyChanged();
			}
		}

		/// <summary>
		/// The address of this connection.
		/// </summary>
		[EditorVisible("Data")]
		public string Address { get; protected set; }

		/// <summary>
		/// The port this connection is running on.
		/// </summary>
		[EditorVisible("Data")]
		public int Port { get; protected set; }

		/// <summary>
		/// Returns the player which is associated with this replicator.
		/// </summary>
		public Player GetPlayer()
		{
			return Player;
		}

		/// <summary>
		/// Returns a string representation of network statistics.
		/// </summary>
		/// <returns></returns>
		public string GetLidgrenStatsString()
		{
			return _connection.Statistics.ToString();
		}

		/// <summary>
		/// Disconnects the replicator, ending the connection.
		/// </summary>
		public virtual void CloseConnection(string goodbye = "")
		{
			_connection.Disconnect(goodbye);
			if (!IsDestroyed)
				Destroy();
		}

		/// <inheritdoc />
		public override void Destroy()
		{
			base.Destroy();
			if (IsConnected)
				CloseConnection("Replicator was destroyed.");
		}
	}
}