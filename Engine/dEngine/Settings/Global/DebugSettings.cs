// DebugSettings.cs - dEngine
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
using System.Diagnostics;
using System.Management;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using dEngine.Graphics;
using dEngine.Instances.Attributes;
using Neo.IronLua;

using SharpDX.DXGI;

namespace dEngine.Settings.Global
{
	/// <summary>
	/// Debug settings.
	/// </summary>
	[TypeId(188)]
	public class DebugSettings : Settings
	{
		private static readonly Process _currentProcess = Process.GetCurrentProcess();
		private static Key _toggleConsoleKey;
		private static int _consoleLines;
		private static bool _loggingEnabled;
		private static bool _debugEngineEnabled;
		private static bool _logHistoryActions;
		private static bool _logHistoryEvents;
		private static bool _logHistoryWaypoints;

		/// <summary>
		/// The name of the CPU you have installed in your computer.
		/// </summary>
		[EditorVisible("Profile")]
		public static string CpuName { get; private set; }

		/// <summary>
		/// The clock speed of the CPU in MHz.
		/// </summary>
		[EditorVisible("Profile")]
		public static uint CpuSpeed { get; private set; }

		/// <summary>
		/// The number of logical processors on the CPU.
		/// </summary>
		[EditorVisible("Profile")]
		public static uint CpuLogicalProcessors { get; private set; }

		/// <summary>
		/// The number of cores on the CPU.
		/// </summary>
		[EditorVisible("Profile")]
		public static uint CpuCores { get; private set; }

		/// <summary>
		/// The name of the GPU currently being used.
		/// </summary>
		[EditorVisible("Profile")]
		public static string GpuName { get; private set; }

        /// <summary>
        /// The GPU vendor.
        /// </summary>
        [EditorVisible("Profile")]
        public static GpuVendor GpuVendor { get; private set; }

        /// <summary>
        /// The PCI ID of the GPU currently being used.
        /// </summary>
        [EditorVisible("Profile")]
		public static int GpuId { get; private set; }

        /// <summary>
        /// The name of the current audio playback device.
        /// </summary>
        [EditorVisible("Profile")]
        public static string PlaybackDeviceName { get; internal set; }

        /// <summary>
        /// The platform the game is running on.
        /// </summary>
        [EditorVisible("Profile")]
		public static PlatformID OsPlatform => Environment.OSVersion.Platform;

        /// <summary>
        /// The platform ID.
        /// </summary>
        [EditorVisible("Profile")]
		public static byte OsPlatformId => (byte)Environment.OSVersion.Platform;

		/// <summary>
		/// The version number of the operating system.
		/// </summary>
		[EditorVisible("Profile")]
		public static string OsVersion => Environment.OSVersion.VersionString;

		/// <summary>
		/// Determines if the operating system is 64 bit.
		/// </summary>
		[EditorVisible("Profile")]
		public static bool OsIs64Bit => Environment.Is64BitOperatingSystem;

		/// <summary>
		/// Determines if the process' priority is boosted when it has focus.
		/// </summary>
		[EditorVisible("Profile")]
		public static bool ProcPriorityBoost => Process.GetCurrentProcess().PriorityBoostEnabled;

		/// <summary>
		/// The base priority of the process.
		/// </summary>
		[EditorVisible("Profile")]
		public static int ProcBasePriority => _currentProcess.BasePriority;

		/// <summary>
		/// The privileged processor time for this process.
		/// </summary>
		[EditorVisible("Profile")]
		public static double ProcPrivilegedProcessorTime => _currentProcess.PrivilegedProcessorTime.TotalSeconds;

		/// <summary>
		/// The key to use for toggling the command console.
		/// </summary>
		[EditorVisible("Console")]
		public static Key ToggleConsoleKey
		{
			get { return _toggleConsoleKey; }
			set
			{
				_toggleConsoleKey = value;
				NotifyChangedStatic();
			}
		}

		/// <summary>
		/// The number of lines the console displays.
		/// </summary>
		[EditorVisible("Console")]
		public static int ConsoleLines
		{
			get { return _consoleLines; }
			set
			{
				_consoleLines = value;
				NotifyChangedStatic();
			}
		}

		/// <summary>
		/// Determines if log files are written to.
		/// </summary>
		[EditorVisible("Logging")]
		public static bool LoggingEnabled
		{
			get { return _loggingEnabled; }
			set
			{
				_loggingEnabled = value;
				NotifyChangedStatic();
			}
		}

		/// <summary>
		/// Determines if HistoryService actions are logged.
		/// </summary>
		[EditorVisible("Logging")]
		public static bool LogHistoryActions
		{
			get { return _logHistoryActions; }
			set
			{
				_logHistoryActions = value;
				NotifyChangedStatic();
			}
		}

		/// <summary>
		/// Determines if HistoryService events are logged.
		/// </summary>
		[EditorVisible("Logging")]
		public static bool LogHistoryEvents
		{
			get { return _logHistoryEvents; }
			set
			{
				_logHistoryEvents = value;
				NotifyChangedStatic();
			}
		}

