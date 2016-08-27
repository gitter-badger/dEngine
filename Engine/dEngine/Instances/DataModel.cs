// DataModel.cs - dEngine
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
using System.IO;
using System.Linq;
using dEngine.Instances.Attributes;
using dEngine.Serializer.V1;
using dEngine.Services;
using dEngine.Utility;
using Dynamitey;
using Neo.IronLua;

namespace dEngine.Instances
{
    /// <summary>
    /// The root object of the hierarchy.
    /// </summary>
    /// <remarks>
    /// The DataModel is the root of the game hierarchy. Its members are related to game and service locator. It is accessed
    /// through the global variable `game`.
    /// ### Services
    /// All <see cref="Service" />s in the game are parented and accessed through the DataModel.
    /// ```lua
    /// local service = game.ReplicatedStorage -- access via children
    /// ```
    /// ```lua
    /// local service = game:GetService("ReplicatedStorage") -- access via GetService
    /// ```
    /// </remarks>
    [TypeId(2)]
    [Uncreatable]
    [ToolboxGroup("Containers")]
    public sealed class DataModel : Instance, ISingleton
    {
        private static Dictionary<string, string> _arguments;
        private readonly IEnumerable<Type> _allServices;
        private readonly Dictionary<string, Service> _createdServices;
        private bool _loaded;

        /// <summary>
        /// Creates the DataModel and the main Workspace.
        /// </summary>
        internal DataModel()
        {
            _closeBindings = new ConcurrentDictionary<Action, OnCloseBinding>();

            _createdServices = new Dictionary<string, Service>();

            _allServices =
            (from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
                from assemblyType in domainAssembly.GetTypes()
                where typeof(Service).IsAssignableFrom(assemblyType)
                select assemblyType).ToArray();

            ParentLocked = true;

            Loaded = new Signal(this);
            ServiceAdded = new Signal<Service>(this);
            ServiceRemoved = new Signal<Service>(this);

            Logger.Trace("DataModel loaded.");
        }

        /// <summary>
        /// A unique identifer for the current game server.
        /// </summary>
        public static string JobId { get; set; }

        /// <summary>
        /// Sets the startup arguments for the game.
        /// </summary>
        public static void SetStartupArguments(Dictionary<string, string> arguments)
        {
            _arguments = arguments;
        }

        /// <summary>
        /// Returns a dictionary of startup arguments.
        /// </summary>
        public LuaTable GetStartupArguments()
        {
            return _arguments.ToLuaTable();
        }

        internal static DataModel GetExisting()
        {
            return Game.DataModel;
        }

        internal void OnClose()
        {
            lock (Locker)
            {
                var callbacks = _closeBindings.Values.OrderBy(binding => binding.Priority);
                foreach (var binding in callbacks)
                    binding.Callback();
            }
        }

        #region Properties

        internal ICollection<Service> Services => _createdServices.Values;

        /// <summary>
        /// The workspace for the main scene.
        /// </summary>
        public Workspace Workspace { get; internal set; }

        /// <summary>
        /// Gets whether or not the game has been loaded.
        /// </summary>
        public bool IsLoaded
        {
            get { return _loaded; }
            set
            {
                _loaded = value;

                if (value)
                    Logger.Trace("DataModel loaded.");

                NotifyChanged(nameof(IsLoaded));
                Loaded.Fire();
            }
        }

        /// <summary />
        [ScriptSecurity(ScriptIdentity.Editor | ScriptIdentity.CoreScript)]
        public void Shutdown(int exitCode = 0)
        {
            ScriptService.AssertIdentity(ScriptIdentity.Editor | ScriptIdentity.CoreScript);
            Engine.Shutdown(exitCode);
        }

        /// <summary>
        /// Sets the analytics tracking ID.
        /// </summary>
        [ScriptSecurity(ScriptIdentity.Editor | ScriptIdentity.CoreScript)]
        public void SetTrackingId(string trackingId)
        {
            ScriptService.AssertIdentity(ScriptIdentity.Editor | ScriptIdentity.CoreScript);
            if (!trackingId.StartsWith("UA-"))
                throw new FormatException("Tracking ID was not a valid Google Analytics ID.");
        }

        /// <summary>
        /// Reports analytical data.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="action"></param>
        /// <param name="label"></param>
        /// <param name="value"></param>
        [ScriptSecurity(ScriptIdentity.Editor | ScriptIdentity.CoreScript)]
        public void ReportInGoogleAnalytics(string category, string action, string label, int value)
        {
            ScriptService.AssertIdentity(ScriptIdentity.Editor | ScriptIdentity.CoreScript);
            throw new NotImplementedException();
        }

        /// <summary />
        [ScriptSecurity(ScriptIdentity.Editor | ScriptIdentity.CoreScript | ScriptIdentity.Plugin)]
        public void ClearContent(bool resettingSimulation)
        {
            ScriptService.AssertIdentity(ScriptIdentity.Editor | ScriptIdentity.CoreScript | ScriptIdentity.Plugin);
            ClearChildren();
        }

