// IWorld.cs - dEngine
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
	/// Interface for worlds/environments.
	/// </summary>
	public interface IWorld
	{
		/// <summary>
		/// The <see cref="Camera" /> that renders this world.
		/// </summary>
		Camera CurrentCamera { get; set; }

		/// <summary>
		/// The <see cref="PhysicsSimulation" />.
		/// </summary>
		PhysicsSimulation Physics { get; }

		/// <summary>
		/// The <see cref="dEngine.Graphics.WorldRenderer" /> for this world.
		/// </summary>
		WorldRenderer RenderObjectProvider { get; }

		/// <summary>
		/// Determines if this world supports rendering. (Requires a <see cref="RenderObjectProvider" />)
		/// </summary>
		bool IsRenderable { get; }

        /// <summary>
        /// Determines if the world is loaded.
        /// </summary>
	    bool IsLoaded { get; }

        /// <summary>
        /// Performs a raycast for the given ray.
        /// </summary>
        LuaTuple<Part, Vector3, Vector3> FindPartOnRay(Ray ray, float maxLength = 1000);

        /// <summary>
        /// Performs a raycast for the given ray, filtering out blacklisted results.
        /// </summary>
        LuaTuple<Part, Vector3, Vector3> FindPartOnRay(Ray ray, LuaTable filterTable, float maxLength = 1000);

        /// <summary>
        /// Performs a raycast for the given ray, filtering based on a predicate.
        /// </summary>
        LuaTuple<Part, Vector3, Vector3> FindPartOnRay(Ray ray, Func<dynamic, dynamic> filterFunc, float maxLength = 1000);
	}
}