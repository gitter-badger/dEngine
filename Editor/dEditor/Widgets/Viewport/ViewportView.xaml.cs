// ViewportView.xaml.cs - dEditor
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
using System.Reflection;
using System.Windows.Forms;
using dEngine;
using DragEventArgs = System.Windows.DragEventArgs;
using UserControl = System.Windows.Controls.UserControl;

namespace dEditor.Widgets.Viewport
{
	/// <summary>
	/// Interaction logic for SceneView.xaml
	/// </summary>
	public partial class ViewportView : UserControl
	{
		public ViewportView()
		{
			InitializeComponent();

			RenderPane.MouseDown += RenderPaneOnMouseDown;
			RenderPane.MouseUp += RenderPaneOnMouseUp;

			RenderPane.GotFocus += (s, e) => { Engine.IsViewportFocused = true; };

			RenderPane.LostFocus += (s, e) => { Engine.IsViewportFocused = false; };
		}

		public ViewportViewModel SceneViewModel => DataContext as ViewportViewModel;

		private void RenderPaneOnMouseDown(object sender, MouseEventArgs args)
		{
			if (args.Button == MouseButtons.Right)
			{
				/*
                var point = Point.Empty;
                User32.GetCursorPos(ref point);
                var rect = new Rectangle(point.X, point.Y, 1, 1);
                User32.ClipCursor(ref rect);
                */
				//RenderPane.Cursor = new Cursor(System.Windows.Forms.Cursor.Current.Handle);
				//System.Windows.Forms.Cursor.Position = System.Windows.Forms.Cursor.Position;
				//System.Windows.Forms.Cursor.Clip = new Rectangle(System.Windows.Forms.Cursor.Position, new Size(0, 0));
			}
		}

		private void RenderPaneOnMouseUp(object sender, MouseEventArgs args)
		{
			if (args.Button == MouseButtons.Right)
			{
				//User32.ClipCursor(IntPtr.Zero);
			}
		}

		private void RenderPane_HandleCreated(object sender, EventArgs e)
		{
			Engine.SetHandle(RenderPane.Handle);
		}

		private System.Windows.DataObject GetDataObject(System.Windows.Forms.DragEventArgs e)
		{
			var info = e.Data.GetType().GetField("innerData", BindingFlags.NonPublic | BindingFlags.Instance);
			var obj = info?.GetValue(e.Data);
			info = obj?.GetType().GetField("innerData", BindingFlags.NonPublic | BindingFlags.Instance);
			return info?.GetValue(obj) as System.Windows.DataObject;
		}

		private void RenderPane_OnDragOver(object sender, System.Windows.Forms.DragEventArgs e)
		{
			var dataObj = GetDataObject(e);
			var viewModel = (ViewportViewModel)DataContext;
			var effect = (System.Windows.DragDropEffects)e.Effect;
			viewModel.OnPreviewDragOver(sender, dataObj, ref effect);
			e.Effect = (DragDropEffects)effect;
		}

		private void RenderPane_OnDragDrop(object sender, System.Windows.Forms.DragEventArgs e)
		{
			var dataObj = GetDataObject(e);
			var viewModel = (ViewportViewModel)DataContext;
			viewModel.OnPreviewDragDrop(sender, dataObj);
		}

		private void UIElement_OnPreviewDragOver(object sender, DragEventArgs e)
		{
			throw new NotImplementedException();
		}
	}
}