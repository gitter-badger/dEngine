// NetworkPeer.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System.Diagnostics;
using dEngine.Instances.Attributes;
using Lidgren.Network;


namespace dEngine.Services.Networking
{
	/// <summary>
	/// Base class for <see cref="NetworkClient" /> and <see cref="NetworkServer" />
	/// </summary>
	[TypeId(74)]
	public abstract class NetworkPeer : Service
	{
		internal NetPeer _peer;
		internal NetPeerConfiguration _peerConfig;

		/// <inheritdoc />
		protected NetworkPeer()
		{
		}

	    internal static bool IsClient()
	    {
	        return Engine.Mode == EngineMode.Game || Engine.Mode == EngineMode.LevelEditor;
        }

        internal static bool IsServer()
        {
            return Engine.Mode == EngineMode.Server || Engine.Mode == EngineMode.LevelEditor;
        }

        /// <summary>
        /// If true, the peer has been started.
        /// </summary>
        [EditorVisible("Data")]
	    public bool IsRunning
	    {
	        get { return _isRunning; }
	        protected set { _isRunning = value; }
	    }

	    internal abstract void ProcessMessages();

	    protected double _accumulator;
	    private bool _isRunning;

	    /// <summary>
        /// Performs a network update.
        /// </summary>
        /// <param name="step">The time since the last step.</param>
	    public void Update(double step)
        {
            if (!_isRunning)
                return;

	        _accumulator += step;
	        if (_accumulator >= step)
            {
                if (IsRunning)
                {
                    ProcessMessages();
                }
            }
	    }
	}
}