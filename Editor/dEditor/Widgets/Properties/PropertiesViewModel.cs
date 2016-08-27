// PropertiesViewModel.cs - dEditor
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using dEditor.Framework;
using dEditor.Instances;
using dEditor.Widgets.Properties.Inspectors;
using dEditor.Widgets.Properties.Inspectors.Checkbox;
using dEditor.Widgets.Properties.Inspectors.Colour;
using dEditor.Widgets.Properties.Inspectors.ColourSequence;
using dEditor.Widgets.Properties.Inspectors.Content;
using dEditor.Widgets.Properties.Inspectors.Enum;
using dEditor.Widgets.Properties.Inspectors.FontFamily;
using dEditor.Widgets.Properties.Inspectors.Instance;
using dEditor.Widgets.Properties.Inspectors.Number;
using dEditor.Widgets.Properties.Inspectors.NumberRange;
using dEditor.Widgets.Properties.Inspectors.TextBox;
using dEditor.Widgets.Properties.Inspectors.Time;
using dEditor.Widgets.Properties.Inspectors.Type;
using dEditor.Widgets.Properties.Inspectors.UDim2;
using dEditor.Widgets.Properties.Inspectors.Vector2;
using dEditor.Widgets.Properties.Inspectors.Vector3;
using dEditor.Widgets.Properties.Inspectors.Vector4;
using dEngine;
using dEngine.Instances;
using dEngine.Instances.Attributes;
using dEngine.Serializer.V1;
using dEngine.Services;
using dEngine.Settings;
using TimeSpan = dEngine.TimeSpan;


namespace dEditor.Widgets.Properties
{
    [Export(typeof(IProperties))]
    public class PropertiesViewModel : Widget, IProperties
    {
        private object _target;

        public PropertiesViewModel()
        {
            SelectionService.Service.Selected.Event += OnSelect;
        }

        public override string DisplayName
        {
            get
            {
                var count = SelectionService.SelectionCount;
                if (count == 0)
                {
                    return "Properties";
                }
                if (count == 1)
                {
                    var first = SelectionService.First();
                    return $"Properties - {first.ClassName} \"{first.Name}\"";
                }
                var className = SelectionService.Unanimous(x => x.ClassName);
                return className != null ? $"Properties - {className}" : "Properties";
            }
        }

        public override PaneLocation PreferredLocation => PaneLocation.Right;

        public List<InspectorBase> Inspectors { get; private set; }

        /// <summary>
        /// If true, the inspector will show properties for selected items, otherwise <see cref="Target" /> is used.
        /// </summary>
        public bool UseSelectionService { get; set; } = true;

        /// <summary>
        /// Determines if setting properties will set waypoints in <see cref="HistoryService" />.
        /// </summary>
        public bool UseHistoryService { get; set; } = true;

        /// <summary>
        /// Category names that are in this blacklist will not be shown.
        /// </summary>
        public HashSet<string> FilteredCategories { get; } = new HashSet<string>();

        /// <summary>
        /// If <see cref="UseSelectionService" /> is false, the inspector will display properties for this instance.
        /// </summary>
        public object Target
        {
            get { return _target; }
            set
            {
                DeselectItem(value);
                _target = value;
                SelectItems(new[] { value });
            }
        }

        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);

