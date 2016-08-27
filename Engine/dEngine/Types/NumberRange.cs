// NumberRange.cs - dEngine
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
using System.IO;


namespace dEngine
{
    /// <summary>
    /// A range represents a minimum and maximum value.
    /// </summary>
    public struct NumberRange : IDataType, IEquatable<NumberRange>
    {
        /// <summary>
        /// The minimum value of the range.
        /// </summary>
        [InstMember(1)]
        public float Min;

        /// <summary>
        /// The maximum value of the range.
        /// </summary>
        [InstMember(2)]
        public float Max;

        /// <summary>
        /// Creates a new range with min and max both set to the given value.
        /// </summary>
        /// <param name="value"></param>
        public NumberRange(float value)
        {
            Min = value;
            Max = value;
        }

        /// <summary>
        /// Creates a new range with min and max both set to the given value.
        /// </summary>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        public NumberRange(float min, float max)
        {
            Min = min;
            Max = max;
        }

        /// <summary>
        /// Determines if the min and max are equal for two <see cref="NumberRange"/>s.
        /// </summary>
        public static bool operator ==(NumberRange a, NumberRange b)
        {
            return a.Min == b.Min && a.Max == b.Max;
        }

        /// <summary>
        /// Determines if the min and max are not equal for two <see cref="NumberRange"/>s.
        /// </summary>
        public static bool operator !=(NumberRange a, NumberRange b)
        {
            return a.Min != b.Min || a.Max != b.Max;
        }

        /// <summary/>
        public bool Equals(NumberRange other)
        {
            return Min.Equals(other.Min) && Max.Equals(other.Max);
        }

        /// <summary/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is NumberRange && Equals((NumberRange)obj);
        }

        /// <summary/>
        public override int GetHashCode()
        {
            unchecked
            {
                return (Min.GetHashCode() * 397) ^ Max.GetHashCode();
            }
        }

        /// <summary/>
        public override string ToString()
        {
            return $"{Min}, {Max}";
        }

        /// <summary/>
        public void Load(BinaryReader reader)
        {
            Min = reader.ReadSingle();
            Max = reader.ReadSingle();
        }

        /// <summary/>
        public void Save(BinaryWriter writer)
        {
            writer.Write(Min);
            writer.Write(Max);
        }
    }
}