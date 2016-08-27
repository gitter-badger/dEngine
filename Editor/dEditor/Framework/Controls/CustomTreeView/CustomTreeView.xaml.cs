// CustomTreeView.xaml.cs - dEditor
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using dEditor.Utility;
using dEditor.Widgets.CodeEditor;
using dEditor.Widgets.Explorer;
using dEngine;
using dEngine.Instances;

namespace dEditor.Framework.Controls.CustomTreeView
{
	/// <summary>
	/// Interaction logic for CustomTreeView.xaml
	/// </summary>
	public partial class CustomTreeView : UserControl
	{
		private const bool _adornerEnabled = false;

		public static readonly DependencyProperty RootItemProperty = DependencyProperty.Register("RootItem",
			typeof(CustomTreeItem), typeof(CustomTreeView));

		private readonly DispatcherTimer _adornerTimer;
		private readonly TreeViewDragAdornerViewModel _dragAdorner;

		private readonly List<ExplorerItem> _dragSelectedItems = new List<ExplorerItem>();

		private bool _didSelect;
		private Point _lastPoint;
		private CustomTreeItem _lastSelected;
		private Point _mouseDownPosition;

		public CustomTreeView()
		{
			InitializeComponent();

			_dragAdorner = new TreeViewDragAdornerViewModel();

			Editor.Current.WindowManager.ShowWindow(_dragAdorner, null, new Dictionary<string, object>
			{
				{"SizeToContent", SizeToContent.WidthAndHeight},
				{"Background", Brushes.Transparent},
				{"WindowStyle", WindowStyle.None},
				{"ShowInTaskbar", false},
				{"AllowsTransparency", true},
				{"ResizeMode", ResizeMode.NoResize}
			});

			_adornerTimer = new DispatcherTimer(DispatcherPriority.Render) {Interval = System.TimeSpan.FromSeconds(1 / 144f)};
			_adornerTimer.Tick += (s, e) => { _dragAdorner.UpdatePosition(s, e); };

			if (Editor.MouseHook != null)
			{
				Editor.MouseHook.MouseUp += OnGlobalMouseHookUp;
				Editor.MouseHook.MouseMove += OnGlobalMouseHookMove;
			}
		}

		public Func<DataObject> GetDragDataFunc { get; set; }
		public Action<object, DragEventArgs> OnDropAction { get; set; }

		public CustomTreeItem RootItem { get; set; }

		public Func<object, DragEventArgs, bool> ValidateDrag { get; set; }

		protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
		{
			_mouseDownPosition = e.GetPosition(Grid);

			if (!UpdateSelection(e, false))
			{
				Canvas.SetLeft(SelectionRectangle, _mouseDownPosition.X);
				Canvas.SetTop(SelectionRectangle, _mouseDownPosition.Y);
				SelectionRectangle.Width = 0;
				SelectionRectangle.Height = 0;
				SelectionRectangle.Visibility = Visibility.Visible;
			}
		}

		private void OnGlobalMouseHookMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (SelectionRectangle.Visibility == Visibility.Visible)
			{
				var mousePos = new Point(e.X, e.Y) - Grid.PointToScreen(new Point(0, 0));
				var mouseDownPos = _mouseDownPosition;

				if (mousePos.X < 0) mousePos.X = 0;
				if (mousePos.X > Grid.ActualWidth) mousePos.X = Grid.ActualWidth;
				if (mousePos.Y < 0) mousePos.Y = 0;
				if (mousePos.Y > Grid.ActualHeight) mousePos.Y = Grid.ActualHeight;

				if (mouseDownPos.X < mousePos.X)
				{
					Canvas.SetLeft(SelectionRectangle, mouseDownPos.X);
					SelectionRectangle.Width = mousePos.X - mouseDownPos.X;
				}
				else
				{
					Canvas.SetLeft(SelectionRectangle, mousePos.X);
					SelectionRectangle.Width = mouseDownPos.X - mousePos.X;
				}

				if (mouseDownPos.Y < mousePos.Y)
				{
					Canvas.SetTop(SelectionRectangle, mouseDownPos.Y);
					SelectionRectangle.Height = mousePos.Y - mouseDownPos.Y;
				}
				else
				{
					Canvas.SetTop(SelectionRectangle, mousePos.Y);
					SelectionRectangle.Height = mouseDownPos.Y - mousePos.Y;
				}

				VisualTreeUtility.ForEach(TreeView, element =>
				{
					var tvi = element as TreeViewItem;

					if (tvi != null)
					{
						var bounds =
							tvi.TransformToAncestor(Grid)
								.TransformBounds(new Rect(0.0, 0.0, tvi.ActualWidth, tvi.ActualHeight));
						var rect = new Rect(0.0, 0.0, Grid.ActualWidth, Grid.ActualHeight);

						var data = (ExplorerItem)tvi.DataContext;

						if (!_dragSelectedItems.Contains(data))
						{
							if (rect.Contains(bounds.TopLeft) || rect.Contains(bounds.BottomRight))
								_dragSelectedItems.Add(data);
							else
								_dragSelectedItems.Remove(data);
						}
					}
				});
			}
		}

