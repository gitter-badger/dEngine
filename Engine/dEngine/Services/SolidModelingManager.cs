// SolidModelingManager.cs - dEngine
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using dEngine.Graphics;
using dEngine.Instances;
using dEngine.Utility;

// ReSharper disable All

namespace dEngine.Services
{
    /// <summary>
	/// Solid Modeling
	/// </summary>
	// Significant portions of this class were ported from https://github.com/Arakis/Net3dBool/
	// Code is licensed under The MIT License, Copyright (C) 2014 Sebastian Loncar
	internal static class SolidModelingManager
	{
		static SolidModelingManager()
		{
			Operations = new ConcurrentDictionary<UnionOperation, byte>();
		}

		public static MemoizingMRUCache<string, RenderObject> RenderBufferObjectCache { get; }

		/// <summary>
		/// A dictionary of <see cref="UnionOperation" /> for rendering.
		/// </summary>
		/// <remarks>
		/// <see cref="NegateOperation" /> and <see cref="IntersectOperation" /> do not need to be queued because they are added to
		/// <see cref="Renderer.TransparentParts" />
		/// </remarks>
		public static ConcurrentDictionary<UnionOperation, byte> Operations { get; private set; }

		public static PartOperation Fuse(Part a, Part b)
		{
			if (a == null || b == null) throw new ArgumentNullException();
			/*
			CsgOperationType opType;

			//PrepareOperation(a, b, out obj1, out obj2, out opType);
			
			switch (opType)
			{
				case CsgOperationType.Union:
					outputSolid = modeller.getUnion();
					break;
				case CsgOperationType.Negate:
					outputSolid = modeller.getDifference();
					break;
				case CsgOperationType.Intersection:
					outputSolid = modeller.getIntersection();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			var vertices3d = outputSolid.getVertices();
			var colours3d = outputSolid.getColors();
			var outputVertices = new Graphics.Vertex[vertices3d.Length];
			for (var i = 0; i < vertices3d.Length; i++)
			{
				var v3d = vertices3d[i];
				var c3d = colours3d[i];

				outputVertices[i] = new Graphics.Vertex
				{
					Position = new Vector3((float)v3d.x, (float)v3d.y, (float)v3d.z),
					Colour = new Colour((float)c3d.r, (float)c3d.g, (float)c3d.b),
				};
			}

			var indices = outputSolid.getIndices();
			var outputIndices = new int[indices.Length];
			for (var i = 0; i < indices.Length; i++)
				outputIndices[i] = indices[i];

			var operation = new UnionOperation();
			operation.SetCsgData(new RenderMesh(outputVertices, outputIndices), new CSGPair(a, b));
			operation.Size = new Vector3(1, 1, 1);
			operation.Parent = Game.Workspace;

			a.Destroy();
			b.Destroy();

			return operation;
			*/
			throw new NotImplementedException();
		}

		/*
		private static Tuple<FaceStatus, FaceStatus, FaceStatus> GetFaceStatusesFromOperationType(
			CsgOperationType opType)
		{
			switch (opType)
			{
				case CsgOperationType.Union:
					return new Tuple<FaceStatus, FaceStatus, FaceStatus>(FaceStatus.Outside, FaceStatus.Same,
						FaceStatus.Outside);
				case CsgOperationType.Negate:
					return new Tuple<FaceStatus, FaceStatus, FaceStatus>(FaceStatus.Outside, FaceStatus.Opposite,
						FaceStatus.Inside);
				case CsgOperationType.Intersection:
					return new Tuple<FaceStatus, FaceStatus, FaceStatus>(FaceStatus.Inside, FaceStatus.Same,
						FaceStatus.Inside);
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
		*/

		/*
		private static void PrepareOperation(Part a, Part b, out Solid lhs, out Solid rhs, out CsgOperationType opType)
		{
			lhs = null;
			rhs = null;
			opType = 0;

			if (a is NegateOperation || b is NegateOperation)
			{
				if (a is NegateOperation && b is NegateOperation) // two subtractions make a union
				{
					opType = CsgOperationType.Union;
				}
				else
				{
					opType = CsgOperationType.Negate;
					if (a is NegateOperation) // subtraction is always on right hand side.
					{
						lhs = Test(b);
						rhs = Test(a);
					}
					else
					{
						lhs = Test(a);
						rhs = Test(b);
					}
				}
			}
			else if (a is IntersectOperation || b is IntersectOperation)
			{
				if (a is IntersectOperation && b is IntersectOperation) // two intersections make a union
				{
					opType = CsgOperationType.Union;
				}
				else
				{
					opType = CsgOperationType.Intersection;
					if (a is IntersectOperation) // intersection is always on right hand side.
					{
						lhs = Test(b);
						rhs = Test(a);
					}
					else
					{
						lhs = Test(a);
						rhs = Test(b);
					}
				}
			}
			else // both a and b are either regular Parts or UnionOperations.
			{
				opType = CsgOperationType.Union;
				lhs = Test(b);
				rhs = Test(a);
			}
		}
	*/

		/// <summary>
		/// Seperates a fused object.
		/// </summary>
		internal static IEnumerable<Part> Seperate(PartOperation fused)
		{
			/*
			using (var stream = new MemoryStream(fused.OperationData))
			{
				var pair = dAsset.Deserialize<CSGPair>(stream, null);

				var a = pair.A;
				a.Parent = fused.Parent;

				var b = pair.B;
				if (b != null) b.Parent = fused.Parent;

				fused.Destroy();

				return new[] {a, b};
			}
			*/
			throw new NotImplementedException();
		}

		public static IEnumerable<PartOperation> Union(IEnumerable<Part> selection)
		{
			var unions = new List<PartOperation>();
			var items = selection as IList<Part> ?? selection.ToList();

			// ReSharper disable PossibleMultipleEnumeration
			for (var i = 0; i < items.Count; i++)
			{
				var a = items[i];

				if (a.IsDestroyed)
					continue; // a union operation will destroy both parts, so use IsDestroyed to ignore them.

				var wh = items.Where(b => b != a).ToList();
				foreach (var union in from b in wh where !b.IsDestroyed select Fuse(a, b))
				{
					items.Add(union);
					unions.Add(union);
				}
			}
			// ReSharper restore PossibleMultipleEnumeration

			return unions;
		}

		/// <summary>
		/// Transforms a <see cref="Part" /> into a <see cref="PartOperation" /> of the given type, or vice versa.
		/// </summary>
		/// <remarks>
		/// Should not be applied to a <see cref="UnionOperation" />.
		/// </remarks>
		public static Part MakeOp<TOperation>(Part part) where TOperation : PartOperation, new()
		{
			throw new NotImplementedException();
			/*
			var op = part as TOperation;
			Part result;

			if (op != null) // part is already negated
			{
				
				using (var stream = new MemoryStream(op.OperationData))
				{
					var pair = dAsset.Deserialize<CSGPair>(stream, null);
					result = pair.A;
					result.CFrame = result.CFrame.toWorldSpace(part.CFrame);
					result.Size = part.Size;
					result.Parent = part.Parent;
				}
			}
			else
			{
				op = new TOperation();
				op.SetCsgData(part.GetMesh(), new CSGPair(part, null));
				op.Size = part.Size;
				op.CFrame = part.CFrame;
				op.Parent = part.Parent;
				result = op;
			}

			part.Destroy();
			return result;
			*/
		}
	}
}