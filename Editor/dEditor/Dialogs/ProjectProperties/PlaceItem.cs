// PlaceItem.cs - dEditor
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System.IO;
using Caliburn.Micro;
using dEditor.Framework;
using dEngine;

namespace dEditor.Dialogs.ProjectProperties
{
	public class PlaceItem : PropertyChangedBase
	{
		public PlaceItem(string file)
		{
			Name = Path.GetFileNameWithoutExtension(file);
			DateModified = File.GetLastWriteTime(file).ToString("yyyy/M/d, hh:mm:ss tt");
			Project.Current.StartupPlaceChanged += OnStartupPlaceChanged;
		}

		public string Name { get; }
		public string DateModified { get; }
		public bool IsCurrent => Game.Workspace.PlaceId == Name;
		public bool IsStartup => Project.Current.StartupPlace == Name;

		~PlaceItem()
		{
			Project.Current.StartupPlaceChanged -= OnStartupPlaceChanged;
		}

		private void OnStartupPlaceChanged(string s)
		{
			NotifyOfPropertyChange(nameof(IsStartup));
		}
    }
}