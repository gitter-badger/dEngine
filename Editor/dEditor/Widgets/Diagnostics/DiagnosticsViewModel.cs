// DiagnosticsViewModel.cs - dEditor
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
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using dEditor.Framework;
using dEditor.Framework.Controls.TreeListView;
using dEditor.Widgets.Diagnostics.SharpTreeView;
using dEngine;
using dEngine.Instances.Diagnostics;

namespace dEditor.Widgets.Diagnostics
{
    [Export(typeof(IDiagnostics))]
    public class DiagnosticsViewModel : Widget, IDiagnostics
    {
        private DiagnosticsRootNode _rootNode;

        public DiagnosticsRootNode RootNode
        {
            get { return _rootNode; }
            private set
            {
                if (Equals(value, _rootNode)) return;
                _rootNode = value;
                NotifyOfPropertyChange();
            }
        }

        public DiagnosticsViewModel()
        {
            DisplayName = "Diagnostics";

            Game.RegisterInitializeCallback(OnGameInitialize);
        }

        private void OnGameInitialize()
        {
            RootNode = new DiagnosticsRootNode();

            Game.Stats.ChildAdded.Connect(c =>
            {
                RootNode.LazyLoading = true;
                RootNode.EnsureLazyChildren();
            });

            RootNode.LazyLoading = true;
            RootNode.EnsureLazyChildren();
        }

        public class DiagnosticsRootNode : SharpTreeNode
        {
            protected override void LoadChildren()
            {
                Children.AddRange(Game.Stats.Children.OfType<StatsItem>().Select(item => new StatsItemTreeNode(item)));
            }
        }

        public override PaneLocation PreferredLocation => PaneLocation.Left;
    }
}