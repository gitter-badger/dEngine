// WindowMode.cs - dEngine
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
    public enum WindowMode
    {
        /// <summary>
        /// The game will be in fullscreen mode. This has improved performance over the other modes.
        /// </summary>
        Fullscreen,
        /// <summary>
        /// The game will be in a regular, resizeable window.
        /// </summary>
        Windowed,
        /// <summary>
        /// The game will be in a borderless window and take up the whole screen, but will not enter proper fullscreen mode.
        /// This may make switching between windows easier, but comes at a performance cost.
        /// </summary>
        Borderless
    }
}