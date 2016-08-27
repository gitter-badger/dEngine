// ProjectManagerOld.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dan@radiantcode.co.uk)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Xml.Serialization;
using dEditor.Modules.Scene;
using dEngine;
using dEngine.Instances;

namespace dEditor.Framework.Services
{
    public static class ProjectManager
    {
        private static readonly string _recentProjectsFile;
        private static RecentProjectsContainer _recentContainer;
        private static Project _currentProject;

        static ProjectManager()
        {
            _recentProjectsFile = Path.Combine(App.Current.EditorDirectory, "recent-projects.xml");
            _recentContainer = new RecentProjectsContainer();
        }

        public static Project CurrentProject
        {
            get { return _currentProject; }
            private set
            {
                _currentProject = value;
                CurrentProjectChanged?.Invoke(value);
            }
        }

        public static HashSet<RecentProject> RecentProjects => _recentContainer.RecentProjects;

        public static bool IsProjectOpen => CurrentProject != null;

        public static event Action<Project> CurrentProjectChanged;

        public static void CreateProject(string name, string path, Stream templateData = null)
        {
            var projectPath = Path.Combine(path, name);
            var binPath = Path.Combine(projectPath, "Bin");
            var contentPath = Path.Combine(projectPath, "Content");
            var placesPath = Path.Combine(contentPath, "Places");

            //var placeData = Assembly.GetExecutingAssembly().GetManifestResourceStream("dEditor.Content.Templates.Places.baseplate.dp");

            Directory.CreateDirectory(projectPath);
            Directory.CreateDirectory(binPath);
            Directory.CreateDirectory(contentPath);
            Directory.CreateDirectory(placesPath);

            var projectFile = Path.Combine(projectPath, name + ".dproj");
            File.CreateText(projectFile).Close();

            var dataModelFile = Path.Combine(projectPath, ".datamodel");
            File.Create(dataModelFile).Close();

            var placeFile = Path.Combine(placesPath, "Default.dp");
            using (var fileStream = File.Create(placeFile))
            {
                //    placeData.Seek(0, SeekOrigin.Begin);
                //    placeData.CopyTo(fileStream);
                CreatePlaceData(fileStream);
            }

            InstanceManager.DataModel.StartupPlace = placeFile;
            InstanceManager.Workspace.PlaceSource = placeFile;

            var project = new Project(name, projectPath, projectFile);

            project.Save();
            OpenProject(projectFile);
        }

        /// <summary>
        /// todo: give choice of pre-saved presets.
        /// </summary>
        private static void CreatePlaceData(Stream placeStream)
        {
            var camera = new Camera();
            InstanceManager.Workspace.CurrentCamera = camera;

            var baseplate = new Part
            {
                Anchored = true,
                Size = new Vector3(512, 10, 512),
                Parent = InstanceManager.Workspace
            };

            SerializationManager.Serialize(InstanceManager.Workspace).CopyTo(placeStream);
        }

        public static Project OpenProject(string projectFile)
        {
            CloseProject();
            var project = Project.Load(projectFile);
            CurrentProject = project;

            var shell = App.Current.Shell;
            var scene = new SceneViewModel();

            using (
                var stream = File.Open(Path.Combine(CurrentProject.ProjectPath, ".datamodel"),
                    FileMode.Open))
            {
                var game = InstanceManager.DataModel;

                SerializationManager.Deserialize(stream, game);

                if (File.Exists(game.StartupPlace))
                    InstanceManager.Workspace.LoadPlace(game.StartupPlace);
            }

            shell.OpenDocument(scene);
            shell.Scene = scene;

            return project;
        }

        public static void CloseProject()
        {
            var shell = App.Current.Shell;
            var scene = shell.Scene;

            CurrentProject = null;

            InstanceManager.DataModel?.ClearChildren();

            if (scene != null)
            {
                shell.CloseDocument(scene);
            }
        }

        /// <summary>
        /// Loads the list of recent projects.
        /// </summary>
        public static void LoadRecentProjectList()
        {
            if (File.Exists(_recentProjectsFile))
            {
                var serializer = new XmlSerializer(typeof(RecentProjectsContainer));
                using (var reader = new StreamReader(_recentProjectsFile))
                {
                    _recentContainer = (RecentProjectsContainer)serializer.Deserialize(reader);
                }
            }

            SaveRecentProjectList();
        }

        /// <summary>
        /// Loads the list of recent projects.
        /// </summary>
        public static void SaveRecentProjectList()
        {
            var serializer = new XmlSerializer(typeof(RecentProjectsContainer));
            using (var reader = new StreamWriter(_recentProjectsFile))
            {
                serializer.Serialize(reader, _recentContainer);
            }
        }
    }

    [Serializable]
    public sealed class Project
    {
        private Project()
        {
            var engineInfo = FileVersionInfo.GetVersionInfo(typeof(Engine).Assembly.Location);
            var editorInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);

