// Keymap.cs - dEditor
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
using System.Collections.Generic;
using System.IO;
using dEngine;
using dEngine.Instances;
using dEngine.Services;
using Expressions;
using MoreLinq;
using Newtonsoft.Json;

// ReSharper disable once LocalizableElement
namespace dEditor.Framework
{
    public static class KeyBindings
    {
        public static string CustomShortcutsPath = Path.Combine(Editor.Current.EditorDocumentsPath, "KeyboardShortcuts.json");

        static KeyBindings()
        {
            _bindings = new List<ContextActionService.ContextBinding>();

            DataModel.GetService<ContextActionService>();
            ContextActionService.DefineState("viewportFocus");
            ContextActionService.DefineState("scriptEditorFocus");
            ContextActionService.DefineState("dragging");
            ContextActionService.DefineState("selectionEmpty");
        }

        internal static void RegisterAction(string name, Action action)
        {
            ContextActionService.Register(name, action);
        }

        public static void Init()
        {
            // load custom shortcuts first
            if (!File.Exists(CustomShortcutsPath))
                File.WriteAllText(CustomShortcutsPath, "[\n]");

            _watcher = new FileSystemWatcher
            {
                Path = Editor.Current.EditorDocumentsPath,
                Filter = "KeyboardShortcuts.json",
                NotifyFilter = NotifyFilters.LastWrite,
                EnableRaisingEvents = true,
            };
            _watcher.Changed += (s, e) => ReloadKeymap();

            _defaultShortcutsJson = ContentProvider.DownloadString("editor://KeyboardShortcuts.json");
            ReloadKeymap();
        }

        private static void ReloadKeymap()
        {
            foreach (var binding in _bindings)
                ContextActionService.Unbind(binding);

            LoadKeymap(_defaultShortcutsJson);
            LoadKeymap(File.ReadAllText(CustomShortcutsPath));
        }

        private static void LoadKeymap(string keymap)
        {
            var items = JsonConvert.DeserializeObject<KeymapItem[]>(keymap);
            foreach (var item in items)
            {
                Key key;
                Key modifier;
                Func<bool> when;

                ParseKey(item.Key, out key, out modifier);
                ParseWhen(item.When, out when);

                try
                {
                    _bindings.Add(ContextActionService.Bind(item.Command, key, modifier, when));
                }
                catch (ExpressionsException e)
                {
                    Editor.Current.Logger.Error(e, "An exception was thrown when parsing a keybinding expression.");
                }
            }
        }

        private static Key KeyFromString(string key)
        {
            switch (key)
            {
                case "alt":
                    return Key.LeftAlt;
                case "shift":
                    return Key.LeftShift;
                case "ctrl":
                    return Key.LeftControl;
                case ".":
                    return Key.FullStop;
                case ",":
                    return Key.Comma;
                case ";":
                    return Key.Semicolon;
                case "'":
                    return Key.Apostrophe;
                case "`":
                    return Key.Grave;
                case "minus":
                case "-":
                    return Key.Subtract;
                case "plus":
                case "+":
                    return Key.Add;
                case "[":
                    return Key.LeftBracket;
                case "]":
                    return Key.RightBracket;
                case "#":
                    return Key.HashSymbol;
                case "/":
                    return Key.ForwardSlash;
                case "\\":
                    return Key.Backslash;
                case "win":
                    return Key.LeftWin;
                case "1":
                    return Key.Num1;
                case "2":
                    return Key.Num2;
                case "3":
                    return Key.Num3;
                case "4":
                    return Key.Num4;
                case "5":
                    return Key.Num5;
                case "6":
                    return Key.Num6;
                case "7":
                    return Key.Num7;
                case "8":
                    return Key.Num8;
                case "9":
                    return Key.Num9;
                case "0":
                    return Key.Num0;
                default:
                    return (Key)Enum.Parse(typeof(Key), key, true);
            }
        }

        private static void ParseKey(string str, out Key key, out Key modifier)
        {
            var keys = str.Split('+');
            key = KeyFromString(keys[keys.Length - 1]);
            modifier = Key.Unknown;
            for (var i = 0; i < keys.Length - 1; i++)
                modifier |= KeyFromString(keys[i]);
        }

        private static void ParseWhen(string when, out Func<bool> predicate)
        {
            if (string.IsNullOrEmpty(when))
            {
                predicate = () => true;
                return;
            }
            var expression = new DynamicExpression(when, ExpressionLanguage.Csharp);


            predicate = () =>
            {
                var context = new ExpressionContext();
                ContextActionService.StateVariables.ForEach(v => context.Variables.Add(v.Key, v.Value));
                var result = expression.Invoke(context, WhenExpressionOptions);
                return (bool)result;
            };
        }

        private static readonly BoundExpressionOptions WhenExpressionOptions = new BoundExpressionOptions
        {
            AllowPrivateAccess = false,
            ResultType = typeof(bool)
        };

        private static FileSystemWatcher _watcher;
        private static string _defaultShortcutsJson;
        private static List<ContextActionService.ContextBinding> _bindings;

        [JsonObject]
        public class KeymapItem
        {
            [JsonProperty("key")]
            public string Key;
            [JsonProperty("Command")]
            public string Command;
            [JsonProperty("when")]
            public string When;
        }
    }
}