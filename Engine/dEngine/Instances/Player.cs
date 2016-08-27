// Player.cs - dEngine
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
using dEngine.Instances.Attributes;
using dEngine.Services;
using dEngine.Services.Networking;
using Neo.IronLua;

using Steamworks;
// ReSharper disable UnusedVariable

namespace dEngine.Instances
{
    /// <summary>
    /// Represents a user in-game. A player object is added to the <see cref="Players" /> service when a player joins the
    /// server.
    /// </summary>
    [TypeId(20), Uncreatable, ToolboxGroup("Gameplay")]
    public class Player : Instance
    {
        /// <summary>
        /// Fired when the player's character has spawned.
        /// </summary>
        public readonly Signal<Character> CharacterAdded;

        /// <summary>
        /// Fired when the player's character is about to be removed.
        /// </summary>
        public readonly Signal<Character> CharacterRemoving;

        /// <summary>
        /// Fired when the player has been disconnected or kicked from the server.
        /// </summary>
        public readonly Signal<string> Disconnected;

        private Character _character;
        private CSteamID _steamId;

        private Team _team;

        /// <inheritdoc />
        public Player()
        {
            Archivable = false;

            CharacterAdded = new Signal<Character>(this);
            CharacterRemoving = new Signal<Character>(this);
            Disconnected = new Signal<string>(this);

            Parent = Players.Service;
            ParentLocked = true;
        }

        internal Player(uint userId) : this()
        {
            UserId = userId;
            SteamId = SteamIdFromUserId(userId);
        }

        /// <summary>
        /// Gets whether or not this player is the <see cref="Players.LocalPlayer" />
        /// </summary>
        internal bool IsLocalPlayer { get; set; }

        internal CSteamID SteamId
        {
            get { return _steamId; }
            set
            {
                _steamId = value;
                UserId = value.GetAccountID().m_AccountID;
            }
        }

        /// <summary>
        /// The team that this player is on.
        /// </summary>
        [InstMember(1), EditorVisible("Data")]
        public Team Team
        {
            get { return _team; }
            set
            {
                var oldTeam = _team;
                if (value == oldTeam) return;
                oldTeam?.RemovePlayer(this);
                _team = value;
                value?.AddPlayer(this);
                NotifyChanged();
            }
        }

        /// <summary>
        /// This player's <see cref="Instances.PlayerGui" />.
        /// </summary>
        public PlayerGui PlayerGui { get; internal set; }

        /// <summary>
        /// The player's <see cref="Instances.Character" />.
        /// </summary>
        [EditorVisible("Data")]
        public Character Character
        {
            get { return _character; }
            set
            {
                if (value == _character) return;
                var lastCharacter = _character;
                _character = value;
                NotifyChanged();
                if (value != null)
                    CharacterAdded.Fire(value);
                else
                    CharacterRemoving.Fire(lastCharacter);
            }
        }

        /// <summary>
        /// The player's UserID. (SteamID3)
        /// </summary>
        [EditorVisible("Data")]
        public uint UserId { get; private set; }

        /// <summary>
        /// The replicator that represents this player's connection.
        /// </summary>
        public ServerReplicator Replicator { get; internal set; }

        /// <summary>
        /// Returns true if this player is a friend of the <see cref="Players.LocalPlayer" />.
        /// </summary>
        public bool IsFriend()
        {
            return SteamFriends.HasFriend(_steamId, EFriendFlags.k_EFriendFlagImmediate);
        }

        /// <summary>
        /// Determines if this player is on the same team as the <see cref="Players.LocalPlayer" />.
        /// </summary>
        /// <returns></returns>
        public bool IsTeamMate()
        {
            return Team == Players.Service.LocalPlayer.Team;
        }

        /// <summary>
        /// Disconnects player from the server.
        /// </summary>
        /// <param name="reason"></param>
        public void Kick(string reason)
        {
            Replicator?.CloseConnection(reason);
        }

        /// <summary>
        /// Returns an array of friends who are online.
        /// </summary>
        /// <param name="maxFriends">The maximum number of entries to return.</param>
        public LuaTable GetOnlineFriends(int maxFriends = 250)
        {
            throw new NotImplementedException();
            /*
            var result = new LuaTable();
            var pages = Players.Service.GetFriendsAsync(UserId);
            int i = 1;
            foreach (var page in pages.EnumeratePages())
            {
                var entry = new LuaTable();
                result[i] = entry;
                i++;
            }
            return result;
            */
        }

        /// <summary>
        /// Returns the player that owns the given character.
        /// </summary>
        public Player GetPlayerFromCharacter(Character character)
        {
            return character.Player;
        }

        /// <summary>
        /// Spawns a new <see cref="Character" /> for the given player. If the player already has a character it will be destroyed.
        /// </summary>
        public void LoadCharacter()
        {
            if (this != Players.Service.LocalPlayer && RunService.Service.IsServer())
            {
                throw new InvalidOperationException(
                    "LoadCharacter can only be called by the host or on the local player.");
            }

            Character?.Destroy();

            var character = new Character {Name = Name, Player = this};

            var mesh = new SkeletalMesh
            {
                Parent = character
            };

            var skeleton = new Skeleton
            {
                Parent = character
            };

            var camera = Game.Workspace.CurrentCamera;
            camera.CameraSubject = character;
            camera.CameraType = CameraType.Custom;

            character.Parent = Game.Workspace;
            character.Teleport(new Vector3(0, 10, 0));
            Character = character;

            // copy guis AFTER character has loaded
            if (Game.StarterGui.ResetGuisOnDeath && IsLocalPlayer)
            {
                PlayerGui.ClearChildren();
                PlayerGui.FetchStarterGuis();
            }

            Character.Died.Event += () =>
            {
                if (Game.Players.CharacterAutoLoads)
                {
                    LoadCharacter();
                }
            };

            Logger.Info("Character loaded.");
        }

        /// <inheritdoc />
        public override void Destroy()
        {
            ParentLocked = false;

            base.Destroy();

            Kick("Player object was destroyed.");
        }

        internal static CSteamID SteamIdFromUserId(uint userId)
        {
            return new CSteamID(new AccountID_t(userId), EUniverse.k_EUniversePublic,
                EAccountType.k_EAccountTypeIndividual);
        }
    }
}