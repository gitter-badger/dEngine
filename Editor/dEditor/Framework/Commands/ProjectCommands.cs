// ProjectCommands.cs - dEditor
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
using System.Windows;
using System.Windows.Input;
using dEditor.Dialogs.NewPlace;
using dEditor.Dialogs.NewProject;
using dEditor.Shell;
using dEngine;
using Microsoft.Win32;
using Key = System.Windows.Input.Key;

namespace dEditor.Framework.Commands
{
	public class NewPlaceCommand : Command
	{
		public override string Name => "New Place";
		public override string Text => "Creates a new place from a template.";
		public override KeyGesture KeyGesture => new KeyGesture(Key.None);

		public override bool CanExecute(object parameter)
		{
			return Project.Current != null;
		}

		public override void Execute(object parameter)
		{
			var model = new NewPlaceViewModel();
			Editor.Current.WindowManager.ShowDialog(model, null, model.GetDialogSettings());
		}
	}

	public class NewProjectCommand : Command
	{
		public override string Name => "New Project";
		public override string Text => "Opens a dialog for creating projects.";
		public override KeyGesture KeyGesture => new KeyGesture(Key.O, ModifierKeys.Control);

		public override bool CanExecute(object parameter)
		{
			return true;
		}

		public override void Execute(object parameter)
		{
			var settings = new Dictionary<string, object>
			{
				{"SizeToContent", SizeToContent.Manual},
				{"MinWidth", 784},
				{"MinHeight", 411},
				{"Width", 939},
				{"Height", 621},
				{"ShowInTaskbar", false}
			};

			var newProjectVm = new NewProjectViewModel();
			Editor.Current.WindowManager.ShowDialog(newProjectVm, null, newProjectVm.GetDialogSettings());
		}
	}

	public class SaveProjectCommand : Command
	{
		public SaveProjectCommand(ShellViewModel shell)
		{
			Editor.Current.ProjectChanged += delegate { UpdateCanExecute(); };
			shell.ActiveDocumentChanged += delegate { UpdateCanExecute(); };
		}

		public override string Name => $"Save Project";
		public override string Text => $"Saves the current project.";
		public override KeyGesture KeyGesture => new KeyGesture(Key.S, ModifierKeys.Control);

		public override bool CanExecute(object parameter)
		{
			return Editor.Current.Project != null;
		}

		public override void Execute(object parameter)
		{
            Editor.Current.Project.Save();
            /*
            try
			{
				Editor.Current.Project.Save();
			}
			catch (Exception e)
			{
				Engine.Logger.Error(e);
				MessageBox.Show("An error occurred while trying to save the project.", "Save Project",
					MessageBoxButton.OK,
					MessageBoxImage.Error);
				Engine.Logger.Error(e);
			}
            */
		}
	}

	public class OpenProjectCommand : Command
	{
		public override string Name => $"Open Project";
		public override string Text => $"Opens a project.";
		public override KeyGesture KeyGesture => new KeyGesture(Key.O, ModifierKeys.Control);

		public override bool CanExecute(object parameter)
		{
			return true;
		}

		public override void Execute(object parameter)
		{
			var proj = parameter as Project;

			if (proj != null)
			{
				proj.Open();
				return;
			}

			if (parameter != null)
				return;

			var dialog = new OpenFileDialog
			{
				Filter = "Project File|*.dproj;"
			};

			if (dialog.ShowDialog() != true)
				return;

			Project project = null;
			project = Project.Load(dialog.FileName);
			project.Open();
		}

		public override string ToString()
		{
			return Name;
		}
	}
}