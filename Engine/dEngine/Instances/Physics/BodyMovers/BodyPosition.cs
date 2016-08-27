// BodyPosition.cs - dEngine
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
    /// Applies a force on its parent to move towards a position.
    /// </summary>
    [TypeId(118)]
    public sealed class BodyPosition : BodyMover
    {
        private float _pressure;
        private float _dampening;
        private Vector3 _maxForce;
        private Vector3 _position;

        /// <summary/>
        [Obsolete]
        public Vector3 position { get { return Position; } set { Position = value; } }

        /// <summary>
        /// The target position of the object.
        /// </summary>
        [InstMember(1), EditorVisible("Data")]
        public Vector3 Position
        {
            get { return _position; }
            set
            {
                if (value.Equals(_position)) return;
                _position = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Summary
        /// </summary>
        [InstMember(2), EditorVisible("Data")]
        public Vector3 MaxForce
        {
            get { return _maxForce; }
            set
            {
                if (value.Equals(_maxForce)) return;
                _maxForce = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// The amount of dampening to apply.
        /// </summary>
        [InstMember(3), EditorVisible("Data")]
        public float Damping
        {
            get { return _dampening; }
            set
            {
                if (value.Equals(_dampening)) return;
                _dampening = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Determines how aggressively the object tries to reach its goal.
        /// </summary>
        [InstMember(4), EditorVisible("Data")]
        public float Power
        {
            get { return _pressure; }
            set
            {
                if (value.Equals(_pressure)) return;
                _pressure = value;
                NotifyChanged();
            }
        }

        protected override void OnStep(Part part)
        {
        }
    }
}