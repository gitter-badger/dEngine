// InsertService.cs - dEngine
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
using dEngine.Serializer.V1;
using dEngine.Utility;


namespace dEngine.Services
{
	/// <summary>
	/// Service for inserting assets into the game.
	/// </summary>
	[TypeId(170), ExplorerOrder(-1)]
	public partial class InsertService : Service
	{
		/// <inheritdoc />
		internal InsertService()
		{
			Service = this;
		}

		internal static object GetExisting()
		{
			return DataModel.GetService<InsertService>();
		}

		/// <summary>
		/// Loads an asset from a content url.
		/// </summary>
		public Instance LoadAsset(string contentId)
		{
		    using (var stream = ContentProvider.DownloadStream(contentId).Result)
		    {
		        if (stream == null)
		            throw new InvalidOperationException($"Failed to load asset: could not fetch. ({contentId})");

		        if (Inst.CheckHeader(stream))
		        {
		            return Inst.Deserialize(stream, skipMagic: true);
		        }

		        throw new NotSupportedException("Downloaded data did not begin with \"INSTBIN\".");
		    }
		}

		/// <summary>
		/// Loads an asset from the Steam Workshop.
		/// </summary>
		/// <param name="workshopId">The workshop ID.</param>
		public Instance LoadAsset(uint workshopId)
		{
			throw new NotImplementedException();
		}
	}

	public partial class InsertService
	{
		// static methods
		public static InsertService Service;
	}
}