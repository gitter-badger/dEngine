// MemorizingMRUCache.cs - dEngine
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
using System.Diagnostics.Contracts;
using System.Linq;

namespace dEngine.Utility
{
	/// <summary>
	/// This data structure is a representation of a memoizing cache - i.e. a
	/// class that will evaluate a function, but keep a cache of recently
	/// evaluated parameters.
	/// Since this is a memoizing cache, it is important that this function be a
	/// "pure" function in the mathematical sense - that a key *always* maps to
	/// a corresponding return value.
	/// </summary>
	/// <typeparam name="TParam">The type of the parameter to the calculation function.</typeparam>
	/// <typeparam name="TVal">
	/// The type of the value returned by the calculation
	/// function.
	/// </typeparam>
	// https://github.com/paulcbetts/splat/blob/master/Splat/MemoizingMRUCache.cs
	// Modified by Dan: Get() no longer caches null values, optimized ContainsKey with TryGetValue.
	public class MemoizingMRUCache<TParam, TVal>
	{
		private readonly Func<TParam, object, TVal> calculationFunction;
		private readonly int maxCacheSize;
		private readonly Action<TVal> releaseFunction;
		private Dictionary<TParam, Tuple<LinkedListNode<TParam>, TVal>> cacheEntries;

		private LinkedList<TParam> cacheMRUList;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="calculationFunc">
		/// The function whose results you want to cache,
		/// which is provided the key value, and an Tag object that is
		/// user-defined
		/// </param>
		/// <param name="maxSize">
		/// The size of the cache to maintain, after which old
		/// items will start to be thrown out.
		/// </param>
		/// <param name="onRelease">
		/// A function to call when a result gets
		/// evicted from the cache (i.e. because Invalidate was called or the
		/// cache is full)
		/// </param>
		public MemoizingMRUCache(Func<TParam, object, TVal> calculationFunc, int maxSize, Action<TVal> onRelease = null)
		{
			Contract.Requires(calculationFunc != null);
			Contract.Requires(maxSize > 0);

			calculationFunction = calculationFunc;
			releaseFunction = onRelease;
			maxCacheSize = maxSize;
			InvalidateAll();
		}

		/// <summary>
		/// Evaluates the function provided, returning the cached value if possible
		/// </summary>
		/// <param name="key">The value to pass to the calculation function.</param>
		/// <param name="context">An additional optional user-specific parameter.</param>
		/// <returns></returns>
		public TVal Get(TParam key, object context = null)
		{
			Tuple<LinkedListNode<TParam>, TVal> found;
			if (cacheEntries.TryGetValue(key, out found))
			{
				//cacheMRUList.Remove(found.Item1);
				//cacheMRUList.AddFirst(found.Item1);
				return found.Item2;
			}

			var result = calculationFunction(key, context);

			if (result == null)
				// ReSharper disable once ExpressionIsAlwaysNull
				return result;

			var node = new LinkedListNode<TParam>(key);
			cacheMRUList.AddFirst(node);
			cacheEntries[key] = new Tuple<LinkedListNode<TParam>, TVal>(node, result);
			maintainCache();

			return result;
		}

		/// <summary>
		/// Tries to get the value for the key, but does not call the calculation function.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		public bool TryGet(TParam key, out TVal result)
		{
			Contract.Requires(key != null);

			Tuple<LinkedListNode<TParam>, TVal> output;
			var ret = cacheEntries.TryGetValue(key, out output);
			if (ret && output != null)
			{
				cacheMRUList.Remove(output.Item1);
				cacheMRUList.AddFirst(output.Item1);
				result = output.Item2;
			}
			else
			{
				result = default(TVal);
			}
			return ret;
		}

		/// <summary>
		/// Ensure that the next time this key is queried, the calculation
		/// function will be called.
		/// </summary>
		public void Invalidate(TParam key)
		{
			Contract.Requires(key != null);

			if (!cacheEntries.ContainsKey(key))
				return;

			var to_remove = cacheEntries[key];
			if (releaseFunction != null)
				releaseFunction(to_remove.Item2);

			cacheMRUList.Remove(to_remove.Item1);
			cacheEntries.Remove(key);
		}

		/// <summary>
		/// Invalidate all items in the cache
		/// </summary>
		public void InvalidateAll()
		{
			if (releaseFunction == null || cacheEntries == null)
			{
				cacheMRUList = new LinkedList<TParam>();
				cacheEntries = new Dictionary<TParam, Tuple<LinkedListNode<TParam>, TVal>>();
				return;
			}

			if (cacheEntries.Count == 0)
				return;

			/*			 We have to remove them one-by-one to call the release function
			 * We ToArray() this so we don't get a "modifying collection while
			 * enumerating" exception. */
			foreach (var v in cacheEntries.Keys.ToArray())
			{
				Invalidate(v);
			}
		}

		/// <summary>
		/// Returns all values currently in the cache
		/// </summary>
		/// <returns></returns>
		public IEnumerable<TVal> CachedValues()
		{
			return cacheEntries.Select(x => x.Value.Item2);
		}

		void maintainCache()
		{
			while (cacheMRUList.Count > maxCacheSize)
			{
				var to_remove = cacheMRUList.Last.Value;
				if (releaseFunction != null)
					releaseFunction(cacheEntries[to_remove].Item2);

				cacheEntries.Remove(cacheMRUList.Last.Value);
				cacheMRUList.RemoveLast();
			}
		}

		[ContractInvariantMethod]
		void Invariants()
		{
			Contract.Invariant(cacheEntries.Count == cacheMRUList.Count);
			Contract.Invariant(cacheEntries.Count <= maxCacheSize);
		}
	}
}