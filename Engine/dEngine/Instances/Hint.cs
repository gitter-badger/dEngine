// Hint.cs - dEngine
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

namespace dEngine.Instances
{
    /// <summary>
    /// Displays a black text bar at the top of the screen.
    /// </summary>
    [TypeId(228), ExplorerOrder(1), Obsolete]
    public sealed class Hint : Message
    {
        /// <summary/>
        public Hint()
        {
            _label.BackgroundColour = Colour.Black;
            _label.TextColour = Colour.White;
            _label.TextStrokeColour = Colour.Transparent;
            _label.Size = new UDim2(1, 0, 0, 20);
        }
    }
}