// SessionCommands.cs - dEditor
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
using System.Windows.Input;
using dEngine;
using dEngine.Services;
using Key = System.Windows.Input.Key;

namespace dEditor.Framework.Commands
{
	public abstract class SessionCommand : Framework.Command
	{
		protected SessionCommand()
		{
			Editor.Current.ProjectChanged += p => Editor.Current.Dispatcher.InvokeAsync(UpdateCanExecute);
			RunService.Service.SimulationStarted.Event +=
				() => Editor.Current.Dispatcher.InvokeAsync(UpdateCanExecute);
            RunService.Service.SimulationResumed.Event += () => Editor.Current.Dispatcher.InvokeAsync(UpdateCanExecute);
            RunService.Service.SimulationPaused.Event += () => Editor.Current.Dispatcher.InvokeAsync(UpdateCanExecute);
			RunService.Service.SimulationEnded.Event += () => Editor.Current.Dispatcher.InvokeAsync(UpdateCanExecute);
		}
	}

	public class PlayCommand : SessionCommand
	{
		public override string Name => "Play";
		public override string Text => "Simulate the game as a player.";
		public override KeyGesture KeyGesture => new KeyGesture(Key.F5);

		public override bool CanExecute(object parameter)
		{
			return Editor.Current.Project != null && RunService.SimulationState != SimulationState.Running;
		}

		public override void Execute(object parameter)
		{
			RunService.Service.Play();
		}
	}

	public class RunCommand : SessionCommand
	{
		public override string Name => "Run";
		public override string Text => "Simulate the game without a player.";
		public override KeyGesture KeyGesture { get; } = null;

		public override bool CanExecute(object parameter)
		{
			return Editor.Current.Project != null && RunService.SimulationState != SimulationState.Running;
		}

		public override void Execute(object parameter)
		{
			RunService.Service.Run();
		}
	}

	public class PauseCommand : SessionCommand
	{
		public override string Name => "Pause";
		public override string Text => "Pauses the simulation.";
		public override KeyGesture KeyGesture => new KeyGesture(Key.F5);

		public override bool CanExecute(object parameter)
		{
			return RunService.SimulationState == SimulationState.Running;
		}

		public override void Execute(object parameter)
		{
			RunService.Service.Pause();
		}
	}

	public class StopCommand : SessionCommand
	{
		public override string Name => "Stop";
		public override string Text => "Stops the simulation and resets the scene.";
		public override KeyGesture KeyGesture => new KeyGesture(Key.F5, ModifierKeys.Shift);

		public override bool CanExecute(object parameter)
		{
			return RunService.SimulationState != SimulationState.Stopped;
		}

		public override void Execute(object parameter)
		{
			RunService.Service.Stop();
		}
	}
}