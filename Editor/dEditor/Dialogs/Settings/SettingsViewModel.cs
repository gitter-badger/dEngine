// SettingsViewModel.cs - dEditor
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
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using dEditor.Widgets.Properties;
using dEngine;

namespace dEditor.Dialogs.Settings
{
    using Settings = dEngine.Settings.Settings;
    public class SettingsViewModel : Conductor<PropertiesViewModel>
    {
        public readonly PropertiesViewModel Properties;
        private Settings _selectedSettings;

        public SettingsViewModel()
        {
            Properties = new PropertiesViewModel {UseSelectionService = false, UseHistoryService = false};
            Properties.FilteredCategories.Add("Data");
            Properties.FilteredCategories.Add("Behaviour");

            Settings =
                Engine.Settings.Children.OfType<Settings>()
                    .Concat(Engine.UserSettings.Children.OfType<Settings>());
        }

        public IEnumerable<Settings> Settings { get; }

        public Settings SelectedSettings
        {
            get { return _selectedSettings; }
            set
            {
                _selectedSettings = value;
                Properties.Target = value;
            }
        }

        public void ResetAllSettings()
        {
            Engine.Settings.RestoreDefaults();
        }

        public void ApplyAndClose()
        {
            TryClose(true);
        }

        public override void TryClose(bool? dialogResult = null)
        {
            base.TryClose(dialogResult);

            if (dialogResult == true)
            {
                Engine.Settings.Save();
                Engine.UserSettings.Save();
            }
        }

        protected override void OnActivate()
        {
            ActivateItem(Properties);
        }
    }
}