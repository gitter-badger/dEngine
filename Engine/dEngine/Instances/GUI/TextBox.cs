// TextBox.cs - dEngine
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
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using dEngine.Graphics;
using dEngine.Instances.Attributes;
using dEngine.Services;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;

namespace dEngine.Instances
{
	/// <summary>
	/// A text box which allows typing.
	/// </summary>
	[TypeId(158)]
	public class TextBox : TextElement
	{
		private readonly Stopwatch _caretStopwatch;
		private bool _allowMultiline;
		private bool _allowSelection;
		private Brush _caretBrush;
		private SharpDX.Vector2 _caretEndPosition;
		private HitTestMetrics _caretHitResult;
		private int _caretIndex;

		private SharpDX.Vector2 _caretPosition;

		private bool _caretVisible;
		private bool _clearTextOnFocus;
		private bool _controlHeld;
		private bool _shiftHeld;

		/// <inheritdoc />
		public TextBox()
		{
			_caretStopwatch = Stopwatch.StartNew();

		    Focusable = true;

            GotFocus.Event += OnGotFocus;

			InputService.Service.InputBegan.Event += OnInputBegan;
			InputService.Service.InputEnded.Event += OnInputEnded;

			Renderer.InvokeResourceDependent(() => { _caretBrush = Renderer.Brushes.Get(Colour.Black); });
		}

		/// <summary>
		/// The index of the caret.
		/// </summary>
		[EditorVisible("Data")]
		public int CaretIndex
		{
			get { return _caretIndex; }
			set
			{
				value = Math.Max(0, Math.Min(_text.Length, value));

				if (value == _caretIndex) return;

				_caretStopwatch.Restart();
				_caretIndex = value;

				lock (this)
				{
					float x, y;
					_caretHitResult = _textLayout.HitTestTextPosition(_caretIndex, false, out x, out y);

					var startPos = new SharpDX.Vector2(x - .5f, y - .5f);
					var endPos = startPos + new SharpDX.Vector2(0, _caretHitResult.Height);

					_caretPosition = startPos;
					_caretEndPosition = endPos;
				}
			}
		}

		/// <summary>
		/// Determines if the current text should be cleared when the TextBox is focused.
		/// </summary>
		[EditorVisible("Data"), InstMember(1)]
		public bool ClearTextOnFocus
		{
			get { return _clearTextOnFocus; }
			set
			{
				if (value == _clearTextOnFocus) return;
				_clearTextOnFocus = value;
				NotifyChanged();
			}
		}

		/// <summary>
		/// Determines if new lines can be inputed.
		/// </summary>
		/// <remarks>
		/// This does not affect text manually set by the <see cref="TextElement.Text" /> property.
		/// </remarks>
		[EditorVisible("Data"), InstMember(2)]
		public bool AllowMultiLine
		{
			get { return _allowMultiline; }
			set
			{
				if (value == _allowMultiline) return;
				_allowMultiline = value;
				NotifyChanged();
			}
		}

		/// <summary>
		/// Determines if text can be selected by dragging.
		/// </summary>
		[EditorVisible("Data"), InstMember(3)]
		public bool AllowSelection
		{
			get { return _allowSelection; }
			set
			{
				if (value == _allowSelection) return;
				_allowSelection = value;

				if (!value)
				{
					Text = _text.Replace('\n', new char());
				}

				NotifyChanged();
			}
		}

		/// <inheritdoc />
		public override void Destroy()
		{
			base.Destroy();
			InputService.Service.InputBegan.Event -= OnInputBegan;
			InputService.Service.InputEnded.Event -= OnInputEnded;
		}

		[System.Security.SuppressUnmanagedCodeSecurity]
		[DllImport("user32.dll", SetLastError = true)]
		internal static extern uint MapVirtualKey(uint uCode, uint uMapType);

