// ScriptSourceCode.cs - dEngine
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
using dEngine.Instances.Attributes;
using dEngine.Utility.Extensions;

namespace dEngine.Data
{
    /// <summary>
    /// Asset that contains a text string.
    /// </summary>
    [TypeId(26)]
    public class TextSource : AssetBase
    {
        /// <summary>
        /// The source code.
        /// </summary>
        public string Text { get; set; }

        /// <inheritdoc />
        public override ContentType ContentType => ContentType.Text;

        /// <inheritdoc />
        protected override void OnSave(BinaryWriter writer)
        {
            writer.Write(Text.ToCharArray());
        }

        /// <inheritdoc />
        protected override void OnLoad(BinaryReader reader)
        {
            Text = new string(reader.ReadChars((int)reader.BaseStream.Length));
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
        }
    }
}