// Terrain.cs - dEngine
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
using System.Diagnostics;
using System.Threading.Tasks;
using BulletSharp;
using dEngine.Graphics;
using dEngine.Graphics.Structs;
using dEngine.Instances.Attributes;
using dEngine.Services;
using dEngine.Utility;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;
using MapFlags = SharpDX.Direct3D11.MapFlags;

// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable UnusedMember.Local

namespace dEngine.Instances
{
	/// <summary>
	/// An object representing the voxel-based terrain system.
	/// </summary>
	[TypeId(24), Uncreatable, ExplorerOrder(0), ToolboxGroup("Bricks")]
	public class Terrain : Part, ISingleton
	{
		/// <summary>
		/// The maximum number of chunks per axis.
		/// </summary>
		public const int MaxChunks = 1024;

		/// <summary>
		/// The width, height and length of a chunk.
		/// </summary>
		public const short ChunkSize = 64;

		/// <summary>
		/// The number of cells per chunk.
		/// </summary>
		public const int CellsPerChunk = ChunkSize * ChunkSize * ChunkSize;

		private static GfxShader _terrainShader;
		private static GfxShader.Pass _terrainPass;
		internal static Terrain Singleton;
		private readonly ConcurrentDictionary<Vector3int16, Chunk> _chunks;
		private readonly ISurfaceExtractor _surfaceExtractor;

		internal Terrain(Workspace workspace)
		{
			_chunks = new ConcurrentDictionary<Vector3int16, Chunk>();
			_surfaceExtractor = new TransvoxelExtractor(this);

			Parent = workspace;
			ParentLocked = true;

			Singleton = this;

			if (!UnitTestDetector.IsInUnitTest)
			{
				Debug.Assert(Parent != null, "Parent != null");
				Debug.Assert(Parent == workspace, "Parent == workspace");
			}

			/*
			var size = 128;
			for (int i = 0; i <= size; i++)
			{
				for (int j = 0; j <= size; j++)
				{
					for (int k = 0; k <= size; k++)
					{
						double div = 64.0;
						double val = (SimplexNoise.Noise((i) / div, (j) / div, (k) / div)) * 128.0;

						//val = -100;

						if (i == 1 && j == 0 && k == 33)
						{
							
						}

						SetCell(i, j, k, CellMaterial.Grass, (sbyte)val);
					}
				}
			}
			*/
			//FillRegion(new Region3int16(0, 0, 0, 6, 6, 6), CellMaterial.Grass);
			//SetCell(0, 0, 0, CellMaterial.Grass);
		}

		/// <summary>
		/// Displays the boundaries of the largest possible region.
		/// </summary>
		[EditorVisible("Data")]
		public Region3int16 MaxExtents =>
			new Region3int16(new Vector3int16(short.MinValue, short.MinValue, short.MinValue),
				new Vector3int16(short.MaxValue, short.MaxValue, short.MaxValue));

		internal Cell this[Vector3int16 pos]
		{
			get { return GetCell(pos); }
			set { SetCell(pos, value.Material, value.Density); }
		}

		private int fastFloor(double x)
		{
			return x >= 0 ? (int)x : (int)x - 1;
		}

		internal static object GetExisting()
		{
			return Game.Workspace.Terrain;
		}

		private int RoundDownDivide(int a, int b)
		{
			if (a >= 0) return a / b;
			return (a - b + 1) / b;
		}

		/// <summary>
		/// Returns the cell at the given cell coordinates.
		/// </summary>
		public Cell GetCell(int x, int y, int z)
		{
			Chunk chunk;
			var coord = new Vector3int16(RoundDownDivide(x, ChunkSize), RoundDownDivide(y, ChunkSize),
				RoundDownDivide(z, ChunkSize));
			_chunks.TryGetValue(coord, out chunk);

			if (chunk == null)
				return default(Cell);

			var cell = chunk[
				Math.Abs(x) % ChunkSize,
				Math.Abs(y) % ChunkSize,
				Math.Abs(z) % ChunkSize];
			return cell;
		}

		internal Cell GetCell(Vector3int16 cellPos)
		{
			return GetCell(cellPos.x, cellPos.y, cellPos.z);
		}

		/// <summary>
		/// Sets a cell at the given cell coordinates.
		/// </summary>
		public void SetCell(int x, int y, int z, CellMaterial material, sbyte density)
		{
			Chunk chunk;
			var coord = new Vector3int16(RoundDownDivide(x, ChunkSize), RoundDownDivide(y, ChunkSize),
				RoundDownDivide(z, ChunkSize));

			lock (Locker)
			{
				if (!_chunks.TryGetValue(coord, out chunk))
				{
					chunk = new Chunk(this, coord);
					_chunks[coord] = chunk;
				}
			}

			chunk[
				Math.Abs(x % ChunkSize),
				Math.Abs(y % ChunkSize),
				Math.Abs(z % ChunkSize)] = new Cell(material, density);

			if (chunk.OccupiedCells == 0)
			{
				_chunks.TryRemove(coord);
			}
		}

