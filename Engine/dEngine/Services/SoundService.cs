// SoundService.cs - dEngine
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using C5;
using CSCore.CoreAudioAPI;
using CSCore.XAudio2;
using CSCore.XAudio2.X3DAudio;
using dEngine.Instances;
using dEngine.Instances.Attributes;
using dEngine.Instances.Diagnostics;
using dEngine.Settings.Global;
using dEngine.Settings.User;
using dEngine.Utility;
using dEngine.Utility.Extensions;
using dEngine.Utility.Native;
using Neo.IronLua;


namespace dEngine.Services
{
    /// <summary>
    /// SoundService manages global sound properties.
    /// </summary>
    [TypeId(60), ExplorerOrder(50)]
    public partial class SoundService : Service
    {
        internal static SoundService Service;

        private ReverbType _ambientReverb;
        private float _distanceFactor;
        private float _dopplerScale;
        private float _rolloffScale;

        /// <summary />
        public SoundService()
        {
            Service = this;

            _ambientReverb = ReverbType.NoReverb;
            _distanceFactor = 1.0f;
            _dopplerScale = 1.0f;
            _rolloffScale = 1.0f;
        }

        private LuaTable GetDevices(DataFlow dataFlow)
        {
            var table = new LuaTable();
            var thread = ScriptService.CurrentThread;

            int i = 0;
            Task.Run(() =>
            {
                using (var enumerator = new MMDeviceEnumerator())
                    foreach (var device in enumerator.EnumAudioEndpoints(dataFlow, DeviceState.Active))
                        table[i++] = device.FriendlyName;
            }).ContinueWith(x => ScriptService.ResumeThread(thread));

            ScriptService.YieldThread();

            return table;
        }

        /// <summary>
        /// Returns a list of active recording devices.
        /// </summary>
        [YieldFunction]
        public LuaTable GetRecordingDevices()
        {
            return GetDevices(DataFlow.Capture);
        }

        /// <summary>
        /// Returns a list of active playback devices.
        /// </summary>
        [YieldFunction]
        public LuaTable GetPlaybackDevices()
        {
            return GetDevices(DataFlow.Render);
        }

        /// <summary>
        /// Sets the device to record sound with.
        /// </summary>
        public void SetRecordingDevice(int deviceIndex)
        {
            _recordingDeviceIndex = deviceIndex;
        }

        /// <summary>
        /// Sets the device to output sound to.
        /// </summary>
        public void SetPlaybackDevice(int deviceIndex)
        {
            _playbackDeviceIndex = deviceIndex;
            Engine.AudioThread.Enqueue(Reset);
        }

