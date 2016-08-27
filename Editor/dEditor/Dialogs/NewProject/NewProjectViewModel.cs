// NewProjectViewModel.cs - dEditor
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Caliburn.Micro;
using dEditor.Dialogs.NewPlace;
using dEditor.Framework;
using dEditor.Widgets.StartPage;
using Ookii.Dialogs.Wpf;

namespace dEditor.Dialogs.NewProject
{
	public class NewProjectViewModel : Screen
	{
		private bool _canCreate;
		private string _location;
		private string _name;

		public NewProjectViewModel()
		{
			_location = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "dEditor",
				"Projects");
			_name = "MyGame";

			DisplayName = "New Project";

			Templates = new ObservableCollection<ProjectTemplate>
			{
				new ProjectTemplate("Baseplate", NewPlaceViewModel.LoadBaseplate)
			};

			SelectedTemplate = Templates.First();
			UpdateCanCreate();
		}

		public ObservableCollection<ProjectTemplate> Templates { get; }

		public ProjectTemplate SelectedTemplate { get; set; }

		public string Name
		{
			get { return _name; }
			set
			{
				_name = value;
				NotifyOfPropertyChange();
				UpdateCanCreate();
			}
		}

		public string Location
		{
			get { return _location; }
			set
			{
				_location = value;
				NotifyOfPropertyChange();
				UpdateCanCreate();
			}
		}

		public bool CanCreate
		{
			get { return _canCreate; }
			set
			{
				_canCreate = value;
				NotifyOfPropertyChange();
			}
		}

		private Stream GetTemplateStream(string name)
		{
			return
				Assembly.GetExecutingAssembly()
					.GetManifestResourceStream($"dEditor.Content.Templates.{name}.{name}.place");
		}

		private void UpdateCanCreate()
		{
			if (!string.IsNullOrWhiteSpace(_name) && _name.Length < 43 && SelectedTemplate != null)
			{
				CanCreate = true;
			}
			else
			{
				CanCreate = false;
			}
		}

		public void Browse()
		{
			var dialog = new VistaFolderBrowserDialog
			{
				SelectedPath = _location,
				Description = "Select Project Path",
				UseDescriptionForTitle = true
			};

			if (dialog.ShowDialog() == true)
			{
				Location = dialog.SelectedPath;
			}
		}

		public void Create()
		{
			if (Directory.Exists(Path.Combine(_location, _name)))
			{
				MessageBox.Show($"A folder with the name \"{Name}\" already exists in the given location.",
					"Directory Exists", MessageBoxButton.OK, MessageBoxImage.Exclamation);
				return;
			}

			Project.Create(_name, _location);

			TryClose(true);
		}

		public void Cancel()
		{
			TryClose(false);
		}

		public IDictionary<string, object> GetDialogSettings()
		{
			return new Dictionary<string, object>
			{
				{"SizeToContent", SizeToContent.Manual},
				{"MinWidth", 784},
				{"MinHeight", 411},
				{"Width", 768},
				{"Height", 493},
				{"ShowInTaskbar", false},
				{"WindowStyle", WindowStyle.ToolWindow}
			};
		}
	}
}