		private void SetCell(Vector3int16 cellPos, CellMaterial material, sbyte density)
		{
			SetCell(cellPos.x, cellPos.y, cellPos.z, material, density);
		}

		/// <summary>
		/// Returns the world position of the center of the terrain cell at x, y, z.
		/// </summary>
		/// <param name="x">The X position in cell coords.</param>
		/// <param name="y">The Y position in cell coords.</param>
		/// <param name="z">The Z position in cell coords.</param>
		public Vector3 CellCenterToWorld(int x, int y, int z)
		{
			return new Vector3(2 + x * 4, 2 + y * 4, 2 + z * 4);
		}

		/// <summary>
		/// Returns the world position of the lower-left-forward corner of the terrain cell at x, y, z.
		/// </summary>
		/// <param name="x">The X position in cell coords.</param>
		/// <param name="y">The Y position in cell coords.</param>
		/// <param name="z">The Z position in cell coords.</param>
		public Vector3 CellCornerToWorld(int x, int y, int z)
		{
			return CellCenterToWorld(x, y, z) - new Vector3(4, 4, 4);
		}

		/// <summary>
		/// Returns the cell coordinate for the given world position.
		/// </summary>
		/// <param name="position"></param>
		/// <returns></returns>
		public Vector3int16 WorldToCell(Vector3 position)
		{
			return new Vector3int16((short)Math.Floor(position.x / 4), (short)Math.Floor(position.y / 4),
				(short)Math.Floor(position.z / 4));
		}

		/// <summary>
		/// Replaces all voxels with empty space.
		/// </summary>
		public void Clear()
		{
			Parallel.ForEach(_chunks.Values, chunk =>
			{
				chunk.ClearChunk();
				if (chunk.OccupiedCells == 0)
					_chunks.TryRemove(chunk.Position);
			});
		}

		/// <summary>
		/// Returns the number of non-empty cells.
		/// </summary>
		public int CountCells()
		{
			return 0;
		}

		/// <summary>
		/// Fills a region of terrain.
		/// </summary>
		/// <param name="region">The region to fill.</param>
		/// <param name="material">The material for each cell.</param>
		/// <param name="density">The density for each cell.</param>
		public void FillRegion(Region3int16 region, CellMaterial material, sbyte density)
		{
			Parallel.For(region.Min.x, region.Max.x, x =>
			{
				for (short y = region.Min.y; y < region.Max.y; y++)
				{
					for (short z = region.Min.z; z < region.Max.z; z++)
					{
						SetCell(x, y, z, material, density);
					}
				}
			});
		}

		internal void Draw(ref DeviceContext context)
		{
			PixHelper.BeginEvent(Color.Zero, "Terrain");
			if (_terrainPass == null)
			{
				_terrainShader = Shaders.Get("Terrain");
				_terrainPass = _terrainShader.GetPass("Main");
			}

			lock (Renderer.Context)
			{
				_terrainPass.Use(ref context);

				foreach (var chunk in _chunks)
				{
					chunk.Value.Draw(ref context);
				}
			}
			PixHelper.EndEvent();
		}

		#region Nested Types

		/// <summary>
		/// A terrain chunk.
		/// </summary>
		public class Chunk
		{
			internal readonly Cell[] _cells;
			private readonly Terrain _terrain;
			private Buffer _constantBuffer;
			private bool _geometryBuilding;
			private bool _geometryDirty;
			private Buffer _indexBuffer;
			private int _indexCount;
			private Buffer _vertexBuffer;
			private VertexBufferBinding _vertexBufferBinding;

			internal Chunk(Terrain terrain, Vector3int16 coord)
			{
				_terrain = terrain;
				Position = coord;
				_cells = new Cell[CellsPerChunk];
			}

			internal Vector3int16 Position { get; }
			internal TriangleMesh PhysicsMesh { get; private set; }
			internal int SizeInBytes => _vertexBuffer.Description.SizeInBytes + _indexBuffer.Description.SizeInBytes;

			internal Chunk NeighbourX
			{
				get
				{
					Chunk chunk;
					_terrain._chunks.TryGetValue(new Vector3int16(Position.x + 1, Position.y, Position.z), out chunk);
					return chunk;
				}
			}

			internal Chunk NeighbourNegX
			{
				get
				{
					Chunk chunk;
					_terrain._chunks.TryGetValue(new Vector3int16(Position.x - 1, Position.y, Position.z), out chunk);
					return chunk;
				}
			}

