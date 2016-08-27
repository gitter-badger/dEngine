// Program.cs - dEditor
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.IO;
using dEngine;
using dEngine.Instances;
using dEngine.Services.Networking;

namespace TeamBuildServer
{
	public class TeamBuild
	{
		public static int Port { get; private set; } = 32300;
		public static TeamBuildProject Project { get; private set; }

		private static void Main(string[] args)
		{
#if DEBUG
			Start(new TeamBuildProject("Test", 480, "Place1",
				"C:/Users/Dan/Documents/dEditor/Projects/MyGametest/MyGametest.dproj"));
#endif
		}

		public static void Start(TeamBuildProject project)
		{
			Engine.SaveGame = SaveGame;

            Engine.Start(EngineMode.Server);

			var networkServer = Game.NetworkServer = DataModel.GetService<NetworkServer>();
			networkServer.CustomMessageHandler = (typeId, message) => { return false; };
			networkServer.ConfigureAsTeamBuildServer(true);

			Game.DataModel.BindToClose(new Action(() =>
			{
				Game.DataModel.SaveGame(SaveFilter.SaveGame);
				Game.DataModel.SaveGame(SaveFilter.SaveWorld);
			}), 100000);

			Game.Workspace.LoadPlace(project.Place);
		}

		private static void SaveGame(Stream stream)
		{
		}
	}
}