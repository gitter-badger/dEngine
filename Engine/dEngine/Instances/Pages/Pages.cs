// Pages.cs - dEngine
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
using dEngine.Instances.Attributes;
using Neo.IronLua;


namespace dEngine.Instances
{
	/// <summary>
	/// A table of pages, where a page is a sorted list of key/value pairs.
	/// </summary>
	[TypeId(190), Uncreatable]
	public abstract class Pages : Instance
	{
		/// <summary>
		/// The current page.
		/// </summary>
		protected SortedList<string, object> _currentPage;

		/// <summary>
		/// Determines if there are any more pages.
		/// </summary>
		public bool IsFinished { get; protected set; }

		/// <summary>
		/// Returns the current page.
		/// </summary>
		/// <returns></returns>
		public LuaTable GetCurrentPage()
		{
			var table = new LuaTable();
			foreach (var pair in _currentPage)
			{
				table[pair.Key] = pair.Value;
			}
			return table;
		}

		/// <summary>
		/// Requests the next page.
		/// </summary>
		public virtual void AdvanceToNextPage()
		{
			if (IsFinished)
				throw new InvalidOperationException("Cannot advance to next page: there are no more pages.");
		}

		internal IEnumerable<SortedList<string, object>> EnumeratePages()
		{
			while (!IsFinished)
			{
				AdvanceToNextPage();
				yield return _currentPage;
			}
		}
	}
}