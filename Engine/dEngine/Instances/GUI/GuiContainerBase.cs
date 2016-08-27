// GuiContainerBase.cs - dEngine
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

namespace dEngine.Instances
{
	/// <summary>
	/// Base class for gui containers.
	/// </summary>
	[TypeId(37), ToolboxGroup("Containers")]
	public abstract class GuiContainerBase : Instance
	{
		/// <inheritdoc />
		protected GuiContainerBase()
		{
			Game.Workspace.CameraChanged.Event += OnWorkspaceCurrentCameraChanged;
		}

		private void OnWorkspaceCurrentCameraChanged(Camera camera)
		{
			foreach (var child in Children)
			{
                ICameraUser cameraUser;

                if ((cameraUser = child as ICameraUser) != null)
				{
                    cameraUser.Camera = camera;
				}
			}
		}

		/// <inheritdoc />
		protected override void OnChildAdded(Instance child)
		{
			base.OnChildAdded(child);

			var cameraUser = child as ICameraUser;
			if (cameraUser != null)
                cameraUser.Camera = Game.Workspace.CurrentCamera;
		}

		/// <inheritdoc />
		protected override void OnChildRemoved(Instance child)
		{
			base.OnChildRemoved(child);

            ICameraUser cameraUser;

            if ((cameraUser = child as ICameraUser) != null)
            {
                cameraUser.Camera = null;
            }
        }

		/// <inheritdoc />
		public override void Destroy()
		{
			base.Destroy();
			Game.Workspace.CameraChanged.Event -= OnWorkspaceCurrentCameraChanged;
		}
	}
}