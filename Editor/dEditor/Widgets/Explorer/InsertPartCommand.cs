// InsertPartCommand.cs - dEditor
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
using dEngine;
using dEngine.Instances;
using dEngine.Services;
using dEngine.Utility;

namespace dEditor.Widgets.Explorer
{
    public class InsertPartCommand : Command
    {
        public override string Name => "Insert Part";
        public override string Text => "Inserts a Part into the selected object.";
        public override KeyGesture KeyGesture { get; }

        public override bool CanExecute(object parameter)
        {
            return SelectionService.SelectionCount > 0;
        }

        public override void Execute(object parameter)
        {
            // ReSharper disable once ObjectCreationAsStatement
            new Part {Parent = SelectionService.First()};
        }
    }
}