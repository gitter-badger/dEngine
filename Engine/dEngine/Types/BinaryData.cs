// BinaryData.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System.IO;

namespace dEngine
{
    /// <summary>
    /// Stores binary data.
    /// </summary>
    /// <remarks>
    /// Maximum value will be 2GB.
    /// </remarks>
    public class BinaryData : IDataType
    {
        private Stream stream;

        /// <summary/>
        public BinaryData()
        {
            
        }

        /// <summary>
        /// Creates a <see cref="BinaryData"/> from the given bytes.
        /// </summary>
        public BinaryData(byte[] bytes)
        {
            stream = new MemoryStream(bytes);
        }

        /// <summary>
        /// Creates a <see cref="BinaryData"/> from the given stream.
        /// </summary>
        public BinaryData(Stream stream)
        {
            this.stream = stream;
        }

        /// <summary/>
        public void Save(BinaryWriter writer)
        {
            writer.Write(stream.Length);
            stream.CopyTo(writer.BaseStream);
        }

        /// <summary/>
        public void Load(BinaryReader reader)
        {
            var count = reader.ReadInt32();
            stream = new MemoryStream(reader.ReadBytes(count));
        }
    }
}