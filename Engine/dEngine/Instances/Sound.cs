// Sound.cs - dEngine
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
using CSCore;
using CSCore.XAudio2;
using dEngine.Data;
using dEngine.Instances.Attributes;
using dEngine.Services;
using dEngine.Utility;
using dEngine.Utility.Extensions;

using SharpDX;

namespace dEngine.Instances
{
    /// <summary>
    /// An instance which represents a single sound.
    /// </summary>
    [TypeId(18), ToolboxGroup("Gameplay"), ExplorerOrder(1)]
    public class Sound : Instance
    {
        /// <summary>
        /// Fired when the sound ends.
        /// </summary>
        /// <eventParam name="soundId" />
        public readonly Signal<string> Ended;

        /// <summary>
        /// Fired when the sound has loaded.
        /// </summary>
        /// <eventParam name="soundId" />
        public readonly Signal<string> Loaded;

        /// <summary>
        /// Fired every time the sound loops.
        /// </summary>
        /// <eventParam name="soundId" />
        /// <eventParam name="loopCount" />
        public readonly Signal<string, int> OnLoop;

        /// <summary>
        /// Fired when the sound is paused.
        /// </summary>
        /// <eventParam name="soundId" />
        public readonly Signal<string> Paused;

        /// <summary>
        /// Fired when the sound is played.
        /// </summary>
        /// <eventParam name="soundId" />
        public readonly Signal<string> Played;

        /// <summary>
        /// Fired when the sound is played after being paused.
        /// </summary>
        /// <eventParam name="soundId" />
        public readonly Signal<string> Resumed;

        /// <summary>
        /// Fired when the sound is stopped.
        /// </summary>
        /// <eventParam name="soundId" />
        public readonly Signal<string> Stopped;

        private float _attenuation;

        private AttenuationType _attenuationType;
        private XAudio2Buffer _audioBuffer;
        private VoiceCallback _callback;
        private bool _isPaused;
        private bool _isPlaying;
        private int _loopCount;
        private bool _looped;
        private XAudio2Buffer _loopedAudioBuffer;
        private float _maxDistance;
        private float _minDistance;
        private Part _parentPart;
        private float _pitch;
        private Content<AudioData> _soundId;
        private TimeSpan _trackLength;
        private XAudio2SourceVoice _voice;
        private float _volume;

        /// <summary />
        public Sound()
        {
            _volume = 1;
            StartingPosition = TimeSpan.Zero;
            _pitch = 1;
            _soundId = new Content<AudioData>();
            _minDistance = 0;
            _maxDistance = 1000;

            OnLoop = new Signal<string, int>(this);
            Ended = new Signal<string>(this);
            Loaded = new Signal<string>(this);
            Paused = new Signal<string>(this);
            Played = new Signal<string>(this);
            Resumed = new Signal<string>(this);
            Stopped = new Signal<string>(this);

            SoundService.AddSound(this);
        }

        private bool _is3D;

        /// <summary>
        /// The source of the media file.
        /// </summary>
        [InstMember(1), EditorVisible("Data")]
        public Content<AudioData> SoundId
        {
            get { return _soundId; }
            set
            {
                if (value == _soundId) return;
                Stop();
                SoundService.DeleteVoice(_voice);
                _voice = null;
                IsLoaded = false;

                _soundId = value;
                //value.Subscribe(this, OnSoundLoaded);
                NotifyChanged();
            }
        }

        /// <summary>
        /// If true the audio will loop.
        /// </summary>
        [InstMember(2), EditorVisible("Behaviour")]
        public bool Looped
        {
            get { return _looped; }
            set
            {
                if (value == _looped) return;
                _looped = value;

                if (_isPlaying)
                {
                    if (value)
                        Reset();
                    else
                        _voice?.ExitLoop();
                }

                NotifyChanged();
            }
        }

        /// <summary>
        /// The volume multiplier of the sound.
        /// </summary>
        [InstMember(4), EditorVisible("Data")]
        public float Volume
        {
            get { return _volume; }
            set
            {
                if (value == _volume) return;
                _volume = Mathf.Clamp(value, 0, 2);
                UpdateVolume();
                NotifyChanged();
            }
        }

        /// <summary>
        /// The volume multiplier of the sound.
        /// </summary>
        [InstMember(5), EditorVisible("Data")]
        public float Pitch
        {
            get { return _pitch; }
            set
            {
                if (value == _pitch) return;
                _pitch = Math.Max(0, value);
                _voice?.SetFrequencyRatio(_pitch);
                NotifyChanged();
            }
        }

