// AudioData.cs - dEngine
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
using CSCore;
using CSCore.Codecs.FLAC;
using CSCore.Codecs.MP3;
using CSCore.Codecs.WAV;
using CSCore.MediaFoundation;
using CSCore.SoundOut;
using CSCore.Streams.SampleConverter;
using dEngine.Instances.Attributes;
using dEngine.Utility;
using dEngine.Utility.Extensions;
using dEngine.Utility.Native;

#pragma warning disable 1591

namespace dEngine.Data
{
    [TypeId(20)]
    public class AudioData : AssetBase
    {
        [InstMember(1)]
        private byte[] _bytes;

        internal IWaveSource AudioSource { get; private set; }

        public override ContentType ContentType => ContentType.Sound;

        /// <inheritdoc />
        protected override void BeforeSerialization()
        {
            base.BeforeSerialization();

            var outputStream = new MemoryStream();
            var encoder = new WaveWriter(outputStream, AudioSource.WaveFormat);

            var buffer = AudioSource.ToByteArray();
            encoder.Write(buffer, 0, buffer.Length);
            _bytes = outputStream.ToArray();
        }

        /// <inheritdoc />
        protected override void AfterDeserialization()
        {
            base.AfterDeserialization();
            var stream = new MemoryStream(_bytes);
            AudioSource = GetSoundSource(stream);
        }

        private static readonly byte[] _mp3Magic = { (byte)'I', (byte)'D', (byte)'3' };
        private static readonly byte[] _mp3Magic2 = { 0xFF, 0xFB };
        private static readonly byte[] _oggMagic = { (byte)'O', (byte)'g', (byte)'g', (byte)'S' };
        private static readonly byte[] _riffMagic = { (byte)'R', (byte)'I', (byte)'F', (byte)'F' };
        private static readonly byte[] _waveMagic = { (byte)'W', (byte)'A', (byte)'V', (byte)'E', (byte)'f', (byte)'m', (byte)'t' };
        private static readonly byte[] _opusMagic = { (byte)'O', (byte)'p', (byte)'u', (byte)'s', (byte)'H', (byte)'e', (byte)'a', (byte)'d' };
        private static readonly byte[] _flacMagic = { (byte)'f', (byte)'L', (byte)'a', (byte)'C' };
        private static readonly byte[] _m4aMagic = { 0x00, 0x00, 0x00, (byte)' ', (byte)'f', (byte)'t', (byte)'y' };

        internal static ISoundOut GetSoundOut()
        {
            if (WasapiOut.IsSupportedOnCurrentPlatform)
                return new WasapiOut();
            return new DirectSoundOut();
        }

        private IWaveSource GetSoundSource(Stream stream)
        {
            var magic = new byte[8];
            stream.Read(magic, 0, 8);
            stream.Position = 0;

            if (VisualC.CompareMemory(magic, _mp3Magic, 3) == 0 ||
                VisualC.CompareMemory(magic, _mp3Magic2, 2) == 0)
            {
                return new DmoMp3Decoder(stream);
            }
            if (VisualC.CompareMemory(magic, _oggMagic, 4) == 0)
            {
                stream.Position = 0x1C;
                stream.Read(magic, 0, 8);
                stream.Position = 0;

                if (VisualC.CompareMemory(magic, _opusMagic, 8) == 0)
                {
                    throw new NotSupportedException("Ogg Opus is not currently supported.");
                }

                var decoder = new VorbisDecoder(stream);
                var converter = new SampleToPcm16(decoder);
                return converter;
            }
            if (VisualC.CompareMemory(magic, _flacMagic, 4) == 0)
            {
                return new FlacFile(stream, FlacPreScanMode.Sync);
            }
            if (VisualC.CompareMemory(magic, _riffMagic, 4) == 0)
            {
                stream.Position = 8;
                stream.Read(magic, 0, 7);
                if (VisualC.CompareMemory(magic, _waveMagic, 7) == 0)
                {
                    return new MediaFoundationDecoder(stream);
                }
                stream.Position = 0;
            }
            if (VisualC.CompareMemory(magic, _m4aMagic, 7) == 0)
            {
                return new MediaFoundationDecoder(stream);
            }

            throw new FormatException("The stream is an unsupported format.");
        }

        protected override void OnLoad(BinaryReader reader)
        {
            var stream = reader.BaseStream;
            _bytes = new byte[stream.Length];
            stream.Read(_bytes, 0, _bytes.Length);
            stream.Position = 0;
            AudioSource = GetSoundSource(stream);
            IsLoaded = true;
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                AudioSource?.Dispose();
            }

            _bytes = null;

            _disposed = true;
        }
    }
}