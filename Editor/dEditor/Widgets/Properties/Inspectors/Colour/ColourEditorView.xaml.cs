// ColourEditorView.xaml.cs - dEditor
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
using System.Windows.Controls;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;

namespace dEditor.Widgets.Properties.Inspectors.Colour
{
	/// <summary>
	/// Interaction logic for ColourEditorView.xaml
	/// </summary>
	public partial class ColourEditorView
	{
		public ColourEditorView()
		{
			InitializeComponent();

            /*
			ColorPicker.AvailableColors = new ObservableCollection<ColorItem>
			{
				new ColorItem(Color.FromRgb(164, 189, 71), "Br. yellowish green"),
				new ColorItem(Color.FromRgb(245, 208, 48), "Bright Yellow"),
				new ColorItem(Color.FromRgb(218, 133, 65), "Bright Orange"),
				new ColorItem(Color.FromRgb(196, 40, 28), "Bright Red"),
				new ColorItem(Color.FromRgb(107, 50, 124), "Bright Purple"),
				new ColorItem(Color.FromRgb(13, 105, 172), "Bright Blue"),
				new ColorItem(Color.FromRgb(0, 143, 156), "Bright Bluish Green"),
				new ColorItem(Color.FromRgb(75, 151, 75), "Bright Green"),
				new ColorItem(Color.FromRgb(0, 0, 0), "Extra"), // extra
				new ColorItem(Color.FromRgb(0, 0, 0), "Extra"), // extra
				new ColorItem(Color.FromRgb(255, 255, 255), "Institutional White"),
				new ColorItem(Color.FromRgb(242, 242, 242), "White"),
				new ColorItem(Color.FromRgb(230, 230, 230), "Light Stone Grey"),
				new ColorItem(Color.FromRgb(205, 205, 205), "Mid Grey"),
				new ColorItem(Color.FromRgb(165, 165, 165), "Medium Stone Grey"),
				new ColorItem(Color.FromRgb(99, 99, 99), "Dark Stone Grey"),
				new ColorItem(Color.FromRgb(27, 42, 53), "Black"),
				new ColorItem(Color.FromRgb(0, 0, 0), "Really Black"),
				new ColorItem(Color.FromRgb(0, 0, 0), "Extra"), // extra
				new ColorItem(Color.FromRgb(0, 0, 0), "Extra") // extra
			};
            */
		}
	}
}