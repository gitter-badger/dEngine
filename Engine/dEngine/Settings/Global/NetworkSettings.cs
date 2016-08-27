// NetworkSettings.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System.Runtime.CompilerServices;
using dEngine.Instances.Attributes;
using dEngine.Services.Networking;
using Lidgren.Network;

namespace dEngine.Settings.Global
{
    /// <summary>
    /// Networking settings.
    /// </summary>
    /// <remarks>
    /// Lag Simulation only works in debug builds.
    /// </remarks>
    [TypeId(215)]
    public class NetworkSettings : Settings
    {
        private static readonly NetPeerConfiguration _peerConfig;
        private static int _preferredClientPort;
        private static int _preferredServerPort;
        private static float _simulatedAverageLatency;
        private static float _simulatedDuplicatesChance;
        private static float _simulatedMinimumLatency;
        private static float _simulatedPacketLoss;
        private static float _simulatedRandomLatency;
        private static double _tickrate;

        static NetworkSettings()
        {
            _peerConfig = new NetPeerConfiguration($"dEngineGame_{Engine.AppId}");
        }

        /// <summary>
        /// The preferred port to use as a client.
        /// </summary>
        [EditorVisible("Port", "Preferred Client Port")]
        public static int PreferredClientPort
        {
            get { return _preferredClientPort; }
            set
            {
                if (value == _preferredClientPort) return;
                _preferredClientPort = value;
                NotifyChangedStatic();
            }
        }

        /// <summary>
        /// The preferred port to use as a server.
        /// </summary>
        [EditorVisible("Port", "Preferred Server Port")]
        public static int PreferredServerPort
        {
            get { return _preferredServerPort; }
            set
            {
                if (value == _preferredServerPort) return;
                _preferredServerPort = value;
                NotifyChangedStatic();
            }
        }
        
        /// <summary>
        /// Enables UPnP support, which allows port forwarding and access to the external IP address.
        /// </summary>
        [EditorVisible("UPnP", "Enable UPnP")]
        public static bool EnableUPnP
        {
            get { return _peerConfig.EnableUPnP; }
            set
            {
                if (value == _peerConfig.EnableUPnP) return;
                _peerConfig.EnableUPnP = value;
                NotifyChangedStatic();
            }
        }

        /// <summary>
        /// The maximum transmission unit.
        /// </summary>
        [EditorVisible("MTU", "Mtu Override")]
        public static int Mtu
        {
            get { return _peerConfig.MaximumTransmissionUnit; }
            set
            {
                if (value == _peerConfig.MaximumTransmissionUnit) return;
                if (value <= 0) value = 1400;
                _peerConfig.MaximumTransmissionUnit = value;
                NotifyChangedStatic();
            }
        }

        /// <summary>
        /// Determines if the MTU should automatically expand.
        /// </summary>
        [EditorVisible("MTU", "Auto Expand Mtu")]
        public static bool AutoExpandMtu
        {
            get { return _peerConfig.AutoExpandMTU; }
            set
            {
                if (value == _peerConfig.AutoExpandMTU) return;
                _peerConfig.AutoExpandMTU = value;
                NotifyChangedStatic();
            }
        }

        /// <summary>
        /// The behaviour of unreliable sends above <see cref="Mtu"/>.
        /// </summary>
        [EditorVisible("MTU", "Unreliable Size Behaviour")]
        public static UnreliableSizeBehaviour UnreliableSizeBehaviour
        {
            get { return (UnreliableSizeBehaviour)_peerConfig.UnreliableSizeBehaviour; }
            set
            {
                _peerConfig.UnreliableSizeBehaviour = (NetUnreliableSizeBehaviour)value;
                NotifyChangedStatic();
            }
        }

        /// <summary>
        /// Simulates packet loss.
        /// </summary>
        [EditorVisible("Lag Simulation", "Simulated Packet Loss"), Range(0, 1)]
        public static float SimulatedPacketLoss
        {
            get { return _simulatedPacketLoss; }
            set
            {
                if (value == _simulatedPacketLoss) return;
                _simulatedPacketLoss = value;
                NotifyChangedStatic();
            }
        }

        /// <summary>
        /// The chance for a packet to be duplicated.
        /// </summary>
        [EditorVisible("Lag Simulation", "Simulated Duplicates Chance"), Range(0, 1)]
        public static float SimulatedDuplicatesChance
        {
            get { return _simulatedDuplicatesChance; }
            set
            {
                if (value == _simulatedDuplicatesChance) return;
                _simulatedDuplicatesChance = value;
                NotifyChangedStatic();
            }
        }

        /// <summary>
        /// The minimum amount of latency in seconds.
        /// </summary>
        [EditorVisible("Lag Simulation", "Simulated Minimum Latency")]
        public static float SimulatedMinimumLatency
        {
            get { return _simulatedMinimumLatency; }
            set
            {
                if (value == _simulatedMinimumLatency) return;
                _simulatedMinimumLatency = value;
                NotifyChangedStatic();
            }
        }

        /// <summary>
        /// The value for random latency in seconds.
        /// </summary>
        [EditorVisible("Lag Simulation", "Simulated Random Latency")]
        public static float SimulatedRandomLatency
        {
            get { return _simulatedRandomLatency; }
            set
            {
                if (value == _simulatedRandomLatency) return;
                _simulatedRandomLatency = value;
                NotifyChangedStatic();
            }
        }