            if (close)
            {
                Game.Selection.Selected.Event -= OnSelect;
            }
        }

        private void SelectItems(IEnumerable<object> selection)
        {
            var groups = new SortedDictionary<string, GroupEntry>(StringComparer.Ordinal);

            foreach (var obj in selection)
            {
                var isSettings = obj is Settings;
                var bindingAttr = BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance;

                if (isSettings)
                {
                    bindingAttr |= BindingFlags.Static;
                }

                var type = Inst.CacheType(obj.GetType());
                var properties = type.Properties;

                foreach (var property in properties)
                {
                    if (!property.IsPublic) continue;
                    if (property.IsStatic && !isSettings) continue;
                    if (!property.IsStatic && isSettings) continue;

                    var visibleAttr = property.EditorVisible;

                    if (visibleAttr == null || FilteredCategories.Contains(visibleAttr.Group))
                        continue;

                    if (property.IsDeprecated && !EditorSettings.ShowDeprecated)
                        continue;

                    GroupEntry groupEntry;

                    var groupNeedsAdded = false;

                    if (!groups.TryGetValue(visibleAttr.Group, out groupEntry))
                    {
                        groupEntry = new GroupEntry();
                        groupNeedsAdded = true;
                    }

                    IEditor editor;
                    if (!groupEntry.Editors.TryGetValue(property.Name, out editor))
                    {
                        editor = GetEditor(obj, property, visibleAttr.Data);

                        if (editor != null)
                        {
                            groupEntry.Editors.Add(property.Name, editor);
                            editor.PropertiesWidget = this;
                        }
                        else
                            Debug.WriteLine($"No editor for type {property.PropertyType.Name}");
                    }
                    else
                    {
                        editor.AddObject(obj, property);
                    }

                    if (groupNeedsAdded && editor != null)
                        groups.Add(visibleAttr.Group, groupEntry);
                }
            }

            var inspectors = new List<InspectorBase>(12);
            foreach (var group in groups)
            {
                inspectors.Add(new CollapsibleGroupViewModel(group.Key, group.Value.Editors.Values.Cast<InspectorBase>()));
            }

            Inspectors = inspectors;
            NotifyOfPropertyChange(nameof(Inspectors));
            NotifyOfPropertyChange(nameof(DisplayName));
        }

        private void DeselectItem(object obj)
        {
            if (obj == null)
                return;

            var inspectors = Inspectors;

            if (inspectors == null)
                return;

            foreach (var inspector in inspectors)
            {
                var editor = inspector as EditorBase<IEditor>;

                if (editor?.Objects.ContainsKey(obj) == true)
                {
                    editor.RemoveObject(obj);

                    if (editor.Objects.Count == 0)
                    {
                        inspectors.Remove(editor);
                        NotifyOfPropertyChange(nameof(Inspectors));
                        NotifyOfPropertyChange(nameof(DisplayName));
                    }
                }
            }
        }

        private void OnSelect(Instance obj)
        {
            if (UseSelectionService)
            {
                Application.Current.Dispatcher.InvokeAsync(() => SelectItems(SelectionService.ToList()));
            }
        }

        private IEditor GetEditor(object obj, Inst.CachedProperty desc, object data)
        {
            switch (desc.PropertyType.Name)
            {
                case nameof(String):
                    return new TextBoxEditorViewModel(obj, desc);
                case nameof(Type):
                    return new TypeEditorViewModel(obj, desc, data);
                case nameof(Vector4):
                    return new Vector4EditorViewModel(obj, desc);
                case nameof(Vector3):
                    return new Vector3EditorViewModel(obj, desc);
                case nameof(Vector2):
                    return new Vector2EditorViewModel(obj, desc);
                case nameof(UDim2):
                    return new UDim2EditorViewModel(obj, desc);
                case nameof(Boolean):
                    return new CheckboxEditorViewModel(obj, desc);
                case nameof(Colour):
                    return new ColourEditorViewModel(obj, desc);
                case nameof(Single):
                    return new NumberEditorViewModel<float>(obj, desc);
                case nameof(Double):
                    return new NumberEditorViewModel<double>(obj, desc);
                case nameof(Int32):
                    return new NumberEditorViewModel<int>(obj, desc);
                case nameof(UInt32):
                    return new NumberEditorViewModel<uint>(obj, desc);
                case nameof(TimeSpan):
                    return new TimeEditorViewModel(obj, desc);
                case nameof(ColourSequence):
                    return new ColourSequenceEditorViewModel(obj, desc);
                case nameof(NumberRange):
                    return new NumberRangeEditorViewModel(obj, desc);
                //case nameof(NumberSequence):
                //return new NumberSequenceEditorViewModel(obj, desc);
                case nameof(FontFamily):
                    return new FontFamilyEditorViewModel(obj, desc);
                default:
                    if (typeof(string).IsAssignableFrom(desc.PropertyType))
                        return new TextBoxEditorViewModel(obj, desc);
                    if (desc.PropertyType.IsEnum)
                        return new EnumEditorViewModel(obj, desc);
                    if (desc.PropertyType.IsGenericType && desc.PropertyType.GetGenericTypeDefinition() == typeof(Content<>))
                        return new ContentEditorViewModel(obj, desc);
                    if (typeof(Instance).IsAssignableFrom(desc.PropertyType))
                        return new InstanceEditorViewModel(obj, desc);
                    break;
            }

            return null;
        }

        private class GroupEntry
        {
            public GroupEntry()
            {
                Editors = new SortedDictionary<string, IEditor>();
            }

            public SortedDictionary<string, IEditor> Editors { get; }
        }

        private class GroupSorter : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                return string.Compare(x, y, StringComparison.Ordinal);
            }
        }
    }
}