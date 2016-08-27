// Players.cs - dEngine
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
using System.Collections.Generic;
using System.Linq;
using dEngine.Instances;
using dEngine.Instances.Attributes;
using Neo.IronLua;


namespace dEngine.Services
{
	/// <summary>
	/// A service for handling players. When a client joints the game, a <see cref="Player" /> is parented to this service.
	/// </summary>
	[TypeId(6), ExplorerOrder(2), ToolboxGroup("Containers")]
	public sealed class Players : Service
	{
		internal static Players Service;

		/// <summary>
		/// Fired when a client sends a chat message to the server.
		/// </summary>
		public readonly Signal<string, byte> Chatted;

		/// <summary>
		/// Fired when a player joins the game.
		/// </summary>
		public readonly Signal<Player> PlayerAdded;

		/// <summary>
		/// Fired when a player leaves the game.
		/// </summary>
		public readonly Signal<Player> PlayerRemoved;

		private bool _characterAutoLoads;
		private Player _localPlayer;

		/// <inheritdoc />
		public Players()
		{
			Service = this;
			CharacterAutoLoads = true;

			PlayerAdded = new Signal<Player>(this);
			PlayerRemoved = new Signal<Player>(this);
			Chatted = new Signal<string, byte>(this);

			PlayerAdded.Event += player =>
			{
				if (_characterAutoLoads)
					player.LoadCharacter();

				Logger.Info($"PlayerAdded: \"{player.Name}\"");
			};

			Service = this;
		}


        /// <summary>
        /// Gets a list of players.
        /// </summary>
        internal IEnumerable<Player> AllPlayers => Children.OfType<Player>();

        /// <summary>
        /// The player object for the current client.
        /// </summary>
        /// <remarks>
        /// Accessing this property from a server will cause an exception.
        /// </remarks>
        public Player LocalPlayer
		{
			get { return _localPlayer; }
			internal set
			{
                if (Engine.Mode == EngineMode.Server)
                    throw new InvalidOperationException("The LocalPlayer property cannot be accessed from a server.");
                _localPlayer = value;
				if (value != null)
					value.IsLocalPlayer = true;
			}
		}

		/// <summary>
		/// Determines if a <see cref="Character" /> is spawned when the player joins/dies.
		/// </summary>
		[InstMember(1)]
		public bool CharacterAutoLoads
		{
			get { return _characterAutoLoads; }
			set
			{
				if (value == _characterAutoLoads)
					return;

				_characterAutoLoads = value;
				NotifyChanged(nameof(CharacterAutoLoads));
			}
		}

	    private int _maxPlayers;

	    /// <summary>
	    /// The maximum amount of players.
	    /// </summary>
	    [InstMember(2), EditorVisible("Data")]
	    public int MaxPlayers
	    {
	        get { return _maxPlayers; }
	        set
	        {
	            if (value == _maxPlayers) return;
	            _maxPlayers = value;
	            NotifyChanged();
	        }
	    }

        public int NumPlayers { get; private set; }

	    /// <summary>
		/// Creates the LocalPlayer.
		/// </summary>
		public Player CreateLocalPlayer()
		{
			if (_localPlayer != null)
				throw new InvalidOperationException("LocalPlayer already exists.");
		    var plr = new Player(LoginService.SteamId.GetAccountID().m_AccountID)
		    {
		        Name = LoginService.ProfileName
		    };
            LocalPlayer = plr;
            plr.PlayerGui = new PlayerGui {Parent = plr, ParentLocked = true};
		    plr.PlayerGui.FetchStarterGuis();

		    return plr;
		}

		internal static object GetExisting()
		{
			return DataModel.GetService<Players>();
		}

		/// <summary>
		/// Returns a player with the provided steam ID.
		/// </summary>
		/// <param name="steamId">The Steam User ID to look for.</param>
		public Player GetPlayerByUserId(uint steamId)
		{
			return (Player)Children.FirstOrDefault(kv => (kv as Player)?.UserId == steamId);
		}

		/// <summary>
		/// Gets a list of connected players.
		/// </summary>
		/// <returns></returns>
		public LuaTable GetPlayers()
		{
			var table = new LuaTable();
			foreach (var child in Children)
			{
				var player = child as Player;
				if (player != null)
				{
					table.SetArrayValue(0, player);
				}
			}
			return table;
		}

		/// <summary>
		/// Returns a <see cref="FriendPages" /> object which contains information for the given player's friends.
		/// </summary>
		/// <remarks>
		/// The content of the array are the following:
		/// <list type="table">
		///     <listheader>
		///         <term>Name</term>
		///         <term>Type</term>
		///         <term>Description</term>
		///     </listheader>
		///     <item>
		///         <term>UserId</term>
		///         <term>number</term>
		///         <term>The UserId of the friend.</term>
		///     </item>
		///     <item>
		///         <term>Username</term>
		///         <term>string</term>
		///         <term>The username of the friend.</term>
		///     </item>
		///     <item>
		///         <term>IsOnline</term>
		///         <term>Determines If the friend is currently online.</term>
		///         <term>bool</term>
		///     </item>
		/// </list>
		/// </remarks>
		/// <param name="userId">The UserID of the player who has the friends list.</param>
		// TODO: Figure out how to get friends of public profiles
		public FriendPages GetFriendsAsync(uint userId)
		{
			var pages = new FriendPages(userId);
			return pages;
		}

		/// <summary>
		/// Sends a chat message to the server.
		/// </summary>
		/// <param name="message">The message to send.</param>
		/// <param name="channel">The channel to send the message on. (0-255)</param>
		public void Chat(string message, byte channel)
		{
			throw new NotImplementedException();
		}

        /// <inheritdoc/>
	    protected override void OnChildAdded(Instance child)
	    {
	        base.OnChildAdded(child);
	        if (child is Player)
	            NumPlayers++;

        }

        /// <inheritdoc/>
        protected override void OnChildRemoved(Instance child)
        {
            base.OnChildAdded(child);
            if (child is Player)
                NumPlayers--;

        }
    }
}