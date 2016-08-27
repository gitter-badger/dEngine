// LuaSettings.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System.Runtime.CompilerServices;
using dEngine.Instances.Attributes;


namespace dEngine.Settings.Global
{
	/// <summary>
	/// Settings for the Lua runtime.
	/// </summary>
	[TypeId(185)]
	public class LuaSettings : Settings
	{
		private static bool _debugEngineEnabled;
		private static double _defaultWaitTime;
		private static bool _areScriptStartsReported;

		/// <summary>
		/// If true, script starts will be logged.
		/// </summary>
		[EditorVisible("Diagnostics")]
		public static bool AreScriptStartsReported
		{
			get { return _areScriptStartsReported; }
			set
			{
				_areScriptStartsReported = value;
				NotifyChangedStatic();
			}
		}

		/// <summary>
		/// The amount of time to wait for parameterless wait() calls.
		/// </summary>
		[EditorVisible("Settings")]
		public static double DefaultWaitTime
		{
			get { return _defaultWaitTime; }
			set
			{
				_defaultWaitTime = value;
				NotifyChangedStatic();
			}
		}

		/// <summary>
		/// Determines whether scripts will be compiled with a debug engine.
		/// </summary>
		[EditorVisible("Debug")]
		public static bool DebugEngineEnabled
		{
			get { return _debugEngineEnabled; }
			set
			{
				_debugEngineEnabled = value;
				NotifyChangedStatic();
			}
		}

		private static void NotifyChangedStatic([CallerMemberName] string propertyName = null)
		{
			Engine.Settings?.LuaSettings?.NotifyChanged(propertyName);
		}

		/// <inheritdoc />
		public override void RestoreDefaults()
		{
			AreScriptStartsReported = false;
		    DebugEngineEnabled = true;
            DefaultWaitTime = 0.03;
		}
	}
}