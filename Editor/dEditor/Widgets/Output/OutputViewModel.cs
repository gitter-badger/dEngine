// OutputViewModel.cs - dEditor
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Windows;
using dEditor.Framework;
using dEngine;
using dEngine.Instances;
using dEngine.Services;


namespace dEditor.Widgets.Output
{
	[Export(typeof(IOutput))]
	public class OutputViewModel : Widget, IOutput
	{
		private bool _wordWrap;
	    private ObservableCollection<OutputItem> _entries;

	    public OutputViewModel()
		{
			WordWrap = true;
            Entries = new ObservableCollection<OutputItem>();

	        var logService = DataModel.GetService<LogService>();
            logService.MessageOutput.Event += OnMessageOutputEvent;
		}

	    private void OnMessageOutputEvent(string msg, LogLevel level, string logger)
        {
            if (!logger.StartsWith("dEngine.Instances") &&
                !logger.StartsWith("dEngine.Services") &&
                !logger.StartsWith("I:"))
                return;

            Application.Current?.Dispatcher.InvokeAsync(() =>
            {
                Entries.Add(new OutputItem(ref msg, ref level, ref logger));
            });
        }

	    protected override void OnDeactivate(bool close)
	    {
	        base.OnDeactivate(close);

	        if (close)
	        {
                LogService.GetExisting().MessageOutput.Event -= OnMessageOutputEvent;
            }
	    }

	    public override string DisplayName => "Output";
		public override PaneLocation PreferredLocation => PaneLocation.Bottom;

	    [Export]
	    public bool WordWrap
	    {
	        get { return _wordWrap; }
	        set
	        {
	            if (value == _wordWrap) return;
	            _wordWrap = value;
	            NotifyOfPropertyChange();
	        }
	    }

	    public void Clear()
	    {
	        Entries = new ObservableCollection<OutputItem>();
	    }

	    public ObservableCollection<OutputItem> Entries
	    {
	        get { return _entries; }
	        set
	        {
	            if (Equals(value, _entries)) return;
	            _entries = value;
	            NotifyOfPropertyChange();
	        }
	    }

	    public override void SaveState(BinaryWriter writer)
		{
			writer.Write(WordWrap);
		}

		public override void LoadState(BinaryReader reader)
		{
			WordWrap = reader.ReadBoolean();
		}

		public void ToggleWordWrap()
		{
			WordWrap = !WordWrap;
		}
	}
}