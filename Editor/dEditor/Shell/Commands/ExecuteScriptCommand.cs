// ExecuteScriptCommand.cs - dEditor
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
using Caliburn.Micro;
using dEditor.Framework;
using dEditor.Shell.CommandBar;
using dEngine.Utility.Extensions;
using Microsoft.Win32;

namespace dEditor.Shell.Commands
{
    public class ExecuteScriptCommand : Framework.Command
    {
        public override string Name { get; } = "Execute Script";
        public override string Text { get; } = "Executes a script from a file.";
        public override KeyGesture KeyGesture { get; } = new KeyGesture(Key.None);

        public override bool CanExecute(object parameter)
        {
            return Project.Current != null;
        }

        public override void Execute(object parameter)
        {
            var dialog = new OpenFileDialog {Filter = "Lua Script|*.lua;"};

            if (dialog.ShowDialog() == true)
            {
                IoC.Get<ICommandBar>().Execute(dialog.OpenFile().ReadString(), true);
            }
        }
    }
}