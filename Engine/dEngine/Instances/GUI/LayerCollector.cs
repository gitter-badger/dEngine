// LayerCollector.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using C5;
using dEngine.Graphics;
using dEngine.Instances.Attributes;
using dEngine.Instances.Interfaces;
using dEngine.Services;
using dEngine.Utility;
using SharpDX;
using SharpDX.Direct2D1;

namespace dEngine.Instances
{
	/// <summary>
	/// A object draws <see cref="GuiElement" />s.
	/// </summary>
	/// <remarks>
	/// Must be parented to a <see cref="Camera" /> to render.
	/// </remarks>
	[TypeId(96)]
	public abstract class LayerCollector : GuiBase2D, ICameraUser
    {
		private const int _selectionBorderOffset = 4;

		private static readonly Colour _selectionColour = new Colour(25 / 255f, 152 / 255f, 255 / 255f);
		private static readonly Brush _selectionBrush = Renderer.Brushes?.Get(_selectionColour);
		private static readonly RoundedRectangle _emptyRectangle = new RoundedRectangle();
		private readonly object _zIndexLocker = new object();

		internal readonly SortedArray<GuiElement> ZIndexedElements;
		private Camera _camera;
		private bool _enabled;
		private Bitmap1 _target;

		private bool _validParent;

		/// <inheritdoc />
		protected LayerCollector()
		{
			_enabled = true;

			ZIndexedElements = new SortedArray<GuiElement>(new ZIndexComparer());
		}

		/// <inheritdoc />
		public override Vector2 AbsolutePosition
		{
			get { return Vector2.Zero; }
			internal set { throw new InvalidOperationException("Cannot set AbsolutePosition of LayerCollector."); }
		}

		/// <inheritdoc />
		[EditorVisible("Data")]
		public override Vector2 AbsoluteSize
		{
			get { return _camera?.ViewportSize ?? Vector2.Zero; }
			internal set { throw new InvalidOperationException("Cannot set AbsoluteSize of LayerCollector."); }
		}

		/// <summary>
		/// Determines if the container and its elements are visible/usable.
		/// </summary>
		[InstMember(2), EditorVisible("Data")]
		public bool Enabled
		{
			get { return _enabled; }
			set
			{
				_enabled = value;
				NotifyChanged(nameof(Enabled));
			}
		}

		internal Bitmap1 Target
		{
			get { return _target; }
			set
			{
				lock (Renderer.Locker)
				{
					if (value == _target) return;
					_target = value;
				}
			}
		}

        Camera ICameraUser.Camera
		{
			get { return _camera; }
			set
			{
				var camera = _camera;
				if (camera != null)
				{
					lock (camera.Locker)
					{
						camera.LayerCollectors.Remove(this);
					}
					camera.ViewportSizeChanged.Event += OnViewportSizeChanged;
				}

				_camera = value;

				if (value != null)
				{
					lock (value.Locker)
					{
						value.LayerCollectors.Add(this);
					}
				    lock (Renderer.Locker)
				    {
				        Target = value.RenderTarget2D;
				    }
				    value.ViewportSizeChanged.Event += OnViewportSizeChanged;
				}
			}
		}

		private void OnViewportSizeChanged(Vector2 vector2)
		{
			foreach (var kv in Children)
			{
				(kv as GuiElement)?.Measure();
			}
		}

		protected override void OnAncestryChanged(Instance child, Instance parent)
		{
			base.OnAncestryChanged(child, parent);

			if (child == this)
			{
				_validParent = IsDescendantOf(Players.Service.LocalPlayer) || parent == Game.StarterGui;
			}
		}

		internal override bool CheckCanDraw()
		{
			if (!_enabled)
				return false;

			if (Parent == Players.Service.LocalPlayer)
				return true;

			return _validParent;
		}

