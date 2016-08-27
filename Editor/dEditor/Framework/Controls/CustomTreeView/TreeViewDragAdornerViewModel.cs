// TreeViewDragAdornerViewModel.cs - dEditor
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
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using Screen = Caliburn.Micro.Screen;

namespace dEditor.Framework.Controls.CustomTreeView
{
	public class TreeViewDragAdornerViewModel : Screen
	{
		private ImageSource _image;
		private bool _isDragging;
		private Window _window;

		public bool IsDragging
		{
			get { return _isDragging; }
			set
			{
				if (value == _isDragging) return;
				_isDragging = value;

				_window.Visibility = value ? Visibility.Visible : Visibility.Collapsed;

				NotifyOfPropertyChange();
			}
		}

		public ImageSource Image
		{
			get { return _image; }
			set
			{
				if (Equals(value, _image)) return;
				_image = value;
				NotifyOfPropertyChange();
			}
		}

		public Point Offset { get; set; }
		public DependencyObject Source { get; set; }

		protected override void OnViewAttached(object view, object context)
		{
			base.OnViewAttached(view, context);
			_window = (Window)GetView();
		}

		public void UpdatePosition(object sender, EventArgs e)
		{
			var mousePos = Control.MousePosition;
			_window.Top = mousePos.Y - Offset.Y;
			_window.Left = mousePos.X - Offset.X;

			/*
			_window.RaiseEvent(new MouseEventArgs(Mouse.PrimaryDevice, 0)
			{
				RoutedEvent = Mouse.MouseMoveEvent,
				Source = Source,
			});
			*/
		}
	}
}