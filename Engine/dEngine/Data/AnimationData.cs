// AnimationData.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System.IO;
using dEngine.Instances.Attributes;


namespace dEngine.Data
{
	/// <summary>
	/// Container class for animation data;
	/// </summary>
	[TypeId(23)]
	public class AnimationData : AssetBase
	{
		/// <inheritdoc />
		public override ContentType ContentType => ContentType.Animation;

	    protected override void OnSave(BinaryWriter writer)
	    {
	        //base.OnSave(writer);
	    }

	    protected override void OnLoad(BinaryReader reader)
	    {
	        //base.OnLoad(reader);
	    }

	    /// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
			}

			_disposed = true;
		}
	}
}