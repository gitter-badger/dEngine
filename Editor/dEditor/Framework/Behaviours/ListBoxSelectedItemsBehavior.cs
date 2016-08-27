// ListBoxSelectedItemsBehavior.cs - dEditor
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace dEditor.Framework.Behaviours
{
	public class ListBoxSelectedItemsBehavior : Behavior<ListBox>
	{
		public static readonly DependencyProperty SelectedItemsProperty =
			DependencyProperty.Register("SelectedItems", typeof(IEnumerable), typeof(ListBoxSelectedItemsBehavior),
				new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		public IEnumerable SelectedItems
		{
			get { return (IEnumerable)GetValue(SelectedItemsProperty); }
			set { SetValue(SelectedItemsProperty, value); }
		}

		protected override void OnAttached()
		{
			AssociatedObject.SelectionChanged += AssociatedObjectSelectionChanged;
		}

		protected override void OnDetaching()
		{
			AssociatedObject.SelectionChanged -= AssociatedObjectSelectionChanged;
		}

		void AssociatedObjectSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var array = new object[AssociatedObject.SelectedItems.Count];
			AssociatedObject.SelectedItems.CopyTo(array, 0);
			SelectedItems = array;
		}
	}
}