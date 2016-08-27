// CoreGui.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using dEngine.Instances.Attributes;
using dEngine.Services;
using dEngine.Settings.Global;

#pragma warning disable 1591

namespace dEngine.Instances
{
	/// <summary>
	/// A gui container that always renders to <see cref="Workspace.CurrentCamera" /> and is hidden from the Explorer view.
	/// </summary>
	[TypeId(155), LevelEditorRelated, Uncreatable]
	public class CoreGui : GuiContainerBase
	{
		public TextLabel FPSLabel;

		internal CoreGui()
		{
			Console = new ConsoleElement(this);

			FPSLabel = new TextLabel
			{
				Parent = this,
				BackgroundColour = Colour.Transparent,
				Position = new UDim2(0, -4, 0, 4),
				Size = new UDim2(1, 0, 1, 0),
				TextAlignmentX = AlignmentX.Right,
				TextAlignmentY = AlignmentY.Top
			};
		}

		internal ConsoleElement Console { get; private set; }

		internal class ConsoleElement
		{
			private bool _consoleFull;
			private TextLabel[] _lines;
			private int _linesWritten;
			internal Frame ContainerFrame;
			internal Frame RootFrame;

			internal TextBox TextBox;

			internal ConsoleElement(CoreGui coreGui)
			{
				Build(coreGui);
				RootFrame.Visible = false;

				InputService.Service.InputBegan.Event += input =>
				{
					if (input.InputType == InputType.Keyboard)
					{
						if (input.Key == DebugSettings.ToggleConsoleKey)
						{
							var visible = !RootFrame.Visible;
							RootFrame.Visible = visible;

							if (visible)
								TextBox.Focus();
							else
								TextBox.Unfocus();
						}
					}
				};
			}

			internal bool Enabled
			{
				get { return RootFrame.Visible; }
				set { RootFrame.Visible = value; }
			}

			private void Build(CoreGui coreGui)
			{
				RootFrame = new Frame
				{
					Name = "ConsoleRoot",
					Parent = coreGui,
					Position = new UDim2(0, 30, 0, 30),
					Size = new UDim2(.5f, 0, 0, 120),
					BackgroundTransparency = 1
				};

				var title = new TextLabel
				{
					Name = "TitleLabel",
					Parent = RootFrame,
					Size = new UDim2(0, 150, 0, 18),
					BackgroundColour = Colour.White,
					BorderThickness = 0,
					TextColour = Colour.Black,
					Text = " Console",
					FontSize = 15,
					TextAlignmentX = AlignmentX.Left,
					TextAlignmentY = AlignmentY.Middle
				};

				ContainerFrame = new Frame
				{
					Name = "Container",
					Parent = RootFrame,
					Position = new UDim2(0, 0, 0, 18),
					Size = new UDim2(1, 0, 1, -18),
					BackgroundColour = new Colour(.5f, .5f, .5f, .5f),
					BorderThickness = 0
				};

				_lines = new TextLabel[6];

				for (int i = 0; i < _lines.Length; i++)
				{
					_lines[i] = new TextLabel
					{
						Parent = ContainerFrame,
						BackgroundColour = Colour.Transparent,
						Size = new UDim2(1, 0, 0, 14),
						Position = new UDim2(0, 0, 0, 14 * i),
						FontSize = 14,
						TextAlignmentX = AlignmentX.Left,
						Text = ""
					};
				}

				TextBox = new TextBox
				{
					Parent = ContainerFrame,
					Position = new UDim2(0, 0, 1, -14),
					Size = new UDim2(1, 0, 0, 14)
				};
			}

			public void WriteLine(string text)
			{
				if (_consoleFull)
				{
					for (int i = 0; i <= _lines.Length - 2; i++)
					{
						_lines[i].Text = _lines[i + 1].Text;
					}
				}

				_lines[_linesWritten].Text = text;

				if (_linesWritten == _lines.Length - 1)
					_consoleFull = true;
				else
					_linesWritten++;
			}
		}
	}
}