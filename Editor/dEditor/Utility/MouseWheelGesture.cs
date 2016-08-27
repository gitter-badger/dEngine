// MouseWheelGesture.cs - dEditor
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System.Windows.Input;

namespace dEditor.Utility
{
	public class MouseWheelGesture : MouseGesture
	{
		public MouseWheelGesture() : base(MouseAction.WheelClick)
		{
		}

		public MouseWheelGesture(ModifierKeys modifiers) : base(MouseAction.WheelClick, modifiers)
		{
		}

		public WheelDirection Direction { get; set; }

		public static MouseWheelGesture Up
		{
			get { return new MouseWheelGesture {Direction = WheelDirection.Up}; }
		}

		public static MouseWheelGesture Down
		{
			get { return new MouseWheelGesture {Direction = WheelDirection.Down}; }
		}

		public static MouseWheelGesture CtrlUp
		{
			get { return new MouseWheelGesture(ModifierKeys.Control) {Direction = WheelDirection.Up}; }
		}

		public static MouseWheelGesture CtrlDown
		{
			get { return new MouseWheelGesture(ModifierKeys.Control) {Direction = WheelDirection.Down}; }
		}


		public enum WheelDirection
		{
			None,
			Up,
			Down
		}


		public override bool Matches(object targetElement, InputEventArgs inputEventArgs)
		{
			if (!base.Matches(targetElement, inputEventArgs)) return false;
			if (!(inputEventArgs is MouseWheelEventArgs)) return false;
			var args = (MouseWheelEventArgs)inputEventArgs;
			switch (Direction)
			{
				case WheelDirection.None:
					return args.Delta == 0;
				case WheelDirection.Up:
					return args.Delta > 0;
				case WheelDirection.Down:
					return args.Delta < 0;
				default:
					return false;
			}
		}
	}
}