// PathStatus.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using dEngine.Services;

namespace dEngine
{
	/// <summary>
	/// Enum for the result of <see cref="PathfindingService.ComputeRawPathAsync"/>.
	/// </summary>
	public enum PathStatus
	{
		/// <summary>
		/// Path was found successfully.
		/// </summary>
		Success,
		/// <summary>
		/// Path doesn't exist, returns path to closest point.
		/// </summary>
		ClosestNoPath,
		/// <summary>
		/// Goal is out of MaxDistance range, returns path to closest point within MaxDistance.
		/// </summary>
		ClosestOutOfRange,
		/// <summary>
		/// Failed to compute path; the starting point is not empty.
		/// </summary>
		FailStartNotEmpty,
		/// <summary>
		/// Failed to compute path; the finish point is not empty.
		/// </summary>
		FailFinishNotEmpty,
	}
}