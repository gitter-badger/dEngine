// RocketPropulsion.cs - dEngine
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
using dEngine.Instances.Attributes;

namespace dEngine.Instances
{
    /// <summary>
    /// Applies force to its parent in a similar manner to a rocket.
    /// </summary>
    [TypeId(132)]
    public sealed class RocketPropulsion : BodyMover
    {
        private float _cartoonFactor;
        private float _maxSpeed;
        private float _maxThrust;
        private Vector3 _maxTorque;
        private Part _target;
        private Vector3 _targetOffset;
        private float _targetRadius;
        private float _thrustD;
        private float _thrustP;
        private float _turnD;
        private float _turnP;

        /// <summary>
        /// Summary
        /// </summary>
        [InstMember(1), EditorVisible("Data")]
        public float CartoonFactor
        {
            get { return _cartoonFactor; }
            set
            {
                if (value.Equals(_cartoonFactor)) return;
                _cartoonFactor = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Summary
        /// </summary>
        [InstMember(2), EditorVisible("Data")]
        public float MaxSpeed
        {
            get { return _maxSpeed; }
            set
            {
                if (value.Equals(_maxSpeed)) return;
                _maxSpeed = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Summary
        /// </summary>
        [InstMember(3), EditorVisible("Data")]
        public float MaxThrust
        {
            get { return _maxThrust; }
            set
            {
                if (value.Equals(_maxThrust)) return;
                _maxThrust = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// The maximum amount of torque that can be applied.
        /// </summary>
        [InstMember(4), EditorVisible("Data")]
        public Vector3 MaxTorque
        {
            get { return _maxTorque; }
            set
            {
                if (value.Equals(_maxTorque)) return;
                _maxTorque = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Summary
        /// </summary>
        [InstMember(5), EditorVisible("Data")]
        public Part Target
        {
            get { return _target; }
            set
            {
                if (value.Equals(_target)) return;
                _target = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Summary
        /// </summary>
        [InstMember(6), EditorVisible("Data")]
        public Vector3 TargetOffset
        {
            get { return _targetOffset; }
            set
            {
                if (value.Equals(_targetOffset)) return;
                _targetOffset = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Summary
        /// </summary>
        [InstMember(7), EditorVisible("Data")]
        public float TargetRadius
        {
            get { return _targetRadius; }
            set
            {
                if (value.Equals(_targetRadius)) return;
                _targetRadius = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Summary
        /// </summary>
        [InstMember(8), EditorVisible("Data")]
        public float ThrustD
        {
            get { return _thrustD; }
            set
            {
                if (value.Equals(_thrustD)) return;
                _thrustD = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Summary
        /// </summary>
        [InstMember(10), EditorVisible("Data")]
        public float ThrustP
        {
            get { return _thrustP; }
            set
            {
                if (value.Equals(_thrustP)) return;
                _thrustP = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Summary
        /// </summary>
        [InstMember(11), EditorVisible("Data")]
        public float TurnD
        {
            get { return _turnD; }
            set
            {
                if (value.Equals(_turnD)) return;
                _turnD = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Summary
        /// </summary>
        [InstMember(12), EditorVisible("Data")]
        public float TurnP
        {
            get { return _turnP; }
            set
            {
                if (value.Equals(_turnP)) return;
                _turnP = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Stops the rocket moving from its target, causing it to fall.
        /// </summary>
        public void Abort()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Makes the rocket fly towards its target.
        /// </summary>
        public void Fire()
        {
            throw new NotImplementedException();
        }

        protected override void OnStep(Part part)
        {
        }
    }
}