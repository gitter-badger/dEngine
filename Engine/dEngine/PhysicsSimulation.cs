// PhysicsSimulation.cs - dEngine
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
using BulletSharp;
using dEngine.Graphics;
using dEngine.Instances;
using dEngine.Services;
using dEngine.Settings.Global;
using Neo.IronLua;
using SharpDX.Direct3D11;

namespace dEngine
{
    /// <summary>
    /// An object for managing a physics simulation.
    /// </summary>
    public class PhysicsSimulation
    {
        /// <summary>
        /// The scale of the physics engine.
        /// </summary>
        public const float Scale = 1 / 20f;

        internal static readonly object Locker = new object();
        private readonly Stopwatch _stopwatch = new Stopwatch();

        private readonly PhysicsDebugDraw _debugDraw;

        internal DbvtBroadphase Broadphase;
        internal CollisionConfiguration CollisionConfiguration;
        internal CollisionDispatcher Dispatcher;
        internal SequentialImpulseConstraintSolver Solver;
        internal DynamicsWorld World;
        private readonly GhostPairCallback _ghostPairCallback;

        /// <summary>
        /// Constructs a new PhysicsSimulation. Must be created on the physics thread.
        /// </summary>
        internal PhysicsSimulation()
        {
            _ghostPairCallback = new GhostPairCallback();
            Broadphase = new DbvtBroadphase();
            CollisionConfiguration = new DefaultCollisionConfiguration();

            Dispatcher = new CollisionDispatcher(CollisionConfiguration);
            Solver = new SequentialImpulseConstraintSolver(); // Projected Gauss Seidel
            World = new DiscreteDynamicsWorld(Dispatcher, Broadphase, Solver, CollisionConfiguration)
            {
                DebugDrawer = _debugDraw = new PhysicsDebugDraw()
            };
            World.PairCache.SetInternalGhostPairCallback(_ghostPairCallback);

            _stopwatch.Start();
        }

        /// <summary>
        /// The gravity force.
        /// </summary>
        internal Vector3 Gravity
        {
            get { return (Vector3)World.Gravity; }
            set
            {
                lock (Locker)
                {
                    World.Gravity = (BulletSharp.Math.Vector3)value;
                }
            }
        }

        internal ulong SolverSeed
        {
            get { return Solver.RandSeed; }
            set { Solver.RandSeed = value; }
        }

        internal event Action Stepped;

        /// <summary />
        public void UpdateDebugDrawModes()
        {
            _debugDraw.DebugMode = PhysicsSettings.DebugDrawModes;
        }

        internal void DrawWorld(ref DeviceContext context, ref Camera camera)
        {
            _debugDraw.Draw(ref context, ref camera);
            World.DebugDrawWorld();
        }

        /// <summary>
        /// Steps the simulation.
        /// </summary>
        internal void Step(float step)
        {
            lock (Locker)
            {
                if (RunService.SimulationState != SimulationState.Running)
                {
                    if (!(_stopwatch.Elapsed.TotalSeconds > Engine.GameThread.FixedStep)) return;
                    //World.UpdateAabbs();
                    //World.ComputeOverlappingPairs();
                    _stopwatch.Restart();
                }
                else
                {
                    Stepped?.Invoke();
                    //World.StepSimulation(step, 1, Engine.GameThread.FixedStep);
                }
            }
        }

        /// <summary>
        /// Adds a <see cref="Part" /> to the simulation.
        /// </summary>
        /// <param name="part"></param>
        internal void AddPart(Part part)
        {
            lock (Locker)
            {
                if (part.IsDestroyed)
                    return;
                var rigidBody = part.RigidBody;
                Debug.Assert(rigidBody != null, "RigidBody != null");
                World.AddRigidBody(rigidBody);
            }
        }

        /// <summary>
        /// Removes a <see cref="Part" /> from the simulation.
        /// </summary>
        /// <param name="part"></param>
        internal void RemovePart(Part part)
        {
            lock (Locker)
            {
                var rigidBody = part.RigidBody;

                Debug.Assert(rigidBody != null, "rigidBody != null");

                if (!rigidBody.IsInWorld)
                    return;
                
                World.RemoveRigidBody(rigidBody);
            }
        }

        /// <summary>
        /// Adds a constraint to the world.
        /// </summary>
        internal void AddConstraint(Constraint constraint)
        {
            World.AddConstraint(constraint, false);
        }

        /// <summary>
        /// Removes a constraint from the world.
        /// </summary>
        internal void RemoveConstraint(Constraint constraint)
        {
            World.RemoveConstraint(constraint);
        }

        /// <summary>
        /// Performs an un-filtered raycast.
        /// </summary>
        /// <param name="ray">The ray to perform the cast on.</param>
        /// <param name="maxLength">The maximum length of the ray.</param>
        /// <returns></returns>
        internal RayCastResult FindPartOnRay(Ray ray, float maxLength = 3000)
        {
            lock (Locker)
            {
                var from = ray.Origin;
                Vector3 to;

                Vector3.Multiply(ref ray.Direction, maxLength, out to);
                Vector3.Add(ref to, ref from, out to);
                
                var bulletFrom = new BulletSharp.Math.Vector3(from.x, from.y, from.z);
                var bulletTo = new BulletSharp.Math.Vector3(to.x, to.y, to.z);

                var resultCallback = new ClosestRayResultCallback(ref bulletFrom, ref bulletTo);
                World.RayTest(bulletFrom, bulletTo, resultCallback);

                return new RayCastResult(ref resultCallback);
            }
        }


        /// <summary>
        /// Performs a filtered raycast.
        /// </summary>
        /// <param name="ray">The ray to perform the cast on.</param>
        /// <param name="filterSet">A list of items to be ignored.</param>
        /// <param name="maxLength">The maximum length of the ray.</param>
        /// <returns></returns>
        internal RayCastResult FindPartOnRay(Ray ray, HashSet<Instance> filterSet,
            float maxLength = 1000)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Performs an un-filtered raycast.
        /// </summary>
        /// <param name="ray">The ray to perform the cast on.</param>
        /// <param name="filterFunc">A predicate that determines whether an object is ignored.</param>
        /// <param name="maxLength">The maximum length of the ray.</param>
        /// <returns></returns>
        internal RayCastResult FindPartOnRay(Ray ray, Func<Part, bool> filterFunc,
            float maxLength = 1000)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Contains information about a raycast.
        /// </summary>
        public struct RayCastResult
        {
            internal RayCastResult(ref Part part, ref Vector3 position, ref Vector3 normal)
            {
                HitObject = part;
                Position = position;
                Normal = normal;
            }

            internal RayCastResult(ref ClosestRayResultCallback callback)
            {
                var hitPoint = callback.HitPointWorld;
                var hitNormal = callback.HitNormalWorld;

                HitObject = callback.CollisionObject?.UserObject as Part;
                Position = new Vector3(hitPoint.X, hitPoint.Y, hitPoint.Z);
                Normal= new Vector3(hitNormal.X, hitNormal.Y, hitNormal.Z);
            }

            /// <summary>
            /// The hit part.
            /// </summary>
            public readonly Part HitObject;

            /// <summary>
            /// The hit normal.
            /// </summary>
            public readonly Vector3 Normal;

            /// <summary>
            /// The hit position.
            /// </summary>
            public readonly Vector3 Position;

            public LuaTuple<Part, Vector3, Vector3> ToTuple()
            {
                return new LuaTuple<Part, Vector3, Vector3>(HitObject, Position, Normal);
            }
        }
    }
}