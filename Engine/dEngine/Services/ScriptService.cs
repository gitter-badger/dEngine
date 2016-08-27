// ScriptRuntimeService.cs - dEngine
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security;
using System.Text;
using dEngine.Instances;
using dEngine.Instances.Attributes;
using dEngine.Utility;
using dEngine.Utility.Extensions;
using Neo.IronLua;
using NLog;


#pragma warning disable 4014

namespace dEngine.Services
{
    /// <summary>
    /// <see cref="ScriptService" /> handles the execution of scripts at runtime.
    /// </summary>
    [TypeId(148), ExplorerOrder(-1)]
    public partial class ScriptService : Service
    {
        internal static ScriptService Service;

        /// <summary>
        /// Fires when a script errors.
        /// </summary>
        public readonly Signal<string, string, Script> ScriptErrored;

        internal ScriptService()
        {
            ScriptErrored = new Signal<string, string, Script>(this);
            Service = this;
        }

        private static readonly Stopwatch _stopwatch;
        private static readonly ConcurrentDictionary<LuaThread, ScriptIdentity> _threadIdentities;
        private static readonly ConcurrentDictionary<string, TypeCacheItem> _typeCache;
        private static readonly System.DateTime _unixEpoch = new System.DateTime(1970, 1, 1).ToUniversalTime();

        internal static Logger LoggerInternal = LogManager.GetCurrentClassLogger();
        internal static StringBuilder ErrorStringBuilder = new StringBuilder();

        static ScriptService()
        {
            _stopwatch = Stopwatch.StartNew();
            _threadIdentities = new ConcurrentDictionary<LuaThread, ScriptIdentity>();
            _typeCache = new ConcurrentDictionary<string, TypeCacheItem>();
            Scripts = new ConcurrentDictionary<string, Script>();
        }

        internal static bool IsWritingError => ErrorStringBuilder.Length != 0;

        /// <summary>
        /// The Lua environment object.
        /// </summary>
        internal static Lua Lua { get; private set; }

        /// <summary>
        /// A collection of enum types in the <see cref="Enums" /> namespace.
        /// </summary>
        internal static IEnumerable<Type> EnumTypes { get; private set; }

        /// <summary>
        /// A dictionary of scripts.
        /// </summary>
        public static ConcurrentDictionary<string, Script> Scripts { get; internal set; }

        /// <summary>
        /// The script that is currently executing at this time.
        /// </summary>
        public static LuaSourceContainer CurrentScript { get; set; }

        /// <summary>
        /// The shared global environment for scripts.
        /// </summary>
        public static ScriptGlobal GlobalEnvironment { get; set; }

        internal static object GetExisting()
        {
            return DataModel.GetService<ScriptService>();
        }

        internal static void Init()
        {
            LoggerInternal = LogManager.GetLogger(nameof(ScriptService));
            EnumTypes = typeof(Engine).Assembly.GetTypes().Where(x => x.IsEnum);
            CacheTypes();
            Lua = new Lua(LuaIntegerType.Int32, LuaFloatType.Double);

            GlobalEnvironment = new ScriptGlobal(Lua);

            ExecutionQueue = new ConcurrentWorkQueue<Execution>(execution =>
            {
                if (!execution.TryFulfill())
                    ExecutionQueue.Enqueue(execution); // send to back of queue
                else
                    execution.Set();
            });
        }

        internal static void HandleException(Exception e, string scriptId)
        {
            var script = Scripts[scriptId];

            if (IsWritingError)
            {
                foreach (
                    var line in
                        ErrorStringBuilder.ToString()
                            .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (line.StartsWith("Script"))
                        script.Print(line, LogLevel.Trace);
                    else
                        script.Print(line, LogLevel.Error);
                }

                script.Print("Stack End", LogLevel.Trace);
            }
            else
            {
                var parseException = e as LuaParseException;

                script.Print(parseException != null
                    ? $"{script.GetFullName()}:{parseException.Line}: {e.Message}"
                    : $"{script.GetFullName()}:{script.Debugger?.CurrentLine.ToString() ?? "?"}: {e.Message}", LogLevel.Info);
            }

            //LoggerInternal.Trace(luaExceptionData.StackTrace);

            ErrorStringBuilder.Clear();
        }

        /// <summary>
        /// Adds script to script collection.
        /// </summary>
        public static void RegisterScript(Script script, string instanceId)
        {
            Scripts.TryAdd(instanceId, script);
            ScriptAdded?.Invoke(script);
        }

        /// <summary>
        /// Removes script from script collection.
        /// </summary>
        public static void DeregisterScript(Script script, string instanceId)
        {
            Scripts.TryRemove(instanceId);
            ScriptRemoved?.Invoke(script);
        }

        /// <summary>
        /// Fired when a script is added with <see cref="RegisterScript" />
        /// </summary>
        public static event Action<Script> ScriptAdded;

