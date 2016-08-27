// TreeItemBase.cs - dEditor
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
using Caliburn.Micro;
using dEditor.Utility;

namespace dEditor.Framework
{
	public abstract class TreeItemBase : PropertyChangedBase, IComparable<TreeItemBase>
	{
		private Uri _icon;
		private bool _isExpanded;
		private bool _isSelected;
		private string _name;

		protected TreeItemBase()
		{
			Children = new SortedObservableCollection<TreeItemBase>();
		}

		public string Name
		{
			get { return _name; }
			set
			{
				if (value == _name) return;
				_name = value;
				NotifyOfPropertyChange(() => Name);
			}
		}

		public Uri Icon
		{
			get { return _icon; }
			set
			{
				if (Equals(value, _icon)) return;
				_icon = value;
				NotifyOfPropertyChange(() => Icon);
			}
		}

		public bool IsSelected
		{
			get { return _isSelected; }
			set
			{
				if (Equals(value, _isSelected)) return;
				_isSelected = value;
				NotifyOfPropertyChange();
			}
		}

		public bool IsExpanded
		{
			get { return _isExpanded; }
			set
			{
				if (Equals(value, _isExpanded)) return;
				_isExpanded = value;
				NotifyOfPropertyChange();
			}
		}

		public SortedObservableCollection<TreeItemBase> Children { get; }

		public virtual int CompareTo(TreeItemBase other)
		{
			return Name.CompareTo(other.Name);
		}
	}
}