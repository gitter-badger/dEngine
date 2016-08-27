// RawInputDefinitions.cs - dEngine
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
	internal static class RawInputDefinitions
	{
		/// <summary>
		/// Enumeration containing the type device the raw input is coming from.
		/// </summary>
		public enum RawInputType
		{
			/// <summary>
			/// Mouse input.
			/// </summary>
			Mouse = 0,

			/// <summary>
			/// Keyboard input.
			/// </summary>
			Keyboard = 1,

			/// <summary>
			/// Human interface device input.
			/// </summary>
			HID = 2,

			/// <summary>
			/// Another device that is not the keyboard or the mouse.
			/// </summary>
			Other = 3
		}

		/// <summary>
		/// Enumeration containing flags for raw keyboard input.
		/// </summary>
		[Flags]
		public enum RawKeyboardFlags : ushort
		{
			/// <summary></summary>
			KeyMake = 0,

			/// <summary></summary>
			KeyBreak = 1,

			/// <summary></summary>
			KeyE0 = 2,

			/// <summary></summary>
			KeyE1 = 4,

			/// <summary></summary>
			TerminalServerSetLED = 8,

			/// <summary></summary>
			TerminalServerShadow = 0x10,

			/// <summary></summary>
			TerminalServerVKPACKET = 0x20
		}

		/// <summary>
		/// Enumeration containing the button data for raw mouse input.
		/// </summary>
		[Flags]
		public enum RawMouseButtons
			: ushort
		{
			/// <summary>No button.</summary>
			None = 0,

			/// <summary>Left (button 1) down.</summary>
			LeftDown = 0x0001,

			/// <summary>Left (button 1) up.</summary>
			LeftUp = 0x0002,

			/// <summary>Right (button 2) down.</summary>
			RightDown = 0x0004,

			/// <summary>Right (button 2) up.</summary>
			RightUp = 0x0008,

			/// <summary>Middle (button 3) down.</summary>
			MiddleDown = 0x0010,

			/// <summary>Middle (button 3) up.</summary>
			MiddleUp = 0x0020,

			/// <summary>Button 4 down.</summary>
			Button4Down = 0x0040,

			/// <summary>Button 4 up.</summary>
			Button4Up = 0x0080,

			/// <summary>Button 5 down.</summary>
			Button5Down = 0x0100,

			/// <summary>Button 5 up.</summary>
			Button5Up = 0x0200,

			/// <summary>Mouse wheel moved.</summary>
			MouseWheel = 0x0400
		}


		/// <summary>
		/// Contains the raw input from a device.
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		internal struct RawInput
		{
			/// <summary>
			/// Header for the data.
			/// </summary>
			public RawInputHeader Header;

			public Union Data;

			[StructLayout(LayoutKind.Explicit)]
			public struct Union
			{
				/// <summary>
				/// Mouse raw input data.
				/// </summary>
				[FieldOffset(0)] public RawMouse Mouse;

				/// <summary>
				/// Keyboard raw input data.
				/// </summary>
				[FieldOffset(0)] public RawKeyboard Keyboard;

				/// <summary>
				/// HID raw input data.
				/// </summary>
				[FieldOffset(0)] public RawHID HID;
			}
		}

		/// <summary>
		/// Value type for raw input from a HID.
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		public struct RawHID
		{
			/// <summary>Size of the HID data in bytes.</summary>
			public int Size;

			/// <summary>Number of HID in Data.</summary>
			public int Count;

			/// <summary>Data for the HID.</summary>
			public IntPtr Data;
		}

		/// <summary>
		/// Value type for a raw input header.
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		internal struct RawInputHeader
		{
			/// <summary>Type of device the input is coming from.</summary>
			public RawInputType Type;

			/// <summary>Size of the packet of data.</summary>
			public int Size;

			/// <summary>Handle to the device sending the data.</summary>
			public IntPtr Device;

			/// <summary>wParam from the window message.</summary>
			public IntPtr wParam;
		}

		/// <summary>
		/// Contains information about the state of the mouse.
		/// </summary>
		[StructLayout(LayoutKind.Explicit)]
		internal struct RawMouse
		{
			/// <summary>
			/// The mouse state.
			/// </summary>
			[FieldOffset(0)] public RawMouseFlags Flags;

			/// <summary>
			/// Flags for the event.
			/// </summary>
			[FieldOffset(4)] public RawMouseButtons ButtonFlags;

			/// <summary>
			/// If the mouse wheel is moved, this will contain the delta amount.
			/// </summary>
			[FieldOffset(6)] public ushort ButtonData;

			/// <summary>
			/// Raw button data.
			/// </summary>
			[FieldOffset(8)] public uint RawButtons;

			/// <summary>
			/// The motion in the X direction. This is signed relative motion or
			/// absolute motion, depending on the value of usFlags.
			/// </summary>
			[FieldOffset(12)] public int LastX;

			/// <summary>
			/// The motion in the Y direction. This is signed relative motion or absolute motion,
			/// depending on the value of usFlags.
			/// </summary>
			[FieldOffset(16)] public int LastY;

			/// <summary>
			/// The device-specific additional information for the event.
			/// </summary>
			[FieldOffset(20)] public uint ExtraInformation;
		}

		/// <summary>
		/// Enumeration containing the flags for raw mouse data.
		/// </summary>
		[Flags]
		internal enum RawMouseFlags : ushort
		{
			/// <summary>Relative to the last position.</summary>
			MoveRelative = 0,

			/// <summary>Absolute positioning.</summary>
			MoveAbsolute = 1,

			/// <summary>Coordinate data is mapped to a virtual desktop.</summary>
			VirtualDesktop = 2,

			/// <summary>Attributes for the mouse have changed.</summary>
			AttributesChanged = 4
		}

		/// <summary>
		/// Value type for raw input from a keyboard.
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		public struct RawKeyboard
		{
			/// <summary>Scan code for key depression.</summary>
			public short MakeCode;

			/// <summary>Scan code information.</summary>
			public RawKeyboardFlags Flags;

			/// <summary>Reserved.</summary>
			public short Reserved;

			/// <summary>Virtual key code.</summary>
			public System.Windows.Forms.Keys VirtualKey;

			/// <summary>Corresponding window message.</summary>
			public uint Message;

			/// <summary>Extra information.</summary>
			public int ExtraInformation;
		}
	}
}