// SelectablePanel.cs - dEditor
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
using System.Windows.Forms;

namespace dEditor.Widgets.Viewport
{
	internal class SelectablePanel : Panel
	{
		public SelectablePanel()
		{
			SetStyle(ControlStyles.Selectable, true);
			TabStop = true;
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			Focus();
			base.OnMouseDown(e);
		}

		protected override bool IsInputKey(Keys keyData)
		{
			if (keyData == Keys.Up || keyData == Keys.Down) return true;
			if (keyData == Keys.Left || keyData == Keys.Right) return true;
			return base.IsInputKey(keyData);
		}

		protected override void OnEnter(EventArgs e)
		{
			Invalidate();
			base.OnEnter(e);
		}

		protected override void OnLeave(EventArgs e)
		{
			Invalidate();
			base.OnLeave(e);
		}

		protected override void OnPaint(PaintEventArgs pe)
		{
			base.OnPaint(pe);
			if (Focused)
			{
				var rc = ClientRectangle;
				rc.Inflate(-2, -2);
				ControlPaint.DrawFocusRectangle(pe.Graphics, rc);
			}
		}
	}
}