// Primitives.cs - dEngine
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
using dEngine.Data;
using dEngine.Services;
using dEngine.Utility.FileFormats.Model;

namespace dEngine.Graphics
{
	/// <summary>
	/// RenderBufferObject for primitives.
	/// </summary>
	internal static class Primitives
	{
		private static bool _primitivesLoaded;

		internal static Geometry CubeGeometry;
		internal static Geometry CylinderGeometry;
		internal static Geometry SphereGeometry;
		internal static Geometry WedgeGeometry;
		internal static Geometry ConeGeometry;
		internal static Geometry PlaneGeometry;

		/*
		static Primitives()
		{
			Load();
		}
		*/

		/// <summary>
		/// Loads primitive geometry.
		/// </summary>
		internal static void Load()
		{
			var primitiveStream = ContentProvider.DownloadStream("internal://content/meshes/primitives.fbx").Result;

            var primitives = FBX.Import(primitiveStream, new FBX.ImportSettings
			{
				MergeMeshes = false,
				NormalImportMethod = FBX.NormalImportMethod.ImportNormals,
			}, "fbx", "primitives");
            
            CubeGeometry = primitives.Meshes["Cube_Main"];
			ConeGeometry = primitives.Meshes["Cone_Main"];
			WedgeGeometry = primitives.Meshes["Wedge_Main"];
			SphereGeometry = primitives.Meshes["Sphere_Main"];
			CylinderGeometry = primitives.Meshes["Cylinder_Main"];
			PlaneGeometry = primitives.Meshes["Plane_Main"];

			_primitivesLoaded = true;
		}

		public static Geometry GeometryFromShape(Shape shape)
		{
			switch (shape)
			{
				case Shape.Cube:
					return CubeGeometry;
				case Shape.Sphere:
					return SphereGeometry;
				case Shape.Cylinder:
					return CylinderGeometry;
				case Shape.Wedge:
					return WedgeGeometry;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}