// Document.cs - dEditor
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System.Threading.Tasks;
using System.Windows.Input;
using dEngine.Instances;

namespace dEditor.Framework
{
	/// <summary>
	/// A document is a type of module which is docked in the middle of the window.
	/// </summary>
	public abstract class Document : LayoutItem
	{
		protected Document()
		{
			ShouldReopenOnStart = false;
		}

		public override ICommand CloseCommand
		{
			get { return _closeCommand ?? (_closeCommand = new RelayCommand(p => TryClose(null), p => true)); }
		}

		public virtual void OnHide()
		{
		}

		public virtual Task OnSave(LuaSourceContainer container)
		{
			return Task.FromResult(true);
		}
	}
}