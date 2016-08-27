// Comments.cs - dEngine
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
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Xml;
#pragma warning disable 1591

namespace dEngine.Utility
{
    /// <summary>
    /// Manages comments.
    /// </summary>
    public class Comments
    {
        private readonly Dictionary<string, Comment> _commentDictionary;

        /// <summary />
        public Comments(string file)
        {
            _commentDictionary = new Dictionary<string, Comment>();
            using (var reader = new XmlTextReader(file))
            {
                string key = "?";
                var comment = new Comment();
                string parentName = "";
                int eventParamIndex = 0;

                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (reader.Name == "member")
                            {
                                key = reader.GetAttribute("name");
                                comment = new Comment { Parameters = new Dictionary<string, string>() };
                            }
                            else if (reader.Name == "param")
                            {
                                key = reader.GetAttribute("name");
                            }
                            else if (reader.Name == "summary")
                            {
                                var text = reader.ReadInnerXml().Trim();
                                if (!string.IsNullOrEmpty(text))
                                    comment.Summary = string.Join(Environment.NewLine,
                                        text.Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                                            .Select(l => l.Trim()));
                            }
                            else if (reader.Name == "remarks")
                            {
                                var text = reader.ReadInnerXml().Trim();
                                if (!string.IsNullOrEmpty(text))
                                    comment.Remarks = string.Join(Environment.NewLine,
                                        text.Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                                            .Select(l => l.Trim()));
                            }
                            else if (reader.Name == "eventParam")
                            {
                                key = reader.GetAttribute("name");
                                comment.Parameters[eventParamIndex++.ToString()] = key;
                            }
                            break;
                        case XmlNodeType.EndElement:
                            if (reader.Name == "member")
                            {
                                Debug.Assert(key != null, "key != null");
                                _commentDictionary[key] = comment;
                            }
                            break;
                        case XmlNodeType.Text:
                            if (parentName == "param")
                                comment.Parameters[key ?? "param"] = reader.ReadString().Trim();
                            break;
                    }

                    parentName = reader.Name;
                }
            }
        }

        /// <summary>
        /// Tries to get the comment for the given class.
        /// </summary>
        public void Get(Type type, out Comment comment)
        {
            if (!_commentDictionary.TryGetValue($"T:{type}", out comment))
                comment = new Comment();
        }

        /// <summary>
        /// Tries to get the comment for the given field.
        /// </summary>
        public void Get(FieldInfo field, out Comment comment)
        {
            if (!_commentDictionary.TryGetValue($"M:{field.DeclaringType}.{field.Name}", out comment))
                comment = new Comment();
        }

        /// <summary>
        /// Tries to get the comment for the given property.
        /// </summary>
        public void Get(PropertyInfo property, out Comment comment)
        {
            if (!_commentDictionary.TryGetValue($"P:{property.DeclaringType}.{property.Name}", out comment))
                comment = new Comment();
        }

        /// <summary>
        /// Tries to get the comment for the given method.
        /// </summary>
        public void Get(MethodInfo method, out Comment comment)
        {
            if (!_commentDictionary.TryGetValue($"M:{method.DeclaringType}.{method.Name}", out comment))
                comment = new Comment();
        }

        /// <summary>
        /// Tries to get the comment for the given key.
        /// </summary>
        public void Get(string key, out Comment comment)
        {
            if (!_commentDictionary.TryGetValue(key, out comment))
                comment = new Comment();
        }

        public class Comment
        {
            public string Summary = "";
            public string Remarks = "";
            public Dictionary<string, string> Parameters = new Dictionary<string, string>();
        }
    }
}