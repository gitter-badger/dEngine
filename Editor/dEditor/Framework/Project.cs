// Project.cs - dEditor
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
using System.IO;
using System.Windows;
using System.Xml.Serialization;
using Caliburn.Micro;
using dEditor.Dialogs.NewPlace;
using dEditor.Widgets.Viewport;
using dEngine;
using dEngine.Instances;
using dEngine.Instances.Attributes;
using dEngine.Serializer.V1;
using dEngine.Services;
using TeamBuildServer;

namespace dEditor.Framework
{
	[Serializable, TypeId(2142)]
	public sealed class Project : PropertyChangedBase
	{
		private string _startupPlace;
		private ViewportViewModel _viewportVm;

		public Project()
		{
			AppId = 480;
		}

		[XmlIgnore]
		public string ProjectPath { get; private set; }

		[XmlIgnore]
		public string ProjectFile { get; private set; }

		/// <summary>
		/// Determines if the project is currently loaded.
		/// </summary>
		[XmlIgnore]
		public bool IsOpen { get; private set; }

		/// <summary>
		/// Determines if the project was created this session.
		/// </summary>
		[XmlIgnore]
		public bool IsNew { get; set; }

		/// <summary>
		/// Gets the currently loaded project. (null if no project is loaded)
		/// </summary>
		public static Project Current => Editor.Current.Project;

		/// <summary>
		/// Gets the path to the project's content folder.
		/// </summary>
		public string ContentPath => Path.Combine(ProjectPath, "Content");

		/// <summary>
		/// Fired when <see cref="StartupPlace" /> is changed.
		/// </summary>
		public event Action<string> StartupPlaceChanged;

		/// <summary>
		/// Initializes a project object from the given project file.
		/// </summary>
		public static Project Load(string projFile)
		{
			if (!File.Exists(projFile))
				throw new FileNotFoundException("Project file not found.", projFile);

			using (var reader = new StreamReader(projFile))
			{
				var serializer = new XmlSerializer(typeof(Project));

				var project = (Project)serializer.Deserialize(reader);
				project.ProjectFile = projFile;
				project.ProjectPath = Path.GetDirectoryName(projFile);

				return project;
			}
		}

		/// <summary>
		/// Creates a new project from scratch and opens it.
		/// </summary>
		/// <remarks>
		/// A project directory should look like the following:
		/// MyProject
		/// --Content
		/// -----Animations
		/// -----Audios
		/// -----Models
		/// -----Images
		/// --Places
		/// ------default.place
		/// ------place0.place
		/// ------place1.place
		/// --MyProject.game
		/// --MyProject.dproj
		/// --MyProject.ico (optional)
		/// </remarks>
		public static Project Create(string name, string directory)
		{
			var rootPath = Path.Combine(directory, name);
			var contentPath = Path.Combine(rootPath, "Content");
			var placePath = Path.Combine(rootPath, "Places");

			if (Directory.Exists(rootPath))
				throw new InvalidOperationException($"The directory \"{directory}/{name}\" already exists.");

			Directory.CreateDirectory(rootPath);
			Directory.CreateDirectory(contentPath);
			Directory.CreateDirectory(placePath);

			var projFile = Path.Combine(rootPath, $"{name}.dproj");
			File.Create(projFile).Close();

			var gameFile = Path.Combine(rootPath, $"{name}.game");
			File.Create(gameFile).Close();

			var proj = new Project
			{
				Name = name,
				CompanyName = Environment.UserName,
				ProjectPath = rootPath,
				ProjectFile = projFile,
				StartupPlace = "Place1",
				IsNew = true
			};

			Editor.Current.Project = proj;
			Game.Workspace.PlaceId = proj.StartupPlace;

            NewPlaceViewModel.LoadBaseplate();

			proj.Save();
			proj.Open();


			return proj;
		}

		/// <summary>
		/// Saves the loaded project.
		/// </summary>
		public void Save(bool saveDM = true)
		{
			if (RunService.SimulationState != SimulationState.Stopped)
				throw new InvalidOperationException("Cannot save while simulation is running.");

			if (!IsOpen && !IsNew)
				throw new InvalidOperationException("Attempt to save unloaded project.");

			var projSerializer = new XmlSerializer(typeof(Project));
			using (var stream = File.Open(ProjectFile, FileMode.Truncate))
			{
				projSerializer.Serialize(stream, this);
			}

			if (saveDM)
			{
				DataModel.Save(SaveFilter.SaveGame);
				DataModel.Save(SaveFilter.SaveWorld);
			}
		}

