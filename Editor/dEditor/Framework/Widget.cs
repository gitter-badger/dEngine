// Widget.cs - dEditor
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

namespace dEditor.Framework
{
	/// <summary>
	/// A widget which can be docked to the shell.
	/// </summary>
	public abstract class Widget : LayoutItem
	{
		public abstract PaneLocation PreferredLocation { get; }

		public virtual double PreferredWidth => 200;

		public virtual double PreferredHeight => 200;

		public override ICommand CloseCommand
		{
			get { return _closeCommand ?? (_closeCommand = new RelayCommand(p => IsVisible = false, p => true)); }
		}

		public override void TryClose(bool? dialogResult = null)
		{
			base.TryClose(dialogResult);

			Editor.Current.Shell.Widgets.Remove(this);
		}
	}
}