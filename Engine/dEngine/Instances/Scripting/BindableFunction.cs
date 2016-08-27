// BindableFunction.cs - dEngine
// Copyright (C) 2016-2016 DanDevPC (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//  
// You should have received a copy of the GNU General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System;
using dEngine.Instances.Attributes;
using Dynamitey;
using Neo.IronLua;

namespace dEngine.Instances
{
    /// <summary>
    /// Acts as a custom function to allow communication between scripts.
    /// </summary>
    [TypeId(54), ToolboxGroup("Scripting")]
    [Warning("BindableFunctions do not allows for communication between the server and client. If you are looking for this functionality use [RemoteFunctions](index?.htmltitle=RemoteFunction).")]
    public class BindableFunction : Instance
    {
        private MulticastDelegate _onInvoke;

        /// <summary>
        /// Invokes the <see cref="OnInvoke"/> method.
        /// </summary>
        /// <param name="args">The arguments to pass to the callback.</param>
        /// <returns>The return value of the callback.</returns>
        public LuaResult Invoke(params object[] args)
        {
            return (LuaResult)OnInvoke.FastDynamicInvoke(args);
        }

        /// <summary>
        /// A function to be invoked when <see cref="Invoke"/> is called.
        /// </summary>
        public MulticastDelegate OnInvoke
        {
            get { return _onInvoke; }
            set
            {
                _onInvoke = value;
                NotifyChanged();
            }
        }
    }
}