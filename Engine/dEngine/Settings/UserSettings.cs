// UserSettings.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using dEngine.Instances;
using dEngine.Instances.Attributes;
using dEngine.Settings.User;


namespace dEngine.Settings
{
	/// <summary>
	/// User settings container.
	/// </summary>
	[TypeId(187)]
	public class UserSettings : GenericSettings, ISingleton
	{
        /// <summary/>
		public UserSettings()
		{
			UserGameSettings = CreateSettings<UserGameSettings>("Game");
            UserAnalyticsSettings = CreateSettings<UserAnalyticsSettings>("Analytics");
		}

        /// <summary>
        /// The <see cref="UserGameSettings"/>.
        /// </summary>
		public UserGameSettings UserGameSettings { get; }

        /// <summary>
        /// The <see cref="UserAnalyticsSettings"/>.
        /// </summary>
		public UserAnalyticsSettings UserAnalyticsSettings { get; }

        internal static object GetExisting()
		{
			return Engine.UserSettings;
		}
	}
}