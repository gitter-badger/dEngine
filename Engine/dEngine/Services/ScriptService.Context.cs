// ScriptService.Context.cs - dEngine
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
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using dEngine.Instances;
using dEngine.Instances.Diagnostics;
using dEngine.Settings;
using dEngine.Settings.Global;
using dEngine.Utility;
using dEngine.Utility.Extensions;
using Dynamitey;
using Neo.IronLua;
using SharpDX;

#pragma warning disable 1591

namespace dEngine.Services
{
    public partial class ScriptService
    {
        private static Delegate CreateDelegate(MethodInfo method, object target = null)
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }

            if (!method.IsStatic)
            {
                //throw new ArgumentException("The provided method must be static.", "method");
            }

            var del = Expression.GetDelegateType(
                (from parameter in method.GetParameters() select parameter.ParameterType)
                    .Concat(new[] {method.ReturnType})
                    .ToArray());

            if (method.IsStatic)
            {
                return method.CreateDelegate(del);
            }
            return method.CreateDelegate(del, target);
        }

        /// <summary>
        /// A <see cref="Neo.IronLua.LuaGlobal" /> for dEngine scripts.
        /// </summary>
        public class ScriptGlobal : LuaGlobal
        {
            internal ScriptGlobal(Lua lua) : base(lua)
            {
                this["game"] = Game.DataModel;
                this["workspace"] = Game.Workspace;
                this["io"] = null;
                this["os"] = null;
                this["dofile"] = null;
                this["dochunk"] = null;
                this["Enum"] = BuildEnumTable();
                var instanceTable = new LuaTable();
                instanceTable.DefineFunction("new", new Func<string, Instance, Instance>(NewInstance));
                //instanceTable.DefineFunction("new", new Func<string, LuaTable, Instance>(NewInstance));
                this["Instance"] = instanceTable;

                RegisterPackage("math", typeof(MathPackage));

                foreach (
                    var methodInfo in
                        GetType()
                            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static |
                                        BindingFlags.DeclaredOnly))
                {
                    var name = methodInfo.Name;
                    var del = CreateDelegate(methodInfo, this);
                    DefineFunction(name, del, false);
                    DefineFunction(char.ToLowerInvariant(name[0]) + name.Substring(1), del, false);
                }

                RegisterPackage("debug", typeof(DebugPackage));

                foreach (var t in typeof(IDataType).GetDescendantTypes())
                {
                    RegisterPackage(t.Name, t);
                }

                var pprintSource = ContentProvider.DownloadStream("internal://scripts/pprint.lua").Result.ReadString();
                var pprint = ScriptService.Lua.CompileChunk(pprintSource, "pprint", new LuaCompileOptions());
                this["pprint"] = DoChunk(pprint).Values[0];

                var middleclassSource =
                    ContentProvider.DownloadStream("internal://scripts/pprint.lua").Result.ReadString();
                var middleclass = ScriptService.Lua.CompileChunk(middleclassSource, "middleclass",
                    new LuaCompileOptions());
                this["class"] = DoChunk(middleclass).Values[0];
            }

            public static void recurse(LuaAction<Instance> func, Instance root)
            {
                foreach (var child in root.Children)
                {
                    func.Invoke(child);
                    recurse(func, child);
                }
            }

            public new static LuaResult unpack(LuaTable t)
            {
                return unpack(t, 1, t.Length);
            }

            public static LuaResult unpack(IEnumerable e)
            {
                var values = new ArrayListEx<object>();
                foreach (var obj in e)
                    values.Add(obj);
                return new LuaResult(values);
            }

            public static void warn(params string[] text)
            {
                CurrentScript.Print(string.Join(" ", text), LogLevel.Warn);
            }

            public static string version(bool meta = false)
            {
                return meta ? Engine.VersionWithMeta : Engine.Version;
            }

            public static string api()
            {
                return API.Dump();
            }

            public static GlobalSettings Settings()
            {
                return Engine.Settings;
            }

            public static UserSettings UserSettings()
            {
                return Engine.UserSettings;
            }

            public static DebuggerManager DebuggerManager()
            {
                AssertIdentity(ScriptIdentity.CoreScript | ScriptIdentity.Editor);
                return Game.DebuggerManager;
            }

            public void printidentity()
            {
                var identity = GetIdentity(LuaThread.running());
                if (identity.HasValue)
                {
                    OnPrint($"Current identity is {(byte)identity.Value} ({identity})");
                }
                else
                {
                    OnPrint("This thread has no identity.");
                }
            }

            public static double Wait(double seconds)
            {
                seconds = Math.Max(seconds, LuaSettings.DefaultWaitTime);

                var sw = Stopwatch.StartNew();

                var e = new WaitExecution(CurrentScript, (LuaThread)LuaThread.running().Values[0], seconds);

                ExecutionQueue.Enqueue(e);

                YieldThread();

                return sw.Elapsed.TotalSeconds;
            }

            public static double ElapsedTime()
            {
                return Environment.TickCount / 10000000.0;
            }

            public static double Tick()
            {
                return System.DateTime.UtcNow.Subtract(_unixEpoch).TotalSeconds;
            }

            public static double Time()
            {
                return DateTime.UnixTimestamp();
            }

            public static void Spawn(MulticastDelegate f)
            {
                var sw = Stopwatch.StartNew();

                var e = new WaitExecution(CurrentScript, (LuaThread)LuaThread.running().Values[0], 0,
                    () => f.FastDynamicInvoke(sw.Elapsed.TotalSeconds, Game.Workspace.DistributedGameTime));

                ExecutionQueue.Enqueue(e);
            }

            public static LuaResult require(ModuleScript module)
            {
                if (module.ReturnValue != null)
                    return module.ReturnValue;

                var e = new StartExecution(module);

                try
                {
                    e.TryFulfill();

                    if (e.Result?.Count != 1)
                        throw new Exception("Module did not return exactly one value.");
                }
                catch (Exception exception)
                {
                    throw new Exception("Requested module experienced an error while loading", exception);
                }

                return module.ReturnValue = e.Result;
            }

            private static LuaTable BuildEnumTable()
            {
                var enumTable = new LuaTable();

                foreach (var type in EnumTypes)
                {
                    var subTable = new LuaTable();
                    var names = Enum.GetNames(type);
                    var values = Enum.GetValues(type).Cast<dynamic>().ToList();

                    for (var i = 0; i < names.Length; i++)
                    {
                        subTable[names[i]] = values[i];
                    }

                    enumTable[type.Name] = subTable;
                }

                return enumTable;
            }

            protected override void OnPrint(string str)
            {
                CurrentScript.Print(str, LogLevel.Info);
            }

            /// <inheritdoc />
            protected override object OnIndex(object key)
            {
                if (Equals(key, "script"))
                {
                    AssertIdentity(ScriptIdentity.Editor, false);
                    return CurrentScript;
                }

                if (Equals(key, "core"))
                {
                    AssertIdentity(ScriptIdentity.CoreScript | ScriptIdentity.Editor | ScriptIdentity.Plugin);
                    return Game.CoreEnvironment;
                }

                if (Equals(key, "plugin"))
                {
                    AssertIdentity(ScriptIdentity.Plugin);
                    return CurrentScript.Tag;
                }

                if (Equals(key, "stats"))
                {
                    AssertIdentity(ScriptIdentity.Plugin);
                    return Game.Stats;
                }

                return base.OnIndex(key);
            }
        }
    }


    internal static class MathPackage
    {
        public static double huge => double.MaxValue;
        public static double pi => Math.PI;
        public static double twopi => MathUtil.TwoPi;
        public static double piovertwo => MathUtil.TwoPi;
        public static double e => Math.E;
        public static int mininteger => int.MinValue;
        public static int maxinteger => int.MaxValue;

        public static double round(double x)
        {
            return Math.Round(x);
        }

        public static double round(double x, int decimals)
        {
            return Math.Round(x, decimals);
        }

        public static double abs(double x)
        {
            return Math.Abs(x);
        }

        public static double acos(double x)
        {
            return Math.Acos(x);
        }

        public static double asin(double x)
        {
            return Math.Asin(x);
        }

        public static double atan(double x)
        {
            return Math.Atan(x);
        }

        public static double atan2(double y, double x)
        {
            return Math.Atan2(y, x);
        }

        public static double ceil(double x)
        {
            return Math.Ceiling(x);
        }

        public static double cos(double x)
        {
            return Math.Cos(x);
        }

        public static double cosh(double x)
        {
            return Math.Cosh(x);
        }

        public static double deg(double x)
        {
            return x * 180.0 / Math.PI;
        }

        public static double exp(double x)
        {
            return Math.Exp(x);
        }

        public static double floor(double x)
        {
            return x >= 0 ? (int)x : (int)x - 1;
        }

        public static double fmod(double x, double y)
        {
            return x % y;
        }

        public static LuaTuple<double, int> frexp(double x)
        {
            if (x == 0) return new LuaTuple<double, int>(0.0, 0);
            double delta = x > 0 ? 1 : -1;
            x = x * delta;
            var exponent = (int)(Math.Floor(Math.Log(x) / Math.Log(2)) + 1);
            var mantissa = x / Math.Pow(2, exponent);
            return new LuaTuple<double, int>(mantissa * delta, exponent);
        }

        public static double ldexp(double m, int exp)
        {
            return m * (2 ^ exp);
        }

        public static double log(double x, double b = Math.E)
        {
            return Math.Log(x, b);
        }

        public static double max(double[] x)
        {
            if (x == null || x.Length == 0)
                throw new ArgumentException("Number expected, got no value.", nameof(x));

            double r = double.MinValue;
            for (int i = 0; i < x.Length; i++)
                if (r < x[i])
                    r = x[i];

            return r;
        }

        public static double min(double[] x)
        {
            if (x == null || x.Length == 0)
                throw new ArgumentException("Number expected, got no value.", nameof(x));

            double r = Double.MaxValue;
            for (int i = 0; i < x.Length; i++)
                if (r > x[i])
                    r = x[i];

            return r;
        }

        public static LuaTuple<double, double> modf(double x)
        {
            if (x < 0)
            {
                var y = Math.Ceiling(x);
                return new LuaTuple<double, double>(y, y - x);
            }
            else
            {
                var y = Math.Floor(x);
                return new LuaTuple<double, double>(y, x - y);
            }
        }

        public static double pow(double x, double y)
        {
            return Math.Pow(x, y);
        }

        public static double rad(double x)
        {
            return x * Math.PI / 180.0;
        }

        public static object random(object m = null, object n = null)
        {
            return LuaLibraryMath.random(m, n);
        }

        public static void randomseed(object x)
        {
            LuaLibraryMath.randomseed(x);
        }

        public static double sin(double x)
        {
            return Math.Sin(x);
        }

        public static double sinh(double x)
        {
            return Math.Sinh(x);
        }

        public static double sqrt(double x)
        {
            return Math.Sqrt(x);
        }

        public static double tan(double x)
        {
            return Math.Tan(x);
        }

        public static double tanh(double x)
        {
            return Math.Tanh(x);
        }

        public static string type(object x)
        {
            return LuaLibraryMath.type(x);
        }

        public static double sign(double x)
        {
            return x < 0
                ? -1
                : x > 0
                    ? 1
                    : 0;
        }

        public static double saturate(double x)
        {
            return x < 0
                ? 0
                : x > 1
                    ? 1
                    : 0;
        }

        public static double lerp(double a, double b, double k)
        {
            return a * (1 - k) + b * k;
        }

        public static double smoothstep(double edge0, double edge1, double x)
        {
            x = saturate((x - edge0) / (edge1 - edge0));
            return x * x * (3 - 2 * x);
        }

        public static double smootherstep(double amount)
        {
            if (amount <= 0.0)
                return 0.0f;
            if (amount < 1.0)
                return (amount * amount * amount * (amount * (amount * 6.0 - 15.0) + 10.0));
            return 1f;
        }

        public static double clamp(double value, double min, double max)
        {
            return value < min
                ? min
                : value > max
                    ? max
                    : value;
        }

        public static double gaussian(double amplitude, double x, double y, double centerX, double centerY,
            double sigmaX, double sigmaY)
        {
            double num1 = x - centerX;
            double num2 = y - centerY;
            double num3 = num1 * num1 / (2.0 * sigmaX * sigmaX);
            double num4 = num2 * num2 / (2.0 * sigmaY * sigmaY);
            return amplitude * Math.Exp(-(num3 + num4));
        }

        public static object tointeger(object x)
        {
            try
            {
                return (long)Lua.RtConvertValue(x, typeof(long));
            }
            catch
            {
                return null;
            }
        }

        public static bool ult(long m, long n)
        {
            return m < n;
        }

        public static double noise(double x, double y)
        {
            return SimplexNoise.Noise(x, y);
        }

        public static double noise(double x, double y, double z)
        {
            return SimplexNoise.Noise(x, y, y);
        }

        public static double noise(double x, double y, double z, double w)
        {
            return SimplexNoise.Noise(x, y, z, w);
        }
    }

    internal static class DebugPackage
    {
        public static object debugger()
        {
            var id =
                LuaExceptionData.GetStackTrace(new StackTrace())
                    .First(x => x.Type == LuaStackFrameType.Lua)
                    .FileName;

            var script = ScriptService.Scripts[id];
            return script.Debugger;
        }

        public static string traceback()
        {
            var builder = new StringBuilder();

            var stackFrame = LuaExceptionData.GetStackTrace(new StackTrace()).Where(x =>
                x.Type == LuaStackFrameType.Lua);

            Script script = null;

            foreach (var item in stackFrame)
            {
                if (script == null)
                    script = ScriptService.Scripts[item.FileName];

                builder.Append($"Script '{script.GetFullName()}', Line {item.LineNumber}");

                if (item.MethodName != item.FileName)
                {
                    var methodname = item.MethodName.Substring(item.FileName.Length + 1);
                    builder.Append(" " + methodname);
                }

                builder.AppendLine();
            }

            builder.Append("Stack End");

            return builder.ToString();
        }

        public static LuaResult getupvalue(object f, int index)
        {
            return Lua.RtGetUpValue(f as Delegate, index);
        }

        public static LuaResult upvalueid(object f, int index)
        {
            return new LuaResult(Lua.RtUpValueId(f as Delegate, index));
        }

        public static LuaResult setupvalue(object f, int index, object v)
        {
            return new LuaResult(Lua.RtSetUpValue(f as Delegate, index, v));
        }

        public static void upvaluejoin(object f1, int n1, object f2, int n2)
        {
            Lua.RtUpValueJoin(f1 as Delegate, n1, f2 as Delegate, n2);
        }
    }
}