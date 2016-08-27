// ShadowMode.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.
#pragma warning disable 1591
namespace dEngine
{
    public enum ShadowMode
    {
        FixedSizePcf = 0,
        GridPcf = 1,
        RandomDiscPcf = 2,
        OptimizedPcf = 3,
        Vsm = 4,
        Evsm2 = 5,
        Evsm4 = 6,
        MsmHamburger = 7,
        MsmHausdorff = 8,
    }
}