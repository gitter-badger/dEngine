// CoreEnvironment.cs - dEngine
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
using dEngine.Instances;
using dEngine.Instances.Attributes;
using dEngine.Instances.Diagnostics;
using dEngine.Utility;
using Neo.IronLua;

#pragma warning disable 1591

namespace dEngine.Services
{
    /// <summary/>
	[TypeId(154), Uncreatable]
	public class CoreEnvironment : Instance, IWorld
	{
		internal CoreEnvironment()
		{
			CoreGui = new CoreGui {Parent = this, ParentLocked = true};
			//GeneralDebugGui = new GeneralDebugGui(CoreGui);

			Physics = null;
		}

		public static CoreGui CoreGui { get; private set; }

		public static GeneralDebugGui GeneralDebugGui { get; private set; }

		public Camera CurrentCamera
		{
			get { return Game.Workspace.CurrentCamera; }
			set { throw new NotSupportedException(); }
		}

		public PhysicsSimulation Physics { get; set; }

		public WorldRenderer RenderObjectProvider => Game.Workspace.RenderObjectProvider;

        public bool IsRenderable => false;
        public bool IsLoaded => true;

        /// <inheritdoc />
		public LuaTuple<Part, Vector3, Vector3> FindPartOnRay(Ray ray, float maxLength = 1000)
		{
			return Physics.FindPartOnRay(ray, maxLength).ToTuple();
		}

		/// <inheritdoc />
		public LuaTuple<Part, Vector3, Vector3> FindPartOnRay(Ray ray, LuaTable filterTable, float maxLength = 1000)
		{
			return Physics.FindPartOnRay(ray, filterTable.ToHashSet<Instance>(), maxLength).ToTuple();
		}

		/// <inheritdoc />
		public LuaTuple<Part, Vector3, Vector3> FindPartOnRay(Ray ray, Func<dynamic, dynamic> filterFunc, float maxLength = 1000)
		{
			return Physics.FindPartOnRay(ray, part => filterFunc.Invoke(part), maxLength).ToTuple();
		}
	}
}