// InstanceRenderData.cs - dEngine
// Copyright (C) 2015-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System.Runtime.InteropServices;

using SharpDX;

#pragma warning disable 1591

namespace dEngine.Graphics.Structs
{
	/// <summary>
	/// Instance data structure for shaders.
	/// </summary>
	public struct InstanceRenderData
	{
		internal static int Stride = Marshal.SizeOf<InstanceRenderData>();

		/// <summary>
		/// The transparency of the instance.
		/// </summary>
		public float Transparency;

		/// <summary>
		/// The size of the instance.
		/// </summary>
		public Vector3 Size;

		/// <summary>
		/// The base colour.
		/// </summary>
		public Colour Colour;

		/// <summary>
		/// The shading model.
		/// </summary>
		public ShadingModel ShadingModel;

		/// <summary>
		/// The emissive multiplier.
		/// </summary>
		public float Emissive;

		/// <summary>
		/// Roughness
		/// </summary>
		public float Smoothness;

		/// <summary>
		/// Roughness
		/// </summary>
		public float Metallic;

		/// <summary>
		/// The model matrix.
		/// </summary>
		public Matrix ModelMatrix;
	}
}