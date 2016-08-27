// StatsItem.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using dEngine.Instances.Attributes;
using dEngine.Services;

#pragma warning disable 1591

namespace dEngine.Instances.Diagnostics
{
    [TypeId(204), Uncreatable]
    public class StatsItem : Instance
    {
        protected double _value;
        protected string _valueString;

        public StatsItem()
        {
            Archivable = false;
            Parent = Game.Stats;
            ParentLocked = true;
        }

        internal StatsItem(string name, Instance parent)
        {
            Name = name;
            Parent = parent;
        }

        internal double Value { get { return _value; } set { _value = value; } }
        internal string ValueString { get { return _valueString ?? _value.ToString(); } set { _valueString = value; } }

        [ScriptSecurity(ScriptIdentity.Plugin)]
        public double GetValue()
        {
            ScriptService.AssertIdentity(ScriptIdentity.Plugin);
            return Value;
        }

        [ScriptSecurity(ScriptIdentity.Plugin)]
        public string GetValueString()
        {
            ScriptService.AssertIdentity(ScriptIdentity.Plugin);
            return ValueString;
        }
    }
}