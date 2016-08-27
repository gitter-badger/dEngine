// InstanceMember.cs - dEngine
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
#pragma warning disable 1591

namespace dEngine
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class InstMemberAttribute : Attribute
    {
        public short Tag { get; set; }

        public InstMemberAttribute(short id)
        {
            Tag = id;
        }
    }
    [AttributeUsage(AttributeTargets.Method)]
    public class InstBeforeSerializationAttribute : Attribute
    {

    }
    [AttributeUsage(AttributeTargets.Method)]
    public class InstAfterSerializationAttribute : Attribute
    {

    }
    [AttributeUsage(AttributeTargets.Method)]
    public class InstBeforeDeserializationAttribute : Attribute
    {

    }
    [AttributeUsage(AttributeTargets.Method)]
    public class InstAfterDeserializationAttribute : Attribute
    {

    }
}