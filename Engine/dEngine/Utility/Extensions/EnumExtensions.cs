// EnumExtensions.cs - dEngine
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

namespace dEngine
{
	internal static class EnumExtensions
	{
		public static Axis ToAxis(this NormalId normalId)
		{
			switch (normalId)
			{
				case NormalId.Left:
					return Axis.X;
				case NormalId.Right:
					return Axis.X;
				case NormalId.Top:
					return Axis.Y;
				case NormalId.Bottom:
					return Axis.Y;
				case NormalId.Front:
					return Axis.Z;
				case NormalId.Back:
					return Axis.Z;
				default:
					throw new ArgumentOutOfRangeException(nameof(normalId), normalId, null);
			}
		}
	}
}