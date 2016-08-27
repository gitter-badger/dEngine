// CameraData.cs - dEngine
// Copyright (C) 2015-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System.Runtime.InteropServices;
using SharpDX;

namespace dEngine.Graphics.Structs
{
	/// <summary>
	/// Camera data structure for shaders.
	/// </summary>
	[StructLayout(LayoutKind.Explicit, Pack = 16, Size = 240)]
	internal struct CameraData : IConstantBufferData
	{
#pragma warning disable 169
        [FieldOffset(0)]
		public Matrix ViewMatrix;
        [FieldOffset(64)]
        public Matrix ViewProjectionMatrix;
        [FieldOffset(128)]
        public Matrix InverseViewProjection;
        [FieldOffset(192)]
        public Vector3 Position;
        [FieldOffset(204)]
        private readonly float _pad0;
        [FieldOffset(208)]
        public float ClipNear;
        [FieldOffset(212)]
        public float ClipFar;
        [FieldOffset(216)]
        public float Unused0;
        [FieldOffset(220)]
        public float Unused1;
        [FieldOffset(224)]
        public SharpDX.Vector4 ScreenParams; // Width, Height,
#pragma warning restore 169

		public static int Stride = Marshal.SizeOf<CameraData>();
	}
}