            Author = Environment.UserName;
            Version = "0.0.0";
            EngineVersion = engineInfo.FileVersion;
            EditorVersion = editorInfo.FileVersion;
            SplashUrl = "https://cdn3.iconfinder.com/data/icons/ballicons-free/128/joypad.png";
            IconUrl = "http://www.roblox.com/favicon.ico";
            AppId = 480;
        }

        /// <summary>
        /// Creates a new project from scratch.
        /// </summary>
        /// <param name="name">The name of the project.</param>
        /// <param name="projectPath">The path to the project directory.</param>
        /// <param name="projectFile">The Path to the .dproj file.</param>
        public Project(string name, string projectPath, string projectFile) : this()
        {
            Name = name;
            ProjectPath = projectPath;
            ProjectFile = projectFile;
        }

        /// <summary>
        /// The path to the project directory.
        /// </summary>
        [XmlIgnore]
        public string ProjectPath { get; set; }

        /// <summary>
        /// The path to the .dproj file.
        /// </summary>
        [XmlIgnore]
        public string ProjectFile { get; set; }

        /// <summary>
        /// The path to the places directory.
        /// </summary>
        [XmlIgnore]
        public string PlacesPath => Path.Combine(ProjectPath, "Content", "Places");

        [XmlElement("Name")]
        public string Name { get; set; }

        [XmlElement("Author")]
        public string Author { get; set; }

        [XmlElement("Version")]
        public string Version { get; set; }

        [XmlElement("EngineVersion")]
        public string EngineVersion { get; set; }

        [XmlElement("EditorVersion")]
        public string EditorVersion { get; set; }

        [XmlElement("SplashUrl")]
        public string SplashUrl { get; set; }

        [XmlElement("IconUrl")]
        public string IconUrl { get; set; }

        [XmlElement("AppId")]
        public long AppId { get; set; }

        /// <summary>
        /// Renames the project and directory.
        /// </summary>
        public void Rename(string newName)
        {
            var newPath = Path.Combine(ProjectPath, "..", newName);
            var newFile = Path.Combine(newPath, newName + ".dproj");

            var oldName = Path.GetFileName(ProjectFile); // with extension

            try
            {
                Directory.Move(ProjectPath, newPath); // rename folder
                File.Move(Path.Combine(newPath, oldName), newFile); // copy to new .dproj
            }
            catch (IOException e)
            {
                MessageBox.Show(e.Message, "Project Rename");
                return;
            }

            Name = newName;
            ProjectPath = newPath;
            ProjectFile = newFile;
            Save(); // update dproj data
        }

        public static Project Load(string projFile)
        {
            using (var reader = new StreamReader(projFile))
            {
                var serializer = new XmlSerializer(typeof(Project));
                var project = (Project)serializer.Deserialize(reader);
                project.ProjectFile = projFile;
                project.ProjectPath = Path.GetDirectoryName(projFile);

                ProjectManager.RecentProjects.Add(new RecentProject(project.Name, projFile));

                return project;
            }
        }

        internal void Save()
        {
            var serializer = new XmlSerializer(typeof(Project));

            using (var stream = File.Open(ProjectFile, FileMode.Truncate))
            {
                serializer.Serialize(stream, this);
            }

            // Save the datamodel (everything but workspace)
            using (
                var dataModelFile = File.Open(Path.Combine(ProjectPath, ".datamodel"), FileMode.Open))
            {
                var newData = SerializationManager.Serialize(InstanceManager.DataModel);
                dataModelFile.SetLength(0); // truncate
                newData.CopyTo(dataModelFile);
            }

            var activePlaceSource = InstanceManager.Workspace.PlaceSource;
            if (!string.IsNullOrEmpty(activePlaceSource) && File.Exists(activePlaceSource))
            {
                // Save the workspace for the active place
                using (
                    var activePlaceFile = File.Open(activePlaceSource, FileMode.Open))
                {
                    var newData = SerializationManager.Serialize(InstanceManager.Workspace);
                    activePlaceFile.SetLength(0); // truncate
                    newData.CopyTo(activePlaceFile);
                }
            }
        }
    }

    [Serializable]
    public class RecentProjectsContainer
    {
        public RecentProjectsContainer()
        {
            RecentProjects = new HashSet<RecentProject>();
        }

        [XmlArray("RecentProjects")]
        [XmlArrayItem("RecentProject", typeof(RecentProject))]
        public HashSet<RecentProject> RecentProjects { get; set; }

        public void Add(Project project)
        {
            var recent = new RecentProject(project.Name, project.ProjectFile);

            RecentProjects.Remove(recent);
            RecentProjects.Add(recent);
        }
    }

    [Serializable]
    public class RecentProject : IEquatable<RecentProject>
    {
        public RecentProject()
        {
        }

        public RecentProject(string name, string projectFile)
        {
            Name = name;
            ProjectFile = projectFile;
        }

        [XmlElement("Name")]
        public string Name { get; set; }

        [XmlElement("ProjectFile")]
        public string ProjectFile { get; set; }

        public bool Equals(RecentProject other)
        {
            return ProjectFile == other.ProjectFile;
        }

        public override int GetHashCode()
        {
            return ProjectFile.GetHashCode();
        }
    }
}