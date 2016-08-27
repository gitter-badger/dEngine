// Service.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using dEngine.Instances;
using dEngine.Instances.Attributes;


namespace dEngine.Services
{
	/// <summary>
	/// A service is a top-level singleton class.
	/// </summary>
	/// <remarks>
	/// Services can be retrived by using the <see cref="DataModel.GetService"/> method.
	/// </remarks>
	[TypeId(62), ToolboxGroup("Services")]
	public abstract class Service : Instance, ISingleton
	{
		/// <inheritdoc />
		protected Service()
		{
			Parent = Game.DataModel;

			ParentLocked = true;
		}
	}
}