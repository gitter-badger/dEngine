// ContentBrowserViewModel.cs - dEditor
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using dEditor.Framework;


namespace dEditor.Widgets.ContentBrowser
{
	[Export(typeof(IContentBrowser))]
	public sealed class ContentBrowserViewModel : Widget, IContentBrowser
	{
		private List<ContentItem> _contents;
		private bool _directoryTreeVisible;
		private List<DirectoryTreeItem> _rootDirectories;
		private IList<ContentItem> _selectedContents;
		private DirectoryTreeItem _selectedDirectory;
		private bool _showFolders;
		private bool _showNonContent;

		public ContentBrowserViewModel()
		{
			DisplayName = "Content Browser";
			UpdateDirectoryTree();
			IsDirectoryTreeVisible = true;
			ShowFolders = true;
			ShowNonContent = false;
			SelectedContents = new List<ContentItem>();
			Editor.Current.ProjectChanged += OnProjectChanged;
		}

	    public string FolderFilter { get; set; }
	    public string FileFilter { get; set; }


        public DirectoryTreeItem SelectedDirectory
		{
			get { return _selectedDirectory; }
			set
			{
			    if (value == null) return;
				if (Equals(value, _selectedDirectory)) return;
				_selectedDirectory = value;
				value.IsSelected = true;
				UpdateContents();
				NotifyOfPropertyChange();
			}
		}

		public IList<ContentItem> SelectedContents
		{
			get { return _selectedContents; }
			set
			{
				if (Equals(value, _selectedContents)) return;
				_selectedContents = value;
				NotifyOfPropertyChange();
			}
		}

		public override PaneLocation PreferredLocation => PaneLocation.Bottom;

		public bool IsEnabled => Project.Current != null;

		public List<DirectoryTreeItem> RootDirectories
		{
			get { return _rootDirectories; }
			private set
			{
				if (Equals(value, _rootDirectories)) return;
				_rootDirectories = value;
				NotifyOfPropertyChange();
			}
		}

		public List<ContentItem> Contents
		{
			get { return _contents; }
			private set
			{
				if (Equals(value, _contents)) return;
				_contents = value;
				NotifyOfPropertyChange();
			}
		}

		public bool AnySelected => SelectedContents?.Count > 0;

		[Export]
		public bool IsDirectoryTreeVisible
		{
			get { return _directoryTreeVisible; }
			set
			{
				if (value == _directoryTreeVisible) return;
				_directoryTreeVisible = value;
				NotifyOfPropertyChange();
			}
		}

		[Export]
		public bool ShowFolders
		{
			get { return _showFolders; }
			set
			{
				if (value == _showFolders) return;
				_showFolders = value;
				UpdateContents();
				NotifyOfPropertyChange();
			}
		}

		[Export]
		public bool ShowNonContent
		{
			get { return _showNonContent; }
			set
			{
				if (value == _showNonContent) return;
				_showNonContent = value;
				UpdateContents();
				NotifyOfPropertyChange();
			}
		}

		protected override void OnDeactivate(bool close)
		{
			base.OnDeactivate(close);

			if (close)
			{
				Editor.Current.ProjectChanged -= OnProjectChanged;
			}
		}

		private void OnProjectChanged(Project project)
		{
			NotifyOfPropertyChange(nameof(IsEnabled));
			UpdateDirectoryTree();
		}

		public void UpdateDirectoryTree()
		{
			if (Project.Current == null)
			{
				RootDirectories = null;
				return;
			}

			RootDirectories = new List<DirectoryTreeItem>
			{
				new DirectoryTreeItem("Content", new DirectoryInfo(Project.Current.ContentPath))
			};
		}

		public void UpdateContents()
		{
			if (SelectedDirectory == null)
				return;

			var dir = SelectedDirectory.Directory;

			Contents = null;

			if (ShowFolders)
			{
				Contents =
					dir.GetDirectories()
						.Select(d => new ContentItem(d))
						.OrderBy(c => c.Name)
						.Concat(
							dir.GetFiles()
								.Select(f => new ContentItem(f))
								.Where(f => _showNonContent || f.IsContent || f.IsFolder)
								.OrderBy(c => c.Name)).ToList();
			}
			else
			{
				Contents = dir.GetFiles("*.*", SearchOption.AllDirectories).Select(f => new ContentItem(f)).ToList();
			}
		}

		public void ShowDirectoryTree()
		{
			IsDirectoryTreeVisible = true;
		}

		public void HideDirectoryTree()
		{
			IsDirectoryTreeVisible = false;
		}

		public static DirectoryTreeItem GetDirectoryItemFromContentItem(IEnumerable<DirectoryTreeItem> collection,
			ContentItem contentItem)
		{
			foreach (var item in collection)
			{
				if (item.Directory.FullName == contentItem.Directory.FullName)
					return item;
				return GetDirectoryItemFromContentItem(item.SubFolders, contentItem);
			}

			return null;
		}
	}
}