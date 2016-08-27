// ContentItem.cs - dEditor
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
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using Caliburn.Micro;
using dEngine;
using dEngine.Serializer.V1;

namespace dEditor.Widgets.ContentBrowser
{
	public class ContentItem : PropertyChangedBase
	{
		private Uri _icon;

		public ContentItem(FileInfo file)
		{
			Name = file.Name;
		    Extension = file.Extension;
            File = file;
			UpdateIcon();
			using (var stream = file.Open(FileMode.Open, FileAccess.Read))
				Type = Inst.PeekContent(stream);
		}

		public ContentItem(DirectoryInfo directory)
		{
			Name = directory.Name;
            Directory = directory;
			UpdateIcon();
			Type = null;
		}

		public ContentItem(string name)
		{
			Name = name;
		}

		public string Name { get; private set; }
        public string Extension { get; private set; }
        public ContentType? Type { get; }
		public FileInfo File { get; }
		public DirectoryInfo Directory { get; }

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

		public bool IsFile => File != null;
		public bool IsFolder => Directory != null;
		public bool IsContent => Type != null;

		public void OnMouseLeftButtonDown(MouseButtonEventArgs args)
		{
			if (args.ClickCount == 2)
			{
				var contentBrowser = IoC.Get<ContentBrowserViewModel>();

				if (IsFolder)
				{
					contentBrowser.SelectedDirectory =
						ContentBrowserViewModel.GetDirectoryItemFromContentItem(contentBrowser.RootDirectories, this);
				}
				else
				{
					switch (Type)
					{
						case ContentType.StaticMesh:
						case ContentType.SkeletalMesh:
						case ContentType.Model:
						case ContentType.Texture:
						case ContentType.Sound:
						case ContentType.Animation:
						case ContentType.Cubemap:
						case ContentType.Video:
						case ContentType.Material:
                            throw new NotImplementedException();
							break;
						case ContentType.Unknown:
						case null:
							Process.Start(File.FullName);
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}
				}
			}
		}

		public void UpdateIcon()
		{
			string iconName;

			// TODO: generate thumbnails

			if (IsFolder)
			{
				iconName = "Folder_256x";
			}
			else
			{
				switch (Extension)
				{
					case "game":
						iconName = "dEngine_256x";
						break;
					case "place":
						iconName = "Web_256x";
						break;
					case "stmesh":
						iconName = "3DScene_256x";
						break;
					case "skmesh":
						iconName = "3DScene_256x";
						break;
					case "model":
						iconName = "Bricks_256x";
						break;
					case "texture":
						iconName = "Image_256x";
						break;
					case "sound":
						iconName = "SoundFile_256x";
						break;
					case "anim":
						iconName = "UseCaseDiagram_256x";
						break;
					case "script":
						iconName = "LuaFile_256x";
						break;
					case "cubemap":
						iconName = "ResourceTemplate_256x";
						break;
					case "video":
						iconName = "PlayVideo_256x";
						break;
					case "skeleton":
						iconName = "UseCaseDiagram_256x";
						break;
					case "material":
						iconName = "Member_256x";
						break;
					default:
						iconName = "File.png";
						break;
				}
			}

			Icon = new Uri($"/dEditor;component/Content/Icons/Toolbar/{iconName}.png", UriKind.Relative);
		}
	}
}