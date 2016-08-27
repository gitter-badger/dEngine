// Vertex.cs - dEngine
// Copyright (C) 2015-2016 DanDev (dandev.sco@gmail.com)
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
    internal struct Vertex : IDataType
	{
		[InstMember(1)] public Vector3 Position;
		[InstMember(2)] public Vector3 Normal;
		[InstMember(3)] public Vector2 TexCoord;
		[InstMember(4)] public Colour Colour;
		[InstMember(5)] public Vector3 Tangent;
		[InstMember(6)] public Vector3 BiTangent;

		public Vertex(Vector3 position, Vector3 normal, Vector2 texCoord, Colour colour, Vector3 tangent,
			Vector3 biTangent)
		{
			Position = position;
			Normal = normal;
			TexCoord = texCoord;
			Colour = colour;
			Tangent = tangent;
			BiTangent = biTangent;
		}

		public static int Stride = Marshal.SizeOf<Vertex>();

	    public void Load(BinaryReader reader)
	    {
	    }

	    public void Save(BinaryWriter writer)
        {
            Position.Save(writer);
            Normal.Save(writer);
            TexCoord.Save(writer);
            Colour.Save(writer);
            Tangent.Save(writer);
            BiTangent.Save(writer);
        }
	}
#pragma warning restore 1591
}