// InstanceId.cs - dEngine
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
using dEngine.Instances;

namespace dEngine
{
    /// <summary>
    /// A GUID for an <see cref="Instance" />.
    /// </summary>
    public struct InstanceId : IDataType
    {
        /// <summary>
        /// An empty instance ID.
        /// </summary>
        public static readonly InstanceId Empty = new InstanceId(Guid.Empty);

        private string _id;

        /// <summary>
        /// Creates an InstanceId from a GUID.
        /// </summary>
        public InstanceId(Guid guid)
        {
            _id = Compress(guid);
        }

        /// <summary>
        /// Creates an InstanceId from a string.
        /// </summary>
        public InstanceId(string id)
        {
            _id = id;
        }

        /// <summary>
        /// Creates an InstanceId from a string.
        /// </summary>
        public InstanceId(ref string id)
        {
            _id = id;
        }

        /// <summary />
        public void Save(BinaryWriter writer)
        {
            writer.Write(_id);
        }

        /// <summary />
        public void Load(BinaryReader reader)
        {
            _id = reader.ReadString();
        }

        private static string Compress(Guid guid)
        {
            return Convert.ToBase64String(guid.ToByteArray())
                .Substring(0, 22)
                .Replace('+', '-')
                .Replace('/', '_');
        }

        private static Guid Decompress(ref string id)
        {
            var base64 = id
                .Replace('_', '/')
                .Replace('-', '+')
                         + "==";
            var bytes = Convert.FromBase64String(base64);
            return new Guid(bytes);
        }

        /// <summary>
        /// Returns the ID as a string.
        /// </summary>
        public override string ToString()
        {
            return _id;
        }

        /// <summary>
        /// Converts a InstanceId to an string.
        /// </summary>
        public static implicit operator string(InstanceId instanceId)
        {
            return instanceId._id;
        }

        /// <summary>
        /// Converts a InstanceId to an string.
        /// </summary>
        public static explicit operator Guid(InstanceId instanceId)
        {
            return Decompress(ref instanceId._id);
        }

        /// <summary>
        /// Converts a string to an InstanceId.
        /// </summary>
        public static implicit operator InstanceId(string id)
        {
            return new InstanceId(ref id);
        }

        /// <summary>
        /// Generates a new unique InstanceId.
        /// </summary>
        public static InstanceId Generate()
        {
            return new InstanceId(Guid.NewGuid());
        }

        /// <summary>
        /// Determines of two IDs are equal.
        /// </summary>
        public bool Equals(InstanceId other)
        {
            return string.Equals(_id, other._id);
        }

        /// <summary>
        /// Determines of two IDs are equal.
        /// </summary>
        public bool Equals(ref InstanceId other)
        {
            return string.Equals(_id, other._id);
        }

        /// <summary>
        /// Determines if the ID is equal to the given object.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is InstanceId && Equals((InstanceId)obj);
        }

        /// <summary>
        /// Gets the hash of the ID.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return _id?.GetHashCode() ?? 0;
        }

        /// <summary>
        /// Determines if two IDs are equal.
        /// </summary>
        public static bool operator ==(InstanceId left, InstanceId right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines if two IDs are not equal.
        /// </summary>
        public static bool operator !=(InstanceId left, InstanceId right)
        {
            return !left.Equals(right);
        }
    }
}