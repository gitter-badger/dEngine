// Command.cs - dEditor
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

namespace dEditor.Framework
{
	public abstract class Command : ICommand
	{
		public abstract string Name { get; }
		public abstract string Text { get; }
		public virtual Uri IconSource => null;
		public abstract KeyGesture KeyGesture { get; }

		public event EventHandler CanExecuteChanged;

		public abstract bool CanExecute(object parameter);
		public abstract void Execute(object parameter);

		public void UpdateCanExecute()
		{
			CanExecuteChanged?.Invoke(this, EventArgs.Empty);
		}
	}
}