        private readonly ConcurrentDictionary<Action, OnCloseBinding> _closeBindings;

        private class OnCloseBinding
        {
            public OnCloseBinding(Action callback, int priority)
            {
                Callback = callback;
                Priority = priority;
            }

            public readonly Action Callback;
            public readonly int Priority;
        }

        /// <summary>
        /// Binds a function to be called when the game is closing.
        /// </summary>
        /// <param name="callback">The callback function.</param>
        /// <param name="priority">The priority. Higher numbers are called first.</param>
        public void BindToClose(LuaAction callback, int priority = 0)
        {
            _closeBindings.TryAdd(callback, new OnCloseBinding(callback, priority));
        }

        /// <summary>
        /// Removes a function that was previously bound to the game closing.
        /// </summary>
        public void UnbindFromClose(LuaAction callback)
        {
            _closeBindings.TryRemove(callback);
        }

        /// <summary>
        /// The path to the startup place for level editors.
        /// </summary>
        [InstMember(1)]
        [Obsolete]
        public string StartupPlace
        {
            get { return _startupPlace; }
            set
            {
                if (Equals(value, _startupPlace)) return;
                _startupPlace = value;
                NotifyChanged();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Fired when the game has loaded.
        /// </summary>
        /// <seealso cref="dEngine.Services.Workspace.PlaceLoaded" />
        public readonly Signal Loaded;

        /// <summary>
        /// Fired when a service is added.
        /// </summary>
        public readonly Signal<Service> ServiceAdded;

        /// <summary>
        /// Fired when a service is removed.
        /// </summary>
        public readonly Signal<Service> ServiceRemoved;

        private string _startupPlace;

        #endregion

        #region Methods

        internal override void AfterDeserialization(Inst.Context context)
        {
            base.AfterDeserialization(context);
            Children.Insert(Workspace, 0);
        }

        /// <inheritdoc />
        protected override void OnChildAdded(Instance child)
        {
            base.OnChildAdded(child);
            var service = child as Service;
            if (service != null)
                ServiceAdded.Fire(service);
        }

        /// <inheritdoc />
        protected override void OnChildRemoved(Instance child)
        {
            base.OnChildRemoved(child);
            var service = child as Service;
            if (service != null)
                ServiceRemoved.Fire(service);
        }

        /// <summary>
        /// Serializes the DataModel and/or Workspace.
        /// </summary>
        public void SaveGame(SaveFilter filter)
        {
            Logger.Info($"Saving DataModel. ({filter})");

            Save(filter);
        }

        /// <summary>
        /// Returns a list of all created services.
        /// </summary>
        public LuaTable GetServices()
        {
            lock (Locker)
            {
                return _createdServices.Values.ToLuaTable();
            }
        }

        /// <summary>
        /// Returns the service with the specified name. Returns null if the service was not already created.
        /// </summary>
        /// <param name="className">The class name of the service.</param>
        public Service FindService(string className)
        {
            lock (Locker)
            {
                Service service;
                return !_createdServices.TryGetValue(className, out service) ? null : service;
            }
        }

        /// <summary>
        /// Returns the service with the specified name. If the service has not been created, create it.
        /// </summary>
        /// <param name="className">The class name of the service.</param>
        public Service GetService(string className)
        {
            lock (Locker)
            {
                var service = FindService(className);

                if (service != null) return service;
                var type = _allServices.FirstOrDefault(x => x.Name == className);
                if (type == null) throw new Exception($"Cannot get service of type \"{className}\"");
                service = (Service)Dynamic.InvokeConstructor(type);
                _createdServices[className] = service;
                return service;
            }
        }

        /// <summary>
        /// Returns the service with the specified type.
        /// </summary>
        /// <typeparam name="T">The service type.</typeparam>
        public static T GetService<T>() where T : Service
        {
            return (T)Game.DataModel.GetService(typeof(T).Name);
        }

        internal static void Save(SaveFilter filter)
        {
            var stream = new MemoryStream(1024);
            Save(stream, filter);
        }

        internal static void Save(Stream stream, SaveFilter filter)
        {
            if (filter == SaveFilter.SaveWorld)
                lock (Game.Workspace.Locker)
                {
                    if (Game.Workspace.PlaceId != string.Empty)
                    {
                        Inst.Serialize(Game.Workspace, stream);
                        stream.Position = 0;
                        Engine.SavePlace?.Invoke(Game.Workspace.PlaceId, stream);
                    }
                    else
                    {
                        Game.DataModel.Logger.Warn($"Workspace.Place is empty: cannot save.");
                    }
                }

            if (filter == SaveFilter.SaveGame)
            {
                Inst.Serialize(Game.DataModel, stream, includeWorkspaceInGame: false);
                stream.Position = 0;
                Engine.SaveGame?.Invoke(stream);
            }

            if (filter == SaveFilter.SaveTogether)
                Inst.Serialize(Game.DataModel, stream, includeWorkspaceInGame: true);
        }

        #endregion
    }
}