		private void OnInputBegan(InputObject input)
		{
			if (IsFocused())
			{
				var key = input.Key;

				var newText = "";

				RawBool isTrailingHit;
				RawBool isInside;
				HitTestMetrics hit;

				switch (key)
				{
					case Key.LeftShift:
					case Key.RightShift:
						_shiftHeld = true;
						return;
					case Key.LeftControl:
					case Key.RightControl:
						_controlHeld = true;
						return;
					case Key.Left:
						MoveCaret(-1);
						return;
					case Key.Right:
						MoveCaret(1);
						return;
					case Key.Up:
						hit = _textLayout.HitTestPoint(_caretHitResult.Left, _caretHitResult.Top - 1, out isTrailingHit,
							out isInside);
						CaretIndex = hit.TextPosition;
						return;
					case Key.Down:
						hit = _textLayout.HitTestPoint(_caretHitResult.Left,
							_caretHitResult.Top + _caretHitResult.Height + 1,
							out isTrailingHit, out isInside);
						CaretIndex = hit.TextPosition + 1;
						return;
					case Key.Backspace:
						if (_text.Length >= 1 && _caretIndex >= 1)
						{
							Text = _text.Remove(_caretIndex - 1, 1);
							CaretIndex--;
						}
						return;
					case Key.Enter:
						if (AllowMultiLine)
							newText += '\n';
						else
							Unfocus();
						break;
					case Key.Space:
						newText += ' ';
						break;
					default:
						if (key == Key.V && _controlHeld)
						{
							var pastedText = Clipboard.GetText();
							newText += pastedText;
							break;
						}

						char keyChar;
						var capsLockHeld = Control.IsKeyLocked(Keys.CapsLock);
						uint nonVirtualKey = MapVirtualKey((uint)key, 2);

						if ((_shiftHeld && !capsLockHeld) || (!_shiftHeld && capsLockHeld))
							keyChar = Convert.ToChar(nonVirtualKey);
						else if ((_shiftHeld && capsLockHeld) || (!_shiftHeld && !capsLockHeld))
							keyChar = char.ToLower(Convert.ToChar(nonVirtualKey));
						else
							return;

						newText += keyChar;
						break;
				}

				Text = _text.Insert(CaretIndex, newText);
				CaretIndex += newText.Length;
			}
		}

		/// <summary>
		/// Moves caret N spaces to right.
		/// </summary>
		private void MoveCaret(int n)
		{
			CaretIndex += 1 * n;
			_caretVisible = true;
		}

		private void OnInputEnded(InputObject input)
		{
			switch (input.Key)
			{
				case Key.LeftShift:
				case Key.RightShift:
					_shiftHeld = false;
					break;
				case Key.LeftControl:
				case Key.RightControl:
					_controlHeld = false;
					break;
			}
		}

		private void OnGotFocus()
		{
			if (_clearTextOnFocus)
				Text = "";
			else
			{
				lock (this)
				{
					if (_textLayout == null)
						return;
					RawBool isTrailingHit;
					RawBool isInside;
					var hit = _textLayout.HitTestPoint(InputService.CursorX, InputService.CursorY, out isTrailingHit, out isInside);
					CaretIndex = hit.TextPosition;
					if (isTrailingHit)
						CaretIndex++;
				}
			}
		}

		internal override void Draw()
		{
			var collector = Container;
			var target = Renderer.Context2D;

			base.Draw();

			if (Focused) // draw caret
			{
				if (_caretStopwatch.Elapsed.TotalMilliseconds >= SystemInformation.CaretBlinkTime)
				{
					_caretVisible = !_caretVisible;
					_caretStopwatch.Restart();
				}

				if (!_caretVisible) return;

				var startPos = _caretPosition + new SharpDX.Vector2(-AbsoluteSize.x / 2, -AbsoluteSize.y / 2);
				var endPos = _caretEndPosition + new SharpDX.Vector2(-AbsoluteSize.x / 2, -AbsoluteSize.y / 2);

				target.DrawLine(startPos, endPos, _caretBrush);
			}
		}
	}
}