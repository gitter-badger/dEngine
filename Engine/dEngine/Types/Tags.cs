// Tags.cs - dEngine
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using dEngine.Utility;
using Neo.IronLua;


namespace dEngine
{
	/// <summary>
	/// A list of tags.
	/// </summary>
	public class Tags : IDataType, IEnumerable<string>
	{
		private readonly ConcurrentDictionary<string, byte> _tags;

		/// <summary />
		public Tags(params string[] tags)
		{
			_tags = new ConcurrentDictionary<string, byte>();
			foreach (var tag in tags)
			{
				_tags[tag] = default(byte);
			}
		}

		/// <summary />
		public IEnumerator<string> GetEnumerator()
		{
			return _tags.Keys.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <summary />
		public static Tags @new(params string[] tags)
		{
			return new Tags(tags);
		}

		/// <summary>
		/// Adds a tag.
		/// </summary>
		public void Add(string tag)
		{
			_tags.TryAdd(tag);
		}

		/// <summary>
		/// Removes a tag.
		/// </summary>
		public void Remove(string tag)
		{
			_tags.TryRemove(tag);
		}

		/// <summary>
		/// Determines if the tags include the given tag.
		/// </summary>
		public bool HasTag(string tag)
		{
			return _tags.ContainsKey(tag);
		}

		/// <summary>
		/// Returns an array of the tags.
		/// </summary>
		public string[] ToArray()
		{
			return _tags.Keys.ToArray();
		}

		/// <summary>
		/// Returns the tags in a table.
		/// </summary>
		public LuaTable ToTable()
		{
			var table = new LuaTable();

			int i = 1;
			foreach (var tag in _tags)
				table[i++] = tag;

			return table;
		}

        /// <summary/>
	    public void Load(BinaryReader reader)
	    {
	        var tagCount = reader.ReadInt32();
	        for (int i = 0; i < tagCount; i++)
	        {
	            _tags[reader.ReadString()] = 0;
	        }
	    }

        /// <summary/>
        public void Save(BinaryWriter writer)
	    {
	        var tagCount = _tags.Count;
	        foreach (var kv in _tags)
	        {
	            writer.Write(kv.Key);
	        }
	    }
	}
}