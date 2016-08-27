// ShellView.xaml.cs - dEditor
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
using System.Windows;
using System.Windows.Controls;

namespace dEditor.Shell
{
    /// <summary>
    /// Interaction logic for ShellView.xaml
    /// </summary>
    public partial class ShellView : UserControl
    {
        public ShellView()
        {
            InitializeComponent();
        }

        private void OnManagerLayoutUpdated(object sender, EventArgs e)
        {
            UpdateFloatingWindows();
        }

        public void UpdateFloatingWindows()
        {
            var mainWindow = Window.GetWindow(this);
            var mainWindowIcon = mainWindow?.Icon;
            var showFloatingWindowsInTaskbar = ((ShellViewModel)DataContext).ShowFloatingWindowsInTaskbar;
            foreach (var window in DockingManager.FloatingWindows)
            {
                window.Icon = mainWindowIcon;
                window.ShowInTaskbar = showFloatingWindowsInTaskbar;
            }
        }
    }
}