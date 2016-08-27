// LinkOutputItem.cs - dEditor
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using dEngine;

namespace dEditor.Widgets.Output
{
    public class LinkOutputItem : OutputItem
    {
        public LinkOutputItem(ref string msg, ref LogLevel level, ref string logger, ref string url) : base(ref msg, ref level, ref logger)
        {
            Url = url;
        }

        public string Url { get;  }
    }
}