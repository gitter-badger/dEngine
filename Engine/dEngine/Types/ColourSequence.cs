// ColourSequence.cs - dEngine
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

namespace dEngine
{
    /// <summary>
    /// A sequence of colours.
    /// </summary>
    public class ColourSequence : Sequence<Colour>, IDataType
    {
        internal ColourSequence()
        {
            
        }

        /// <summary/>
        public ColourSequence(Colour value) : base(value)
        {
        }

        /// <summary/>
        public ColourSequence(Keypoint[] values) : base(values)
        {
        }

        /// <summary/>
        public ColourSequence(IEnumerable<Keypoint> values) : base(values)
        {
        }

        /// <summary/>
        public void Load(BinaryReader reader)
        {
            var keyCount = reader.ReadInt32();
            _keypoints = new Keypoint[keyCount];
            for (int i = 0; i < keyCount; i++)
            {
                var time = reader.ReadSingle();
                var value = new Colour();
                value.Load(reader);
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
                keypoint.Value.Save(writer);
            }
        }
    }
}