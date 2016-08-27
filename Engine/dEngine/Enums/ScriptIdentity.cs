// ScriptIdentity.cs - dEngine
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
    [Flags]
    public enum ScriptIdentity
    {
        Clr  = 1 << 0,
        Script     = 1 << 1,
        Mod        = 1 << 2,
        Plugin     = 1 << 3,
        Editor     = 1 << 4,
        CoreScript = 1 << 5,
        Server     = 1 << 56,
    }
}