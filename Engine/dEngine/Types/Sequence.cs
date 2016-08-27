// SequenceBase.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace dEngine
{
    /// <summary>
    /// Base class for sequences.
    /// </summary>
    public abstract class Sequence<T> where T : struct
    {
        [InstMember(1)]
        protected Keypoint[] _keypoints;

        /// <summary>
        /// The sequence values.
        /// </summary>
        public Keypoint[] Keypoints => _keypoints; // TODO: see if array is acceptable, or LuaTable required

        /// <summary>
        /// Empty constructor.
        /// </summary>
        protected Sequence()
        { }

        /// <summary>
        /// Creates a sequence using two keypoints of the same value.
        /// </summary>
        protected Sequence(T value)
        {
            _keypoints = new[] { new Keypoint(0, value), new Keypoint(1, value) };
        }

        /// <summary>
        /// Creates a new keypoint.
        /// </summary>
        public static Keypoint NewKeypoint(float time, T value)
        {
            return new Keypoint(time, value);
        }

        /// <summary>
        /// Creates a sequence from an array of keypoints.
        /// </summary>
        protected Sequence(Keypoint[] values)
        {
            _keypoints = values;
        }

        /// <summary>
        /// Creates a sequence from a table of keypoints.
        /// </summary>
        protected Sequence(IEnumerable<Keypoint> values)
        {
            _keypoints = values.ToArray();
        }

        /// <summary/>
        public override string ToString()
        {
            var str = "";
            var len = _keypoints.Length;
            for (var i = 0; i < len; i++)
            {
                str += _keypoints[i];
                if (i == len - 1)
                    str += ", ";
            }
            return str;
        }

        /// <summary>
        /// A keypoint for a sequence.
        /// </summary>
        public struct Keypoint
        {
            /// <summary>
            /// The time in the sequence that the keypoint will be positioned at.
            /// </summary>
            [InstMember(1)]
            public readonly float Time;

            /// <summary>
            /// The value of the keypoint/
            /// </summary>
            [InstMember(2)]
            public readonly T Value;

            /// <summary>
            /// Creates a sequence keypoint.
            /// </summary>
            public Keypoint(float time, T value)
            {
                Time = time;
                Value = value;
            }

            /// <summary>
            /// Returns a string representation of the keypoint.
            /// </summary>
            public override string ToString()
            {
                return $"{Value}, {Time}";
            }
        }
    }
}