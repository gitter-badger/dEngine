// HistoryCommands.cs - dEditor
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System.Windows.Input;
using dEngine;
using dEngine.Services;
using Key = System.Windows.Input.Key;

namespace dEditor.Framework.Commands
{
	public abstract class UndoRedoCommand : Command
	{
		protected UndoRedoCommand()
		{
			if (Game.IsInitialized)
			{
				HookHistoryEvents();
			}
			else
			{
				Game.Initialized += (s, e) => { HookHistoryEvents(); };
			}
		}

		private void HookHistoryEvents()
		{
			HistoryService.Service.Undone.Event += w => UpdateCanExecute();
			HistoryService.Service.Redone.Event += w => UpdateCanExecute();
			HistoryService.Service.WaypointSet.Event += a => UpdateCanExecute();
		}
	}

	public class UndoCommand : UndoRedoCommand
	{
		public override string Name => "Undo";
		public override string Text => "Undoes the last action.";
		public override KeyGesture KeyGesture => new KeyGesture(Key.Z, ModifierKeys.Control);

		public override bool CanExecute(object parameter)
		{
			return HistoryService.CanUndoInternal().Item1;
		}

		public override void Execute(object parameter)
		{
			HistoryService.Service.Undo();
		}
	}

	public class RedoCommand : UndoRedoCommand
	{
		public override string Name => "Redo";
		public override string Text => "Redoes the last undone action.";
		public override KeyGesture KeyGesture => new KeyGesture(Key.Z, ModifierKeys.Control | ModifierKeys.Shift);

		public override bool CanExecute(object parameter)
		{
			return HistoryService.CanRedoInternal().Item1;
		}

		public override void Execute(object parameter)
		{
			HistoryService.Service.Redo();
		}
	}
}