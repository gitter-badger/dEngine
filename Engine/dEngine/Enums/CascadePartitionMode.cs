// CascadePartitionMode.cs - dEngine
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
	/// The cascade partitioning method.
	/// </summary>
	public enum CascadePartitionMode
	{
		/// <summary>
		/// Use manually defined splits.
		/// </summary>
		Manual,

		/// <summary>
		/// Use a logarithmic algorithm to determine splits.
		/// </summary>
		Logarithmic,

		/// <summary>
		/// Use the PSSM algorithm to determine splits.
		/// </summary>
		ParallelSplit
	}
}