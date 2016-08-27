﻿// SharpTreeNodeAdapter.cs - dEditor
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System.Linq;
using dEditor.Widgets.Diagnostics.SharpTreeView;
using dEngine.Instances.Diagnostics;

namespace dEditor.Widgets.Diagnostics
{
    public class StatsItemTreeNode : SharpTreeNode
    {
        public StatsItem Item { get; }

        public string Value => Item.ValueString;

        public StatsItemTreeNode(StatsItem item)
        {
            Item = item;
        }

        public override object Text => Item.Name;
        public override object Icon => null;
        public override bool ShowExpander => Item.Children.Count > 0;

        public override void Delete()
        {
            base.Delete();
            Parent.Children.Remove(this);
        }

        protected override void LoadChildren()
        {
            Children.AddRange(Item.Children.OfType<StatsItem>().Select(item => new StatsItemTreeNode(item)));
        }
    }
}