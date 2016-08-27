// IRenderable.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using dEngine.Graphics;
using dEngine.Graphics.Structs;

namespace dEngine.Instances
{
	/// <summary>
	/// An instance which can be rendered.
	/// </summary>
	public interface IRenderable
	{
		/// <summary>
		/// The <see cref="dEngine.Graphics.RenderObject" /> this instance is a part of. This property is set by the
		/// <see cref="dEngine.Graphics.RenderObject.Add" /> method.
		/// </summary>
		RenderObject RenderObject { get; set; }

		/// <summary>
		/// The index of this instance in the RenderObject's instance array. This property is set by the
		/// <see cref="dEngine.Graphics.RenderObject.Add" /> method.
		/// </summary>
		int RenderIndex { get; set; }

		/// <summary>
		/// The render data.
		/// </summary>
		InstanceRenderData RenderData { get; set; }

		/// <summary>
		/// Update the <see cref="RenderData" /> struct and write the data to the <see cref="RenderObject" />'s CPU buffer.
		/// </summary>
		void UpdateRenderData();
	}
}