// Stack.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using dEngine.Instances.Attributes;

namespace dEngine.Instances
{
	/// <summary>
	/// Stacks child elements.
	/// </summary>
	[TypeId(151), ToolboxGroup("GUI")]
	public class Stack : Frame
	{
		private readonly C5.SortedArray<GuiElement> _elements;
		private FlowDirection _orientation;
		private Vector2 _directionVec;
		private bool _needsRearranged;
		private Vector2 _offset;
		private bool _reversed;

		/// <inheritdoc />
		public Stack()
		{
			_elements = new C5.SortedArray<GuiElement>(new FrameIndexComparer(this));
			Orientation = FlowDirection.Vertical;
		}

		/// <summary>
		/// Determines whether to stack elements in reverse-order.
		/// </summary>
		[InstMember(1), EditorVisible("Behaviour")]
		public bool Reversed
		{
			get { return _reversed; }
			set
			{
				if (value == _reversed) return;
				_reversed = value;
				_needsRearranged = true;
				NotifyChanged();
			}
		}

		/// <summary>
		/// The direction to stack child elements.
		/// </summary>
		[InstMember(2), EditorVisible("Data")]
		public FlowDirection Orientation
		{
			get { return _orientation; }
			set
			{
				if (value == _orientation) return;
				_orientation = value;
				_directionVec = (value == FlowDirection.Horizontal) ? new Vector2(1, 0) : new Vector2(0, 1);
				_needsRearranged = true;
				NotifyChanged();
			}
		}

		/// <summary>
		/// The space inbetween elements.
		/// </summary>
		[InstMember(3), EditorVisible("Data")]
		public Vector2 Offset
		{
			get { return _offset; }
			set
			{
				if (_offset == value)
					_offset = value;
				NotifyChanged();
			}
		}

		/// <inheritdoc />
		public override void Measure()
		{
			base.Measure();
			_needsRearranged = true;
		}

		/// <inheritdoc />
		protected override void OnChildAdded(Instance instance)
		{
			var element = instance as GuiElement;
			if (element == null) return;
			lock (Locker)
			{
				_elements.Add(element);
			}
			element.Measured.Event += OnElementMeasured;
			_needsRearranged = true;
		}

		/// <inheritdoc />
		protected override void OnChildRemoved(Instance instance)
		{
			var element = instance as GuiElement;
			if (element == null) return;
			lock (Locker)
			{
				_elements.Remove(element);
			}
			element.Measured.Event -= OnElementMeasured;
			_needsRearranged = true;
		}

		private void OnElementMeasured()
		{
			_needsRearranged = true;
		}

		internal override void Draw()
		{
			GuiElement lastEl = null;
			if (_needsRearranged)
			{
				lock (Locker)
				{
					var count = _elements.Count;
					for (int i = 0; i < count; i++)
					{
						var el = _elements[i];

						var startP = AbsolutePosition;
						var startS = Vector2.Zero;

						if (lastEl != null)
						{
							startP = lastEl.AbsolutePosition;
							startS = lastEl.AbsoluteSize * _directionVec;
						}

						if (Reversed)
							el.AbsolutePosition = startP - _offset - startS;
						else
							el.AbsolutePosition = startP + _offset + startS;
						el.Arrange();

						el.Children.EnterReadLock();
						foreach (var child in el.Children)
						{
							var chel = child as GuiElement;
							chel?.Measure();
						}
						el.Children.ExitReadLock();

						lastEl = el;
					}
					_needsRearranged = false;
				}
			}

			base.Draw();
		}

		/// <summary>
		/// Comparer for <see cref="GuiElement.FrameIndex" />.
		/// </summary>
		internal class FrameIndexComparer : IComparer<GuiElement>
		{
			private readonly Stack _stack;

			internal FrameIndexComparer(Stack stack)
			{
				_stack = stack;
			}

			/// <summary>
			/// Compares two GuiElements by their <see cref="GuiElement.FrameIndex" />.
			/// </summary>
			public int Compare(GuiElement x, GuiElement y)
			{
				var reversed = _stack.Reversed;
				if (x.FrameIndex > y.FrameIndex)
					return reversed ? -1 : 1;
				if (x.FrameIndex < y.FrameIndex)
					return reversed ? 1 : -1;
				return x.Equals(y) ? 0 : 1;
			}
		}

		internal void Update(GuiElement element)
		{
			lock (Locker)
			{
				_elements.Remove(element);
				_elements.Add(element);
			}
		}
	}
}