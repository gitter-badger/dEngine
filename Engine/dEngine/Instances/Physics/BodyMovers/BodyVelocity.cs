// BodyVelocity.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using dEngine.Instances.Attributes;
using BulletVector3 = BulletSharp.Math.Vector3;


namespace dEngine.Instances
{
    /// <summary>
    /// Applies a force to maintain a certain velocity.
    /// </summary>
    [TypeId(131)]
    public sealed class BodyVelocity : BodyMover
    {
        private readonly float _gainIntegral = 1;
        private float _p;
        private readonly float _d = 1;
        private Vector3 _integralTerm;
        private Vector3 _maxForce;
        private Vector3 _velocity;
        private BulletVector3 _lastProcessVar;

        /// <summary/>
        public BodyVelocity()
        {
            _maxForce = new Vector3(4000);
            _velocity = new Vector3(0, 2, 0);
        }

        /// <summary>
        /// The amount of force applied on each axis.
        /// </summary>
        [InstMember(1), EditorVisible("Data")]
        public Vector3 MaxForce
        {
            get { return _maxForce; }
            set
            {
                _maxForce = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Determines how aggressively the object tries to reach its goal.
        /// </summary>
        [InstMember(2), EditorVisible("Data")]
        public float P
        {
            get { return _p; }
            set
            {
                if (value.Equals(_p)) return;
                _p = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// The desired velocity.
        /// </summary>
        [InstMember(3), EditorVisible("Data")]
        public Vector3 Velocity
        {
            get { return _velocity; }
            set
            {
                _velocity = value;
                NotifyChanged();
            }
        }

        /// <summary />
        protected override void OnStep(Part part)
        {
            var setPoint = _velocity;
            var processVar = (Vector3)part.RigidBody.LinearVelocity;
            var error = setPoint - processVar;

            var p0 = _velocity;

            var output = _p * error + p0;

            if (output > _maxForce)
                output = _maxForce;
            else if (output < -_maxForce)
                output = -_maxForce;

            part.RigidBody.LinearVelocity = (BulletVector3)output;
        }
    }
}