		/// <summary>
		/// Opens the project.
		/// </summary>
		public void Open()
		{
			IsOpen = true;
			Editor.Current.Shell.Viewport?.TryClose();
			Editor.Current.Project = this;

			try
			{
				var gameFile = Path.Combine(ProjectPath, $"{Name}.game");

				if (!IsNew)
				{
					using (var gameStream = File.OpenRead(gameFile))
						Inst.Deserialize(gameStream, Game.DataModel);
				}

				Game.Workspace.LoadPlace(StartupPlace);
			}
#if !DEBUG
			catch (Exception e)
			{
				MessageBox.Show($"Could not load project:\n\n{e.Message}", "Open Project", MessageBoxButton.OK,
					MessageBoxImage.Error);
				Close();
				return;
			}
#endif
			finally
			{
			}

			Editor.Current.Shell.Viewport = _viewportVm = new ViewportViewModel();
			Editor.Current.Shell.OpenDocument(_viewportVm);
			var loginService = DataModel.GetService<LoginService>();
            if (!loginService.IsLoggedIn())
                loginService.TryLogin();
		}

		/// <summary>
		/// Closes the project.
		/// </summary>
		public void Close()
		{
			if (!IsOpen) return;
			IsOpen = false;

			Game.DataModel.ClearChildren();
			Game.Workspace.LoadPlace(null);

			_viewportVm?.TryClose();

			Editor.Current.Project = null;

            LoginService.Service.Logout();
        }

		public override string ToString()
		{
			return Name;
		}

		/// <summary>
		/// Renames the project and its related files.
		/// </summary>
		/// <param name="newName"></param>
		public void Rename(string newName)
		{
			if (newName == Name)
				return;

			throw new NotImplementedException();
		}


		public static string ToContentPath(string folder)
		{
			Uri pathUri = new Uri(Project.Current.ContentPath);
			// Folders must end in a slash
			if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
			{
				folder += Path.DirectorySeparatorChar;
			}
			Uri folderUri = new Uri(folder);
			var uri =
				Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
			return $"content://{uri}";
		}

		public static explicit operator TeamBuildProject(Project project)
		{
			return new TeamBuildProject(project.Name, project.AppId, Game.Workspace.PlaceId, project.ProjectPath);
		}

		#region XML Properties

		/// <summary>
		/// The author of the project.
		/// </summary>
		[XmlElement("CompanyName"), EditorVisible("Publisher")]
		public string CompanyName { get; set; }

		/// <summary>
		/// The project homepage.
		/// </summary>
		[XmlElement("Homepage"), EditorVisible("Publisher")]
		public string Homepage { get; set; }

		/// <summary>
		/// The project contact information.
		/// </summary>
		[XmlElement("SupportContact"), EditorVisible("Publisher")]
		public string SupportContact { get; set; }

		/// <summary>
		/// The project thumbnail.
		/// </summary>
		[XmlElement("ProjectThumbnail"), EditorVisible("About"), ContentId(ContentType.Texture)]
		public string Thumbnail { get; set; }

		/// <summary>
		/// The project description.
		/// </summary>
		[XmlElement("Description"), EditorVisible("About")]
		public string Description { get; set; }

		/// <summary>
		/// The project name.
		/// </summary>
		[XmlElement("Name"), EditorVisible("About")]
		public string Name { get; set; }

		/// <summary>
		/// The project version.
		/// </summary>
		[XmlElement("Version"), EditorVisible("About")]
		public string Version { get; set; }

		/// <summary>
		/// The copyright/trademark notices.
		/// </summary>
		[XmlElement("CopyrightNotice"), EditorVisible("Legal")]
		public string CopyrightNotice { get; set; }

		/// <summary>
		/// The licensing terms.
		/// </summary>
		[XmlElement("LicenseTerms"), EditorVisible("Legal")]
		public string LicenseTerms { get; set; }

		/// <summary>
		/// A relative path to startup place.
		/// </summary>
		[XmlElement("StartupPlace")]
		public string StartupPlace
		{
			get { return _startupPlace; }
			set
			{
				_startupPlace = value;
				StartupPlaceChanged?.Invoke(value);
			}
		}

		/// <summary>
		/// The Steam App ID for this project.
		/// </summary>
		[XmlElement("AppId"), EditorVisible("Steam")]
		public uint AppId { get; set; }

		#endregion
	}
}