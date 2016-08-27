// ExceptionHelper.cs - dEditor
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
using System.Windows;
using dEngine.Utility.Extensions;
using Ookii.Dialogs.Wpf;

namespace dEditor.Utility
{
	public static class ExceptionHelper
	{
		private static bool IsDialogOpen;

		public static void ShowCrashDialog(this Exception e)
		{
			if (IsDialogOpen)
				return;

            Editor.Current?.Logger.Error(e);

			e = e.InnerException ?? e;

			var dialog = new TaskDialog
			{
				WindowTitle = "dEditor",
				ButtonStyle = TaskDialogButtonStyle.CommandLinks,
				AllowDialogCancellation = false,
				MainInstruction = "dEditor has stopped working.",
				Content = "An unhandled exception was thrown, so the program needs to exit."
			};

			var restartButton = new TaskDialogButton("Restart the program");
			var closeButton = new TaskDialogButton("Close the program");

			dialog.Buttons.Add(restartButton);
			dialog.Buttons.Add(closeButton);

			dialog.ExpandFooterArea = true;

			dialog.ExpandedControlText = "Show exception details";
			dialog.ExpandedInformation = $"{e.Source} : {e.Message} {Environment.NewLine} {e.StackTrace}";


			IsDialogOpen = true;
			var resultButton = dialog.ShowDialog();
			IsDialogOpen = false;

			if (resultButton == restartButton)
			{
				System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
				Environment.Exit(e.HResult);
			}
            else if (resultButton == closeButton)
            {
                Environment.Exit(e.HResult);
            }
		}
	}
}