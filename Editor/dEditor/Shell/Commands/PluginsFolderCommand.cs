// PluginsFolderCommand.cs - dEditor
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System.Diagnostics;
using System.Windows.Input;
using dEditor.Framework;

namespace dEditor.Shell.Commands
{
    public class PluginsFolderCommand : Command
    {
        public override string Name => "Plugins Folder";
        public override string Text => "Opens the plugins folder.";
        public override KeyGesture KeyGesture { get; }

        public override bool CanExecute(object parameter)
        {
            return true;
        }

        public override void Execute(object parameter)
        {
            Process.Start(Editor.Current.PluginsPath);
        }
    }
}