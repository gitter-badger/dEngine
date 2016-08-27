// AppBootstrapper.cs - dEditor
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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Windows;
using System.Windows.Threading;
using Caliburn.Micro;
using dEditor.Framework;
using dEditor.Instances;
using dEditor.Shell;
using dEditor.Shell.Client;
using dEditor.Shell.CommandBar;
using dEditor.Shell.StatusBar;
using dEditor.Tools;
using dEditor.Widgets.CodeEditor;
using dEditor.Widgets.StartPage;
using dEngine;
using dEngine.Instances;
using dEngine.Serializer.V1;
using dEngine.Services;
using dEngine.Utility.Extensions;
using DateTime = System.DateTime;


namespace dEditor
{
    public class AppBootstrapper : BootstrapperBase
    {
        private static readonly Assembly _externalBaml =
            Assembly.LoadFile(Path.Combine(Environment.CurrentDirectory, "dEditor.exe"));

        public AppBootstrapper()
        {
            Initialize();

            Engine.SaveGame = data =>
            {
                var project = Project.Current;
                Debug.Assert(project != null);
                var path = Path.Combine(project.ProjectPath, $"{project.Name}.game");
                using (var file = File.Create(path))
                    data.CopyTo(file);
            };

            Engine.SavePlace = (place, data) =>
            {
                var project = Project.Current;
                Debug.Assert(project != null);
                var path = Path.Combine(project.ProjectPath, "Places", $"{place}.place");
                using (var file = File.Create(path))
                    data.CopyTo(file);
            };

            ContentProvider.CustomFetchHandler = CustomFetchHandler;
            Engine.Start(EngineMode.LevelEditor);
            DataModel.SetStartupArguments(new Dictionary<string, string> { { "IsEditor", "true" } });
            InputService.MouseInputApi = InputApi.Windows;
            Editor.Current.Settings = new EditorSettings { Name = "Editor", Parent = Engine.Settings };
            Engine.Settings.Load();
            KeyBindings.Init();

            //InstTest();

            ToolManager.SelectTool.IsEquipped = true;

            AutoSaveTimer = new DispatcherTimer(System.TimeSpan.FromMinutes(EditorSettings.AutosaveInterval),
                DispatcherPriority.Background, PerformAutoSave, Editor.Current.Dispatcher);
        }

        internal DispatcherTimer AutoSaveTimer;

