// StartPageViewModel.cs - dEditor
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
using System.ComponentModel.Composition;
using System.Windows.Media.Imaging;
using dEditor.Framework;
using dEditor.Framework.Commands;

namespace dEditor.Widgets.StartPage
{
    [Export(typeof(IStartPage))]
	public class StartPageViewModel : Document, IStartPage
	{
		public StartPageViewModel()
		{
			DisplayName = "Start Page";

			RecentProjects = new ObservableCollection<Project>();

			NotifyOfPropertyChange(() => NoRecentProjects);
		}

		public ObservableCollection<Project> RecentProjects { get; set; }

		public bool NoRecentProjects => RecentProjects.Count == 0;

		public override BitmapSource IconSource { get; } =
			new BitmapImage(new Uri("/dEditor;component/Content/Icons/Toolbar/house_16xLG.png", UriKind.Relative));

		public OpenProjectCommand OpenProjectCommand => Editor.Current.Shell.OpenProjectCommand;
		public NewProjectCommand NewProjectCommand => Editor.Current.Shell.NewProjectCommand;
	}
}