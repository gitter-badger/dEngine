// NVAPI.cs - dEngine
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
using System.Runtime.InteropServices;

namespace dEngine.Utility
{
    internal static class NvApi
    {
        public enum ThermalTarget
        {
            None = 0,
            Gpu = 1,
            Memory = 2,
            PowerSupply = 4,
            Board = 8,
            VcdBoard = 9,
            VcdInlet = 10,
            VcdOutlet = 11,
            All = 15,
            Unknown = -1
        }

        public const int MaxPhysicalGpus = 64;
        public const int MaxUsagesPerGpu = 34;

        internal static int GpuCount;
        internal static IntPtr[] GpuHandles = new IntPtr[MaxPhysicalGpus];

        static unsafe NvApi()
        {
            Initialize = Marshal.GetDelegateForFunctionPointer<NvAPI_Initialize>(QueryInterface(0x0150E828));
            EnumPhysicalGpus = Marshal.GetDelegateForFunctionPointer<NvAPI_EnumPhysicalGPUs>(QueryInterface(0xE5AC921F));
            GetThermalSettings =
                Marshal.GetDelegateForFunctionPointer<NvAPI_GPU_GetThermalSettings>(QueryInterface(0xE3640A56));
            Initialize();
            UpdateGpuHandles();
        }

        internal static NvAPI_Initialize Initialize { get; }
        internal static NvAPI_EnumPhysicalGPUs EnumPhysicalGpus { get; }
        internal static NvAPI_GPU_GetThermalSettings GetThermalSettings { get; }

        internal static unsafe void UpdateGpuHandles()
        {
            var ptr = (int*)Marshal.AllocHGlobal(MaxPhysicalGpus * Marshal.SizeOf(typeof(IntPtr)));
            var ptr2 = (int*)Marshal.AllocHGlobal(4);
            int test = 0;

            EnumPhysicalGpus(ref ptr, ref test);

            ThermalSettings temp = new ThermalSettings();
            GetThermalSettings(IntPtr.Zero, 0, ref temp);

            for (int i = 0; i < (int)ptr2; i++)
            {
                GpuHandles[i] = Marshal.ReadIntPtr((IntPtr)ptr, IntPtr.Size* i);
            }
        }

        [DllImport("nvapi64.dll", EntryPoint = "nvapi_QueryInterface")]
        internal static extern IntPtr QueryInterface(uint offset);

        internal delegate int NvAPI_Initialize();

        internal unsafe delegate int NvAPI_EnumPhysicalGPUs(ref int* handles, ref int count);

        internal delegate int NvAPI_GPU_GetThermalSettings(IntPtr handle, int sensorIndex, ref ThermalSettings temp);

        [StructLayout(LayoutKind.Sequential, Size = 17)]
        public class Sensor
        {
            /// <summary>
            /// ADM1032, MAX6649...
            /// </summary>
            public int Controller;

            /// <summary>
            /// The current temperature value of the thermal sensor in degree Celsius
            /// </summary>
            public int CurrentTemp;

            /// <summary>
            /// The max default temperature value of the thermal sensor in degree Celsius
            /// </summary>
            public int DefaultMaxTemp;

            /// <summary>
            /// The min default temperature value of the thermal sensor in degree Celsius
            /// </summary>
            public int DefaultMinTemp;

            /// <summary>
            /// Thermal sensor targeted @ GPU, memory, chipset, powersupply, Visual Computing Device, etc
            /// </summary>
            public ThermalTarget Target;
        }

        [StructLayout(LayoutKind.Explicit, Size = 8 + (17 * MaxPhysicalGpus))]
        public class ThermalSettings
        {
            [FieldOffset(4)] public int Count;
            [FieldOffset(8)] public Sensor[] Sensors;
            [FieldOffset(0)] public int Version;
        }
    }
}