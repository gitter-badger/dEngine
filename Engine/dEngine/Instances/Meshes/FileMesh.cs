// FileMesh.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using dEngine.Data;
using dEngine.Instances.Attributes;


namespace dEngine.Instances
{
	[TypeId(26)]
	public abstract class FileMesh : Mesh
	{
		private Content<Geometry> _meshContent;

		/// <inheritdoc />
		protected FileMesh()
		{
			_meshContent = new Content<Geometry>();
		}

		/// <summary>
		/// The content ID of the skeletal mesh data.
		/// </summary>
		[InstMember(1), EditorVisible("Data")]
		public Content<Geometry> MeshId
		{
			get { return _meshContent; }
			set
			{
				_meshContent = value;
				value.Subscribe(this, (id, asset) => { SetGeometry(asset); });
			}
		}
	}
}