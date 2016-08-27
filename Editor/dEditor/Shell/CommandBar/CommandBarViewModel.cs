// CommandBarViewModel.cs - dEditor
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using dEngine;
using dEngine.Instances;

namespace dEditor.Shell.CommandBar
{
    [Export(typeof(ICommandBar))]
    public class CommandBarViewModel : ICommandBar
    {
        [Export]
        public IList<string> RecentCommands { get; } = new ObservableCollection<string>();

        public void Execute(string command, bool mute = false)
        {
            if (Editor.Current.Project == null)
            {
                Engine.Logger.Error("Command cannot be executed without an open project.");
                return;
            }

            RecentCommands.Add(command);

            var script = new Script
            {
                Archivable = false,
                Name = command,
                Source = command,
                ParentLocked = true,
                Identity = ScriptIdentity.Editor
            };

            if (!mute)
                script.Print($"> {command}", LogLevel.Info);

            script.Run();
            /*
#pragma warning disable 4014
            script.RunAsync().ContinueWith(x =>
            {
                script.ParentLocked = false;
                script.Destroy();
            });
#pragma warning restore 4014
*/
        }
    }
}