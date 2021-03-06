﻿// DataStoreService.cs - dEngine
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
using dEngine.Instances;
using dEngine.Instances.Attributes;
using dEngine.Instances.DataStores;


#pragma warning disable 1591
namespace dEngine.Services
{
	/// <summary>
	/// A service for managing DataStores.
	/// </summary>
	[TypeId(57)]
	public class DataStoreService : Service
	{
		/// <inheritdoc/>
		public DataStoreService()
		{
			Service = this;
		}

		public GlobalDataStore GetDataStore(string name, string scope = "global")
		{
			throw new NotImplementedException();
		}

		public GlobalDataStore GetGlobalDataStore()
		{
			throw new NotImplementedException();
		}

		public OrderedDataStore GetOrderedDataStore()
		{
			throw new NotImplementedException();
		}

		internal static DataStoreService Service;

		internal static object GetExisting()
		{
			return DataModel.GetService<DataStoreService>();
		}
	}
}