        /// <summary>
        /// The global environment reverb type.
        /// </summary>
        [InstMember(1), EditorVisible("Data")]
        public ReverbType AmbientReverb
        {
            get { return _ambientReverb; }
            set
            {
                if (value == _ambientReverb) return;
                _ambientReverb = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Modifies the distance for every sound.
        /// </summary>
        [InstMember(2), EditorVisible("Data")]
        public float DistanceFactor
        {
            get { return _distanceFactor; }
            set
            {
                if (value == _distanceFactor) return;
                _distanceFactor = value;
                StaticDistanceFactor = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Scales the doppler effect.
        /// </summary>
        [InstMember(3), EditorVisible("Data")]
        public float DopplerScale
        {
            get { return _dopplerScale; }
            set
            {
                if (value == _dopplerScale) return;
                _dopplerScale = value;
                StaticDopplerFactor = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Determines how fast sounds will drop off.
        /// </summary>
        [InstMember(4), EditorVisible("Data")]
        public float RolloffScale
        {
            get { return _rolloffScale; }
            set
            {
                if (value == _rolloffScale) return;
                _rolloffScale = value;
                StaticRolloffScale = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Sets the object that the users hears from.
        /// </summary>
        public void SetListener(IListenable listenable)
        {
            Listener = new Listener();
            _listenable = listenable ?? _camera;
        }
    }

    public partial class SoundService // Static definitions
    {
        internal static Listener Listener;

        internal static object SoundLocker = new object();

        private static Camera _camera;
        private static IListenable _listenable;
        private static bool _silent;
        private static ConcurrentQueue<XAudio2Voice> _deleteQueue;
        private static ArrayList<Sound> _sounds3D;
        private static HashedLinkedList<Sound> _sounds;
        private static TreeSet<Sound> _activeSounds;

        internal static XAudio2_7 XAudio2 { get; private set; }
        internal static X3DAudioCore X3DAudio { get; private set; }
        internal static XAudio2MasteringVoice MasteringVoice { get; private set; }

        internal static bool IsCriticalError { get; private set; }

        internal static float StaticDopplerFactor;
        internal static float StaticRolloffScale;
        internal static float StaticSpeedOfSound;
        internal static float StaticDistanceFactor;

        internal static StatsItem Stats;
        internal static StatsItem CpuStats;
        internal static StatsItem MemoryStats;

        internal static StatsItem VoiceStats;
        internal static StatsItem VoiceActiveStats;
        internal static StatsItem VoiceTotalStats;

        internal static StatsItem SoundsActiveStats;
        internal static StatsItem Sounds3DStats;
        internal static StatsItem SoundsTotalStats;

        internal static StatsItem GlitchesStats;

        internal static void Init()
        {
            _deleteQueue = new ConcurrentQueue<XAudio2Voice>();
            _sounds = new HashedLinkedList<Sound>();
            _sounds3D = new ArrayList<Sound>();
            _activeSounds = new TreeSet<Sound>(new SoundPriorityComparer());


            StaticSpeedOfSound = 343.3f;

            XAudio2 = new XAudio2_7();

            _playbackDeviceIndex = (int)XAudio2.DefaultDevice;
            _recordingDeviceIndex = 0;
#if DEBUG
            XAudio2.SetDebugConfiguration(new DebugConfiguration
            {
                TraceMask = LogMask.LogErrors | LogMask.LogWarnings,
                LogFunctionName = true,
                LogThreadId = true
            });
#endif
            XAudio2.StartEngine();

            XAudio2.CriticalError += XAudio2OnCriticalError;

            Reset();

            if (Game.IsInitialized)
            {
                Game.Workspace.CameraChanged.Connect(WorkspaceCameraChanged);
            }
            else
            {
                Game.Initialized += (s, e) => { Game.Workspace.CameraChanged.Connect(WorkspaceCameraChanged); };
            }

            Listener = new Listener();
            
            Stats = new StatsItem("Sound", Game.Stats) { Value = (int)XAudio2.Version, ValueString = XAudio2.Version.ToString() };
            CpuStats = new StatsItem("CPU", Stats);

            VoiceStats = new StatsItem("Voices", Stats);
            VoiceActiveStats = new StatsItem("Active", VoiceStats);
            VoiceTotalStats = new StatsItem("Total", VoiceStats);

            MemoryStats = new StatsItem("Memory", Stats);

            SoundsActiveStats = new StatsItem("# Sounds", Stats);
            SoundsTotalStats = new StatsItem("# Unused", Stats);
            Sounds3DStats = new StatsItem("# 3D", Stats);

            GlitchesStats = new StatsItem("Glitches", Stats);
        }

        private static readonly Stopwatch _stopwatch = Stopwatch.StartNew();
        private static int _playbackDeviceIndex;
        private static int _recordingDeviceIndex;

        internal const float StatsUpdateRate = 1;

        internal static bool Update()
        {
            if (_silent || IsCriticalError || _sounds.Count == 0)
            {
                Thread.Sleep(16);
                return false;
            }

            if (DebugSettings.ProfilingEnabled)
            {
                if (_stopwatch.Elapsed.TotalSeconds > StatsUpdateRate)
                {
                    if (DebugSettings.XAudio2ProfilingEnabled)
                    {
                        ulong totalCycles;
                        Kernel32.QueryProcessCycleTime(Engine.Process.Handle, out totalCycles);

                        var perfData = XAudio2.PerformanceData;

                        var audioCycles = perfData.TotalCyclesSinceLastQuery;

                        CpuStats.Value = audioCycles;
                        CpuStats.ValueString = (audioCycles / (long)totalCycles).ToString("P");

                        VoiceStats.Value = perfData.TotalSourceVoiceCount;
                        VoiceActiveStats.Value = perfData.ActiveSourceVoiceCount;
                        VoiceTotalStats.Value = perfData.TotalSourceVoiceCount;

                        MemoryStats.Value = perfData.MemoryUsageInBytes;
                        MemoryStats.ValueString = perfData.MemoryUsageInBytes.ToPrettySize();
                    }

                    _stopwatch.Restart();
                }
            }

            /*
            XAudio2Voice voice;
            while (_deleteQueue.TryDequeue(out voice))
            {
                try
                {
                    voice.DestroyVoice();
                }
                catch (FileNotFoundException)
                {
                }
            }
            */

            lock (SoundLocker)
            {
                var count = _sounds3D.Count;
                for (int i = 0; i < count; i++)
                {
                    _sounds3D[i].Update();
                }
            }

            return true;
        }

        internal static void Suspend()
        {
            _silent = true;
            MasteringVoice?.SetVolume(0, 0);
        }

        internal static void Resume()
        {
            _silent = false;
            UpdateMix();
        }

        internal static void Reset()
        {
            var sampleRate = SoundSettings.SampleRate;
            int inputChannels;
            switch (UserGameSettings.SpeakerConfiguration)
            {
                case SpeakerConfiguration.Stero:
                    inputChannels = 2;
                    break;
                case SpeakerConfiguration.Surround:
                    inputChannels = XAudio2_7.DefaultChannels;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var deviceDetails = XAudio2.GetDeviceDetails(_playbackDeviceIndex);
            DebugSettings.PlaybackDeviceName = deviceDetails.DisplayName;
            try
            {
                MasteringVoice = XAudio2.CreateMasteringVoice(inputChannels, sampleRate);
            }
            catch (Exception e)
            {
                
            }
            X3DAudio = new X3DAudioCore(deviceDetails.OutputFormat.ChannelMask);
            UpdateMix();
        }

        internal static void Shutdown()
        {
        }

        internal static bool TryActivateSound(Sound sound, XAudio2SourceVoice voice)
        {
            lock (SoundLocker)
            {
                if (_activeSounds.Count >= SoundSettings.MaxActiveSoundCount)
                {
                    return false;
                }

                var result = _activeSounds.Add(sound);

                if (result)
                    SoundsActiveStats.Value++;

                return result;
            }
        }

        internal static void DeactivateSound(Sound sound)
        {
            lock (SoundLocker)
            {
                _activeSounds.Remove(sound);
                SoundsActiveStats.Value--;
            }
        }

        internal static void AddSound(Sound sound)
        {
            lock (SoundLocker)
            {
                _sounds.Add(sound);
                SoundsTotalStats.Value++;
            }
        }

        internal static void RemoveSound(Sound sound)
        {
            lock (SoundLocker)
            {
                _sounds.Remove(sound);
                SoundsTotalStats.Value--;
            }
        }

        internal static void StopAllSounds()
        {
            lock (SoundLocker)
            {
                foreach (var sound in _sounds)
                {
                    sound.Stop();
                }
            }
        }

        internal static object GetExisting()
        {
            return DataModel.GetService<SoundService>();
        }

        internal static void DeleteVoice(XAudio2SourceVoice voice)
        {
            if (voice == null) return;
            _deleteQueue.Enqueue(voice);
        }

        /// <summary />
        internal static void UpdateMix()
        {
            if (XAudio2 == null)
                return;

            MasteringVoice.Volume = UserGameSettings.MasterVolume;
            //MasteringVoice.SetEffectChain(new [] { new EffectDescriptor() {IUnknownEffect = _masteringLimiter.NativePointer} });
            //MasteringVoice.EnableEffect(0);
        }

        private static void XAudio2OnCriticalError(object sender, XAudio2CriticalErrorEventArgs args)
        {
            IsCriticalError = true;
            Suspend();
            Service.Logger.Error($"XAudio2 critical error ({args.HResult}) - SoundService has been suspended.");
        }

        private static void WorkspaceCameraChanged(Camera camera)
        {
            _camera?.Moved.Disconnect(OnCameraMoved);

            _camera = camera;
            if (_listenable == null)
                _listenable = camera;

            _camera?.Moved.Connect(OnCameraMoved);
        }

        private static void OnCameraMoved()
        {
            if (_listenable == _camera)
            {
                _listenable.UpdateListener(ref Listener);
            }
        }

        internal static float SemitonesToFrequencyRatio(float semitones)
        {
            return Mathf.Pow(2, semitones / 12);
        }

        internal static float FrequencyRatioToSemitones(float frequencyRatio)
        {
            return (float)(Math.Log10(frequencyRatio) * 12 * Math.PI);
        }

        internal static float DecibelsToAmplitudeRatio(float decibels)
        {
            return Mathf.Pow(10, decibels / 20);
        }

        private class SoundPriorityComparer : IComparer<Sound>
        {
            public int Compare(Sound x, Sound y)
            {
                return x.Volume.CompareTo(y.Volume);
            }
        }

        internal static void SetSound3D(Sound sound, bool is3D)
        {
            lock (SoundLocker)
            {
                if (is3D)
                {
                    _sounds3D.Add(sound);
                }
                else
                {
                    _sounds3D.Remove(sound);
                }
            }
        }

        /// <summary>
        /// Returns the JSON data of a Soundcloud track as a table.
        /// </summary>
        public LuaTable GetSoundcloudTrackInfo(int trackId)
        {
            var clientId = ContentProvider.SoundcloudClientId;
            if (string.IsNullOrEmpty(clientId))
                throw new InvalidOperationException("Soundcloud client ID was not set.");
            var url = $"http://api.soundcloud.com/tracks/{trackId}?client_id={clientId}";
            var track = HttpService.Get(url, true, new Dictionary<string, object>()).Result.ReadString();
            var table = HttpService.Service.JsonDecode(track);
            return table;
        }
    }
}