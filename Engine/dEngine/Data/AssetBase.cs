// AssetBase.cs - dEngine
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
using System.Text;
using dEngine.Utility.Native;

namespace dEngine.Data
{
    /// <summary>
    /// Base class for assets.
    /// </summary>
    public abstract class AssetBase : IDisposable
    {
        protected bool _disposed;

        /// <summary>
        /// The path to the source file.
        /// </summary>
        public string SourceFile { get; set; } = string.Empty;

        /// <summary>
        /// A list of tags.
        /// </summary>
        public Tags Tags { get; set; } = new Tags();

        /// <summary>
        /// The <see cref="ContentType" /> of this asset.
        /// </summary>
        public abstract ContentType ContentType { get; }

        /// <summary>
        /// Determines if the geometry has been disposed.
        /// </summary>
        public bool IsDisposed => _disposed;

        /// <summary>
        /// Determines if data was successfuly loaded into the asset.
        /// </summary>
        public bool IsLoaded { get; protected set; }

        public static readonly byte[] Magic = new [] {(byte)'A', (byte)'S' , (byte)'S' , (byte)'E' , (byte)'t' };

        /// <summary>
        /// Disposes of the asset.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Saves the asset to a stream.
        /// </summary>
        internal void Save(Stream stream)
        {
            using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
            {
                writer.Write(SourceFile ?? string.Empty);

                Tags.Save(writer);

                OnSave(writer);
            }
        }

        /// <summary>
        /// Loads the asset from a stream.
        /// </summary>
        internal void Load(Stream stream)
        {
            using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
            {
                OnLoad(reader);
            }
        }

        private static char[] _assetHeader = "ASSETBIN".ToCharArray();

        /// <summary/>
	    protected virtual void OnSave(BinaryWriter writer)
        {
            writer.Write(_assetHeader);
            writer.Write((byte)ContentType);
        }

        /// <summary/>
        protected virtual void OnLoad(BinaryReader reader)
        {
            var header = reader.ReadChars(_assetHeader.Length);
            if (VisualC.CompareMemory(_assetHeader, header, header.Length) != 0)
                throw new InvalidDataException("Could not load asset with invalid header.");
        }

        /// <summary/>
        ~AssetBase()
        {
            Dispose(false);
        }

        /// <summary>
        /// Dispoes of the asset.
        /// </summary>
        /// <param name="disposing">Determines whether object was disposed or deconstructed.</param>
        protected abstract void Dispose(bool disposing);

#pragma warning disable 1591
        [InstBeforeSerialization]
        protected virtual void BeforeSerialization()
        {
            if (_disposed)
                throw new InvalidOperationException("Cannot serialize disposed asset.");
        }

        [InstAfterSerialization]
        protected virtual void AfterSerialization()
        {
        }

        [InstBeforeDeserialization]
        protected virtual void BeforeDeserialization()
        {
        }

        [InstAfterDeserialization]
        protected virtual void AfterDeserialization()
        {
        }
#pragma warning restore 1591
    }
}