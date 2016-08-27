// BindableEvent.cs - dEngine
// Copyright (C) 2016-2016 DanDevPC (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//  
// You should have received a copy of the GNU General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using dEngine.Instances.Attributes;
using Neo.IronLua;

namespace dEngine.Instances
{
    /// <summary>
    /// Acts as a custom event to allow communication between scripts. 
    /// </summary>
    [TypeId(55), ToolboxGroup("Scripting")]
    [Warning("BindableEvents do not allows for communication between the server and client. If you are looking for this functionality use [RemoteEvents](index?.htmltitle=RemoteEvent).")]
    public class BindableEvent : Instance
    {
        /// <summary/>
        public BindableEvent()
        {
            Event = new Signal<LuaResult>(this);
        }

        /// <summary>
        /// Fires the <see cref="Event"/>.
        /// </summary>
        /// <param name="arguments">The arguments to pass to the event.</param>
        public void Fire(params object[] arguments)
        {
            Event.Fire(new LuaResult(arguments));
        }

        /// <summary>
        /// An event which is fired by <see cref="Fire"/>.
        /// </summary>
        /// <eventParam name="arguments"/>
        public readonly Signal<LuaResult> Event;
    }
}