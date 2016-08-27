// ScriptService.Execution.cs - dEngine
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
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using dEngine.Instances;
using dEngine.Settings.Global;
using dEngine.Utility;
using Neo.IronLua;
#pragma warning disable 4014

namespace dEngine.Services
{
    public partial class ScriptService
    {
        internal static ConcurrentWorkQueue<Execution> ExecutionQueue { get; set; }

        /// <summary>
        /// Starts a script.
        /// </summary>
        public static async Task<LuaResult> ExecuteAsync(Script script, object[] args = null)
        {
            var e = new StartExecution(script, args);
            ExecutionQueue.Enqueue(e);

            await e.WaitAsync().ConfigureAwait(false);

            return await Task.FromResult(e.Result).ConfigureAwait(false);
        }

        internal static void ExecuteAllScripts(bool isClient, bool isServer)
        {
            foreach (var script in Scripts.Values)
            {
                if (!script.CheckCanRun())
                    continue;

                if (isServer)
                {
                    if (!(script is LocalScript) &&
                        (script.IsDescendantOf(Game.Workspace) ||
                         script.IsDescendantOf(ServerScriptService.Service)))
                    {
                        script.Run();
                    }
                }

                if (isClient)
                {
                    if (script is LocalScript && script.IsDescendantOf(Players.Service.LocalPlayer))
                    {
                        script.Run();
                    }
                }
            }
        }

        internal static void StopAllScripts()
        {
            foreach (var script in Scripts.Values)
            {
                script.Stop();
            }
        }

        internal static void ResumeWaitingScripts()
        {
            if (RunService.SimulationState == SimulationState.Paused || _stopwatch.Elapsed.TotalSeconds < 0.03)
                return;

            ExecutionQueue.Work();

            _stopwatch.Restart();
        }

        internal abstract class Execution
        {
            private readonly SemaphoreSlim _semaphore;
            protected readonly LuaSourceContainer Script;

            internal Execution(LuaSourceContainer script = null)
            {
                Script = script;
                _semaphore = new SemaphoreSlim(0, 1);
            }

            internal void Set()
            {
                _semaphore.Release();
            }

            internal async Task WaitAsync()
            {
                await _semaphore.WaitAsync().ConfigureAwait(false);
            }

            internal void Wait()
            {
                _semaphore.Wait();
            }

            internal abstract bool TryFulfill();
        }

        internal class StartExecution : Execution
        {
            public StartExecution(LuaSourceContainer script, object[] args = null) : base(script)
            {
                Args = args ?? new object[0];
            }

            internal object[] Args { get; }
            internal LuaResult Result { get; private set; }

            internal override bool TryFulfill()
            {
                var compileOptions = new CustomCompileOptions(Script)
                {
                    DebugEngine = LuaSettings.DebugEngineEnabled ? (Script.Debugger?.NeoDebugger ?? (Script.Debugger = new ScriptDebugger(Script)).NeoDebugger) : null
                };

                var del = (Action)delegate
                {
                    try
                    {
                        if (LuaSettings.AreScriptStartsReported)
                            Script.Print($"Script started. ({Script.GetFullName()})", LogLevel.Trace);
                        Script.IsRunning = true;
                        CurrentScript = Script;

                        var chunk = Lua.CompileChunk(new StringReader(Script.Source), Script.InstanceId, compileOptions, new KeyValuePair<string, Type>("args", typeof(LuaTable)));
                        var argsTable = Args.ToLuaTable();

                        Result = Script.LuaGlobal.DoChunk(chunk, argsTable);
                        Script.IsRunning = false;
                    }
                    catch (Exception e)
                    {
                        HandleException(e, Script.InstanceId);
                        Result = null;
                    }
                };

                var thread = new LuaThread(del);
                _threadIdentities[thread] = Script.Identity;

                Script.LuaThread = thread;
                ResumeThread(thread);

                return true;
            }
        }

        internal class WaitExecution : Execution
        {
            private readonly System.DateTime _destination;
            private readonly LuaThread _luaThread;
            private readonly Action _continueAction;

            internal WaitExecution(LuaSourceContainer source, LuaThread thread, double seconds, Action continueAction = null) : base(source)
            {
                _luaThread = thread;
                _continueAction = continueAction;
                _destination = System.DateTime.Now + System.TimeSpan.FromSeconds(seconds);
            }

            internal override bool TryFulfill()
            {
                if (System.DateTime.Now < _destination)
                    return false;

                if (Script.IsRunning)
                {
                    ResumeThread(_luaThread);
                    CurrentScript = Script;
                }

                _continueAction?.Invoke();

                return true;
            }
        }

        /// <summary>
        /// Gets the coroutine that the accessing script is running on.
        /// </summary>
        internal static LuaThread CurrentThread => (LuaThread)LuaThread.running()[0];

        /// <summary>
        /// Yields the coroutine that the accessing script is running on.
        /// </summary>
        internal static void YieldThread()
        {
            LuaThread.yield(null);
        }
    }
}