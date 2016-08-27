// UserGameSettings.cs - dEngine
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
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using dEngine.Instances.Attributes;
using dEngine.Services;
using dEngine.Utility;
using Neo.IronLua;


namespace dEngine.Settings.User
{
    /// <summary>
    /// Game settings.
    /// </summary>
    [TypeId(189)]
    public class UserGameSettings : Settings, ICustomSettings
    {
        private static float _mouseSensitivityFirstPerson;
        private static float _mouseSensitivityThirdPerson;
        private static float _masterVolume;
        private static float _musicVolume;
        private static float _effectsVolume;
        private static Dictionary<string, string> _definedSettings;
        private static SpeakerType _speakerType;
        private static SpeakerConfiguration _speakerConfiguration;
        private static bool _voipEnabled;
        private static float _dialogueVolume;
        private static float _voipVolume;
        private static ColourBlindness _colourBlindSupport;
        private static float _brightness;
        private static float _fieldOfView;
        private static Vector2 _fullscreenResolution;
        private static float _cameraSpeed;
        private static int _fullscreenMonitor;
        private static WindowMode _windowMode;
        private static float _cameraShiftSpeed;
        private static bool _cameraSpeedup;

        /// <summary>
        /// Fired when a custom setting is changed.
        /// </summary>
        public readonly Signal<string, string> CustomSettingChanged;

        /// <summary />
        public UserGameSettings()
        {
            _definedSettings = new Dictionary<string, string>();
            CustomSettingChanged = new Signal<string, string>(this);
        }

        /// <summary>
        /// Adjusts the brightness of the screen.
        /// </summary>
        [EditorVisible("Graphics")]
        public static float Brightness
        {
            get { return _brightness; }
            set
            {
                if (value == _brightness) return;
                _brightness = Mathf.Clamp(value, 0, 1);
                NotifyChangedStatic();
            }
        }

        /// <summary>
        /// The vertical field of view of the camera.
        /// </summary>
        [EditorVisible("Graphics", "Field of View")]
        public static float FieldOfView
        {
            get { return _fieldOfView; }
            set
            {
                if (value == _fieldOfView) return;
                _fieldOfView = Mathf.Clamp(value, 50, 90);
                NotifyChangedStatic();
            }
        }

        /// <summary>
        /// The window mode to use.
        /// </summary>
        [EditorVisible("Graphics", "Window Mode")]
        public static WindowMode WindowMode
        {
            get { return _windowMode; }
            set
            {
                if (value == _windowMode) return;
                _windowMode = value;
                NotifyChangedStatic();
            }
        }

        /// <summary>
        /// The monitor to use as the primary screen.
        /// </summary>
        [EditorVisible("Graphics", "Monitor")]
        public static int FullscreenMonitor
        {
            get { return _fullscreenMonitor; }
            set
            {
                if (value == _fullscreenMonitor) return;
                _fullscreenMonitor = value;

                var allScreens = Screen.AllScreens;
                value = Math.Min(Math.Max(value, 0), allScreens.Length - 1);
                var screen = allScreens[value];
                FullscreenResolution = new Vector2(screen.Bounds.Width, screen.Bounds.Height);

                NotifyChangedStatic();
            }
        }

        /// <summary>
        /// The width to render the game at in fullscreen.
        /// </summary>
        [EditorVisible("Graphics", "Resolution")]
        public static Vector2 FullscreenResolution
        {
            get { return _fullscreenResolution; }
            set
            {
                if (value == _fullscreenResolution) return;
                _fullscreenResolution = value;
                NotifyChangedStatic();
            }
        }


        /// <summary>
        /// Changes the colour of the HUD to benefit colour blind players.
        /// </summary>
        [EditorVisible("Graphics", "Colour-blind Support")]
        public static ColourBlindness ColourBlindSupport
        {
            get { return _colourBlindSupport; }
            set
            {
                if (value == _colourBlindSupport) return;
                _colourBlindSupport = value;
                NotifyChangedStatic();
            }
        }

        /// <summary>
        /// The mouse sensitivity in first person.
        /// </summary>
        [EditorVisible("Controls", "Mouse Sensitivity (First Person)")]
        public static float MouseSensitivityFirstPerson
        {
            get { return _mouseSensitivityFirstPerson; }
            set
            {
                _mouseSensitivityFirstPerson = Mathf.Clamp(value, -0.0049f, 1f);
                NotifyChangedStatic();
            }
        }

        /// <summary>
        /// The mouse sensitivity in third person.
        /// </summary>
        [EditorVisible("Controls", "Mouse Sensitivity (Third Person)")]
        public static float MouseSensitivityThirdPerson
        {
            get { return _mouseSensitivityThirdPerson; }
            set
            {
                _mouseSensitivityThirdPerson = Mathf.Clamp(value, -0.0049f, 1f);
                NotifyChangedStatic();
            }
        }

        /// <summary>
        /// The mouse sensitivity in third person.
        /// </summary>
        [EditorVisible("Controls", "Mouse Sensitivity (Vehicle)")]
        public static float MouseSensitivityVehicle
        {
            get { return _mouseSensitivityThirdPerson; }
            set
            {
                _mouseSensitivityThirdPerson = value;
                NotifyChangedStatic();
            }
        }

        /// <summary>
        /// The speed of the camera.
        /// </summary>
        [EditorVisible("Camera", "Camera Speed")]
        public static float CameraSpeed
        {
            get { return _cameraSpeed; }
            set
            {
                _cameraSpeed = value;
                NotifyChangedStatic();
            }
        }

