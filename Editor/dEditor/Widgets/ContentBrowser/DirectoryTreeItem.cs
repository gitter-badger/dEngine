// DirectoryTreeItem.cs - dEditor
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
using System.IO;
using Caliburn.Micro;

namespace dEditor.Widgets.ContentBrowser
{
	public class DirectoryTreeItem : PropertyChangedBase
	{
		private readonly bool _isRoot;
		private Uri _icon;
		private bool _isExpanded;
		private bool _isSelected;

		private DirectoryTreeItem(string name, DirectoryInfo directory, bool isRoot)
		{
			Directory = directory;
			_isRoot = isRoot;
			Name = name;
			Path = directory.FullName;
			UpdateSubFolders();
			UpdateIcon();
		}

		public DirectoryTreeItem(string name, DirectoryInfo directory) : this(name, directory, true)
		{
		}

		public string Name { get; set; }

		public string Path { get; set; }

		public Uri Icon
		{
			get { return _icon; }
			set
			{
				if (Equals(value, _icon)) return;
				_icon = value;
				NotifyOfPropertyChange();
			}
		}

		public bool IsExpanded
		{
			get { return _isExpanded; }
			set
			{
				if (value == _isExpanded) return;
				_isExpanded = value;
				UpdateIcon();
				NotifyOfPropertyChange();
			}
		}

		public ObservableCollection<DirectoryTreeItem> SubFolders { get; private set; }

		public double FontSize => _isRoot ? 14 : 12;
		public double Height => _isRoot ? 28 : 22;

		public bool IsSelected
		{
			get { return _isSelected; }
			set
			{
				_isSelected = value;
				NotifyOfPropertyChange();
			}
		}

		public DirectoryInfo Directory { get; }

		private void UpdateIcon()
		{
			string path;

			if (_isExpanded)
			{
				path = _isRoot
					? "/dEditor;component/Content/Icons/Toolbar/folder_Open_16xLG.png"
					: "/dEditor;component/Content/Icons/Toolbar/FolderOpen_16x.png";
			}
			else
			{
				path = _isRoot
					? "/dEditor;component/Content/Icons/Toolbar/folder_Closed_16xLG.png"
					: "/dEditor;component/Content/Icons/Toolbar/Folder_16x.png";
			}

			Icon =
				new Uri(path, UriKind.Relative);
		}

		public void UpdateSubFolders()
		{
			SubFolders = new ObservableCollection<DirectoryTreeItem>();

			foreach (var subDir in Directory.GetDirectories())
			{
				SubFolders.Add(new DirectoryTreeItem(subDir.Name, subDir, false));
			}
		}
	}
}