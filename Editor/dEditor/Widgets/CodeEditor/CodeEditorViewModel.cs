// CodeEditorViewModel.cs - dEditor
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
using System.ComponentModel.Composition;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using Caliburn.Micro;
using dEditor.Framework;
using dEditor.Instances;
using dEditor.Utility;
using dEditor.Widgets.CodeEditor.Commands;
using dEngine.Instances;
using dEngine.Services;
using dEngine.Utility.Extensions;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;

namespace dEditor.Widgets.CodeEditor
{
    [Export(typeof(ICodeEditor))]
    public class CodeEditorViewModel : Document, ICodeEditor
    {
        public static readonly DependencyProperty ZoomInCommandProperty = DependencyProperty.Register("ZoomInCommand",
            typeof(ICommand), typeof(CodeEditorViewModel));

        public static readonly DependencyProperty ZoomOutCommandProperty = DependencyProperty.Register("ZoomOutCommand",
            typeof(ICommand), typeof(CodeEditorViewModel));

        private FontFamily _fontFamily;
        private float _fontSize;
        private SolidColorBrush _foregroundBrush;
        private float _zoomScale = 1.0f;

        public CodeEditorViewModel()
        {
            AvalonEditCommands.IndentSelection.InputGestures.Clear();
            AvalonEditCommands.DeleteLine.InputGestures.Clear();

            ContextActionService.Register("scriptEditor.indent", () =>
            {
                var test = new TextEditor();
                AvalonEditCommands.IndentSelection.Execute(null, test);
            });

            ContextActionService.Register("scriptEditor.deleteLine", () =>
            {
                var test = new TextEditor();
                AvalonEditCommands.DeleteLine.Execute(null, test);
            });
        }

        protected override void OnActivate()
        {
            ContextActionService.SetState("scriptEditorFocus", true);
        }

        protected override void OnDeactivate(bool close)
        {
            ContextActionService.SetState("scriptEditorFocus", false);
        }

        public CodeEditorViewModel(LuaSourceContainer luaSourceContainer)
        {
            LuaSourceContainer = luaSourceContainer;
            NotifyOfPropertyChange(() => LuaSourceContainer);
            NotifyOfPropertyChange(() => DisplayName);

            UpdateTheme();

            InsertBreakpointCommand = new InsertBreakpointCommand(this);
        }

        public bool IsReadOnly => !string.IsNullOrEmpty(LuaSourceContainer?.LinkedSource);

        public static RoutedUICommand ZoomInCommand { get; }
        public static RoutedUICommand ZoomOutCommand { get; }
        public InsertBreakpointCommand InsertBreakpointCommand { get; }

        public float ZoomScale
        {
            get { return _zoomScale; }
            set
            {
                if (value.Equals(_zoomScale)) return;
                _zoomScale = Math.Min(4.0f, Math.Max(0.2f, value));
                NotifyOfPropertyChange();
            }
        }

        public float FontSize
        {
            get { return _fontSize; }
            set
            {
                _fontSize = value;
                NotifyOfPropertyChange();
            }
        }

        public FontFamily FontFamily
        {
            get { return _fontFamily; }
            set
            {
                _fontFamily = value;
                NotifyOfPropertyChange();
            }
        }

        public LuaSourceContainer LuaSourceContainer { get; }

        public override string DisplayName => LuaSourceContainer?.Name ?? "";

        public IHighlightingDefinition SyntaxHighlighting { get; set; }

        public SolidColorBrush ForegroundBrush
        {
            get { return _foregroundBrush; }
            set
            {
                if (Equals(value, _foregroundBrush)) return;
                _foregroundBrush = value;
                NotifyOfPropertyChange();
            }
        }

        public void UpdateTheme()
        {
            using (var stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("dEditor.Content.Syntax.Lua.xshd"))
            {
                var xml = stream.ReadString();
                xml = xml.Replace("#Number", EditorSettings.NumberColour.ToHexString());
                xml = xml.Replace("#Operator", EditorSettings.OperatorColour.ToHexString());
                xml = xml.Replace("#String", EditorSettings.StringColour.ToHexString());
                xml = xml.Replace("#Keyword", EditorSettings.KeywordColour.ToHexString());
                xml = xml.Replace("#Comment", EditorSettings.CommentColour.ToHexString());
                xml = xml.Replace("#Comment", EditorSettings.CommentColour.ToHexString());

                var tc = EditorSettings.TextColour;
                ForegroundBrush =
                    new SolidColorBrush(Color.FromRgb((byte)(tc.r * 255), (byte)(tc.g * 255), (byte)(tc.b * 255)));

                var reader = new XmlTextReader(new StringReader(xml));

                SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                NotifyOfPropertyChange(() => SyntaxHighlighting);
            }

            FontSize = EditorSettings.Font.Size;
            FontFamily = new FontFamily(EditorSettings.Font.FontFamily.Name);
        }

        public string CurrentSelection { get; set; }
        public TextEditor TextEditor { get; set; }

        public override int GetHashCode()
        {
            return LuaSourceContainer.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as CodeEditorViewModel;
            return other != null
                   && Equals(LuaSourceContainer, other.LuaSourceContainer);
        }

        public override void CanClose(Action<bool> callback)
        {
            callback(true);
        }

        public static void TryOpenScript(LuaSourceContainer script, int lineNumber = 0)
        {
            var editor = IoC.Get<ICodeEditor>(script.InstanceId);
            Editor.Current.Shell.ActiveDocument = (CodeEditorViewModel)editor;
        }
    }
}