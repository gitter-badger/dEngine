// WeightedVertex.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System.IO;
using System.Runtime.InteropServices;


namespace dEngine.Graphics.Structs
{
#pragma warning disable 1591
	/// <summary>
	/// Vertex data structure for shaders.
	/// </summary>
	[StructLayout(LayoutKind.Explicit, Size =
		sizeof(uint) * RenderConstants.MaxBonesPerVertex + sizeof(float) * RenderConstants.MaxBonesPerVertex)]
	internal struct WeightedVertex : IDataType
	{
		[InstMember(1), FieldOffset(0)] public uint[] BoneIDs;
		[InstMember(2), FieldOffset(sizeof(uint) * RenderConstants.MaxBonesPerVertex)] public float[] BoneWeights;

		public WeightedVertex(uint[] boneIds, float[] boneWeights)
		{
			BoneIDs = boneIds;
			BoneWeights = boneWeights;
		}

		public static int Stride = Marshal.SizeOf<WeightedVertex>();

	    public void Load(BinaryReader reader)
	    {
	        var boneCount = RenderConstants.MaxBonesPerVertex;

            BoneIDs = new uint[RenderConstants.MaxBonesPerVertex];
            BoneWeights = new float[RenderConstants.MaxBonesPerVertex];

	        for (int i = 0; i < boneCount; i++)
	            BoneIDs[i] = reader.ReadUInt32();

            for (int i = 0; i < boneCount; i++)
                BoneWeights[i] = reader.ReadSingle();
        }

	    public void Save(BinaryWriter writer)
	    {
	        var boneCount = RenderConstants.MaxBonesPerVertex;
            
            for (int i = 0; i < boneCount; i++)
                writer.Write(BoneIDs[i]);

            for (int i = 0; i < boneCount; i++)
                writer.Write(BoneWeights[i]);
        }
	}
#pragma warning restore 1591
}