// LayoutItem.cs - dEditor
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
using System.IO;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Caliburn.Micro;

namespace dEditor.Framework
{
	public abstract class LayoutItem : Screen
	{
		protected RelayCommand _closeCommand;
		private Guid _guid;
		private bool _isVisible;
		private bool _selected;

		protected LayoutItem()
		{
			_guid = Guid.NewGuid();
		}

		public Guid Id => _guid;

		public string ContentId => _guid.ToString();

		public virtual BitmapSource IconSource { get; }

		public bool ShouldReopenOnStart { get; set; } = true;

		public abstract ICommand CloseCommand { get; }

		public bool IsSelected
		{
			get { return _selected; }
			set
			{
				_selected = value;
				NotifyOfPropertyChange();
			}
		}

		public bool IsVisible
		{
			get { return _isVisible; }
			set
			{
				_isVisible = value;
				NotifyOfPropertyChange();
			}
		}

		public virtual void LoadState(BinaryReader reader)
		{
		}

		public virtual void SaveState(BinaryWriter writer)
		{
		}
	}
}