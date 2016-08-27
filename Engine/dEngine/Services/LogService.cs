// LogService.cs - dEngine
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
using System.Diagnostics;
using System.Reflection;
using System.Text;
using dEngine.Instances;
using dEngine.Instances.Attributes;
using dEngine.Settings.Global;
using Microsoft.Scripting.Utils;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace dEngine.Services
{
    /// <summary>
    /// A service for managing logging.
    /// </summary>
    [TypeId(140), ExplorerOrder(-1)]
    public class LogService : Service
    {
        internal static LogService Service;

        /// <summary />
        public LogService()
        {
            MessageOutput = new Signal<string, LogLevel, string>(this);

            Service = this;
        }

        /// <summary>
        /// Executes a string of code on the server-side.
        /// </summary>
        [ScriptSecurity(ScriptIdentity.CoreScript)]
        public void ExecuteScript(string source)
        {
            ScriptService.AssertIdentity(ScriptIdentity.CoreScript);
            throw new NotImplementedException();
        }

        internal static ILogger GetLogger(string name)
        {
            return new LoggerNLog(LogManager.GetLogger(name));
        }

        internal static ILogger GetLogger()
        {
            int skipFrames = 2;
            Type declaringType;
            string str;
            do
            {
                MethodBase method = new StackFrame(skipFrames, false).GetMethod();
                declaringType = method.DeclaringType;
                if (declaringType == null)
                {
                    str = method.Name;
                    break;
                }
                ++skipFrames;
                str = declaringType.FullName;
            } while (declaringType.GetModule().Name.Equals("mscorlib.dll", StringComparison.OrdinalIgnoreCase));
            return GetLogger(str);
        }

        internal static LogService GetExisting()
        {
            return DataModel.GetService<LogService>();
        }

        private static void ArchiveTarget(FileTarget target)
        {
            target.MaxArchiveFiles = 3;
            target.ArchiveNumbering = ArchiveNumberingMode.Rolling;
            target.ArchiveEvery = FileArchivePeriod.None;
            target.EnableArchiveFileCompression = false;
            target.ArchiveOldFileOnStartup = true;
        }

        /// <summary>
        /// Configures logging targets and rules.
        /// </summary>
        internal static void ConfigureLoggers()
        {
            const bool keepFileOpen = true;

            var engineFileTarget = new FileTarget
            {
                Name = "EngineFileTarget",
                FileName = "${basedir}/logs/engine.log",
                Layout = "${date:format=HH\\:mm\\:ss.fff} (${threadid}) ${logger:shortName=true}: ${message}",
                Encoding = Encoding.UTF8,
                AutoFlush = true,
                DeleteOldFileOnStartup = true,
                CreateDirs = true,
                KeepFileOpen = keepFileOpen,
                ConcurrentWrites = true,
                ConcurrentWriteAttempts = 1,
            };
            ArchiveTarget(engineFileTarget);

            var editorFileTarget = new FileTarget
            {
                Name = "EditorFileTarget",
                FileName = "${basedir}/logs/editor.log",
                Layout = "${date:format=HH\\:mm\\:ss.fff} (${threadid}) ${logger:shortName=true}: ${message}",
                Encoding = Encoding.UTF8,
                AutoFlush = true,
                DeleteOldFileOnStartup = true,
                CreateDirs = true,
                KeepFileOpen = keepFileOpen,
                ConcurrentWriteAttempts = 1,
            };

            var scriptFileTarget = new FileTarget
            {
                Name = "ScriptFileTarget",
                FileName = "${basedir}/logs/scripts.log",
                Layout = "${longdate} - ${logger:shortName=true}: ${message}",
                Encoding = Encoding.UTF8,
                AutoFlush = true,
                DeleteOldFileOnStartup = true,
                CreateDirs = true,
                KeepFileOpen = keepFileOpen,
                ConcurrentWrites = true,
            };
            ArchiveTarget(scriptFileTarget);

            var engineMethodTarget = new MethodCallTarget
            {
                Name = "EngineMethodTarget",
                ClassName = "dEngine.Services.LogService, dEngine",
                MethodName = nameof(OnLog),
            };

            engineMethodTarget.Parameters.Add(new MethodCallParameter {Layout = "${logger}"});
            engineMethodTarget.Parameters.Add(new MethodCallParameter {Layout = "${level}"});
            engineMethodTarget.Parameters.Add(new MethodCallParameter
            {
                Layout = "${date:format=HH\\:mm\\:ss.fff} - ${message}"
            });

            if (LogManager.Configuration == null)
                LogManager.Configuration = new LoggingConfiguration();

            var logConfig = LogManager.Configuration;

            logConfig.AddTarget(engineMethodTarget);
            logConfig.AddTarget(engineFileTarget);
            logConfig.AddTarget(editorFileTarget);
            logConfig.AddTarget(scriptFileTarget);

            logConfig.LoggingRules.Add(new LoggingRule("*", NLog.LogLevel.Trace, engineMethodTarget));
            logConfig.LoggingRules.Add(new LoggingRule("dEngine.*", NLog.LogLevel.Trace, engineFileTarget));
            logConfig.LoggingRules.Add(new LoggingRule("dEditor.*", NLog.LogLevel.Trace, editorFileTarget));
            logConfig.LoggingRules.Add(new LoggingRule("I:*", NLog.LogLevel.Trace, scriptFileTarget));

            LogManager.ReconfigExistingLoggers();
        }

        /// <summary />
        public static void OnLog(string logger, string level, string message)
        {
            Console.WriteLine(message);
            /*
#if DEBUG
            if (level == "Error" || level == "Warn")
            {
                Debug.WriteLine(message);
            }
#endif
*/
            var logLevel = (LogLevel)Enum.Parse(typeof(LogLevel), level);

            if (Game.IsInitialized)
            {
                var service = Service ?? GetExisting();
                if (DebugSettings.LoggingEnabled)
                    service.MessageOutput.Fire(message, logLevel, logger);
            }
        }

        private class LoggerNLog : ILogger
        {
            private readonly Logger _logger;

            internal LoggerNLog(Logger nlogger)
            {
                _logger = nlogger;
            }

            public void Log(string message, LogLevel level)
            {
                switch (level)
                {
                    case LogLevel.Trace:
                        _logger.Trace(message);
                        break;
                    case LogLevel.Info:
                        _logger.Info(message);
                        break;
                    case LogLevel.Warn:
                        _logger.Warn(message);
                        break;
                    case LogLevel.Error:
                        _logger.Error(message);
                        break;
                    case LogLevel.Fatal:
                        _logger.Fatal(message);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(level), level, null);
                }
            }

            public void Trace(string message)
            {
                _logger.Trace(message);
            }

            public void Info(string message)
            {
                _logger.Info(message);
            }

            public void Warn(string message)
            {
                _logger.Warn(message);
            }

            public void Error(string message)
            {
                _logger.Error(message);
            }

            public void Error(Exception e, string message = null)
            {
                for (;;) // loop through inner exceptions
                {
                    if (!string.IsNullOrEmpty(message))
                        _logger.Error(message);

                    _logger.Error(e.Message);

                    if (e.StackTrace != null)
                    {
                        foreach (var item in e.StackTrace.Split(new[] {"\r\n", "\n"}, 0))
                        {
                            _logger.Trace(item);
                        }
                    }

                    if (e.InnerException != null)
                    {
                        e = e.InnerException;
                        message = null;
                        continue;
                    }
                    break;
                }
            }

            public void Fatal(string message)
            {
                _logger.Fatal(message);
            }

            public void Fatal(Exception e, string message = null)
            {
                for (;;) // loop through inner exceptions
                {
                    if (!string.IsNullOrEmpty(message))
                        _logger.Fatal(message);

                    _logger.Fatal(e.Message);

                    if (e.StackTrace != null)
                    {
                        foreach (var item in e.StackTrace.Split(new[] {"\r\n", "\n"}, 0))
                        {
                            _logger.Trace(item);
                        }
                    }

                    if (e.InnerException != null)
                    {
                        e = e.InnerException;
                        message = null;
                        continue;
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Fired when a message is logged.
        /// </summary>
        /// <remarks>
        /// Outputting a message within the callback will case a stack overflow.
        /// </remarks>
        public readonly Signal<string, LogLevel, string> MessageOutput;
    }

#pragma warning disable 1591
    public interface ILogger
    {
        void Log(string message, LogLevel level);
        void Trace(string message);
        void Info(string message);
        void Warn(string message);
        void Error(string message);
        void Fatal(string message);
        void Error(Exception exception, string message = null);
        void Fatal(Exception exception, string message = null);
    }
#pragma warning restore 1591
}