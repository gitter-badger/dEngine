// CSGOperation.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System.IO;
using dEngine.Data;
using dEngine.Instances.Attributes;


#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

namespace dEngine.Instances
{
	/// <summary>
	/// A base class for CSG operations.
	/// </summary>
	// TODO: redo CSG
	[TypeId(141), ToolboxGroup("Bricks")]
	public abstract class PartOperation : Part
	{
		private bool _applyMaterial;
		private CollisionFidelity _collisionFidelity;
		private Geometry _geometry;
		private StaticMesh _staticMesh;
		private MemoryStream _stream;

		/// <inheritdoc />
		protected PartOperation()
		{
			_stream = new MemoryStream();
			_applyMaterial = false;
			_collisionFidelity = CollisionFidelity.ConvexHull;
		}

		/// <summary>
		/// Determines whether <see cref="Part.Material" /> and <see cref="Part.BrickColour" /> properties are applied.
		/// </summary>
		public bool ApplyMaterial
		{
			get { return _applyMaterial; }
			set
			{
				if (value == _applyMaterial)
					return;

				_applyMaterial = value;
				NotifyChanged(nameof(ApplyMaterial));
			}
		}

		/// <summary>
		/// Determines the quality of the collision mesh for this operation.
		/// </summary>
		public CollisionFidelity CollisionFidelity
		{
			get { return _collisionFidelity; }
			set
			{
				if (value == _collisionFidelity)
					return;

				_collisionFidelity = value;
				NotifyChanged(nameof(CollisionFidelity));
			}
		}
        
		internal class CSGPair
		{
			[InstMember(1)] internal readonly Part A;
			[InstMember(2)] internal readonly Part B;

			internal CSGPair()
			{
			}

			internal CSGPair(Part a, Part b)
			{
				A = a;
				B = b;
			}
		}
	}
}