
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using dEngine.Services;
using dEngine.Utility;
using Neo.IronLua;


namespace dEngine.Instances
{
    using Attributes;

    /// <summary>
    /// A player team.
    /// </summary>
    [TypeId(19), ToolboxGroup("Gameplay")]
    public class Team : Instance
    {
        private readonly HashSet<Player> _players;

        /// <summary/>
        public Team()
        {
            _players = new HashSet<Player>();
            Parent = Teams.GetExisting();
            ParentLocked = true;
        }

        private bool _autoAssignable;

        /// <summary>
        /// Determines if this team can be balanced.
        /// </summary>
        [InstMember(1), EditorVisible("Behaviour")]
        public bool AutoAssignable
        {
            get { return _autoAssignable; }
            set
            {
                if (value == _autoAssignable)
                    return;
                _autoAssignable = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Gets a list of players that are a member of this team.
        /// </summary>
        public IEnumerable<Player> Players => Children.OfType<Player>();

        /// <summary>
        /// Returns a list of players that are a member of this team.
        /// </summary>
        public LuaTable GetPlayers()
        {
            LuaTable table;
            lock (Locker)
            {
                table = _players.ToLuaTable();
            }
            return table;
        }

        internal void AddPlayer(Player player)
        {
            lock (Locker)
            {
                _players.Add(player);
            }
        }

        internal void RemovePlayer(Player player)
        {
            lock (Locker)
            {
                _players.Remove(player);
            }
        }
    }
}