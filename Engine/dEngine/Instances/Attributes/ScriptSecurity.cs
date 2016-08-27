// SecurityContext.cs - dEngine
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
using dEngine.Services;
using System.Runtime.CompilerServices;

namespace dEngine.Instances.Attributes
{
    /// <summary>
    /// Defines the required script identity to access this member.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property), Serializable]
    public class ScriptSecurityAttribute : Attribute, IValueAttribute
    {
        /// <summary/>
        public ScriptIdentity Identity { get; }

        /// <summary/>
        internal ScriptSecurityAttribute(ScriptIdentity identity)
        {
            Identity = identity;
        }
        
        string IValueAttribute.GetValue()
        {
            return Identity.ToString();
        }
    }
}