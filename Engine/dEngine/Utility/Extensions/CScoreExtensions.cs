// CScoreExtensions.cs - dEngine
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

namespace dEngine.Utility.Extensions
{
    internal static class CScoreExtensions
    {
        public static byte[] ToByteArray(this IWaveSource source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            using (var buffer = new MemoryStream())
            {
                int read;
                var temporaryBuffer = new byte[source.WaveFormat.BytesPerSecond];
                while ((read = source.Read(temporaryBuffer, 0, temporaryBuffer.Length)) > 0)
                {
                    buffer.Write(temporaryBuffer, 0, read);
                }
                return buffer.ToArray();
            }
        }
    }
}