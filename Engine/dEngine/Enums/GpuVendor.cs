// GpuVendor.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
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
    /// Enum for GPU vendors.
    /// </summary>
    public enum GpuVendor
    {
        /// <summary>
        /// The GPU vendor is unknown.
        /// </summary>
        Unknown,
        /// <summary>
        /// The GPU was made by NVIDIA.
        /// </summary>
        Nvidia = 0x10DE,
        /// <summary>
        /// The GPU was made by AMD.
        /// </summary>
        AMD = 0x1002,
        /// <summary>
        /// The GPU was made by Intel.
        /// </summary>
        Intel = 0x8086,
    }
}