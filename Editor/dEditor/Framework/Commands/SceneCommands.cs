// SceneCommands.cs - dEditor
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
using System.Linq;
using System.Windows.Input;
using dEngine;
using dEngine.Instances;
using dEngine.Services;
using Key = System.Windows.Input.Key;

namespace dEditor.Framework.Commands
{
    public class InsertCommand : Command
    {
        public override string Name => "Insert";
        public override string Text => "Inserts an object.";
        public override KeyGesture KeyGesture => new KeyGesture(Key.None);

        public override bool CanExecute(object parameter)
        {
            var type = parameter as Type;
            return type != null && typeof(Instance).IsAssignableFrom(type);
        }

        public override void Execute(object parameter)
        {
            var typeName = (string)parameter;
            var sel = SelectionService.First(x => x is Model);
            var parent = sel ?? Game.Workspace;
            ScriptService.NewInstance(typeName, parent);
        }
    }

    public class SelectChildrenCommand : Command
    {
        public override string Name => "Select Children";
        public override string Text => "Selects the children of this object.";
        public override KeyGesture KeyGesture => new KeyGesture(Key.None);

        public override bool CanExecute(object parameter)
        {
            return SelectionService.Any(x => x.Children.Count > 0);
        }

        public override void Execute(object parameter)
        {
            var items = SelectionService.Where(x => x.Children.Count > 0);

            SelectionService.Service.ClearSelection();

            foreach (var child in items.SelectMany(item => item.Children))
            {
                SelectionService.Service.Select(child);
            }
        }
    }

    public class GroupCommand : Command
    {
        public override string Name => "Group";
        public override string Text => "Groups the selection into a model.";
        public override KeyGesture KeyGesture => new KeyGesture(Key.G, ModifierKeys.Control);

        public override bool CanExecute(object parameter)
        {
            return SelectionService.Any();
        }

        public override void Execute(object parameter)
        {
            var parent = SelectionService.Unanimous(i => i.Parent) ?? Game.Workspace;
            var model = new Model { Parent = parent };
            foreach (var item in SelectionService.Selection)
            {
                item.Parent = model;
            }
        }
    }

    public class UngroupCommand : Command
    {
        public override string Name => "Ungroup";
        public override string Text => "Ungroups the selected model.";
        public override KeyGesture KeyGesture => new KeyGesture(Key.U, ModifierKeys.Control);

        public override bool CanExecute(object parameter)
        {
            return SelectionService.Any(i => i is Model);
        }

        public override void Execute(object parameter)
        {
            var parent = SelectionService.Unanimous(i => i.Parent) ?? Game.Workspace;

            foreach (var item in SelectionService.Selection)
            {
                var model = item as Model;

                if (model != null)
                {
                    var children = model.Children;
                    children.EnterUpgradeableReadLock();
                    for (int i = model.Children.Count - 1; i >= 0; i--)
                    {
                        children[i].Parent = parent;
                    }
                    children.ExitUpgradeableReadLock();
                }
            }
        }
    }

    public class ZoomToCommand : Command
    {
        public override string Name => "Zoom to";
        public override string Text => "Moves the camera to fit the selection.";
        public override KeyGesture KeyGesture => new KeyGesture(Key.F);

        public override bool CanExecute(object parameter)
        {
            return Game.FocusedCamera != null && SelectionService.Any();
        }

        public override void Execute(object parameter)
        {
            var camera = Game.Workspace.CurrentCamera;
            if (camera != null)
            {
                if (camera.CameraType != CameraType.Fixed)
                    camera.CameraType = CameraType.Fixed;

                (camera.CurrentBehaviour as FixedBehaviour)?.FocusOn(SelectionService.OfType<PVInstance>());
            }
        }
    }
}