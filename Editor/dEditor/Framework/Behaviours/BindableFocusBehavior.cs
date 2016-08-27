// BindableFocusBehavior.cs - dEditor
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace dEditor.Framework.Behaviours
{
	public class BindableFocusBehavior : Behavior<Control>
	{
		public static readonly DependencyProperty HasFocusProperty =
			DependencyProperty.Register("HasFocus", typeof(bool), typeof(BindableFocusBehavior),
				new PropertyMetadata(default(bool), HasFocusUpdated));

		public bool HasFocus
		{
			get { return (bool)GetValue(HasFocusProperty); }
			set
			{
				SetValue(HasFocusProperty, value);
				AssociatedObject.Focus();
			}
		}

		private static void HasFocusUpdated(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((BindableFocusBehavior)d).SetFocus();
		}

		private void SetFocus()
		{
			if (HasFocus)
			{
				AssociatedObject.Focus();
			}
		}
	}
}