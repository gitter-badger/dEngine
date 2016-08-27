// GlobalSettings.cs - dEngine
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
using dEngine.Settings.Global;


#pragma warning disable 1591

namespace dEngine.Settings
{
	/// <summary>
	/// Global settings container.
	/// </summary>
	[TypeId(184), Uncreatable]
	public class GlobalSettings : GenericSettings, ISingleton
	{
		/// <inheritdoc />
		public GlobalSettings()
		{
			LuaSettings = CreateSettings<LuaSettings>("Lua");
			RenderSettings = CreateSettings<RenderSettings>("Render");
			DebugSettings = CreateSettings<DebugSettings>("Diagnostics");
            NetworkSettings = CreateSettings<NetworkSettings>("Network");
            PhysicsSettings = CreateSettings<PhysicsSettings>("Physics");
		    SoundSettings = CreateSettings<SoundSettings>("Sound");
		}

		public LuaSettings LuaSettings { get; }
		public RenderSettings RenderSettings { get; }
		public DebugSettings DebugSettings { get; }
	    public NetworkSettings NetworkSettings { get; }
	    public PhysicsSettings PhysicsSettings { get; }
	    public SoundSettings SoundSettings { get; set; }

	    internal static object GetExisting()
		{
			return Engine.Settings;
		}
	}
}