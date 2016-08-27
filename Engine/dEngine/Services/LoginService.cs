// LoginService.cs - dEngine
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
using System.IO;
using dEngine.Instances;
using dEngine.Instances.Attributes;
using Steamworks;

namespace dEngine.Services
{
    /// <summary>
    /// A service for logging into a user account.
    /// </summary>
    [TypeId(48), ExplorerOrder(-1)]
    public class LoginService : Service
    {
        internal static bool LoggedIn { get; set; }
        internal static uint UserId { get; set; }
        internal static CSteamID SteamId { get; set; }
        internal static string ProfileName { get; set; }

        internal static LoginService Service;

        internal static LoginService GetExisting()
        {
            return DataModel.GetService<LoginService>();
        }

        /// <summary>
        /// Determines if the client is currently logged in.
        /// </summary>
        public bool IsLoggedIn()
        {
            return LoggedIn;
        }

        /// <summary>
        /// Used to get the user's ID when the <see cref="Player"/> object is not available.
        /// </summary>
        /// <returns></returns>
        public uint GetUserId()
        {
            return UserId;
        }

        /// <summary/>
        public LoginService()
        {
            Service = this;

            LoginSucceeded = new Signal<string, uint>(this);
            LoginFailed = new Signal<string>(this);
        }

        private bool FailLogin(string msg)
        {
            Logger.Warn(msg);
            LoginFailed.Fire(msg);
            return false;
        }

        private bool SteamLogin()
        {
            if (!File.Exists("steam_appid.txt"))
            {
                try
                {
                    File.WriteAllText("steam_appid.txt", Engine.AppId.ToString());
                }
                catch (Exception e)
                {
                    return FailLogin(e.Message);
                }
            }

            if (!SteamAPI.Init())
            {
                return FailLogin(
                    "Could initialize SteamAPI - Make sure Steam is running and the AppID is valid.");
            }

            SteamAPI.RunCallbacks();
            SteamId = SteamUser.GetSteamID();
            UserId = SteamId.GetAccountID().m_AccountID;
            ProfileName = SteamFriends.GetPersonaName();

            LoginSucceeded.Fire(ProfileName);

            return true;
        }

        private void SteamLogout()
        {
            SteamAPI.Shutdown();
        }

        /// <summary>
        /// Fired when a login attempt is successful.
        /// </summary>
        /// <eventParam name="username"/>
        /// <eventParam name="userId"/>
        public readonly Signal<string, uint> LoginSucceeded;

        /// <summary>
        /// Fired when a login attempt fails.
        /// </summary>
        public readonly Signal<string> LoginFailed;

        /// <summary>
        /// Tries to login to the user's account.
        /// </summary>
        /// <returns>A boolean determining if the login was successful.</returns>
        public bool TryLogin()
        {
            if (LoggedIn)
                throw new InvalidOperationException("Cannot login: already logged in.");
            LoggedIn = SteamLogin();

            if (LoggedIn)
            {
                AnalyticsService.BeginSession();
            }

            return LoggedIn;
        }

        /// <summary>
        /// Logs out of the current account.
        /// </summary>
        public void Logout()
        {
            SteamLogout();
            AnalyticsService.EndSession();
            //LoggedIn = false;
        }
    }
}