			internal Chunk NeighbourY
			{
				get
				{
					Chunk chunk;
					_terrain._chunks.TryGetValue(new Vector3int16(Position.x, Position.y + 1, Position.z), out chunk);
					return chunk;
				}
			}

			internal Chunk NeighbourNegY
			{
				get
				{
					Chunk chunk;
					_terrain._chunks.TryGetValue(new Vector3int16(Position.x, Position.y - 1, Position.z), out chunk);
					return chunk;
				}
			}

			internal Chunk NeighbourZ
			{
				get
				{
					Chunk chunk;
					_terrain._chunks.TryGetValue(new Vector3int16(Position.x, Position.y, Position.z + 1), out chunk);
					return chunk;
				}
			}

			internal Chunk NeighbourNegZ
			{
				get
				{
					Chunk chunk;
					_terrain._chunks.TryGetValue(new Vector3int16(Position.x, Position.y, Position.z - 1), out chunk);
					return chunk;
				}
			}

			/// <summary>
			/// The number of non-empty cells in the chunk.
			/// </summary>
			public int OccupiedCells { get; private set; }

			internal Cell this[int x, int y, int z]
			{
				get { return _cells[x + y * ChunkSize + z * ChunkSize * ChunkSize]; }

				set
				{
					var i = x + y * ChunkSize + z * ChunkSize * ChunkSize;
					var lastDens = _cells[i].Density;
					_cells[i] = value;
					if (lastDens != value.Density)
					{
						if (value.Density == 0)
							OccupiedCells--;
						else
							OccupiedCells++;
					}

					_geometryDirty = true;
				}
			}

			internal Cell this[Vector3int16 index]
			{
				get { return this[index.x, index.y, index.z]; }
				set { this[index.x, index.y, index.z] = value; }
			}

			private void BuildGeometry()
			{
				lock (_terrain.Locker)
				{
					_vertexBuffer?.Dispose();
					_indexBuffer?.Dispose();
				}

				TerrainVertex[] vertices;
				ushort[] indices;

				_terrain._surfaceExtractor.GenLodCell(this, 1, out vertices, out indices);


				if (vertices.Length == 0)
				{
					_geometryDirty = false;
					return;
				}

				lock (_terrain.Locker)
				{
					_vertexBuffer = Buffer.Create(Renderer.Device, BindFlags.VertexBuffer, vertices);
					_indexBuffer = Buffer.Create(Renderer.Device, BindFlags.IndexBuffer, indices);
					_vertexBufferBinding = new VertexBufferBinding(_vertexBuffer, TerrainVertex.Stride, 0);
				}

				_indexCount = indices.Length;
				_geometryDirty = false;
			}

			internal void Draw(ref DeviceContext context)
			{
				if (_geometryDirty)
					BuildGeometry();
				/*
				if (_geometryDirty && !_geometryBuilding)
				{
					_geometryBuilding = true;
					Task.Run(() =>
					{
						BuildGeometry();
						_geometryBuilding = false;
						_geometryDirty = false;
					});
				}
				*/

				if (_indexCount == 0)
					return;

				if (_constantBuffer == null)
				{
					_constantBuffer = new Buffer(Renderer.Device, 64, ResourceUsage.Dynamic, BindFlags.ConstantBuffer,
						CpuAccessFlags.Write, 0, 0);

					DataStream stream;
					context.MapSubresource(_constantBuffer, MapMode.WriteDiscard, MapFlags.None, out stream);
					stream.Write(Matrix.Identity * Matrix.Translation(Position.x, Position.y, Position.z));
					context.UnmapSubresource(_constantBuffer, 0);
				}

				lock (_terrain.Locker)
				{
					context.VertexShader.SetConstantBuffer(2, _constantBuffer);
					context.InputAssembler.SetVertexBuffers(0, _vertexBufferBinding);
					context.InputAssembler.SetIndexBuffer(_indexBuffer, Format.R16_UInt, 0);
					context.DrawIndexed(_indexCount, 0, 0);
				}
			}

			/// <inheritdoc />
			public override string ToString()
			{
				return $"Chunk ({Position})";
			}

			/// <summary>
			/// Sets all cells in the chunk to air.
			/// </summary>
			public void ClearChunk()
			{
				for (int i = 0; i < CellsPerChunk; i++)
					_cells[i] = new Cell(CellMaterial.Air, 0);
				OccupiedCells = 0;
			}
		}

		public struct Cell
		{
			public readonly sbyte Density;
			public readonly CellMaterial Material;

			public Cell(CellMaterial material, sbyte density)
			{
				Material = material;

				if (material == CellMaterial.Air)
					density = 0;

				Density = density;
			}

			/// <inheritdoc />
			public override string ToString()
			{
				return $"Cell ({Material}, {Density})";
			}
		}

		#endregion
	}
}