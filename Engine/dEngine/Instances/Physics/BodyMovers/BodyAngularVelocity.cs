// BodyAngularVelocity.cs - dEngine
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
    /// Applies angular force on an object.
    /// </summary>
    [TypeId(117)]
    public sealed class BodyAngularVelocity : BodyMover
    {
        private Vector3 _angularVelocity;
        private Vector3 _maxTorque;

        private float _power;

        /// <summary>
        /// Summary
        /// </summary>
        [InstMember(1), EditorVisible("Data")]
        public Vector3 AngularVelocity
        {
            get { return _angularVelocity; }
            set
            {
                if (value.Equals(_angularVelocity)) return;
                _angularVelocity = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Summary
        /// </summary>
        [InstMember(2), EditorVisible("Data")]
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
        /// How aggressively the object tries to reach its goal.
        /// </summary>
        [InstMember(3), EditorVisible("Data")]
        public float Power
        {
            get { return _power; }
            set
            {
                if (value == _power) return;
                _power = value;
                NotifyChanged();
            }
        }


        /// <inheritdoc />
        protected override void OnStep(Part part)
        {
            var radius = part.Size.magnitude / 2;
        }
    }
}