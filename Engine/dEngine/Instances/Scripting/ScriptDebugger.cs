// ScriptDebugger.cs - dEngine
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
using System.Linq;
using dEngine.Instances.Attributes;
using dEngine.Services;
using Neo.IronLua;

namespace dEngine.Instances
{
    /// <summary>
    /// A debug system for a script.
    /// </summary>
    [TypeId(123), Uncreatable]
    public class ScriptDebugger : Instance
    {
        private readonly List<DebuggerBreakpoint> _breakpoints;
        private readonly List<DebuggerWatch> _watches;

        /// <summary />
        public ScriptDebugger(LuaSourceContainer script)
        {
            Script = script;
            NeoDebugger = new LuaDebugger(this);

            _breakpoints = new List<DebuggerBreakpoint>();
            _watches = new List<DebuggerWatch>();

            BreakpointAdded = new Signal<DebuggerBreakpoint>(this);
            BreakpointRemoved = new Signal<DebuggerBreakpoint>(this);
            EncounteredBreak = new Signal<DebuggerBreakpoint>(this);
            Resuming = new Signal<double>(this);
            WatchAdded = new Signal<DebuggerWatch>(this);
            WatchRemoved = new Signal<DebuggerWatch>(this);

            script.Debugger = this;
        }

        internal ILuaDebug NeoDebugger { get; }

        /// <summary>
        /// The current line that the script is on.
        /// </summary>
        public int CurrentLine { get; set; }

        /// <summary>
        /// Determines if the script is paused.
        /// </summary>
        public bool IsPaused { get; private set; }


        /// <summary>
        /// The script this debugger is attached to.
        /// </summary>
        public LuaSourceContainer Script { get; }

        internal LuaTraceLineEventArgs LuaTraceLineEventArgs { get; set; }

        /// <summary>
        /// Adds a watch to the debugger.
        /// </summary>
        /// <param name="expression">The expression to compute the watch value.</param>
        /// <returns>The watch.</returns>
        public DebuggerWatch AddWatch(string expression)
        {
            var w = new DebuggerWatch(this, expression);

            w.Destroyed.Event += () =>
            {
                _watches.Remove(w);
                WatchRemoved.Fire(w);
            };

            _watches.Add(w);
            WatchAdded.Fire(w);

            return w;
        }

        /// <summary>
        /// Adds a breakpoint to the debugger.
        /// </summary>
        /// <param name="line">The line to add the breakpoint to.</param>
        /// <returns>The breakpoint.</returns>
        public DebuggerBreakpoint SetBreakpoint(int line)
        {
            if (_breakpoints.Any(x => x.Line == line))
            {
                throw new InvalidOperationException($"A breakpoint is already set at line {line}.");
            }

            var bp = new DebuggerBreakpoint(this, line);

            bp.Destroyed.Event += () =>
            {
                _breakpoints.Remove(bp);
                BreakpointRemoved.Fire(bp);
            };

            _breakpoints.Add(bp);
            BreakpointAdded.Fire(bp);

            return bp;
        }

        /// <summary>
        /// Returns a list of breakpoints.
        /// </summary>
        public IEnumerable<DebuggerBreakpoint> GetBreakpoints()
        {
            return _breakpoints;
        }

        /// <summary>
        /// Returns a list of watches.
        /// </summary>
        public IEnumerable<DebuggerWatch> GetWatches()
        {
            return _watches;
        }

        /// <summary>
        /// Returns all global variables in the script.
        /// </summary>
        public Dictionary<string, object> GetGlobals()
        {
            return Script.LuaGlobal.ToDictionary(pair => (string)pair.Key, pair => pair.Value);
        }

        /// <summary>
        /// Returns all local variables in the script.
        /// </summary>
        public Dictionary<string, object> GetLocals()
        {
            return LuaTraceLineEventArgs.Locals.ToDictionary(pair => (string)pair.Key, pair => pair.Value);
        }

        /// <summary>
        /// Sets a global variable in the script.
        /// </summary>
        public void SetGlobal(string key, object value)
        {
            Script.LuaGlobal[key] = value;
        }

        internal class LuaDebugger : LuaTraceLineDebugger
        {
            private readonly ScriptDebugger _debugger;

            public LuaDebugger(ScriptDebugger debugger)
            {
                _debugger = debugger;
            }

