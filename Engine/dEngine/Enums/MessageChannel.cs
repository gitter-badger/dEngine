// MessageChannel.cs - dEngine
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

namespace dEngine
{
	/// <summary>
	/// Enum for message networking channels.
	/// </summary>
	public enum MessageChannel
	{
		/// <summary>
		/// The standard channel.
		/// </summary>
		Standard = 0,

		/// <summary>
		/// Channel for replication messages.
		/// </summary>
		Replication = 1,

		/// <summary>
		/// Channel for <see cref="RemoteFunction" /> and <see cref="RemoteEvent" />.
		/// </summary>
		Remote = 2
	}
}