		private void OnGlobalMouseHookUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			_adornerTimer.Stop();
			_dragAdorner.IsDragging = false;
			SelectionRectangle.Visibility = Visibility.Hidden;
			Debug.WriteLine("Set didDrag to false");
		}

		protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
		{
			if (_didSelect)
			{
				_didSelect = false;
				return;
			}

			if (e.OriginalSource != null)
				UpdateSelection(e, true);
		}

		protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			UpdateSelection(e, false);
		}

		protected override void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e)
		{
			UpdateSelection(e, false);
		}

		/*
		protected override void OnDragEnter(DragEventArgs e)
		{
			e.Effects = ValidateDrag(e.OriginalSource, e) ? DragDropEffects.Copy : DragDropEffects.None;
			e.Handled = true;
		}

		protected override void OnDragOver(DragEventArgs e)
		{
			e.Effects = ValidateDrag(e.OriginalSource, e) ? DragDropEffects.Copy : DragDropEffects.None;
			e.Handled = true;
		}

		protected override void OnDrop(DragEventArgs e)
		{
			if (ValidateDrag(e.OriginalSource, e))
				OnDropAction(e.OriginalSource, e);

			e.Handled = true;
		}

		private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			UpdateSelection(e, false);
		}

		private void OnPreviewMouseUp(object sender, MouseButtonEventArgs e)
		{
			_didDrag = false;

			if (_didSelect)
			{
				_didSelect = false;
				return;
			}

			if (e.OriginalSource != null)
				UpdateSelection(e, true);
		}

		private void OnPreviewMouseMove(object sender, MouseEventArgs e)
		{
			var mousePosition = e.GetPosition(null);
			var diff = _mouseDownPosition - mousePosition;

			if (!_isDragging && e.LeftButton == MouseButtonState.Pressed &&
				(Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
				 Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
			{
				var treeViewItem = VisualTreeUtility.FindParent<TreeViewItem>((DependencyObject)Mouse.DirectlyOver);

				if (treeViewItem == null)
					return;

				if (_adornerEnabled)
				{
					var adornerBitmap = new RenderTargetBitmap((int)TreeView.ActualWidth, (int)TreeView.ActualHeight, 96,
						96,
						PixelFormats.Pbgra32);

					var drawingVisual = new DrawingVisual();

					using (var dc = drawingVisual.RenderOpen())
					{
						VisualTreeUtility.Where(TreeView, visual =>
						{
							var tvi = visual as TreeViewItem;

							if (tvi == null) return false;
							if (!((CustomTreeItem)tvi.DataContext).IsSelected) return false;

							// get the border so children don't get drawn
							var bd = (Border)VisualTreeUtility.First(tvi, x => (x as Border)?.Name == "Bd");
							var relativePos = bd.TranslatePoint(new Point(0, 0), TreeView);

							var rt = new RenderTargetBitmap((int)bd.ActualWidth * 2, (int)bd.ActualHeight, 96, 96,
								PixelFormats.Pbgra32);
							rt.Render(bd);

							dc.DrawImage(rt, new Rect(relativePos, new Size(bd.ActualWidth * 2, bd.ActualHeight)));

							return false;
						});
					}

					adornerBitmap.Render(drawingVisual);

					_dragAdorner.Source = (DependencyObject)e.OriginalSource;
					_dragAdorner.Image = adornerBitmap;
					_dragAdorner.IsDragging = true;
					_dragAdorner.Offset = e.GetPosition(TreeView);

					_adornerTimer.Start();
				}

				var dragData = GetDragDataFunc();
				DragDrop.DoDragDrop(treeViewItem, dragData, DragDropEffects.Copy | DragDropEffects.None);

				Debug.WriteLine("didDrag set to true");
				_didDrag = true;
			}
		}
		*/

		/// <summary>
		/// Updates the selection.
		/// </summary>
		/// <param name="e">Mouse button event argument.</param>
		/// <param name="allowSelectionClick">
		/// If the clicked item is already selected, this argument determines if other items
		/// should be deselected.
		/// </param>
		private bool UpdateSelection(MouseButtonEventArgs e, bool allowSelectionClick)
		{
			var treeItemVisual = VisualTreeUtility.FindParent<TreeViewItem>((DependencyObject)e.OriginalSource);
			var treeItemViewModel = (CustomTreeItem)treeItemVisual?.DataContext;

			var clickPoint = e.GetPosition(TreeView);
			var controlDown = Keyboard.IsKeyDown(System.Windows.Input.Key.LeftCtrl);
			var shiftDown = Keyboard.IsKeyDown(System.Windows.Input.Key.LeftShift);

			/*
			if (_didDrag)
			{
				Debug.WriteLine("DRAG CANCEL: _didDrag == true");
				_didDrag = false;
				return;
			}*/

			if (!(treeItemVisual == null && shiftDown) && (treeItemVisual == null || shiftDown))
			{
				Game.Selection.ClearSelection();
			}

			if (treeItemVisual == null)
			{
				Debug.WriteLine("DRAG CANCEL: treeeItemVisual == null");
				return false;
			}

			if (e.ClickCount == 2 && e.ButtonState == MouseButtonState.Pressed)
			{
				var codeContainer = treeItemViewModel.Instance as LuaSourceContainer;

				if (codeContainer != null)
				{
					var doc = new CodeEditorViewModel(codeContainer);

					Editor.Current.Shell.OpenDocument(doc);

					codeContainer.Destroyed.Event += () => doc.TryClose();
				}
			}

			if (treeItemViewModel.IsSelected && !allowSelectionClick)
			{
				Debug.WriteLine("DRAG CANCEL: (treeItemViewModel.IsSelected && !allowSelectionClick) == true");
				return false;
			}

			if (!controlDown && !shiftDown)
			{
				Game.Selection.ClearSelection();
			}

			if (shiftDown && _lastSelected != null)
			{
				var needsReversed = clickPoint.Y < _lastPoint.Y;

				VisualTreeUtility.ForEach(TreeView, visual =>
				{
					var tvi = visual as TreeViewItem;
					if (tvi == null) return;
					var relPoint = tvi.TranslatePoint(new Point(0, 0), TreeView);

					var check = needsReversed
						? (relPoint.Y > clickPoint.Y && relPoint.Y < _lastPoint.Y)
						: (relPoint.Y < clickPoint.Y && relPoint.Y > _lastPoint.Y);

					if (check)
						Game.Selection.Select(((CustomTreeItem)tvi.DataContext).Instance);
				});
			}

			if (shiftDown && _lastSelected != null)
			{
				Game.Selection.Select(_lastSelected.Instance);

				_didSelect = true;
			}

			if (controlDown)
			{
				if (treeItemViewModel.Instance.IsSelected)
					Game.Selection.Deselect(treeItemViewModel.Instance);
				else
				{
					Game.Selection.Select(treeItemViewModel.Instance);
					_didSelect = true;
				}
			}
			else
			{
				Game.Selection.Select(treeItemViewModel.Instance);
				_didSelect = true;
			}

			if (!shiftDown)
			{
				_lastSelected = treeItemViewModel;
				_lastPoint = clickPoint;
			}

			return true;
		}
	}
}