		/// <summary>
		/// Determines if HistoryService waypoints are logged.
		/// </summary>
		[EditorVisible("Logging")]
		public static bool LogHistoryWaypoints
		{
			get { return _logHistoryWaypoints; }
			set
			{
				_logHistoryWaypoints = value;
				NotifyChangedStatic();
			}
		}

	    private static bool _profilingEnabled;

	    /// <summary>
	    /// Determines if profiling is enabled.
	    /// </summary>
	    [EditorVisible("Benchmarking", "Profiling Enabled")]
	    public static bool ProfilingEnabled
        {
	        get { return _profilingEnabled; }
	        set
	        {
	            if (value == _profilingEnabled) return;
                _profilingEnabled = value;
	            NotifyChangedStatic();
	        }
	    }

	    private static bool _xaudio2ProfilingEnabled;

	    /// <summary>
	    /// Determines if profiling is enabled for the audio engine.
	    /// </summary>
	    [ EditorVisible("Benchmarking", "XAudio2 Profiling Enabled")]
	    public static bool XAudio2ProfilingEnabled
	    {
	        get { return _xaudio2ProfilingEnabled; }
	        set
	        {
	            if (value == _xaudio2ProfilingEnabled) return;
	            _xaudio2ProfilingEnabled = value;
	            NotifyChangedStatic();
	        }
	    }

        /// <summary>
        /// The manufacturer of the user's device.
        /// </summary>
        [EditorVisible("Profile")]
	    public static string Manufacturer { get; set; }

        /// <summary>
        /// The connection type of the current network adapter.
        /// </summary>
        [EditorVisible("Profile", "Connection Type")]
	    public static AdapterConnection ConnectionType { get; set; }

        /// <summary/>
	    public DebugSettings()
	    {
            foreach (var adapter in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (adapter.OperationalStatus == OperationalStatus.Up)
                {
                    switch (adapter.NetworkInterfaceType)
                    {
                        case NetworkInterfaceType.Wireless80211:
                            ConnectionType  = AdapterConnection.WiFi;
                            break;
                        default:
                            ConnectionType = AdapterConnection.Lan;
                            break;
                    }
                }
            }
        }

	    private static void NotifyChangedStatic([CallerMemberName] string propertyName = null)
		{
			Engine.Settings?.DebugSettings?.NotifyChanged(propertyName);
		}

		/// <inheritdoc />
		public override void RestoreDefaults()
		{
			FillSystemInfo();

			ToggleConsoleKey = Key.Grave;
			ConsoleLines = 13;
			LoggingEnabled = true;
		}

		/// <summary>
		/// Returns an array of adapters.
		/// </summary>
		/// <returns></returns>
		public LuaTable GetAdapters()
		{
			if (!Renderer.IsInitialized)
				throw new InvalidOperationException("Cannot get adapters: Renderer is not initialized.");

			var table = new LuaTable();
			for (int i = 0; i < Renderer.Factory.GetAdapterCount(); i++)
			{
				var adapter = Renderer.Factory.GetAdapter(i);
				table[i + 1] = adapter.Description;
			}
			return table;
		}

		private void FillSystemInfo()
		{
			switch (Environment.OSVersion.Platform)
			{
				case PlatformID.Win32S:
				case PlatformID.Win32Windows:
				case PlatformID.Win32NT:
				case PlatformID.WinCE:
					var processorSearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor");
					foreach (var cpu in processorSearcher.Get())
					{
						CpuName = (string)cpu["Name"];
						CpuSpeed = (uint)cpu["MaxClockSpeed"];
						CpuLogicalProcessors = (uint)cpu["NumberOfLogicalProcessors"];
						CpuCores = (uint)cpu["NumberOfCores"];
					}

					break;
				case PlatformID.Unix:
					break;
				case PlatformID.Xbox:
					break;
				case PlatformID.MacOSX:
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
            
            var computerSystem = new ManagementClass("Win32_ComputerSystem");
            var moCollection = computerSystem.GetInstances();
            if (moCollection.Count != 0)
                foreach (var obj in computerSystem.GetInstances())
                    Manufacturer = (string)obj["Manufacturer"];
        }

		internal static void FillGpuInfo(Adapter adapter)
		{
			GpuName = adapter.Description.Description;
			GpuId = adapter.Description.DeviceId;

		    var vendorId = adapter.Description.VendorId;
            
            switch (vendorId)
		    {
                case 0x10DE:
                    GpuVendor = GpuVendor.Nvidia;
                    break;
                case 0x1002:
                case 0x1022:
                    GpuVendor = GpuVendor.AMD;
                    break;
                case 0x163C:
                case 0x8086:
                case 0x8087:
                    GpuVendor = GpuVendor.Intel;
                    break;
                default:
                    GpuVendor = GpuVendor.Unknown;
		            break;
		    }
		}
	}
}