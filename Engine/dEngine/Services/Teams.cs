// Teams.cs - dEngine
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
using dEngine.Utility.Extensions;
using Neo.IronLua;


namespace dEngine.Services
{
    /// <summary>
    /// A service for managing teams.
    /// </summary>
    [TypeId(150), ToolboxGroup("Containers")]
    public class Teams : Service
    {
        internal static Teams Service;

        /// <summary />
        public Teams()
        {
            Service = this;
        }

        internal IEnumerable<Team> AllTeams => Children.OfType<Team>();

        private void RebalanceTeamsRandom()
        {
            int i = 0;

            var teams = AllTeams.ToArray();

            var newTeams =
                Players.Service.Children.OfType<Player>()
                    .Where(p => p.Team?.AutoAssignable == true)
                    .Split(teams.Length);

            foreach (var team in newTeams)
                foreach (var plr in team)
                    plr.Team = teams[i++];

        }

        private void RebalanceTeamsTrueSkill()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Rebalances players across teams.
        /// </summary>
        /// <param name="method"></param>
        public void RebalanceTeams(TeamBalanceMethod method = TeamBalanceMethod.Random)
        {
            switch (method)
            {
                case TeamBalanceMethod.Random:
                    RebalanceTeamsRandom();
                    break;
                case TeamBalanceMethod.TrueSkill:
                    RebalanceTeamsTrueSkill();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(method), method, null);
            }
        }

        /// <summary>
        /// Returns a list of existing teams.
        /// </summary>
        public LuaTable GetTeams()
        {
            var table = new LuaTable();
            int i = 0;
            foreach (var child in Children)
            {
                var team = child as Team;
                if (team != null)
                    table[++i] = team;
            }
            return table;
        }

        internal static Teams GetExisting()
        {
            return DataModel.GetService<Teams>();
        }
    }
}