        /// <summary>
        /// The average simulated latency in seconds.
        /// </summary>
        [EditorVisible("Lag Simulation", "Simulated Random Latency")]
        public static float SimulatedAverageLatency
        {
            get { return _simulatedAverageLatency; }
            set
            {
                if (value == _simulatedAverageLatency) return;
                _simulatedAverageLatency = value;
                NotifyChangedStatic();
            }
        }

        /// <summary>
        /// The size of the send buffer.
        /// </summary>
        [EditorVisible("Buffer", "Send Buffer Size")]
        public static int SendBufferSize
        {
            get { return _peerConfig.SendBufferSize; }
            set
            {
                if (value == _peerConfig.SendBufferSize) return;
                _peerConfig.SendBufferSize = value;
                NotifyChangedStatic();
            }
        }

        /// <summary>
        /// The size of the receive buffer.
        /// </summary>
        [EditorVisible("Buffer", "Receive Buffer Size")]
        public static int ReceiveBufferSize
        {
            get { return _peerConfig.ReceiveBufferSize; }
            set
            {
                if (value == _peerConfig.ReceiveBufferSize) return;
                _peerConfig.ReceiveBufferSize = value;
                NotifyChangedStatic();
            }
        }

        /// <summary>
        /// The default capacity for outgoing messages.
        /// </summary>
        [EditorVisible("Network", "Outgoing Message Capacity")]
        public static int DefaultOutgoingMessageCapacity
        {
            get { return _peerConfig.DefaultOutgoingMessageCapacity; }
            set
            {
                if (value == _peerConfig.DefaultOutgoingMessageCapacity) return;
                _peerConfig.DefaultOutgoingMessageCapacity = value;
                NotifyChangedStatic();
            }
        }

        /// <summary>
        /// The time between latency calculating pings.
        /// </summary>
        [EditorVisible("Network", "Ping Interval")]
        public static float PingInterval
        {
            get { return _peerConfig.PingInterval; }
            set
            {
                if (value == _peerConfig.PingInterval) return;
                _peerConfig.PingInterval = value;
                NotifyChangedStatic();
            }
        }

        /// <summary>
        /// The number of seconds timeout will be postponed on a successful ping/pong.
        /// </summary>
        [EditorVisible("Network", "Connection Timeout")]
        public static float ConnectionTimeout
        {
            get { return _peerConfig.ConnectionTimeout; }
            set
            {
                if (value == _peerConfig.ConnectionTimeout) return;
                _peerConfig.ConnectionTimeout = value;
                NotifyChangedStatic();
            }
        }

        /// <summary>
        /// Determines if messages should be recycled to avoid execssive garbage collection.
        /// </summary>
        [EditorVisible("Recycling", "Use Message Recycling")]
        public static bool UseMessageRecycling
        {
            get { return _peerConfig.UseMessageRecycling; }
            set
            {
                if (value == _peerConfig.UseMessageRecycling) return;
                _peerConfig.UseMessageRecycling = value;

                NotifyChangedStatic();
            }
        }

        /// <summary>
        /// The value at which network updates are performed in seconds.
        /// </summary>
        [EditorVisible("Network", "Tick Rate")]
        public static double Tickrate
        {
            get { return _tickrate; }
            set
            {
                if (value == _tickrate) return;
                _tickrate = value;
                NotifyChangedStatic();
            }
        }

        /// <summary>
        /// The maximum number of messages to keep in the recycle cache.
        /// </summary>
        [EditorVisible("Recycling", "Use Message Recycling")]
        public static int RecycledCacheMaxCount
        {
            get { return _peerConfig.RecycledCacheMaxCount; }
            set
            {
                if (value == _peerConfig.RecycledCacheMaxCount) return;
                _peerConfig.RecycledCacheMaxCount = value;
                NotifyChangedStatic();
            }
        }

        private static float _networkingScale;

        /// <summary>
        /// Visual smoothing of network data such as players and objects.
        /// </summary>
        /// <remarks>
        /// Lowering this value will reduce latency but could increase visual errors.
        /// </remarks>
        [EditorVisible("Network", "Networking Scale")]
        public static float NetworkingScale
        {
            get { return _networkingScale; }
            set
            {
                if (value == _networkingScale) return;
                _networkingScale = value;
                NotifyChangedStatic();
            }
        }



        internal static NetPeerConfiguration GetNetPeerConfig(NetworkPeer peer)
        {
            var clone = _peerConfig.Clone();
            clone.Port = peer is NetworkClient ? _preferredClientPort : _preferredServerPort;
            clone.MaximumConnections = Game.Players.MaxPlayers;
            return clone;
        }

        /// <summary />
        public override void RestoreDefaults()
        {
            Mtu = NetPeerConfiguration.kDefaultMTU;
            UnreliableSizeBehaviour = UnreliableSizeBehaviour.IgnoreMTU;
            AutoExpandMtu = false;
            ReceiveBufferSize = 131071;
            SendBufferSize = 131071;
            DefaultOutgoingMessageCapacity = 16;
            PingInterval = 4.0f;
            ConnectionTimeout = 25.0f;
            UseMessageRecycling = true;
            RecycledCacheMaxCount = 64;
            Tickrate = 1 / 20f;
        }

        private static void NotifyChangedStatic([CallerMemberName] string propertyName = null)
        {
            Engine.Settings?.NetworkSettings?.NotifyChanged(propertyName);
        }
    }
}