// Faces.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System.Collections;
using System.IO;


#pragma warning disable 1591

namespace dEngine
{
	/// <summary>
	/// A struct of booleans for 6 sides.
	/// </summary>
	public struct Faces : IDataType
	{
		private BitArray _bitArray;

		[InstMember(1)]
		internal byte Bitfield
		{
			get
			{
				var bytes = new byte[1];
				_bitArray.CopyTo(bytes, 0);
				return bytes[0];
			}
			set { _bitArray = new BitArray(new[] {value}); }
		}

		public bool Right => _bitArray.Get(0);
		public bool Top => _bitArray.Get(1);
		public bool Back => _bitArray.Get(2);
		public bool Left => _bitArray.Get(3);
		public bool Bottom => _bitArray.Get(4);
		public bool Front => _bitArray.Get(5);

		public Faces(params NormalId[] normals)
		{
			_bitArray = new BitArray(6);

			var paramCount = normals.Length;
			for (int i = 0; i < paramCount; i++)
				_bitArray.Set((int)normals[i], true);
		}

		public static Faces @new(params NormalId[] normals)
		{
			return new Faces(normals);
		}

	    public void Load(BinaryReader reader)
	    {
	        Bitfield = reader.ReadByte();
	    }

	    public void Save(BinaryWriter writer)
	    {
            writer.Write(Bitfield);
	    }
	}
}