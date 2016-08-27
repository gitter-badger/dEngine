// Cache.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System.Diagnostics;

namespace dEngine.Instances
{
	internal class ReuseCell
	{
		public readonly int[] Verts;

		public ReuseCell(int size)
		{
			Verts = new int[size];

			for (int i = 0; i < size; i++)
				Verts[i] = -1;
		}
	}

	internal class RegularCellCache
	{
		private readonly ReuseCell[][] _cache;
		private readonly int chunkSize;

		public RegularCellCache(int chunksize)
		{
			this.chunkSize = chunksize;
			_cache = new ReuseCell[2][];

			_cache[0] = new ReuseCell[chunkSize * chunkSize];
			_cache[1] = new ReuseCell[chunkSize * chunkSize];

			for (int i = 0; i < chunkSize * chunkSize; i++)
			{
				_cache[0][i] = new ReuseCell(4);
				_cache[1][i] = new ReuseCell(4);
			}
		}


		public ReuseCell this[int x, int y, int z]
		{
			set
			{
				Debug.Assert(x >= 0 && y >= 0 && z >= 0);
				_cache[x & 1][y * chunkSize + z] = value;
			}
		}

		public ReuseCell this[Vector3int16 v]
		{
			set { this[v.x, v.y, v.z] = value; }
		}

		public ReuseCell GetReusedIndex(Vector3int16 pos, byte rDir)
		{
			int rx = rDir & 0x01;
			int rz = (rDir >> 1) & 0x01;
			int ry = (rDir >> 2) & 0x01;

			int dx = pos.x - rx;
			int dy = pos.y - ry;
			int dz = pos.z - rz;

			Debug.Assert(dx >= 0 && dy >= 0 && dz >= 0);
			return _cache[dx & 1][dy * chunkSize + dz];
		}


		internal void SetReusableIndex(Vector3int16 pos, byte reuseIndex, ushort p)
		{
			_cache[pos.x & 1][pos.y * chunkSize + pos.z].Verts[reuseIndex] = p;
		}
	}

	internal class TransitionCache
	{
		private readonly ReuseCell[] _cache;

		public TransitionCache()
		{
			const int cacheSize = 0; // 2 * TransvoxelExtractor.BlockWidth * TransvoxelExtractor.BlockWidth;
			_cache = new ReuseCell[cacheSize];

			for (int i = 0; i < cacheSize; i++)
			{
				_cache[i] = new ReuseCell(12);
			}
		}

		public ReuseCell this[int x, int y]
		{
			get { return null; //_cache[x + (y & 1) * TransvoxelExtractor.BlockWidth];
			}
			set
			{
				//_cache[x + (y & 1) * TransvoxelExtractor.BlockWidth] = value;
			}
		}
	}
}