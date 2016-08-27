// CodeEditorView.xaml.cs - dEditor
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
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
// ReSharper disable UnusedMember.Local
// ReSharper disable PossibleNullReferenceException

namespace dEditor.Widgets.CodeEditor
{
	/// <summary>
	/// Interaction logic for CodeEditorView.xaml
	/// </summary>
	public partial class CodeEditorView
	{
		public CodeEditorView()
		{
			InitializeComponent();
            DataContextChanged += OnDataContextChanged;
		}

	    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
	    {
	        ((CodeEditorViewModel)DataContext).TextEditor = TextEditor;
	    }

	    private void CanZoom(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}

		private void OnZoomIn(object sender, ExecutedRoutedEventArgs e)
		{
			(DataContext as CodeEditorViewModel).ZoomScale += 0.1f;
		}

		private void OnZoomOut(object sender, ExecutedRoutedEventArgs e)
		{
			(DataContext as CodeEditorViewModel).ZoomScale -= 0.1f;
		}

		private void BreakpointContainer_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			//var position = CodeEditor.GetPositionFromPoint(e.GetPosition(CodeEditor));

			//Debug.WriteLine(position?.Line ?? -1);
		}
	}
}