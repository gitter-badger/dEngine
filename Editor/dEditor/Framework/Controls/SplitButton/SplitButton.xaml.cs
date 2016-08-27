// SplitButton.xaml.cs - dEditor
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
using System.Windows.Markup;
using Xceed.Wpf.Toolkit;

namespace dEditor.Framework.Controls.SplitButton
{
	/// <summary>
	/// Interaction logic for SplitButton.xaml
	/// </summary>
	[ContentProperty("CustomContent")]
	public partial class SplitButton : UserControl
	{
		public static readonly DependencyProperty CustomContentProperty = DependencyProperty.Register("CustomContent",
			typeof(object), typeof(DropDownButton), null);

		public static readonly DependencyProperty DropDownContentProperty = DependencyProperty.Register("DropDownContent",
			typeof(object), typeof(DropDownButton), null);

		public SplitButton()
		{
			InitializeComponent();
			DataContext = this;
		}

		public object CustomContent
		{
			get { return GetValue(CustomContentProperty); }
			set { SetValue(CustomContentProperty, value); }
		}

		public object DropDownContent
		{
			get { return GetValue(DropDownContentProperty); }
			set { SetValue(DropDownContentProperty, value); }
		}
	}
}