// UserAnalyticsSettings.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using dEngine.Instances.Attributes;

namespace dEngine.Settings.User
{
    /// <summary>
    /// Analytics settings.
    /// </summary>
    [TypeId(214)]
    public class UserAnalyticsSettings : Settings
    {
        /// <summary>
        /// The device ID.
        /// </summary>
        public static string Device { get; internal set; }
        /// <summary>
        /// The version of the analytics events.
        /// </summary>
        public static int Version { get; internal set; }
        /// <summary>
        /// The version of the analytics SDK.
        /// </summary>
        public static string SdkVersion { get; internal set; }
        /// <summary>
        /// The number of sessions since this file was created.
        /// </summary>
        public static uint SessionCount { get; internal set; }
        /// <summary>
        /// The number of sessions since this file was created.
        /// </summary>
        public static uint TransactionCount { get; internal set; }
        /// <summary>
        /// Determines if "Limit Ad Tracking" is detected on iOS.
        /// </summary>
        public static bool LimitAdTracking { get; internal set; }
        /// <summary>
        /// Determines if the device is rooted.
        /// </summary>
        public static bool Jailbroken { get; internal set; }
        /// <summary>
        /// The gender of the user.
        /// </summary>
        public static Gender Gender { get; internal set; }
        /// <summary>
        /// The birth year of the user.
        /// </summary>
        public static int BirthYear { get; internal set; }

        /// <summary>
        /// The engine version, including the name.
        /// </summary>
        public static string EngineVersion { get; internal set; }

        public override void RestoreDefaults()
        {
            Device = "Generic-PC";
            Version = 2;
            SdkVersion = "rest api v2";
            LimitAdTracking = false;
            Jailbroken = false;
            Gender = Gender.Unspecified;
            BirthYear = 0;
            EngineVersion = $"dEngine {Engine.Version}";
        }
    }
}