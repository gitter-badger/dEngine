// FriendPages.cs - dEngine
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
using dEngine.Instances.Attributes;
using dEngine.Services;

using Steamworks;

namespace dEngine.Instances
{
	/// <summary>
	/// A variation of pages used by <see cref="Players.GetFriendsAsync" />
	/// </summary>
	[TypeId(191)]
	public sealed class FriendPages : Pages
	{
		private readonly int _friendCount;
		private readonly EFriendFlags _friendFlags;
		private readonly CSteamID _steamId;
		private int _friendIndex;

		internal FriendPages(uint userId)
		{
			var accID = LoginService.SteamId.GetAccountID().m_AccountID;
			if (userId != accID)
				throw new NotSupportedException(
					"GetFriendsAsync() can currently only check the friends of the LocalPlayer.");

			_steamId = new CSteamID(new AccountID_t(userId), EUniverse.k_EUniversePublic,
				EAccountType.k_EAccountTypeIndividual);
			_friendFlags = EFriendFlags.k_EFriendFlagImmediate | EFriendFlags.k_EFriendFlagBlocked;
			if ((_friendCount = SteamFriends.GetFriendCount(_friendFlags)) == 0)
				IsFinished = true;
		}

		/// <inheritdoc />
		public override void AdvanceToNextPage()
		{
			base.AdvanceToNextPage();

			var friend = SteamFriends.GetFriendByIndex(_friendIndex, _friendFlags);
			var friendId = friend.GetAccountID().m_AccountID;
			var personaState = SteamFriends.GetFriendPersonaState(friend);

			_currentPage = new SortedList<string, object>
			{
				{"Username", SteamFriends.GetFriendPersonaName(friend)},
				{"UserId", friendId},
				{"IsOnline", personaState != EPersonaState.k_EPersonaStateOffline},
				{"Status", (FriendStatus)personaState},
				{"AvatarUri", $"avatar://{friendId}"}
			};

			_friendIndex++;

			if (_friendIndex == _friendCount)
				IsFinished = true;
		}
	}
}