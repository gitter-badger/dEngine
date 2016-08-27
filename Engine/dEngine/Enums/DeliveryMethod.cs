// DeliveryMethod.cs - dEngine
// Copyright (C) 2015-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

namespace dEngine
{
	/// <summary>
	/// Networking delivery methods.
	/// </summary>
	public enum DeliveryMethod
	{
		/// <summary></summary>
		Unknown = 0,

		/// <summary>
		/// No guarantees, except for preventing duplicates.
		/// </summary>
		Unreliable = 1,

		/// <summary>
		/// Late messages will be dropped if newer ones were already received.
		/// </summary>
		UnreliableSequenced = 2,

		/// <summary>
		/// All messages will arrive, but not necessarily in the same order.
		/// </summary>
		ReliableUnordered = 34,

		/// <summary>
		/// All messages will arrive, but late ones will be dropped.
		/// </summary>
		ReliableSequenced = 35,

		/// <summary>
		/// All messages will arrive, and they will do so in the same order.
		/// </summary>
		ReliableOrdered = 67
	}
}