// CanvasWin32.cs - dEngine
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
using dEngine.Graphics;
using Neo.IronLua;

namespace dEngine.Instances
{
	/// <summary>
	/// Canvas for rendering to a form.
	/// </summary>
	internal class CanvasWin32 : Instance, IWorld
	{
		private readonly Camera _camera;

		public CanvasWin32(IntPtr handle)
		{
			_camera = new Camera {RenderHandle = handle, CFrame = new CFrame(0, 0, 10)};
			RenderObjectProvider = new WorldRenderer();
			Game.AddWorld(this);
		}

		public Camera CurrentCamera
		{
			get { return _camera; }
			set { }
		}

		public PhysicsSimulation Physics => null;
		public WorldRenderer RenderObjectProvider { get; }
		public bool IsRenderable => true;
	    public bool IsLoaded => true;

	    public LuaTuple<Part, Vector3, Vector3> FindPartOnRay(Ray ray, float maxLength = 1000)
		{
			throw new NotImplementedException();
		}

		public LuaTuple<Part, Vector3, Vector3> FindPartOnRay(Ray ray, LuaTable filterTable, float maxLength = 1000)
		{
			throw new NotImplementedException();
		}

		public LuaTuple<Part, Vector3, Vector3> FindPartOnRay(Ray ray, Func<dynamic, dynamic> filterFunc, float maxLength = 1000)
		{
			throw new NotImplementedException();
		}

		public void Dispose()
		{
			_camera.Destroy();
		}
	}
}