            protected override void OnFrameEnter(LuaTraceLineEventArgs e)
            {
                _debugger.CurrentLine = e.SourceLine;

                ScriptService.CurrentScript = _debugger.Script;

                _debugger.LuaTraceLineEventArgs = e;

                var bp = _debugger.GetBreakpoints().FirstOrDefault(x => x.Line == e.SourceLine);

                // TODO: evaluate condition

                if (bp != null)
                {
                    _debugger.EncounteredBreak.Fire(bp);
                }
            }

            protected override void OnExceptionUnwind(LuaTraceLineExceptionEventArgs args)
            {
                var errorBuilder = ScriptService.ErrorStringBuilder;

                if (!ScriptService.IsWritingError)
                {
                    var e = args.Exception;
                    do
                    {
                        Engine.Logger.Error(e.ToString());
                        errorBuilder.AppendLine(e.Message);
                        e = e.InnerException;
                    } while (e != null);
                }

                var line = $"Script '{_debugger.Script.GetFullName()}', Line {args.SourceLine}";

                if (args.ScopeName != _debugger.Script.InstanceId)
                {
                    string scopeType;

                    if (args.Locals.ContainsKey(args.ScopeName))
                        scopeType = "local";
                    else if (_debugger.GetGlobals().ContainsKey(args.ScopeName))
                        scopeType = "global";
                    else
                        scopeType = "function";

                    line += $" - {scopeType} {args.ScopeName}";
                }

                errorBuilder.AppendLine(line);

                base.OnExceptionUnwind(args);
            }
        }

        /// <summary>
        /// Fired when a breakpoint is added to the script.
        /// </summary>
        public readonly Signal<DebuggerBreakpoint> BreakpointAdded;

        /// <summary>
        /// Fired when a breakpoint is removed from the script.
        /// </summary>
        public readonly Signal<DebuggerBreakpoint> BreakpointRemoved;

        /// <summary>
        /// Fired when a breakpoint is hit.
        /// </summary>
        public readonly Signal<DebuggerBreakpoint> EncounteredBreak;

        /// <summary>
        /// Fired when a script resumes after a break.
        /// </summary>
        public readonly Signal<double> Resuming;

        /// <summary>
        /// Fired when a watch is added.
        /// </summary>
        public readonly Signal<DebuggerWatch> WatchAdded;

        /// <summary>
        /// Fired when a watch is removed.
        /// </summary>
        public readonly Signal<DebuggerWatch> WatchRemoved;
    }

    /// <summary>
    /// A script watch.
    /// </summary>
    [TypeId(124), Uncreatable]
    public class DebuggerWatch : Instance
    {
        internal DebuggerWatch(ScriptDebugger debugger, string expression)
        {
            Debugger = debugger;
            Expression = expression;
            Value = null;
            Parent = debugger;
        }

        /// <summary>
        /// The debugger.
        /// </summary>
        public ScriptDebugger Debugger { get; }

        /// <summary>
        /// The watch expression.
        /// </summary>
        public string Expression { get; }

        /// <summary>
        /// The value of the watch.
        /// </summary>
        public object Value { get; }
    }

    /// <summary>
    /// A script breakpoint.
    /// </summary>
    [TypeId(125), Uncreatable]
    public class DebuggerBreakpoint : Instance
    {
        private string _condition;
        private bool _enabled;

        internal DebuggerBreakpoint(ScriptDebugger debugger, int line)
        {
            Debugger = debugger;
            Line = line;
            Parent = debugger;
        }

        /// <summary>
        /// The debugger.
        /// </summary>
        public ScriptDebugger Debugger { get; }

        /// <summary>
        /// The line the breakpoint is on.
        /// </summary>
        public int Line { get; set; }

        /// <summary>
        /// The condition to trigger the breakpoint.
        /// </summary>
        public string Condition
        {
            get { return _condition; }
            set
            {
                if (value == _condition) return;
                _condition = value;
                NotifyChanged(nameof(Condition));
            }
        }

        /// <summary>
        /// Determines whether the breakpoint is enabled.
        /// </summary>
        public bool IsEnabled
        {
            get { return _enabled; }
            set
            {
                if (value == _enabled) return;
                _enabled = value;
                NotifyChanged(nameof(IsEnabled));
            }
        }
    }
}