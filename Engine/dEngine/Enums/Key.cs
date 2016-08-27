﻿// Key.cs - dEngine
// Copyright (C) 2015-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System;

namespace dEngine
{
#pragma warning disable 1591
    [Flags]
    public enum Key : short
	{
		Unknown = 0,
		LButton = 1,
		RButton = 2,
		Cancel = 3,
		MButton = 4,
		XButton1 = 5,
		XButton2 = 6,
		Backspace = 8,
		Tab = 9,
		LineFeed = 10,
		Clear = 12,
		Return = 13,
		Enter = Return,
		//ShiftKey = 16,
		//ControlKey = 17,
		//AltKey = 18,
		Pause = 19,
		CapsLock = 20,
		Space = 32,
		PageUp = 33,
		PageDown = 34,
		End = 35,
		Home = 36,
		Left = 37,
		Up = 38,
		Right = 39,
		Down = 40,
		Select = 41,
		Print = 42,
		Execute = 43,
		PrintScreen = 44,
		Insert = 45,
		Delete = 46,
		Help = 47,
		Num0 = 48,
		Num1 = 49,
		Num2 = 50,
		Num3 = 51,
		Num4 = 52,
		Num5 = 53,
		Num6 = 54,
		Num7 = 55,
		Num8 = 56,
		Num9 = 57,
		A = 65,
		B = 66,
		C = 67,
		D = 68,
		E = 69,
		F = 70,
		G = 71,
		H = 72,
		I = 73,
		J = 74,
		K = 75,
		L = 76,
		M = 77,
		N = 78,
		O = 79,
		P = 80,
		Q = 81,
		R = 82,
		S = 83,
		T = 84,
		U = 85,
		V = 86,
		W = 87,
		X = 88,
		Y = 89,
		Z = 90,
		LeftWin = 91,
		RightWin = 92,
		Apps = 93,
		Sleep = 95,
		NumPad0 = 96,
		NumPad1 = 97,
		NumPad2 = 98,
		NumPad3 = 99,
		NumPad4 = 100,
		NumPad5 = 101,
		NumPad6 = 102,
		NumPad7 = 103,
		NumPad8 = 104,
		NumPad9 = 105,
		Multiply = 106,
		Add = 107,
		Separator = 108,
		Subtract = 109,
		Decimal = 110,
		Divide = 111,
		F1 = 112,
		F2 = 113,
		F3 = 114,
		F4 = 115,
		F5 = 116,
		F6 = 117,
		F7 = 118,
		F8 = 119,
		F9 = 120,
		F10 = 121,
		F11 = 122,
		F12 = 123,
		F13 = 124,
		F14 = 125,
		F15 = 126,
		F16 = 127,
		F17 = 128,
		F18 = 129,
		F19 = 130,
		F20 = 131,
		F21 = 132,
		F22 = 133,
		F23 = 134,
		F24 = 135,
		NumLock = 144,
		Scroll = 145,
		LeftShift = 160,
		RightShift = 161,
		LeftControl = 162,
		RightControl = 163,
		LeftAlt = 164,
		RightAlt = 165,
		BrowserBack = 166,
		BrowserForward = 167,
		BrowserRefresh = 168,
		BrowserStop = 169,
		BrowserSearch = 170,
		BrowserFavorites = 171,
		BrowserHome = 172,
		VolumeMute = 173,
		VolumeDown = 174,
		VolumeUp = 175,
		MediaNextTrack = 176,
		MediaPreviousTrack = 177,
		MediaStop = 178,
		MediaPlayPause = 179,
		LaunchMail = 180,
		SelectMedia = 181,
		LaunchApplication1 = 182,
		LaunchApplication2 = 183,
		Semicolon = 186,
		Plus = 187,
		Comma = 188,
		Minus = 189,
		FullStop = 190,
		ForwardSlash = 191,
		Apostrophe = 192,
		LeftBracket = 219,
		Backslash = 220,
		RightBracket = 221,
		HashSymbol = 222,
		Grave = 223,
		AmericanBackslash = 226,
		ProcessKey = 229,
		Packet = 231,
		Attn = 246,
		Crsel = 247,
		Exsel = 248,
		EraseEof = 249,
		Play = 250,
		Zoom = 251,
		NoName = 252,
		Pa1 = 253,
		OemClear = 254,
        GamepadX = 1000,
		GamepadY = 1001,
		GamepadA = 1002,
		GamepadB = 1003,
		GamepadLB = 1004,
		GamepadRB = 1005,
		GamepadStart = 1006,
		GamepadSelect = 1007,
		GamepadUp = 1008,
		GamepadDown = 1009,
		GamepadLeft = 1010,
		GamepadRight = 1011,
		GamepadThumbLeft = 1012,
		GamepadThumbRight = 1013
	}
#pragma warning restore 1591
}