// LuaSourceContainer.cs - dEngine
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
    /// Base class for an object containing Lua source code.
    /// </summary>
    [TypeId(209)]
    public class LuaSourceContainer : SourceContainer
    {
        /// <summary/>
        public LuaSourceContainer()
        {
            LuaGlobal = ScriptService.GlobalEnvironment;
            Source = "print 'Hello world!'";
        }

        /// <summary>
        /// Gets whether or not the script is currently running code.
        /// </summary>
        public bool IsRunning { get; internal set; }

        internal Player CurrentEditor { get; set; }

        internal LuaThread LuaThread { get; set; }
        internal LuaGlobal LuaGlobal { get; set; }
        internal ScriptDebugger Debugger { get; set; }

        internal ScriptIdentity Identity { get; set; }

        internal void Print(string message, LogLevel level)
        {
            Logger.Log(message, level);
        }

        
    }
}