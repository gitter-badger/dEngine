// CustomTreeItem.cs - dEditor
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.ObjectModel;
using Caliburn.Micro;
using dEngine.Instances;

namespace dEditor.Framework.Controls.CustomTreeView
{
	public abstract class CustomTreeItem : Screen, IComparable<CustomTreeItem>
	{
		public abstract string Name { get; set; }
		public abstract Uri Icon { get; set; }
		public abstract bool IsExpanded { get; set; }
		public abstract bool IsSelected { get; set; }
		public abstract Instance Instance { get; set; }
		public new abstract CustomTreeItem Parent { get; set; }
		public abstract ObservableCollection<CustomTreeItem> Items { get; set; }

		public abstract int CompareTo(CustomTreeItem other);

		public static implicit operator WeakReference<CustomTreeItem>(CustomTreeItem item)
		{
			return new WeakReference<CustomTreeItem>(item);
		}

		public static implicit operator CustomTreeItem(WeakReference<CustomTreeItem> weakRef)
		{
			CustomTreeItem item;
			return weakRef.TryGetTarget(out item) ? item : null;
		}
	}
}