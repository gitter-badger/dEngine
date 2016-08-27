// Explosion.cs - dEngine
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
using BulletSharp;
using dEngine.Instances.Attributes;
using dEngine.Services;

namespace dEngine.Instances
{
    /// <summary>
    /// An explosion applies force to objects within the blast radius at the position of the parent <see cref="Part" />.
    /// </summary>
    [TypeId(220), ExplorerOrder(3), ToolboxGroup("Gameplay")]
    public class Explosion : Instance
    {
        private readonly ParticleEmitter _emitter;
        private readonly GhostObject _ghostObject;
        private float _blastPressure;
        private float _blastRadius;
        private bool _craters;
        private Part _parentPart;
        private bool _visible;

        /// <summary />
        public Explosion()
        {
            _emitter = new ParticleEmitter();
            _ghostObject = new GhostObject {UserObject = this, CollisionFlags = CollisionFlags.NoContactResponse};

            BlastPressure = 500000;
            BlastRadius = 4;
            Craters = true;

            OnHit = new Signal<Part, float>(this);

            ParentChanged.Connect(OnParentChanged);
            RunService.Service.SimulationStarted.Connect(Explode);
        }

        /*
        protected override void OnWorldChanged(IWorld newWorld, IWorld oldWorld)
        {
            base.OnWorldChanged(newWorld, oldWorld);
            oldWorld?.Physics.World.RemoveCollisionObject(_ghostObject);
            newWorld?.Physics.World.AddCollisionObject(_ghostObject, CollisionFilterGroups.SensorTrigger, CollisionFilterGroups.DefaultFilter);
        }
        */

        /// <summary>
        /// Determines the amount of force to apply to objects within the <see cref="BlastRadius" />
        /// </summary>
        [InstMember(1), EditorVisible("Data")]
        public float BlastPressure
        {
            get { return _blastPressure; }
            set
            {
                if (value == _blastPressure) return;
                _blastPressure = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// The radius of the explosion.
        /// </summary>
        [InstMember(2), EditorVisible("Data")]
        public float BlastRadius
        {
            get { return _blastRadius; }
            set
            {
                if (value == _blastRadius) return;
                _blastRadius = value;
                _ghostObject.CollisionShape = new SphereShape(value);
                NotifyChanged();
            }
        }

        /// <summary>
        /// Determines whether the explosion will make craters terrain.
        /// </summary>
        [InstMember(3), EditorVisible("Data")]
        public bool Craters
        {
            get { return _craters; }
            set
            {
                if (value == _craters) return;
                _craters = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Determines whether the explosion is visible.
        /// </summary>
        [InstMember(4), EditorVisible("Data")]
        public bool Visible
        {
            get { return _visible; }
            set
            {
                if (value == _visible) return;
                _visible = value;
                NotifyChanged();
            }
        }

        /// <summary />
        public override void Destroy()
        {
            base.Destroy();
            _emitter.Destroy();
        }

        private void OnParentChanged(Instance parent)
        {
            var part = parent as Part;
            _parentPart = part;
            Explode();
        }

        private void Explode()
        {
            var parentPart = _parentPart;

            if (parentPart == null)
                return;

            var ghost = _ghostObject;

            ghost.WorldTransform = _parentPart.RigidBody.WorldTransform;

            //_emitter.Emit(1);

            if (RunService.SimulationState != SimulationState.Running)
                return;
            
            var resultCallback = new ExplosionContactTestCallback(this, parentPart.Position);
            World.Physics.World.ContactTest(_ghostObject, resultCallback);

            Destroy();
        }

        private class ExplosionContactTestCallback : ContactResultCallback
        {
            private readonly Vector3 _parentPosition;
            private readonly float _pressure;
            private readonly float _radius;
            private readonly Explosion _explosion;

            public ExplosionContactTestCallback(Explosion explosion, Vector3 position)
            {
                _explosion = explosion;
                _parentPosition = explosion._parentPart.Position;
                _pressure = explosion._blastPressure;
                _radius = explosion._blastRadius;
            }

            public override float AddSingleResult(ManifoldPoint cp, CollisionObjectWrapper colObj0Wrap, int partId0, int index0,
                CollisionObjectWrapper colObj1Wrap, int partId1, int index1)
            {
                var part = (Part)colObj1Wrap.CollisionObject.UserObject;

                var didHitCharacter = part is Character;
                
                var partPosition = part.CFrame.p;

                var delta = partPosition - _parentPosition;
                var dist = delta.magnitude;
                var normal = delta == Vector3.Zero ? Vector3.Up : delta.unit;
                normal = Vector3.Up;
                var radius = part.Size.magnitude / 2;
                var surfaceArea = radius * radius;
                var impulse = normal * _pressure * surfaceArea * (1.0f / 4560.0f);
                var frac = didHitCharacter ? 1 - Math.Max(0, Math.Min(1, (dist - 2) / _radius)) : 1;
                impulse *= frac;
                var torque = impulse * 0.5f * radius;
                var bulletImpulse = new BulletSharp.Math.Vector3(impulse.x, impulse.y, impulse.z);
                var bulletPosition = new BulletSharp.Math.Vector3(partPosition.X, partPosition.Y, partPosition.z);
                var bulletTorque = new BulletSharp.Math.Vector3(torque.x, torque.y, torque.z);

                part.RigidBody.ApplyImpulseRef(ref bulletImpulse, ref bulletPosition);
                part.RigidBody.ApplyTorqueImpulseRef(ref bulletTorque);

                _explosion.OnHit.Fire(part, dist);
                return 0;
            }
        }

        /// <summary>
        /// Fires for every physical object that is caught in the <see cref="BlastRadius" />.
        /// </summary>
        /// <eventParam name="object" />
        /// <eventParam name="distance" />
        public readonly Signal<Part, float> OnHit;
    }
}