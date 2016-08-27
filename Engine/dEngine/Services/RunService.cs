// RunService.cs - dEngine
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
using System.IO;
using System.Threading.Tasks;
using C5;
using dEngine.Instances;
using dEngine.Instances.Attributes;
using dEngine.Serializer.V1;
using dEngine.Services.Networking;
using dEngine.Utility;
using NLog;

using SharpDX;
using Logger = NLog.Logger;

namespace dEngine.Services
{
    /// <summary>
    /// A service that handles the game logic loop.
    /// </summary>
    [TypeId(153), ExplorerOrder(-1)]
    public partial class RunService : Service
    {
        private static readonly ConcurrentQueue<TaskCompletionSource<PhysicsSimulation>> _simulationCreationQueue =
            new ConcurrentQueue<TaskCompletionSource<PhysicsSimulation>>();

        private class RenderStepBinding : IEquatable<RenderStepBinding>, IComparable<RenderStepBinding>
        {
            public string Name { get; }
            public Action Action { get; }
            public int Priority { get; }

            public RenderStepBinding(string name, Action action, int priority)
            {
                Name = name;
                Action = action;
                Priority = priority;
            }

            protected bool Equals(RenderStepBinding other)
            {
                return string.Equals(Name, other.Name);
            }

            public int CompareTo(RenderStepBinding other)
            {
                return Priority.CompareTo(other.Priority);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((RenderStepBinding)obj);
            }

            public override int GetHashCode()
            {
                return Name?.GetHashCode() ?? 0;
            }

            bool IEquatable<RenderStepBinding>.Equals(RenderStepBinding other)
            {
                return Name.Equals(other.Name);
            }
        }

        private readonly OrderedSet<RenderStepBinding> _bindings;
        private readonly Dictionary<string, RenderStepBinding> _dictionary;

        /// <summary>
        /// Fired when simulation ends.
        /// </summary>
        public readonly Signal SimulationEnded;

        /// <summary>
        /// Fired when simulation is paused.
        /// </summary>
        public readonly Signal SimulationPaused;

        /// <summary>
        /// Fired when simulation is resumed.
        /// </summary>
        public readonly Signal SimulationResumed;

        /// <summary>
        /// Fired when simulation starts.
        /// </summary>
        public readonly Signal SimulationStarted;

        /// <summary>
        /// Fired after every game thread frame.
        /// </summary>
        /// <eventParam name="step"/>
        public readonly Signal<double> Heartbeat;

        /// <summary>
        /// Fired after every render thread frame.
        /// </summary>
        /// <eventParam name="step"/>
        public readonly Signal<double> RenderStepped;

        /// <summary>
        /// Fired when the RunService is stepped, which is approximately every 1/30th of a second.
        /// </summary>
        /// <eventParam name="step"/>
        public readonly Signal<double> Stepped;

        /// <inheritdoc />
        public RunService()
        {
            Service = this;

            _bindings = new OrderedSet<RenderStepBinding>();
            _dictionary = new Dictionary<string, RenderStepBinding>();

            Heartbeat = new Signal<double>(this);
            RenderStepped = new Signal<double>(this);
            Stepped = new Signal<double>(this);
            SimulationStarted = new Signal(this);
            SimulationPaused = new Signal(this);
            SimulationResumed = new Signal(this);
            SimulationEnded = new Signal(this);

            ContextActionService.Register("playPause", PlayPause);
            ContextActionService.Register("stop", Stop);

            RenderStepped.Connect(OnRenderStep);
        }

        private void PlayPause()
        {
            if (SimulationState == SimulationState.Running)
                Pause();
            else
                Play();
        }

        private void OnRenderStep(double dt)
        {
            // TODO: render step bindings
            /*
            foreach (var nameAction in _dictionary)
            {
                nameAction.Value();
            }
            */
        }

        /// <summary>
        /// Attaches the given function to the render loop.
        /// </summary>
        public void BindToRenderStep(string name, int priority, Action function)
        {
            lock (Locker)
            {
                var binding = new RenderStepBinding(name, function, priority);
                _dictionary.Add(name, binding);
                _bindings.Add(binding);
            }
        }

        /// <summary>
        /// Detaches the given function from the render loop.
        /// </summary>
        public void UnbindFromRenderStep(string name)
        {
            lock (Locker)
            {
                RenderStepBinding binding;
                if (_dictionary.TryGetValue(name, out binding))
                {
                    _dictionary.Remove(name);
                    _bindings.Remove(binding);
                }
            }
        }

        /// <summary>
        /// Starts or resumes the simulation as a player.
        /// </summary>
        public void Play()
        {
            switch (SimulationState)
            {
                case SimulationState.Running:
                    throw new InvalidOperationException("The session is already running.");
                case SimulationState.Paused:
                    SimulationState = SimulationState.Running;
                    Engine.GameThread.Enqueue(SimulationResumed.Fire);
                    break;
                default:
                    _requestPlay = true;
                    break;
            }
        }

        /// <summary>
        /// Starts or resumes the simulation without a player.
        /// </summary>
        public void Run()
        {
            switch (SimulationState)
            {
                case SimulationState.Running:
                    throw new InvalidOperationException("The simulation is already running.");
                case SimulationState.Paused:
                    SimulationState = SimulationState.Running;
                    SimulationStopwatch.Start();
                    Engine.GameThread.Enqueue(SimulationResumed.Fire);
                    break;
                default:
                    _requestRun = true;
                    break;
            }
        }

        /// <summary>
        /// Pauses the simulation.
        /// </summary>
        public void Pause()
        {
            _requestPause = true;
        }

