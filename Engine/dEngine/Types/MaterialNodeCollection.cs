// MaterialNodeCollection.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.IO;
using dEngine.Instances.Materials;
using dEngine.Serializer.V1;
#pragma warning disable 1591

namespace dEngine
{
    public class MaterialNodeCollection : List<Node>, IDataType
    {
        
        public void Save(BinaryWriter writer)
        {
            var count = Count;
            writer.Write(count);
            for (int i = 0; i < count; i++)
            {
                Inst.Serialize(this[i], writer.BaseStream);
            }
        }

        public void Load(BinaryReader reader)
        {
            Clear();
            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var item = (Node)Inst.Deserialize(reader.BaseStream);
                Add(item);
            }
        }
    }
}