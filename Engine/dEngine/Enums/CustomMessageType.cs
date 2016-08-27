// CustomMessageType.cs - dEngine
// Copyright (C) 2015-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using dEngine.Instances;

#pragma warning disable 1591

namespace dEngine
{
	/// <summary>
	/// Enum for custom network messages.
	/// </summary>
	public enum CustomMessageType
	{
		Unknown = 0,

		/// <summary>
		/// A request to replicate an object
		/// </summary>
		ReplicateObject = 1,

		/// <summary>
		/// A request to replicate a property
		/// </summary>
		ReplicateProperty = 2,

		/// <summary></summary>
		RequestObject = 4,

		/// <summary>
		/// A request to fire a <see cref="RemoteEvent" />.
		/// </summary>
		RemoteEventFire = 5,

		/// <summary>
		/// A request to invoke a <see cref="RemoteFunction" />.
		/// </summary>
		RemoteFunctionInvoke = 6,

		/// <summary>
		/// A response to a <see cref="RemoteFunctionInvoke" />.
		/// </summary>
		RemoteFunctionReturn = 7
	}
}