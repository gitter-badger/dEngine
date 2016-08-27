// GuiBase3D.cs - dEngine
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
using dEngine.Instances.Interfaces;
using SharpDX.Direct3D11;

namespace dEngine.Instances
{
	/// <summary>
	/// Base class for 3D GUI objects.
	/// </summary>
	[TypeId(130), ToolboxGroup("3D GUI")]
	public abstract class GuiBase3D : GuiBase, ICameraUser
	{
		private Colour _colour;
		private float _transparency;
		private bool _visible;
	    private Camera _camera;

	    /// <inheritdoc />
		protected GuiBase3D()
		{
			_colour = Colour.Blue;
			_visible = true;
			_transparency = 0;
		}

		/// <summary>
		/// The colour of the gui.
		/// </summary>
		[InstMember(1), EditorVisible("Data")]
		public Colour Colour
		{
			get { return _colour; }
			set
			{
				if (value == _colour)
					return;

				_colour = value;
				NotifyChanged(nameof(Colour));
			}
		}

		/// <summary>
		/// The transparency of the gui.
		/// </summary>
		[InstMember(2), EditorVisible("Data")]
		public float Transparency
		{
			get { return _transparency; }
			set
			{
				if (value == _transparency)
					return;

				_transparency = value;
				NotifyChanged(nameof(Transparency));
			}
		}

		/// <summary>
		/// Determines if the gui object is visible.
		/// </summary>
		[InstMember(3), EditorVisible("Data")]
		public bool Visible
		{
			get { return _visible; }
			set
			{
				if (value == _visible)
					return;

				_visible = value;

				NotifyChanged(nameof(Visible));
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
                        camera.Gui3Ds.Remove(this);
                    }
                }

                _camera = value;

                if (value != null)
                {
                    lock (value.Locker)
                    {
                        value.Gui3Ds.Add(this);
                    }
                }
            }
        }

	    internal abstract void Draw(ref DeviceContext context, ref Camera camera);
	}
}