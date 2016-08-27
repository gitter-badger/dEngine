// MaterialEditorView.xaml.cs - dEditor
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
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace dEditor.Widgets.MaterialEditor
{
	/// <summary>
	/// Interaction logic for MaterialEditorView.xaml
	/// </summary>
	public partial class MaterialEditorView : UserControl
	{
		public MaterialEditorView()
		{
			InitializeComponent();
			DataContextChanged += OnDataContextChanged;
		}

		private void OnDataContextChanged(object sender,
			DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{
			var vm = (MaterialEditorViewModel)DataContext;

			if (MaterialPreviewPanel.IsHandleCreated)
			{
				vm.OnHandleSet(MaterialPreviewPanel.Handle);
			}
			else
			{
				MaterialPreviewPanel.HandleCreated += (s, e) => vm.OnHandleSet(MaterialPreviewPanel.Handle);
			}
		}

		private void GraphCanvas_OnInitialized(object sender, EventArgs e)
		{
			var vm = (MaterialEditorViewModel)DataContext;
			vm.Canvas = sender as Canvas;
		}
	}
}