		internal void Render()
		{
			lock (_zIndexLocker)
			{
				if (Target == null)
					return;

				var context = Renderer.Context2D;
				context.Target = Target;

				Action<GuiBase2D, IEnumerable<Instance>> DrawChildren = null;

				// draws children of LayerCollector or GuiElement.
				DrawChildren = (root, children) =>
				{
					// draw children
					foreach (var inst in children)
					{
						var element = inst as GuiElement;
						if (element == null || !element.CheckCanDraw() || element.ZIndex != 0)
							continue;

						// transform target
						var pos = element.AbsolutePosition + (element.AbsoluteSize / 2);
						context.Transform =
							Matrix3x2.Rotation(element.Rotation * Mathf.Deg2Rad) *
							Matrix3x2.Translation((SharpDX.Vector2)pos);

						var doClip = element.ClipDescendants;

						element.Draw();

						if (doClip)
							context.PushAxisAlignedClip(element.RenderRect, AntialiasMode.Aliased);
						{
							DrawChildren(element, element.Children);
						}
						if (doClip) context.PopAxisAlignedClip();


						// reset target transform
						context.Transform = Matrix3x2.Identity;
					}
				};

				context.BeginDraw();

				DrawChildren(this, Children);
				DrawChildren(this, ZIndexedElements);

				context.EndDraw();
			}
		}

		private bool HitTestItem(ref GuiElement el, float x, float y, bool includeZIndexed, out GuiElement result)
		{
			result = null;

			if (el == null || !el.CheckCanDraw() || (el.ZIndex != 0 && !includeZIndexed))
				return false;

			if (el.LayoutRect.Contains(x, y) && el.IsHitTestVisible & el.BackgroundColour.a > 0)
			{
				result = el;
				return true;
			}

			var children = el.Children;
			for (int i = 0; i < children.Count; i++)
			{
				GuiElement item;
				var child = children[i] as GuiElement;
				if (HitTestItem(ref child, x, y, includeZIndexed, out item))
				{
					result = item;
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Returns the topmost element that contains the given point.
		/// </summary>
		public GuiElement HitTest(float x, float y)
		{
			GuiElement result;
			return HitTest(x, y, out result) ? result : null;
		}

		/// <summary>
		/// Returns the topmost element that contains the given point.
		/// </summary>
		internal bool HitTest(float x, float y, out GuiElement result)
		{
			var children = Children;
			var zIndexedElements = ZIndexedElements;

			result = null;

			for (int i = zIndexedElements.Count; i-- > 0;)
			{
				var el = zIndexedElements[i];
				HitTestItem(ref el, x, y, true, out el);
				if (el != null)
				{
					result = el;
					return true;
				}
			}

			children.EnterReadLock();
			for (int i = children.Count; i-- > 0;)
			{
				var el = children[i] as GuiElement;
				GuiElement childRes;

				if (HitTestItem(ref el, x, y, false, out childRes))
				{
					result = childRes;
					children.ExitReadLock();
					return true;
				}
			}
			children.ExitReadLock();

			return false;
		}

		/// <summary>
		/// Returns a list of elements that contain the given point;
		/// </summary>
		internal Stack<GuiElement> HitTestMultiple(Vector2 point)
		{
			var results = new Stack<GuiElement>();
			var pointDX = (Point)point;

			Action<IEnumerable<Instance>, bool> scanElements = null;

			scanElements = (items, includeZIndexed) =>
			{
				foreach (var inst in items)
				{
					var el = inst as GuiElement;
					if (el == null || !el.CheckCanDraw() || (el.ZIndex != 0 && !includeZIndexed)) continue;

					if (el.LayoutRect.Contains(pointDX))
					{
						results.Push(el);
					}

					scanElements(el.Children, false);
				}
			};

			scanElements(Children, false);
			scanElements(ZIndexedElements, true);

			return results;
		}

		internal void AddZIndexed(GuiElement el)
		{
			lock (_zIndexLocker)
			{
				ZIndexedElements.Add(el);
			}
		}

		internal void RemoveZIndexed(GuiElement el)
		{
			lock (_zIndexLocker)
			{
				ZIndexedElements.Remove(el);
			}
		}

		internal class HitTestRequest
		{
			public OrderedSet<GuiElement> Elements;
			public SharpDX.Vector2 Position;

			public HitTestRequest(OrderedSet<GuiElement> elementSet, Vector2 position)
			{
				Elements = elementSet;
				Position = (SharpDX.Vector2)position;
			}
		}

		private class ZIndexComparer : IComparer<GuiElement>
		{
			public int Compare(GuiElement x, GuiElement y)
			{
				if (x.ZIndex < y.ZIndex)
					return -1;
				if (x.ZIndex > y.ZIndex)
					return 1;
				if (x.Equals(y))
					return 0;

				return 1;
			}
		}

		internal class PriorityComparer : IComparer<LayerCollector>
		{
			public int Compare(LayerCollector x, LayerCollector y)
			{
				if (x.Equals(y))
					return 0;
				if (x.Parent is CoreGui)
					return 1;
				return -1;
			}
		}
	}
}