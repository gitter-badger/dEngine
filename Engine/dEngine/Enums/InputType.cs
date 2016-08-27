// InputType.cs - dEngine
// Copyright (C) 2015-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.


#pragma warning disable 1591

namespace dEngine
{
	/// <summary>
	/// Enum for input types.
	/// </summary>
	public enum InputType
	{
		None = 0,
		MouseButton1 = 1,
		MouseButton2 = 2,
		MouseButton3 = 3,
		MouseWheel = 4,
		MouseMovement = 5,
		Touch = 7,
		Keyboard = 8,
		Focus = 9,
		Accelerometer = 10,
		Gyro = 11,
		GamepadButton = 12,
		GamepadLeftStick = 13,
		GamepadRightStick = 14,
		GamepadLeftTrigger = 15,
		GamepadRightTrigger = 16,
		MouseButton4 = 17,
		MouseButton5 = 18
	}
}