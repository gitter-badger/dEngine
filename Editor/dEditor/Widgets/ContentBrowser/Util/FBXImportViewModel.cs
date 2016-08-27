// FBXImportViewModel.cs - dEditor
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System.Threading;
using Caliburn.Micro;
using dEditor.Framework;
using dEditor.Widgets.Properties;
using dEngine.Utility.FileFormats.Model;

namespace dEditor.Widgets.Assets
{
	public class FBXImportViewModel : Conductor<PropertiesViewModel>
	{
		private string _filePath;
		public PropertiesViewModel Properties;

		public FBXImportViewModel(string file)
		{
			FilePath = file;
			ImportResetEvent = new ManualResetEventSlim();
			ImportSettings = new FBX.ImportSettings(Project.Current.ContentPath);

			Properties = new PropertiesViewModel {UseSelectionService = false, UseHistoryService = false};

			ImportSettings.ImportAsSkeletalChanged += UpdateInspector;
			UpdateInspector(ImportSettings.ImportAsSkeletal);
		}

		public string FilePath
		{
			get { return _filePath; }
			private set
			{
				_filePath = value;
				NotifyOfPropertyChange();
			}
		}

		public FBX.ImportSettings ImportSettings { get; }

		public ManualResetEventSlim ImportResetEvent { get; }

		public bool ImportAll { get; set; }

		private void UpdateInspector(bool value)
		{
			if (value)
			{
				Properties.FilteredCategories.Remove("Skeletal");
				Properties.FilteredCategories.Add("Static");
			}
			else
			{
				Properties.FilteredCategories.Remove("Static");
				Properties.FilteredCategories.Add("Skeletal");
			}

			Properties.Target = null;
			Properties.Target = ImportSettings;
		}

		public void FinishImport()
		{
			ImportResetEvent.Set();
			TryClose(true);
		}

		public void FinishImportAll()
		{
			ImportAll = true;
			ImportResetEvent.Set();
			TryClose(true);
		}

		protected override void OnActivate()
		{
			ActivateItem(Properties);
		}
	}
}