        /// <summary>
        /// Stops the simulation.
        /// </summary>
        public void Stop()
        {
            if (SimulationState != SimulationState.Running)
                SimulationState = SimulationState.Stopped;
            else
                _requestStop = true;
        }

        /// <summary>
        /// Returns true if the session is in run mode.
        /// </summary>
        public bool IsRunMode()
        {
            return SimulationType == SimulationType.Run && SimulationState != SimulationState.Stopped;
        }

        /// <summary>
        /// Returns true if the session is in play mode.
        /// </summary>
        public bool IsPlayMode()
        {
            return SimulationType == SimulationType.Play && SimulationState != SimulationState.Stopped;
        }

        /// <summary>
        /// Returns true if the simulation is paused.
        /// </summary>
        public bool IsPaused()
        {
            return SimulationState == SimulationState.Paused;
        }

        /// <summary>
        /// Returns true if the game is running as a client.
        /// </summary>
        public bool IsClient()
        {
            return NetworkPeer.IsClient();
        }

        /// <summary>
        /// Returns true if the game is running as a server.
        /// </summary>
        public bool IsServer()
        {
            return NetworkPeer.IsServer();
        }

        /// <summary>
        /// Returns true if the game is running from a level editor.
        /// </summary>
        [LevelEditorRelated]
        public bool IsLevelEditor()
        {
            return Engine.Mode == EngineMode.LevelEditor;
        }
    }

    public partial class RunService
    {
        private static Logger InternalLogger;
        private static MemoryStream _dataModelStream;
        private static Stopwatch _updateStopwatch;
        private static bool _requestPause;
        private static bool _requestPlay;
        private static bool _requestRun;
        private static bool _requestStop;

        /// <summary>
        /// The service instance.
        /// </summary>
        public static RunService Service;

        /// <summary>
        /// The session state.
        /// </summary>
        internal static SimulationState SimulationState { get; set; }

        /// <summary>
        /// The type of simulation that is running.
        /// </summary>
        internal static SimulationType SimulationType { get; set; }

        internal static Stopwatch SimulationStopwatch;

        internal static void Init()
        {
            _updateStopwatch = new Stopwatch();
            SimulationStopwatch = new Stopwatch();
            InternalLogger = LogManager.GetCurrentClassLogger();
            SimulationState = SimulationState.Stopped;
        }

        internal static RunService GetExisting()
        {
            return DataModel.GetService<RunService>();
        }

        private static void StartSession(SimulationType simulationType)
        {
            SimulationStopwatch.Start();
            _dataModelStream = new MemoryStream(1024);
            DataModel.Save(_dataModelStream, SaveFilter.SaveTogether);
            _dataModelStream.Position = 0;

            SimulationType = simulationType;
            SimulationState = SimulationState.Running;

            Game.NetworkClient = DataModel.GetService<NetworkClient>();
            Game.NetworkServer = DataModel.GetService<NetworkServer>();

            if (!Game.NetworkClient.IsRunning)
                Game.NetworkClient.Start();

            InternalLogger.Trace("Session started.");

            var isServer = NetworkPeer.IsServer();
            var isClient = NetworkPeer.IsClient();

            if (isServer)
            {
                InternalLogger.Info("Server session starting.");
            }

            ScriptService.ExecuteAllScripts(isClient, isServer);

            if (isClient)
            {
                InternalLogger.Info("Client session starting.");

                if (SimulationType == SimulationType.Play)
                {
                    var localPlayer = Players.Service.CreateLocalPlayer();
                    Players.Service.LocalPlayer = localPlayer;
                    Players.Service.PlayerAdded?.Fire(localPlayer);
                }
            }

            Service.SimulationStarted.Fire();
        }

        private static void PauseSession()
        {
            SimulationStopwatch.Stop();
            SimulationState = SimulationState.Paused;
            Service.SimulationPaused.Fire();
        }

        private static void StopSession()
        {
            SimulationStopwatch.Reset();
            SimulationState = SimulationState.Stopped;

            ScriptService.StopAllScripts();
            SoundService.StopAllSounds();

            if (Service.IsClient())
            {
                Game.NetworkClient.Disconnect();
                Players.Service.LocalPlayer?.Destroy();
                Players.Service.LocalPlayer = null;
            }
            if (Service.IsServer())
            {
                Game.NetworkServer.Stop();
            }

            Game.DataModel.ClearContent(true);
            Inst.Deserialize(_dataModelStream, Game.DataModel);
            Utilities.Dispose(ref _dataModelStream);

            InternalLogger.Trace("Session stopped.");
            Service.SimulationEnded.Fire();
        }

        internal static void Update(double step)
        {
            var rs = Service;

            _updateStopwatch.Restart();

            if (_requestStop)
            {
                StopSession();
                _requestStop = false;
            }
            else if (_requestPlay)
            {
                StartSession(SimulationType.Play);
                _requestPlay = false;
            }
            else if (_requestRun)
            {
                StartSession(SimulationType.Run);
                _requestRun = false;
            }
            else if (_requestPause)
            {
                PauseSession();
                _requestPause = false;
            }

            var camera = Game.FocusedCamera;
            if (camera != null)
                camera.MouseRay = camera.ScreenPointToRay(InputService.CursorX, InputService.CursorY);
            rs.Stepped.Fire(step);
        }

        /// <summary>
        /// Creates a new <see cref="PhysicsSimulation" /> on the physics thread.
        /// </summary>
        /// <returns></returns>
        public static async Task<PhysicsSimulation> CreatePhysicsSimulationAsync()
        {
            var request = new TaskCompletionSource<PhysicsSimulation>();
            _simulationCreationQueue.Enqueue(request);
            return await request.Task.ConfigureAwait(false);
        }
    }
}