// GlobalDataStore.cs - dEngine
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
using System.Threading.Tasks;
using dEngine.Instances.Attributes;
using dEngine.Services;

#pragma warning disable 1591

namespace dEngine.Instances
{
	/// <summary>
	/// A GlobalDataStore allows manipulation of a data base.
	/// </summary>
	[Uncreatable, TypeId(71)]
	public class GlobalDataStore : Instance
	{
		/// <inheritdoc/>
		public GlobalDataStore()
		{
			Parent = DataStoreService.Service;
			ParentLocked = true;
			Archivable = false;

        }

		public object GetAsync(string key)
		{
			throw new NotImplementedException("DataStores not implemented.");
		}

		public object IncrementAsync(string key, int delta = 1)
		{
			throw new NotImplementedException("DataStores not implemented.");
		}

		public void SetAsync(string key, object value)
		{
			throw new NotImplementedException("DataStores not implemented.");
		}

		public void UpdateAsync(string key, Func<object, object> transformer)
		{
			throw new NotImplementedException("DataStores not implemented.");
		}
	}
}