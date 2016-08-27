// RangeAttribute.cs - dEngine
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

namespace dEngine.Instances.Attributes
{
	/// <summary>
	/// An attribute for hinting at the intended range of a property.
	/// </summary>
	public class RangeAttribute : Attribute
	{
        /// <summary>
        /// The maximum value.
        /// </summary>
		public double Max { get; }

        /// <summary>
        /// The minimum value.
        /// </summary>
		public double Min { get; }

        /// <summary/>
		public RangeAttribute(double min, double max)
		{
			Min = min;
			Max = max;
		}
	}
}