        /// <summary>
        /// The distance at which the sound will begin to attenuate.
        /// </summary>
        [InstMember(6), EditorVisible("Data")]
        public float MinDistance
        {
            get { return _minDistance; }
            set
            {
                if (value == _minDistance) return;
                _minDistance = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// The maximum distance that the sound can be heard from.
        /// </summary>
        [InstMember(7), EditorVisible("Data")]
        public float MaxDistance
        {
            get { return _maxDistance; }
            set
            {
                if (value == _maxDistance) return;
                _maxDistance = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// The attenuation mode.
        /// </summary>
        [InstMember(8), EditorVisible("Data")]
        public AttenuationType Attenuation
        {
            get { return _attenuationType; }
            set
            {
                if (value == _attenuationType) return;
                _attenuationType = value;
                NotifyChanged();
            }
        }

        [InstMember(9)]
        internal TimeSpan StartingPosition { get; set; }

        /// <summary>
        /// Gets whether or not the sound is playing.
        /// </summary>
        [EditorVisible("Data")]
        public bool IsPlaying
        {
            get { return _isPlaying; }
            private set
            {
                if (value == _isPlaying) return;
                _isPlaying = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Gets whether or not the sound is paused.
        /// </summary>
        [EditorVisible("Data")]
        public bool IsPaused
        {
            get { return _isPaused; }
            private set
            {
                if (value == _isPaused) return;
                _isPaused = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Gets whether or not the sound has loaded.
        /// </summary>
        [EditorVisible("Data")]
        public bool IsLoaded { get; private set; }

        /// <summary>
        /// The length of the current sound in seconds.
        /// </summary>
        [EditorVisible("Data")]
        public TimeSpan TrackLength
        {
            get { return _trackLength; }
            private set
            {
                _trackLength = value;
                NotifyChanged(nameof(TrackLength));
            }
        }

        internal int SamplesPerSecond
            =>
                (_soundId.Asset.AudioSource.WaveFormat.BytesPerSecond /
                 _soundId.Asset.AudioSource.WaveFormat.BytesPerSample);

        /// <summary>
        /// The playback position of the audio in seconds.
        /// </summary>
        [EditorVisible("Data")]
        public TimeSpan TrackPosition
        {
            get
            {
                if (_voice == null)
                    return StartingPosition;
                var samples = (_soundId.Asset.AudioSource.Length / _soundId.Asset.AudioSource.WaveFormat.BytesPerSample);
                var pos = ((_voice.State.SamplesPlayed - (samples * _loopCount)) / SamplesPerSecond) *
                          SoundService.MasteringVoice.VoiceDetails.InputChannels;
                return TimeSpan.FromSeconds(pos);
            }
            set
            {
                StartingPosition = value;
                if (IsPlaying)
                    Reset();
                NotifyChanged();
            }
        }

        internal Part ParentPart
        {
            get { return _parentPart; }
            set
            {
                if (value == _parentPart) return;
                _parentPart = value;
                if (value != null)
                {
                }
            }
        }

        internal XAudio2Buffer CurrentAudioBuffer => _looped ? _loopedAudioBuffer : _audioBuffer;

        internal void Update()
        {
            if (!_is3D)
                return;

            var part = _parentPart;
            var cf = part.CFrame;
            var pos = cf.p;

            var cam = Game.Workspace.CurrentCamera;
            var camCF = cam.CFrame;
            var camPos = camCF.p;

            var distance = (pos - camPos).magnitude;

            switch (_attenuationType)
            {
                case AttenuationType.Linear:
                    _attenuation = Math.Max(1,
                        1 - SoundService.StaticRolloffScale * (distance - _minDistance) / (_maxDistance - _minDistance));
                    break;
                case AttenuationType.Logarithmic:
                    break;
                case AttenuationType.LogReverse:
                    break;
                case AttenuationType.Inverse:
                    break;
                case AttenuationType.Natural:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            UpdateVolume();
        }

        private void UpdateVolume()
        {
            _voice?.SetVolume(_volume * _attenuation, 0);
        }

        /// <inheritdoc />
        protected override void OnAncestryChanged(Instance child, Instance parent)
        {
            base.OnAncestryChanged(child, parent);
            _parentPart = parent as Part;
            _is3D = _parentPart != null;
            _attenuation = 1.0f;
            SoundService.SetSound3D(this, _is3D);
        }

        /// <inheritdoc />
        public override void Destroy()
        {
            base.Destroy();
            Stop();
            SoundService.RemoveSound(this);
            _callback?.Dispose();
        }

        private void Reset()
        {
            var buffer = CurrentAudioBuffer;
            if (_voice.State.BuffersQueued > 0)
            {
                _voice.Stop();
                _voice.FlushSourceBuffers();
            }
            _voice.SubmitSourceBuffer(buffer);
            buffer.PlayBegin = GetPlayOffset(StartingPosition.totalSeconds);
            _voice.Start();
        }

        /// <summary>
        /// Resumes the sound.
        /// </summary>
        public void Resume()
        {
            if (IsDestroyed || !IsPaused)
                return;

            if (!_looped)
            {
                if (_voice.State.BuffersQueued == 0)
                {
                    _voice.Stop();
                    _voice.FlushSourceBuffers();
                    _voice.SubmitSourceBuffer(CurrentAudioBuffer);
                }
            }

            _voice.Start();

            IsPaused = false;
            IsPlaying = true;
            Resumed.Fire(_soundId);
        }

        private int GetPlayOffset(double seconds)
        {
            var samples = (_soundId.Asset.AudioSource.Length / _soundId.Asset.AudioSource.WaveFormat.BytesPerSample);
            var positionOffset = -((int)_voice.State.SamplesPlayed - (samples * _loopCount));
            positionOffset += (int)(StartingPosition.totalSeconds * SamplesPerSecond);
            return (int)positionOffset;
        }

        /// <summary>
        /// Plays the sound.
        /// </summary>
        public void Play()
        {
            if (IsDestroyed)
                return;

            if (_voice == null)
            {
                _soundId.RetryDownload();
                return;
            }

            var alreadyPlaying = IsPlaying;
            if (alreadyPlaying || _isPaused || SoundService.TryActivateSound(this, _voice))
            {
                if (_voice.State.BuffersQueued > 0)
                {
                    _voice.Stop();
                    _voice.FlushSourceBuffers();
                }

                var buffer = CurrentAudioBuffer;

                buffer.PlayBegin = GetPlayOffset(StartingPosition.totalSeconds);
                _voice.SubmitSourceBuffer(buffer);
                _voice.Start();

                IsPaused = false;
                IsPlaying = true;

                if (!alreadyPlaying)
                    Played.Fire(_soundId);
            }
        }

        /// <summary>
        /// Pauses the sound.
        /// </summary>
        public void Pause()
        {
            if (!IsPlaying)
                return;

            _voice?.Stop();

            IsPaused = true;
            IsPlaying = false;
            Paused.Fire(_soundId);
        }

        /// <summary>
        /// Stops the sound.
        /// </summary>
        public void Stop()
        {
            if (!IsPlaying || _voice == null)
                return;

            if (_looped)
            {
                _voice.ExitLoop();
                _loopCount = 0;
            }
            else
            {
                _voice.Stop();
                _voice.FlushSourceBuffers();
                SoundService.DeactivateSound(this);
            }

            IsPaused = false;
            IsPlaying = false;
            Stopped.Fire(_soundId);

            TrackPosition = TimeSpan.Zero;
        }

        private void OnSoundLoaded(string soundId, AudioData data)
        {
            if (data == null)
            {
                Logger.Error($"Sound failed to load {GetFullName()}: {soundId}");
                return;
            }

            _callback?.Dispose();
            _callback = new VoiceCallback();
            _callback.LoopEnd += CallbackOnLoopEnd;

            var source = data.AudioSource;
            _voice = SoundService.XAudio2.CreateSourceVoice(source.WaveFormat, VoiceFlags.None,
                XAudio2.DefaultFrequencyRatio, null, null, null);

            var length = (TimeSpan)source.GetLength();

            var buffer = source.ToByteArray();
            var stream = new DataStream(buffer.Length, true, true);
            stream.Write(buffer, 0, buffer.Length);

            _audioBuffer = new XAudio2Buffer
            {
                AudioDataPtr = stream.DataPointer,
                AudioBytes = buffer.Length,
                Flags = XAudio2BufferFlags.EndOfStream
            };

            _loopedAudioBuffer = new XAudio2Buffer
            {
                AudioDataPtr = stream.DataPointer,
                AudioBytes = buffer.Length,
                Flags = XAudio2BufferFlags.EndOfStream,
                LoopCount = XAudio2Buffer.LoopInfinite
            };

            data.AudioSource.SetPosition((System.TimeSpan)StartingPosition);
            _voice.Volume = _volume;
            _voice.SetFrequencyRatio(_pitch);

            TrackLength = length;
            IsLoaded = true;
            Loaded.Fire(soundId);
        }

        private void CallbackOnLoopEnd(object sender, XAudio2BufferEventArgs xAudio2BufferEventArgs)
        {
            _loopCount++;
            Engine.GameThread.Enqueue(() => OnLoop.Fire(_soundId, _loopCount));
        }
    }
}