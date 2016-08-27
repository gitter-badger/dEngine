// Mathf.cs - dEngine
// Copyright (C) 2015-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System;

#pragma warning disable 1591

namespace dEngine.Utility
{
	/// <summary>
	/// Utility class for using <seealso cref="Math" /> methods with floats.
	/// </summary>
	public static class Mathf
	{
        public static int GetSetBitCount(long lValue)
        {
            int iCount = 0;

            //Loop the value while there are still bits
            while (lValue != 0)
            {
                //Remove the end bit
                lValue = lValue & (lValue - 1);

                //Increment the count
                iCount++;
            }

            //Return the count
            return iCount;
        }

        /// <summary>
        /// Pi.
        /// </summary>
        public const float Pi = (float)Math.PI;

		/// <summary>
		/// Pi/2
		/// </summary>
		public const float HalfPi = Pi / 2;

		/// <summary>
		/// Degrees to radians converter value.
		/// </summary>
		public const float Deg2Rad = Pi * 2 / 360;

		/// <summary>
		/// Radians to degrees converter value.
		/// </summary>
		public const float Rad2Deg = 360 / (Pi * 2);

		public static float Cos(float x)
		{
			return (float)Math.Cos(x);
		}

		public static float Sin(float x)
		{
			return (float)Math.Sin(x);
		}

		public static float Asin(float x)
		{
			return (float)Math.Asin(x);
		}

		public static float Atan2(float x, float y)
		{
			return (float)Math.Atan2(x, y);
		}

        public static float Pow(float a, float b)
		{
			return (float)Math.Pow(a, b);
		}

		public static float Tan(float f)
		{
			return (float)Math.Tan(f);
		}

		public static float Sqrt(float v)
		{
			return (float)Math.Sqrt(v);
		}

		public static float Clamp(float val, float min, float max)
		{
			return Math.Max(min, Math.Min(max, val));
		}

		public static float Round(float f)
		{
			return (float)Math.Round(f);
		}

		public static float Lerp(float a, float b, float t)
		{
			return a * (1 - t) + b * t;
        }

        internal static object Lerp(double a, double b, double t)
        {
            return a * (1 - t) + b * t;
        }

        public static double PointsToPixels(double points)
		{
			return points * (96.0 / 72.0);
		}

		public static float Acos(float f)
		{
			return (float)Math.Acos(f);
		}

		public static float Atan(float f)
		{
			return (float)Math.Atan(f);
		}

		public static float Log(float f)
		{
			return (float)Math.Log(f);
		}

		public static float Saturate(float f)
		{
			return f > 1 ? 1 : f < 0 ? 0 : f;
		}
	}
}