// NewPlaceViewModel.cs - dEditor
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
using System.ComponentModel;
using System.IO;
using System.Windows;
using Caliburn.Micro;
using dEditor.Dialogs.ProjectProperties;
using dEditor.Framework;
using dEditor.Utility;
using dEngine;
using dEngine.Instances;

namespace dEditor.Dialogs.NewPlace
{
	public class NewPlaceViewModel : Screen, IDataErrorInfo
	{
		private string _placeDescription;
		private string _placeName;
		private TemplateItem _selectedTemplate;

		public NewPlaceViewModel()
		{
			DisplayName = "New Place";
			Templates = new List<TemplateItem>
			{
				new TemplateItem("Empty", "An empty level.",
					new Uri("/dEditor;component/Content/Icons/Templates/grid.png", UriKind.Relative), LoadEmpty),
				new TemplateItem("Baseplate", "A blank slate.",
					new Uri("/dEditor;component/Content/Icons/Templates/baseplate.png", UriKind.Relative), LoadBaseplate)
			};

			SelectedTemplate = Templates[1];
		}

		public List<TemplateItem> Templates { get; }

		public TemplateItem SelectedTemplate
		{
			get { return _selectedTemplate; }
			set
			{
				if (Equals(value, _selectedTemplate)) return;
				_selectedTemplate = value;
				NotifyOfPropertyChange();
			}
		}

		public string PlaceName
		{
			get { return _placeName; }
			set
			{
				if (value == _placeName) return;
				_placeName = value;
				NotifyOfPropertyChange();
			}
		}

		public string PlaceDescription
		{
			get { return _placeDescription; }
			set
			{
				if (value == _placeDescription) return;
				_placeDescription = value;
				NotifyOfPropertyChange();
			}
		}

		public string this[string columnName] => Validate(columnName);

		public string Error { get; }

		private static void LoadEmpty()
		{
		}

		internal static void LoadBaseplate()
		{
			var baseplate = new Part
			{
				Name = "Baseplate",
				Size = new Vector3(512, 20, 512),
                BrickColour = new Colour(0.388235f, 0.372549f, 0.384314f),
                Anchored = true,
				Parent = Game.Workspace
			};
            Game.Workspace.CurrentCamera.CFrame = new CFrame(0.321689844f, 18.2346153f, 23.7645969f, 0.98357147f, -0.0819535926f, 0.160844535f, -0, 0.891008377f, 0.453987002f, -0.180519685f, -0.446528673f, 0.876370251f);
		}

		private string GetPath()
		{
			return Path.Combine(Project.Current.ProjectPath, "Places", $"{PlaceName}.place");
		}

		public void Create()
		{
			if (Validate(nameof(PlaceName)) != string.Empty)
				return;

			Game.Workspace.LoadPlace(null);
			try
			{
				Game.Workspace.PlaceId = PlaceName;
                Game.DataModel.ClearContent(false);
				SelectedTemplate.LoadTemplate();
				Game.DataModel.SaveGame(SaveFilter.SaveWorld);
				Game.Workspace.LoadPlace(PlaceName);
			}
			catch (Exception e)
			{
				MessageBox.Show($"Could not create place: {e.Message}");
				TryClose();
			}
			TryClose(true);
		}

		public void Cancel()
		{
			TryClose(false);
		}

		public string Validate(string propertyName)
		{
			string result = string.Empty;
			switch (propertyName)
			{
				case nameof(PlaceName):
					if (string.IsNullOrWhiteSpace(PlaceName) || !PlaceName.ValidateFileName())
					{
						result = "Invalid filename.";
					}
					else if (File.Exists(GetPath()))
					{
						result = "A place with that name already exists.";
					}
					break;
			}
			return result;
		}

		public IDictionary<string, object> GetDialogSettings()
		{
			return new Dictionary<string, object>
			{
				{"MinWidth", 430},
				{"MinHeight", 532},
				{"WindowStyle", WindowStyle.ToolWindow},
                {"ShowInTaskbar", false}
            };
		}
	}
}