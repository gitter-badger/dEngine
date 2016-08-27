// ClanService.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System.Threading.Tasks;
using dEngine.Instances;
using dEngine.Instances.Attributes;
using Neo.IronLua;

using Steamworks;

namespace dEngine.Services
{
	/// <summary>
	/// A service for fetching information about clans.
	/// </summary>
	[TypeId(80)]
	public class ClanService : Service
	{
		internal static ClanService Service;

		/// <inheritdoc/>
		public ClanService()
		{
			Service = this;
		}

		/// <summary>
		/// Returns a table containing information about the clan.
		/// </summary>
		/// <param name="clanId">The id of the clan.</param>
		[YieldFunction]
		public LuaTable GetClanInfoAsync(uint clanId)
		{
		    var thread = ScriptService.CurrentThread;

			var table = new LuaTable();

			Task.Run(() =>
			{
				var steamId = SteamIdFromClanId(clanId);

				var owner = SteamFriends.GetClanOwner(steamId);

				table["Name"] = SteamFriends.GetClanName(steamId);
				table["Owner"] = new LuaTable
				{
					["Name"] = SteamFriends.GetFriendPersonaName(owner),
					["Id"] = owner.GetAccountID().m_AccountID
				};
				table["Tag"] = SteamFriends.GetClanTag(steamId);
				table["Roles"] = new LuaTable
				{
					[1] = new LuaTable { [Name] = "Officer", }
					[2] = new LuaTable { [Name] = "Moderator" }
					[3] = new LuaTable { [Name] = "Member" }
				};
			}).ContinueWith(x => ScriptService.ResumeThread(thread));

            ScriptService.YieldThread();

            return table;
		}

		internal static CSteamID SteamIdFromClanId(uint userId)
		{
			return new CSteamID(new AccountID_t(userId), EUniverse.k_EUniversePublic,
				EAccountType.k_EAccountTypeClan);
		}

		internal static object GetExisting()
		{
			return DataModel.GetService<ClanService>();
		}
	}
}