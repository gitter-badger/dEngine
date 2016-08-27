// API.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using dEngine.Instances;
using dEngine.Instances.Attributes;
using dEngine.Services;
using dEngine.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#pragma warning disable 1591

namespace dEngine
{
    /// <summary>
    /// Class for API.
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class API
    {
        public Class RootClass { get; set; }
        public Dictionary<string, Class> DataTypes { get; } = new Dictionary<string, Class>();

        static API()
        {
            Comments = new Comments("dEngine.xml");
        }

        /// <summary>
        /// The XML comments.
        /// </summary>
        public static Comments Comments { get; set; }

        private static string GetCategoryFromType(Type type)
        {
            string typeCategory;
            if (typeof(Instance).IsAssignableFrom(type) || type == typeof(Instance))
                typeCategory = "class";
            else if (typeof(IDataType).IsAssignableFrom(type))
                typeCategory = "datatype";
            else
                typeCategory = "internal";
            return typeCategory;
        }

        private static readonly char[] _alphabet = "abcdefghijklmnopqrstuvwxyz".ToCharArray();

        /// <summary>
        /// Dumps the API as a string.
        /// </summary>
        public static string Dump()
        {
            var api = new API();

            var objectClasses = new Dictionary<string, Class>();

            var types =
                Assembly.GetExecutingAssembly()
                    .ExportedTypes.Where(
                        t =>
                            (typeof(Instance).IsAssignableFrom(t) || typeof(IDataType).IsAssignableFrom(t) ||
                            t == typeof(Instance)) && t != typeof(DynamicObject)).OrderBy(t => t.Name);

            foreach (var type in types)
            {
                var @class = new Class { Name = type.Name, FullName = type.FullName, IsAbstract = type.IsAbstract, IsService = typeof(Service).IsAssignableFrom(type)};
                var kind = @class.Kind = GetCategoryFromType(type);

                switch (kind)
                {
                    case "class":
                        objectClasses.Add(@class.FullName, @class);
                        break;
                    case "datatype":
                        api.DataTypes[@class.FullName] = @class;
                        break;
                    default:
                        throw new IndexOutOfRangeException("category");
                }

                @class.BaseClass = type.BaseType?.FullName;

                foreach (var attribute in type.GetCustomAttributes())
                {
                    var attr = new Attribute { Name = attribute.GetType().Name, Value = (attribute as IValueAttribute)?.GetValue() };
                    @class.Attributes.Add(attr.Name, attr);
                }

                Comments.Comment classComment;
                Comments.Get(type, out classComment);
                @class.Summary = classComment.Summary;
                @class.Remarks = classComment.Remarks;

                var methodFlags = BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance;
                foreach (var methodInfo in type.GetMethods(methodFlags).OrderBy(m => m.Name))
                {
                    if (methodInfo.IsSpecialName || methodInfo.DeclaringType != type || methodInfo.Name == "TryGetMember" || methodInfo.Name == "Equals" || methodInfo.Name == "ToString")
                        continue;

                    if (kind == "datatype")
                    {
                        if (methodInfo.Name == "Load" || methodInfo.Name == "Save")
                            continue;
                    }

                    var method = new Function
                    {
                        Name = methodInfo.Name,
                        ReturnType = methodInfo.ReturnType.FullName,
                        DeclaringType = @class.FullName,
                    };

                    Comments.Comment methodComment;
                    Comments.Get(methodInfo, out methodComment);
                    method.Summary = methodComment.Summary;
                    method.Remarks = methodComment.Remarks;

                    foreach (var parameterInfo in methodInfo.GetParameters())
                    {
                        string paramComment = "";
                        methodComment.Parameters?.TryGetValue(parameterInfo.Name, out paramComment);

                        var param = new Parameter
                        {
                            Name = parameterInfo.Name,
                            Comment = paramComment,
                            ParameterType = parameterInfo.ParameterType.FullName,
                            DefaultValue = parameterInfo.HasDefaultValue ? (parameterInfo.DefaultValue?.ToString() ?? "null") : ""
                        };
                        method.Parameters.Add(param);
                    }

                    foreach (var attribute in methodInfo.GetCustomAttributes())
                    {
                        var attr = new Attribute { Name = attribute.GetType().Name, Value = (attribute as IValueAttribute)?.GetValue() }; ;
                        method.Attributes[attr.Name] = attr;
                    }

                    @class.Functions.Add(method);
                }

                var flags = BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance;
                if (typeof(Settings.Settings).IsAssignableFrom(type))
                    flags |= BindingFlags.Static;

                foreach (var property in type.GetProperties(flags).OrderBy(p => p.Name))
                {
                    if (property.DeclaringType != type || property.IsSpecialName || property.Name == "RenderData" || property.Name == "RenderIndex" || property.Name == "RenderObject")
                        continue;

                    if (@class.FullName == typeof(Instance).FullName && property.Name == "Item") // hack to get rid of weird property
                        continue;

                    var prop = new Property
                    {
                        Name = property.Name,
                        PropertyType = property.PropertyType.FullName,
                        ReadOnly = property.SetMethod?.IsPublic != true,
                        DeclaringType = @class.FullName,
                    };

                    Comments.Comment propComment;
                    Comments.Get(property, out propComment);
                    prop.Summary = propComment.Summary;
                    prop.Remarks = propComment.Remarks;

                    foreach (var attribute in property.GetCustomAttributes())
                    {
                        var attr = new Attribute { Name = attribute.GetType().Name, Value = (attribute as IValueAttribute)?.GetValue() }; ;
                        prop.Attributes[attr.Name] = attr;
                    }

                    @class.Properties.Add(prop);
                }

                foreach (var fieldInfo in type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    if (fieldInfo.FieldType != typeof(Signal)
                        && !(fieldInfo.FieldType.IsGenericType
                        && fieldInfo.FieldType.GetGenericTypeDefinition() == typeof(Signal<>)))
                        continue;

                    if (fieldInfo.DeclaringType != type)
                        continue;

                    var paramTypes = fieldInfo.FieldType.GetGenericArguments();

                    Comments.Comment eventComment;
                    Comments.Get(fieldInfo, out eventComment);

                    int paramIndex = 0;

                    var signal = new Signal
                    {
                        Name = fieldInfo.Name,
                        FieldType = fieldInfo.FieldType.ToString(),
                        Summary = eventComment.Summary,
                        Parameters = paramTypes.Select(t =>
                        {
                            string paramName = _alphabet[paramIndex].ToString();

                            eventComment.Parameters?.TryGetValue(paramIndex.ToString(), out paramName);

                            paramIndex++;
                            return new Parameter { Name = paramName, ParameterType = t.FullName };
                        })
                    };
                    @class.Signals.Add(signal);
                }
            }

            foreach (var objectClass in objectClasses.Values)
            {
                if (objectClass.Name == nameof(Instance))
                    continue;
                var superClass = objectClass.BaseClass;
                if (!string.IsNullOrEmpty(superClass))
                {
                    objectClasses[superClass].SubClasses.Add(objectClass);
                }
            }

            api.RootClass = objectClasses[typeof(Instance).FullName];

            return JObject.FromObject(api).ToString(Formatting.None);
        }

        [JsonObject(MemberSerialization.OptOut)]
        public class Attribute
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }

