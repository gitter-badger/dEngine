// ProjectView.xaml.cs - dEditor
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
using dEditor.Framework;
using dEngine;

namespace dEditor.Dialogs.ProjectProperties
{
	/// <summary>
	/// Interaction logic for ProjectViewModel.xaml
	/// </summary>
	public partial class ProjectView : UserControl
	{
		public ProjectView()
		{
			InitializeComponent();
		}

	    private void MakeStartup(object sender, RoutedEventArgs e)
        {
            Project.Current.StartupPlace = ((PlaceItem)sender).Name;
            Project.Current.Save(false);
        }

	    private void Load(object sender, RoutedEventArgs e)
        {
            Game.Workspace.LoadPlace(((PlaceItem)sender).Name);
        }
	}
}