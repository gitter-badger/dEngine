// ProjectViewModel.cs - dEditor
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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using dEditor.Framework.Commands;
using dEditor.Utility;
using dEditor.Widgets.Properties;
using dEngine;
using Microsoft.Scripting.Utils;
using MoreLinq;

namespace dEditor.Dialogs.ProjectProperties
{
	public class ProjectViewModel : Conductor<PropertiesViewModel>
	{
		private string _currentPage;
		private PlaceItem _selectedPlace;

		public ProjectViewModel()
		{
			NewPlaceCommand = new NewPlaceCommand();
			Places = new ObservableCollection<PlaceItem>();
			ActiveItem = Properties = new PropertiesViewModel
			{
				UseSelectionService = false,
				UseHistoryService = false,
				Target = Framework.Project.Current
			};
			Pages["Description"] = Visibility.Hidden;
			Pages["Places"] = Visibility.Hidden;
			UpdatePlaces();

			Game.Workspace.PlaceLoaded.Event += OnPlaceLoaded;
		}

		public NewPlaceCommand NewPlaceCommand { get; }

		public PropertiesViewModel Properties { get; }

		public ObservableCollection<PlaceItem> Places { get; private set; }

		public PlaceItem SelectedPlace
		{
			get { return _selectedPlace; }
			set
			{
				if (Equals(value, _selectedPlace)) return;
				_selectedPlace = value;
				NotifyOfPropertyChange();
			}
		}

		public override string DisplayName
		{
			get { return Framework.Project.Current.Name; }
			set { }
		}

		public Framework.Project Project => Framework.Project.Current;

		public string CurrentPage
		{
			get { return _currentPage; }
			set
			{
				if (value == _currentPage) return;
				_currentPage = value;

				Pages.Keys.ToArray().ForEach(k => Pages[k] = Visibility.Collapsed);
				Pages[value] = Visibility.Visible;
				NotifyOfPropertyChange();
			}
		}

		public ObservableDictionary<string, Visibility> Pages { get; } = new ObservableDictionary<string, Visibility>();

		protected override void OnActivate()
		{
			if (Editor.Current.Project == null)
			{
				TryClose();
			}
		}

		protected override void OnDeactivate(bool close)
		{
			base.OnDeactivate(close);
			if (close)
			{
				Game.Workspace.PlaceLoaded.Event -= OnPlaceLoaded;
			}
		}

		private void OnPlaceLoaded(string name)
		{
			UpdatePlaces();
		}

		private void UpdatePlaces()
		{
			Places = new ObservableCollection<PlaceItem>();
			Places.AddRange(
				Directory.GetFiles(Path.Combine(Framework.Project.Current.ProjectPath, "Places"))
					.Select(p => new PlaceItem(p)));
			SelectedPlace = Places.First(p => p.IsCurrent);
			NotifyOfPropertyChange(nameof(Places));
		}

		public IDictionary<string, object> GetDialogSettings()
		{
			return new Dictionary<string, object>
			{
				{"MinWidth", 700},
				{"MinHeight", 500},
				{"Title", "Project Settings"},
				{"WindowStyle", WindowStyle.ToolWindow}
			};
		}
	}
}