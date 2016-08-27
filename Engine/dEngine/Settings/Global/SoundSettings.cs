// LuaSettings.cs - dEngine
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
using System.Runtime.CompilerServices;
using dEngine.Instances.Attributes;


namespace dEngine.Settings.Global
{
    /// <summary>
    /// Settings for the sound engine.
    /// </summary>
    [TypeId(201)]
    public class SoundSettings : Settings
    {
        private static int _maxActiveSoundCount;

        /// <summary>
        /// The maximum number of sounds that can be played at once.
        /// </summary>
        [EditorVisible("Sound", "Maximum Active Sounds")]
        public static int MaxActiveSoundCount
        {
            get { return _maxActiveSoundCount; }
            set
            {
                if (value == _maxActiveSoundCount) return;
                _maxActiveSoundCount = Math.Max(0, value);
                NotifyChangedStatic();
            }
        }

        private static int _sampleRate;
        private static bool _decodeFec;

        /// <summary>
        /// The output sample rate. If set to zero, the default system sample rate will be used.
        /// </summary>
        [EditorVisible("Sound", "Sample Rate")]
        public static int SampleRate
        {
            get { return _sampleRate; }
            set
            {
                if (value == _sampleRate) return;
                _sampleRate = value;
                NotifyChangedStatic();
            }
        }

        internal static bool DecodeFec
        {
            get { return _decodeFec; }
            set
            {
                if (value == _decodeFec) return;
                _decodeFec = value;
                NotifyChangedStatic();
            }
        }

        private static void NotifyChangedStatic([CallerMemberName] string propertyName = null)
        {
            Engine.Settings?.SoundSettings?.NotifyChanged(propertyName);
        }

        /// <inheritdoc />
        public override void RestoreDefaults()
        {
            MaxActiveSoundCount = 500;
            SampleRate = 0;
        }
    }
}