        [JsonObject(MemberSerialization.OptOut)]
        public class Class : IComparable<Class>
        {
            public string Name { get; set; }
            public List<Property> Properties { get; set; } = new List<Property>();
            public List<Function> Functions { get; set; } = new List<Function>();
            public List<Signal> Signals { get; set; } = new List<Signal>();
            public string BaseClass { get; set; }
            public string Summary { get; set; } = "No summary.";
            public string Remarks { get; set; } = "";
            public string FullName { get; set; }
            public Dictionary<string, Attribute> Attributes { get; set; } = new Dictionary<string, Attribute>();
            public SortedSet<Class> SubClasses { get; set; } = new SortedSet<Class>();
            public string Kind { get; set; }
            public bool IsService { get; set; }
            public bool IsAbstract { get; set; }

            public int CompareTo(Class other)
            {
                return Name.CompareTo(other.Name);
            }
        }

        [JsonObject(MemberSerialization.OptOut)]
        public class DataType
        {

        }

        [JsonObject(MemberSerialization.OptOut)]
        public class Property
        {
            public string Name { get; set; }
            public string PropertyType { get; set; }
            public string Summary { get; set; }
            public string Remarks { get; set; }
            public bool ReadOnly { get; set; }
            public string DeclaringType { get; set; }
            public Dictionary<string, Attribute> Attributes { get; set; } = new Dictionary<string, Attribute>();
        }

        [JsonObject(MemberSerialization.OptOut)]
        public class Function
        {
            public string Name { get; set; }
            public string ReturnType { get; set; }
            public string DeclaringType { get; set; }
            public string Summary { get; set; }
            public string Remarks { get; set; }
            public List<Parameter> Parameters { get; set; } = new List<Parameter>();
            public Dictionary<string, Attribute> Attributes { get; set; } = new Dictionary<string, Attribute>();
        }

        [JsonObject(MemberSerialization.OptOut)]
        public class Parameter
        {
            public string Name { get; set; }
            public string ParameterType { get; set; }
            public string Comment { get; set; }
            public string DefaultValue { get; set; }
        }

        [JsonObject(MemberSerialization.OptOut)]
        public class Signal
        {
            public string Name { get; set; }
            public string Summary { get; set; }
            public string FieldType { get; set; }
            public IEnumerable<Parameter> Parameters { get; set; }
        }
    }
}