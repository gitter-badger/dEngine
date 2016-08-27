// NeoLuaUtility.cs - dEngine
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
using System.Collections.Generic;
using System.Linq;
using Neo.IronLua;

namespace dEngine.Utility
{
	/// <summary>
	/// Utility methods related to NeoLua.
	/// </summary>
	public static class NeoLuaUtility
	{
		/// <summary>
		/// Converts a <see cref="LuaTable" /> to a <see cref="HashSet{T}" />
		/// </summary>
		public static HashSet<T> ToHashSet<T>(this LuaTable result)
		{
			return new HashSet<T>(result.Values.Cast<T>());
		}

		/// <summary>
		/// Packs the dictionary into a <see cref="LuaTable" />
		/// </summary>
		public static LuaTable ToLuaTable<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
		{
			var table = new LuaTable();
			foreach (var kv in dictionary)
			{
				table[kv.Key] = kv.Value;
			}
			return table;
		}

		/// <summary>
		/// Packs the enumerable into a <see cref="LuaTable" />
		/// </summary>
		public static LuaTable ToLuaTable<T>(this IEnumerable<T> values)
		{
			var table = new LuaTable();
			int i = 1;
			foreach (var v in values)
			{
				table[i] = v;
				i++;
			}
			return table;
		}

		/// <summary>
		/// Packs the enumerable into a <see cref="LuaTable" />.
		/// </summary>
		public static LuaTable ToLuaTable<TSource, TResult>(this IEnumerable<TSource> values,
			Func<TSource, TResult> func)
		{
			var table = new LuaTable();
			int i = 1;
			foreach (var v in values)
			{
				table[i] = func(v);
				i++;
			}
			return table;
		}
	}
}