// BindableSelectedItemBehavior.cs - dEditor
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
	public class BindableSelectedItemBehavior : Behavior<TreeView>
	{
		protected override void OnAttached()
		{
			base.OnAttached();

			AssociatedObject.SelectedItemChanged += OnTreeViewSelectedItemChanged;
		}

		protected override void OnDetaching()
		{
			base.OnDetaching();

			if (AssociatedObject != null)
			{
				AssociatedObject.SelectedItemChanged -= OnTreeViewSelectedItemChanged;
			}
		}

		private void OnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			SelectedItem = e.NewValue;
		}

		#region SelectedItem Property

		public object SelectedItem
		{
			get { return GetValue(SelectedItemProperty); }
			set { SetValue(SelectedItemProperty, value); }
		}

		public static readonly DependencyProperty SelectedItemProperty =
			DependencyProperty.Register("SelectedItem", typeof(object), typeof(BindableSelectedItemBehavior),
				new UIPropertyMetadata(null, OnSelectedItemChanged));

		private static void OnSelectedItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			var item = e.NewValue as TreeViewItem;
			if (item != null)
			{
				item.SetValue(TreeViewItem.IsSelectedProperty, true);
			}
		}

		#endregion
	}
}