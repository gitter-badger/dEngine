// EditorBase.cs - dEditor
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Linq;
using dEditor.Framework;
using dEngine;
using dEngine.Serializer.V1;
using dEngine.Services;
using dEngine.Utility;

namespace dEditor.Widgets.Properties.Inspectors
{
    /// <summary>
    /// Base class for 'inspectors' - controls that represent value editors.
    /// </summary>
    public abstract class EditorBase<TValue> : InspectorBase, IEditor
    {
        protected EditorBase(object obj, Inst.CachedProperty propDesc)
        {
            Objects = new Dictionary<object, BoundPropertyInfo>();

            FirstObject = new KeyValuePair<object, BoundPropertyInfo>(obj, AddObject(obj, propDesc));

            DisplayName = FirstObject.Value.DisplayName;

            var instance = obj as dEngine.Instances.Instance;

            if (instance != null)
            {
                instance.Changed.Event += propertyName =>
                {
                    if (propertyName == propDesc.Name)
                    {
                        NotifyOfPropertyChange(() => Value);
                    }
                };
            }
        }

        public dynamic Value
        {
            get
            {
                if (!AreMultipleValuesSame) return default(TValue);
                var value = FirstObject.Value.Value;
                return value;
            }
            set
            {
                var action = new SetPropertyAction(Objects.Values, value);
                if (PropertiesWidget.UseHistoryService)
                {
                    HistoryService.ExecuteAction(action);
                    HistoryService.Waypoint(FirstObject.Value.DisplayName);
                    //NotifyOfPropertyChange(() => Value);
                }
                else
                {
                    action.Execute();
                }
            }
        }

        /// <summary>
        /// Gets whether or not all objects share the same value.
        /// </summary>
        public bool AreMultipleValuesSame
        {
            get { return Objects.Values.All(x => Equals(x.Value, FirstObject.Value.Value)); }
        }

        public KeyValuePair<object, BoundPropertyInfo> FirstObject { get; }

        public override string Name => DisplayName;

        public PropertiesViewModel PropertiesWidget { get; set; }

        public string DisplayName { get; set; }

        public override bool IsReadOnly => FirstObject.Value.IsReadOnly;

        public override string Description
        {
            get
            {
                string desc = Name;
                var prop = FirstObject.Value.Property;
                Comments.Comment comment;
                API.Comments.Get($"P:{prop.DeclaringType}.{prop.Name}", out comment);
                desc = comment.Summary;
                return desc;
            }
        }

        public Dictionary<object, BoundPropertyInfo> Objects { get; }

        public BoundPropertyInfo AddObject(object obj, Inst.CachedProperty property)
        {
            var bound = new BoundPropertyInfo(obj, property);
            Objects.Add(obj, bound);
            return bound;
        }

        public void RemoveObject(object obj)
        {
            BoundPropertyInfo descriptor;

            Objects.TryGetValue(obj, out descriptor);
            Objects.Remove(obj);
        }

        public int CompareTo(IEditor other)
        {
            return string.CompareOrdinal(DisplayName, other.DisplayName);
        }

        private class SetPropertyAction : HistoryService.HistoryAction
        {
            private readonly object _newValue;
            private readonly List<object> _originalValues;
            private readonly IEnumerable<BoundPropertyInfo> _props;

            public SetPropertyAction(IEnumerable<BoundPropertyInfo> props, object value)
            {
                _originalValues = new List<object>(16);
                foreach (var prop in props)
                    _originalValues.Add(prop.Value);

                _newValue = value;
                _props = props;
            }

            public override void Undo()
            {
                int i = 0;
                foreach (var prop in _props)
                    prop.Value = _originalValues[i++];
            }

            public override void Execute()
            {
                foreach (var prop in _props)
                    prop.Value = _newValue;
            }
        }
    }
}