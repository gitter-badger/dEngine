// ExplorerViewModel.cs - dEditor
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
using System.Collections.Concurrent;
using System.ComponentModel.Composition;
using dEditor.Framework;
using dEditor.Framework.Commands;
using dEngine;
using dEngine.Instances;
using dEngine.Utility;

// ReSharper disable VirtualMemberCallInConstructor

namespace dEditor.Widgets.Explorer
{
    [Export(typeof(IExplorer))]
    public class ExplorerViewModel : Widget, IExplorer
    {
        private ExplorerItem _rootItem;

        static ExplorerViewModel()
        {
            ExplorerItems = new ConcurrentDictionary<Instance, WeakReference<ExplorerItem>>();

            Game.InstanceAdded += GameOnInstanceAdded;

            foreach (var i in Game.Instances)
            {
                GameOnInstanceAdded(i.Value);
            }

            Game.InstanceRemoved +=
                i => Editor.Current.Dispatcher.InvokeAsync(() => ExplorerItems.TryRemove(i));
        }

        public ExplorerViewModel()
        {
            DisplayName = "Explorer";
        }

        public Instance LastClickedInstance { get; set; }

        public bool IsFolderSelected => LastClickedInstance is Folder;

        public CopyCommand CopyCommand { get; } = new CopyCommand();
        public PasteCommand PasteCommand { get; } = new PasteCommand();
        public PasteIntoCommand PasteIntoCommand { get; } = new PasteIntoCommand();
        public DuplicateCommand DuplicateCommand { get; } = new DuplicateCommand();
        public MakePluginCommand MakePluginCommand { get; } = new MakePluginCommand();
        public DeleteCommand DeleteCommand { get; } = new DeleteCommand();
        public GroupCommand GroupCommand { get; } = new GroupCommand();
        public UngroupCommand UngroupCommand { get; } = new UngroupCommand();
        public SelectChildrenCommand SelectChildrenCommand { get; } = new SelectChildrenCommand();
        public ZoomToCommand ZoomToCommand { get; } = new ZoomToCommand();
        public InsertPartCommand InsertPartCommand { get; } = new InsertPartCommand();

        public ExplorerItem RootItem
        {
            get { return _rootItem; }
            set
            {
                if (Equals(value, _rootItem)) return;
                _rootItem = value;
                value.IsExpanded = true;
                NotifyOfPropertyChange();
            }
        }

        public static ConcurrentDictionary<Instance, WeakReference<ExplorerItem>> ExplorerItems { get; }

        public override PaneLocation PreferredLocation => PaneLocation.Right;

        private static void GameOnInstanceAdded(Instance i)
        {
            ExplorerItem.TryCreate(i);
        }
    }
}