        protected override object GetInstance(Type service, string key)
        {
            if (service == typeof(IWindowManager))
                service = typeof(WindowManager);

            if (service == typeof(ICommandBar))
            {
                return Editor.Current.Shell.CommandBar;
            }
            if (service == typeof(IStatusBar))
            {
                return Editor.Current.Shell.StatusBar;
            }
            if (service == typeof(ICodeEditor))
            {
                var existing = Editor.Current.Shell?.Items.OfType<CodeEditorViewModel>()
                    .FirstOrDefault(w => w.LuaSourceContainer.InstanceId.Equals(key));
                if (existing != null)
                    return existing;
                
                WeakReference<Instance> obj;
                Game.Instances.TryGetValue(key, out obj);
                var editor = new CodeEditorViewModel((LuaSourceContainer)obj);
                Editor.Current.Shell?.OpenDocument(editor);
                return editor;
            }

            var item = Editor.Current.Shell?.Widgets.FirstOrDefault(service.IsInstanceOfType);

            if (item != null)
                return item;

            return base.GetInstance(service, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            if (typeof(Document).IsAssignableFrom(service))
                return Editor.Current.Shell?.Items.Where(w => w.GetType() == service) ?? Enumerable.Empty<object>();
            if (typeof(Widget).IsAssignableFrom(service))
                return Editor.Current.Shell?.Widgets.Where(w => w.GetType() == service) ?? Enumerable.Empty<object>();
            return new[] { service.FastConstruct() };
        }

        private static Stream CustomFetchHandler(string protocol, string path)
        {
            switch (protocol)
            {
                case "places":
                    return File.OpenRead(Path.Combine(Project.Current.ProjectPath, "Places", path + ".place"));
                case "content":
                    return File.OpenRead(Path.Combine(Project.Current.ContentPath, path));
                case "editor":
                    var editorPath = $"content/{path.ToLower()}";
                    var resourceStream =
                        _externalBaml.GetManifestResourceStream(_externalBaml.GetName().Name + ".g.resources");
                    using (var reader = new ResourceReader(resourceStream))
                    {
                        foreach (DictionaryEntry entry in reader)
                        {
                            var test = entry.Key.ToString().Contains("json");
                            if (test)
                            ;
                            if (entry.Key.ToString() == editorPath)
                            {
                                return (Stream)entry.Value;
                            }
                        }
                    }
                    throw new NotSupportedException($"No file found in editor manifest resource.");
                default:
                    return null;
            }
        }

        private static void PerformAutoSave(object sender, EventArgs e)
        {
            if (RunService.SimulationState != SimulationState.Stopped || // do not auto-save during sessions
                Editor.Current.MainWindow.WindowState == WindowState.Minimized || EditorSettings.AutosaveEnabled)
                // do not auto-save if window is minimized
                return;

            var proj = Editor.Current.Project;

            if (proj == null)
                return;

            Engine.Logger.Trace("Autosaving...");

            var archiveFileName = Path.Combine(EditorSettings.AutosavePath,
                $"{proj.Name}_{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.zip");

            try
            {
                var dir = Path.Combine(proj.ProjectPath, "Bin");

                if (Directory.Exists(dir))
                    Directory.Delete(dir, true);
            }
            catch
            {
                // ignored
            }

            ZipFile.CreateFromDirectory(proj.ProjectPath, archiveFileName, CompressionLevel.Optimal, false);

            //Engine.Logger.Trace($"Autosave complete - {archiveFileName}");
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            var args = AppArgs.Parse(e.Args);
            Editor.Current.Args = args;

            if (!string.IsNullOrEmpty(args.API))
            {
                File.WriteAllText(args.API, API.Dump());
            }

            var settings = new Dictionary<string, object>
            {
                {"SizeToContent", SizeToContent.Manual},
                {"MinWidth", 1024},
                {"MinHeight", 700}
            };

            if (args.UseClientShell)
            {
                DisplayRootViewFor<ClientShellViewModel>(settings);
            }
            else
            {
                DisplayRootViewFor<ShellViewModel>(settings);

                var shell = Editor.Current.Shell;

                if (!File.Exists(Editor.Current.LayoutFileName))
                {
                    var defaultLayoutStream = Assembly.GetExecutingAssembly()
                        .GetManifestResourceStream("dEditor.Content.DefaultLayout.xaml");

                    Debug.Assert(defaultLayoutStream != null);

                    using (var layoutFile = File.Create(Editor.Current.LayoutFileName))
                    {
                        defaultLayoutStream.CopyTo(layoutFile);
                    }
                }

                shell.LayourItemStatePersister.LoadState(shell, shell.View, Editor.Current.LayoutFileName);

                shell.OpenDocument(new StartPageViewModel());

                // TODO: remove test autoload
                var p = Project.Load(@"C:\Users\Dan\Documents\dEditor\Projects\MyGame\MyGame.dproj");
                p.Open();
                
                /*
                var mat = new Material();
                var tex = new TextureParamaterNode();
                mat.AddNode(tex);
                tex.RGB.ConnectTo(((FinalNode)mat.FinalNode).BaseColour);
                shell.OpenDocument(new MaterialEditorViewModel(mat));
                */
            }
        }

        protected override void OnExit(object sender, EventArgs e)
        {
            base.OnExit(sender, e);

            Editor.Current.Shell.Viewport?.TryClose();
        }
    }
}