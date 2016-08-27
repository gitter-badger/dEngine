// InsertBreakpointCommand.cs - dEditor
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
using dEditor.Framework;
using dEngine.Instances;

namespace dEditor.Widgets.CodeEditor.Commands
{
	public class InsertBreakpointCommand : Command
	{
		private readonly CodeEditorViewModel _codeEditor;

		public InsertBreakpointCommand(CodeEditorViewModel codeEditor)
		{
			_codeEditor = codeEditor;
		}

		public override string Name => "Insert Breakpoint";
		public override string Text => "Inserts a breakpoint at the selected line.";
		public override KeyGesture KeyGesture => new KeyGesture(Key.None);

		public override bool CanExecute(object parameter)
		{
			return true;
		}

		public override void Execute(object parameter)
		{
			var script = _codeEditor.LuaSourceContainer as Script;
		}
	}
}