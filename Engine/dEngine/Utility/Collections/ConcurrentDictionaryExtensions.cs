// ConcurrentDictionaryExtensions.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Concurrent;

namespace dEngine.Utility
{
	internal static class ConcurrentDictionaryExtensions
	{
		/// <summary>
		/// Attempts to add the given key to a dictionary with a default value.
		/// </summary>
		public static bool TryAdd<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dict, TKey key)
		{
			return dict.TryAdd(key, default(TValue));
		}

		/// <summary>
		/// Attempts to remove an entry from a dictionary without getting the value.
		/// </summary>
		public static bool TryRemove<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dict, TKey key)
		{
			TValue _;
			return dict.TryRemove(key, out _);
		}
	}
}