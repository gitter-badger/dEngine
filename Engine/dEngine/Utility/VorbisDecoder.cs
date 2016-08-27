// OggDecoder.cs - dEngine
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
using NVorbis;

namespace dEngine.Utility
{
    internal class VorbisDecoder : ISampleSource
    {
        private readonly Stream _stream;
        private readonly VorbisReader _vorbisReader;

        private readonly WaveFormat _waveFormat;
        private bool _disposed;

        public VorbisDecoder(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            if (!stream.CanRead)
                throw new ArgumentException("Stream is not readable.", nameof(stream));
            _stream = stream;
            _vorbisReader = new VorbisReader(stream, false);
            _waveFormat = new WaveFormat(_vorbisReader.SampleRate, 32, _vorbisReader.Channels, AudioEncoding.IeeeFloat);
        }

        public bool CanSeek
        {
            get { return _stream.CanSeek; }
        }

        public WaveFormat WaveFormat
        {
            get { return _waveFormat; }
        }

        //got fixed through workitem #17, thanks for reporting @rgodart.
        public long Length
        {
            get { return CanSeek ? (long)(_vorbisReader.TotalTime.TotalSeconds * _waveFormat.SampleRate * _waveFormat.Channels) : 0; }
        }

        //got fixed through workitem #17, thanks for reporting @rgodart.
        public long Position
        {
            get
            {
                return CanSeek ? (long)(_vorbisReader.DecodedTime.TotalSeconds * _vorbisReader.SampleRate * _vorbisReader.Channels) : 0;
            }
            set
            {
                if (!CanSeek)
                    throw new InvalidOperationException("NVorbisSource is not seekable.");
                if (value < 0 || value > Length)
                    throw new ArgumentOutOfRangeException(nameof(value));

                _vorbisReader.DecodedTime = System.TimeSpan.FromSeconds((double)value / _vorbisReader.SampleRate / _vorbisReader.Channels);
            }
        }

        public int Read(float[] buffer, int offset, int count)
        {
            return _vorbisReader.ReadSamples(buffer, offset, count);
        }

        public void Dispose()
        {
            if (!_disposed)
                _vorbisReader.Dispose();
            else
                throw new ObjectDisposedException("NVorbisSource");
            _disposed = true;
        }
    }
}