        /// <summary>
        /// Fired when a script is removed with <see cref="DeregisterScript" />
        /// </summary>
        public static event Action<Script> ScriptRemoved;

        private static void CacheTypes()
        {
            var baseType = typeof(Instance);

            var asm = baseType.Assembly;

            foreach (var type in asm.GetTypes().Where(x => baseType.IsAssignableFrom(x) && !x.IsAbstract))
            {
                TypeProblem problem = TypeProblem.NoProblem;
                Func<Instance> creator = null;

                if (type.IsAbstract)
                {
                    problem = TypeProblem.NotCreatable;
                }
                else if (typeof(Service).IsAssignableFrom(type))
                {
                    problem = TypeProblem.IsAService;
                }
                else if (type.GetConstructor(Type.EmptyTypes) == null)
                {
                    problem = TypeProblem.RequiresParameters;
                }
                else if (type.GetCustomAttributes(typeof(UncreatableAttribute), false).Any())
                {
                    problem = TypeProblem.NotCreatable;
                }

                if (problem == TypeProblem.NoProblem)
                {
                    var constructor = type.GetConstructor(Type.EmptyTypes);
                    Debug.Assert(constructor != null, "constructor != null");
                    creator =
                        Expression.Lambda<Func<Instance>>(Expression.New(constructor))
                            .Compile();
                }

                var item = new TypeCacheItem(type, creator) { Problem = problem };

                _typeCache.TryAdd(type.Name, item);
            }
        }

        internal static Instance NewInstance(string name, Instance parent)
        {
            name = name ?? "nil";
            TypeCacheItem cacheItem;

            _typeCache.TryGetValue(name, out cacheItem);

            if (cacheItem == null)
            {
                throw new InvalidOperationException(
                    $"Cannot create instance of type \"{name}\": non existent type.");
            }

            switch (cacheItem.Problem)
            {
                case TypeProblem.NoProblem:
                    break;
                case TypeProblem.NotCreatable:
                    throw new InvalidOperationException($"Cannot create instance of type \"{name}\": not creatable.");
                case TypeProblem.RequiresParameters:
                    throw new InvalidOperationException(
                        $"Cannot create instance of type \"{name}\": requires paramaters.");
                case TypeProblem.IsAService:
                    throw new InvalidOperationException(
                        "Services cannot be created with Instance.new(), use DataModel:GetService() instead.");
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var instance = cacheItem.Creator();
            instance.Parent = parent;

            return instance;
        }

        private enum TypeProblem
        {
            NoProblem,
            NotCreatable,
            RequiresParameters,
            IsAService
        }

        private class TypeCacheItem
        {
            public TypeCacheItem(Type t, Func<Instance> creator = null)
            {
                Type = t;
                Creator = creator;
            }

            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            // ReSharper disable once MemberCanBePrivate.Local
            public Type Type { get; }
            public Func<Instance> Creator { get; }
            public TypeProblem Problem { get; set; }
        }

        internal class CustomCompileOptions : LuaCompileOptions
        {
            // ReSharper disable once NotAccessedField.Local
            private LuaSourceContainer _script;

            internal CustomCompileOptions(LuaSourceContainer script)
            {
                _script = script;
            }

            protected override Expression SandboxCore(Expression expression, Expression instance, string sMember)
            {
                var memberExpression = instance as MemberExpression;

                var prop = memberExpression?.Member as PropertyInfo;

                if (prop?.Name == "Clr")
                {
                    AssertIdentity(ScriptIdentity.Editor);
                }
                return base.SandboxCore(expression, instance, sMember);
            }
        }

        /// <summary>
        /// Asserts that the calling thread has the required identity.
        /// </summary>
        /// <param name="target">The required identity.</param>
        /// <param name="condition">
        /// If true, an exception is thrown when the thread does not meet target identity.
        /// If false, an exception is thrown when the thread does the target identity.
        /// </param>
        internal static void AssertIdentity(ScriptIdentity target, bool condition = true)
        {
            var thread = LuaThread.running();
            var identity = GetIdentity(thread);
            if (identity == null || (target.HasFlag(identity) == condition)) return; // threads with no identity are unrestricted
#if DEBUG
            var frame = new StackFrame(1);
            var method = frame.GetMethod();
            var type = method.DeclaringType;
            var name = method.Name;
            throw new SecurityException($"Method {type}.{name} requires identity level {target}. (Current: {identity})");
#else
            throw new SecurityException();
#endif
        }

        internal static ScriptIdentity? GetIdentity(LuaResult thread)
        {
            ScriptIdentity identity;

            var th = (LuaThread)thread.Values[0];

            if (th == null)
                return null;

            if (_threadIdentities.TryGetValue(th, out identity))
                return identity;

            return null;
        }
        
        internal static void ResumeThread(LuaThread thread)
        {
            Engine.GameThread.Enqueue(() => thread.resume(null));
        }
    }
}