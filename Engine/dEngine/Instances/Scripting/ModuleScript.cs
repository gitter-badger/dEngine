// ModuleScript.cs - dEngine
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
using dEngine.Services;
using Neo.IronLua;

namespace dEngine.Instances
{
	/// <summary>
	/// A script which returns a module object when passed as an argument to require()
	/// </summary>
	[TypeId(56), ToolboxGroup("Scripting"), ExplorerOrder(5)]
	public sealed class ModuleScript : LuaSourceContainer
	{
		/// <inheritdoc />
		public ModuleScript()
		{
			Source = "local module = {in.\n\nreturn module\n\n";

            RunService.Service.SimulationEnded.Connect(ResetReturnValue);
		}

	    private void ResetReturnValue()
	    {
	        ReturnValue = null;
	    }

	    internal LuaResult ReturnValue { get; set; }

        /// <summary/>
	    public override void Destroy()
	    {
	        base.Destroy();
            RunService.Service.SimulationEnded.Disconnect(ResetReturnValue);
        }
	}
}