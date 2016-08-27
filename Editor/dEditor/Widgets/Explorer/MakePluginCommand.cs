// MakePluginCommand.cs - dEditor
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System.IO;
using System.Windows.Input;
using Caliburn.Micro;
using dEditor.Framework;
using dEditor.Utility;
using dEngine;
using dEngine.Instances;
using dEngine.Serializer.V1;

namespace dEditor.Widgets.Explorer
{
    public class MakePluginCommand : Command
    {
        public override string Name => "Make Plugin";
        public override string Text => "Saves the selected container as a plugin.";
        public override KeyGesture KeyGesture { get; }

        public MakePluginCommand()
        {
        }

        public override bool CanExecute(object parameter)
        {
            //var item = IoC.Get<IExplorer>().LastClickedInstance;
            //return item is Folder && item.Name.ValidateFileName();
            return true;
        }

        public override void Execute(object parameter)
        {
            var item = IoC.Get<IExplorer>().LastClickedInstance;

            if (item is Folder)
            {
                using (var file = File.Create(Path.Combine(Editor.Current.PluginsPath, $"{item.Name}.plugin")))
                {
                    Inst.Serialize(item, file);
                }
            }
        }
    }
}