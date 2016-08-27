// InputBindingManager.cs - dEditor
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
using System.Windows.Data;
using System.Windows.Input;

namespace dEditor.Framework.Services
{
	public static class InputBindingManager
	{
		public static readonly DependencyProperty UpdatePropertySourceWhenEnterPressedProperty = DependencyProperty
			.RegisterAttached(
				"UpdatePropertySourceWhenEnterPressed", typeof(DependencyProperty), typeof(InputBindingManager),
				new PropertyMetadata(null, OnUpdatePropertySourceWhenEnterPressedPropertyChanged));

		static InputBindingManager()
		{
		}

		public static void SetUpdatePropertySourceWhenEnterPressed(DependencyObject dp, DependencyProperty value)
		{
			dp.SetValue(UpdatePropertySourceWhenEnterPressedProperty, value);
		}

		public static DependencyProperty GetUpdatePropertySourceWhenEnterPressed(DependencyObject dp)
		{
			return (DependencyProperty)dp.GetValue(UpdatePropertySourceWhenEnterPressedProperty);
		}

		private static void OnUpdatePropertySourceWhenEnterPressedPropertyChanged(DependencyObject dp,
			DependencyPropertyChangedEventArgs e)
		{
			UIElement element = dp as UIElement;

			if (element == null)
			{
				return;
			}

			if (e.OldValue != null)
			{
				element.PreviewKeyDown -= HandlePreviewKeyDown;
			}

			if (e.NewValue != null)
			{
				element.PreviewKeyDown += HandlePreviewKeyDown;
			}
		}

		static void HandlePreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				DoUpdateSource(e.Source);
			}
		}

		static void DoUpdateSource(object source)
		{
			DependencyProperty property =
				GetUpdatePropertySourceWhenEnterPressed(source as DependencyObject);

			if (property == null)
			{
				return;
			}

			UIElement elt = source as UIElement;

			if (elt == null)
			{
				return;
			}

			BindingExpression binding = BindingOperations.GetBindingExpression(elt, property);

			if (binding != null)
			{
				binding.UpdateSource();
			}
		}
	}
}