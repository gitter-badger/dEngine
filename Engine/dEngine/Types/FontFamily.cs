// FontFamily.cs - dEngine
// Copyright (C) 2016-2016 DanDevPC (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//  
// You should have received a copy of the GNU General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.IO;

namespace dEngine
{
    /// <summary>
    /// A font family.
    /// </summary>
    public struct FontFamily : IDataType, IFormattable
    {
        private string _family;

        /// <summary />
        public FontFamily(string family)
        {
            _family = family;
        }
        
#pragma warning disable 1591
        public static FontFamily Arial = new FontFamily("Arial");
        public static FontFamily ComicSans = new FontFamily("Comic Sans");
        public static FontFamily TimesNewRoman = new FontFamily("Times New Roman");
        public static FontFamily Helvetica = new FontFamily("Helvetica");
        public static FontFamily SourceSans = new FontFamily("Source Sans");
        public static FontFamily Roboto = new FontFamily("Roboto");
#pragma warning restore 1591

        /// <summary />
        public override string ToString()
        {
            return _family ?? Arial;
        }

        /// <summary />
        public string ToString(string format, IFormatProvider formatProvider)
        {
            return ToString();
        }

        /// <summary />
        public static implicit operator FontFamily(string family)
        {
            return new FontFamily(family);
        }

        /// <summary />
        public static implicit operator string(FontFamily family)
        {
            return family.ToString();
        }

        /// <summary />
        public void Save(BinaryWriter writer)
        {
            writer.Write(ToString());
        }

        /// <summary />
        public void Load(BinaryReader reader)
        {
            _family = reader.ReadString();
        }
    }
}