        /// <summary>
        /// The speed of the camera when the shift key is held.
        /// </summary>
        [EditorVisible("Camera", "Camera Shift Speed")]
        public static float CameraShiftSpeed
        {
            get { return _cameraShiftSpeed; }
            set
            {
                _cameraShiftSpeed = value;
                NotifyChangedStatic();
            }
        }

        /// <summary>
        /// Determines if the camera speed should gradually increase while the forward key is held.
        /// </summary>
        [EditorVisible("Camera", "Camera Speedup")]
        public static bool CameraSpeedup
        {
            get { return _cameraSpeedup; }
            set
            {
                _cameraSpeedup = value;
                NotifyChangedStatic();
            }
        }

        /// <summary>
        /// The master volume.
        /// </summary>
        [EditorVisible("Audio", "Master Volume")]
        public static float MasterVolume
        {
            get { return _masterVolume; }
            set
            {
                _masterVolume = value;
                SoundService.UpdateMix();
                NotifyChangedStatic();
            }
        }

        /// <summary>
        /// The global volume modifier for sound sound effects.
        /// </summary>
        [EditorVisible("Audio", "Music Volume")]
        public static float MusicVolume
        {
            get { return _musicVolume; }
            set
            {
                if (value == _musicVolume) return;
                _musicVolume = value;
                NotifyChangedStatic();
            }
        }

        /// <summary>
        /// The global volume modifier for sound effects.
        /// </summary>
        [EditorVisible("Audio", "Effects Volume")]
        public static float EffectsVolume
        {
            get { return _effectsVolume; }
            set
            {
                if (value == _effectsVolume) return;
                _effectsVolume = value;
                NotifyChangedStatic();
            }
        }

        /// <summary>
        /// The global volume modifier for dialogue.
        /// </summary>
        [EditorVisible("Audio", "Dialogue Volume")]
        public static float DialogueVolume
        {
            get { return _dialogueVolume; }
            set
            {
                if (value == _dialogueVolume) return;
                _dialogueVolume = value;
                NotifyChangedStatic();
            }
        }

        /// <summary>
        /// The speaker type.
        /// </summary>
        [EditorVisible("Audio", "Speaker Type")]
        public static SpeakerType SpeakerType
        {
            get { return _speakerType; }
            set
            {
                if (value == _speakerType) return;
                _speakerType = value;
                NotifyChangedStatic();
            }
        }

        /// <summary>
        /// The speaker configuration.
        /// </summary>
        [EditorVisible("Audio", "Speaker Configuration")]
        public static SpeakerConfiguration SpeakerConfiguration
        {
            get { return _speakerConfiguration; }
            set
            {
                if (value == _speakerConfiguration) return;
                _speakerConfiguration = value;
                NotifyChangedStatic();
                SoundService.Reset();
            }
        }

        /// <summary>
        /// Determines if VOIP is enabled.
        /// </summary>
        [EditorVisible("VOIP", "VOIP Enabled")]
        public static bool VoipEnabled
        {
            get { return _voipEnabled; }
            set
            {
                if (value == _voipEnabled) return;
                _voipEnabled = value;
                NotifyChangedStatic();
            }
        }

        /// <summary>
        /// The global volume modifier for voice comms.
        /// </summary>
        [EditorVisible("VOIP", "VOIP Volume")]
        public static float VoipVolume
        {
            get { return _voipVolume; }
            set
            {
                if (value == _voipVolume) return;
                _voipVolume = value;
                NotifyChangedStatic();
            }
        }

        /// <summary>
        /// Returns a table of custom settings.
        /// </summary>
        public LuaTable GetCustomSettings()
        {
            return _definedSettings.ToLuaTable();
        }

        /// <summary>
        /// Sets the value of a custom setting.
        /// </summary>
        /// <param name="key">The name of the setting.</param>
        /// <param name="value">The value to set.</param>
        public void SetCustomSetting(string key, string value)
        {
            _definedSettings[key] = value;
            CustomSettingChanged.Fire(key, value);
        }

        /// <summary>
        /// Returns the value of a custom setting.
        /// </summary>
        /// <param name="key">The name of the setting.</param>
        public string GetCustomSetting(string key)
        {
            string value;
            _definedSettings.TryGetValue(key, out value);
            return value;
        }

        private static void NotifyChangedStatic([CallerMemberName] string propertyName = null)
        {
            Engine.UserSettings?.UserGameSettings?.NotifyChanged(propertyName);
        }

        /// <inheritdoc />
        public override void RestoreDefaults()
        {
            MouseSensitivityFirstPerson = 0.03f;
            MouseSensitivityThirdPerson = 0.03f;
            MouseSensitivityVehicle = 1.0f;

            CameraSpeed = 1.5f;
            CameraShiftSpeed = 0.2f;
            CameraSpeedup = true;

            Brightness = 1.0f;
            FieldOfView = 60;

            MasterVolume = 1.0f;
            MusicVolume = 1.0f;
            EffectsVolume = 1.0f;
            SpeakerType = SpeakerType.HiFi;
            SpeakerConfiguration = SpeakerConfiguration.Stero;

            FullscreenMonitor = 0;
            ColourBlindSupport = ColourBlindness.None;

            VoipEnabled = true;
            VoipVolume = 1;
        }
    }
}