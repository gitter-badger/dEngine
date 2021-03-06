﻿// SharpGridView.cs - dEditor
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System.Windows;
using System.Windows.Controls;

namespace dEditor.Widgets.Diagnostics.SharpTreeView
{
    public class SharpGridView : GridView
    {
        static SharpGridView()
        {
            ItemContainerStyleKey =
                new ComponentResourceKey(typeof(SharpTreeView), "GridViewItemContainerStyleKey");
        }

        public static ResourceKey ItemContainerStyleKey { get; private set; }

        protected override object ItemContainerDefaultStyleKey
        {
            get
            {
                return ItemContainerStyleKey;
            }
        }
    }
}