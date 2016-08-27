// NumberSequence.cs - dEngine
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
#pragma warning disable 1591
namespace dEngine
{
    /// <summary>
    /// A sequence of numbers.
    /// </summary>
    public sealed class NumberSequence : Sequence<double>, IDataType
    {
        public NumberSequence(double value) : base(value)
        {
        }

        public NumberSequence(Keypoint[] values) : base(values)
        {
        }

        public NumberSequence(IEnumerable<Keypoint> values) : base(values)
        {
        }

        public NumberSequence(params double[] values)
        {
            _keypoints = new Keypoint[values.Length];
            for (var i = 0; i < values.Length; i++)
            {
                _keypoints[i] = new Keypoint((float)i / values.Length, values[i]);
            }
        }

        public static explicit operator NumberSequence(string str)
        {
            var numbers = str.Split(new [] {','}, StringSplitOptions.RemoveEmptyEntries).Select(double.Parse).ToArray();
            var keypoints = new Keypoint[numbers.Length];
            if (numbers.Length > 1)
            {
                for (var i = 0; i < numbers.Length / 2; i += 2)
                {
                    keypoints[i] = new Keypoint((float)numbers[i], numbers[i + 1]);
                }
            }
            return new NumberSequence();
        }


        /// <summary/>
        public void Load(BinaryReader reader)
        {
            var keyCount = reader.ReadInt32();
            _keypoints = new Keypoint[keyCount];
            for (var i = 0; i < keyCount; i++)
            {
                var time = reader.ReadSingle();
                var value = reader.ReadDouble();
                _keypoints[i] = new Keypoint(time, value);
            }
        }

        /// <summary/>
        public void Save(BinaryWriter writer)
        {
            var keyCount = _keypoints.Length;
            writer.Write(keyCount);
            for (var i = 0; i < keyCount; i++)
            {
                var keypoint = _keypoints[i];
                writer.Write(keypoint.Time);
                writer.Write(keypoint.Value);
            }
        }
    }
}