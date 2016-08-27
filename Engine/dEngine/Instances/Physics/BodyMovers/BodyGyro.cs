// BodyGyro.cs - dEngine
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


namespace dEngine.Instances
{
    /// <summary>
    /// BodyGyro attempts to keep a fixed orientation of its parent relative to <see cref="CFrame" />.
    /// </summary>
    [TypeId(116)]
    public sealed class BodyGyro : BodyMover
    {
        private CFrame _cframe;
        private Vector3 _maxTorque;
        private float _dampening;
        private float _pressure;

        /// <summary>
        /// The target orientation of the object.
        /// </summary>
        /// <remarks>
        /// Only the rotation of the CFrame is used.
        /// </remarks>
        [InstMember(1), EditorVisible("Data")]
        public CFrame CFrame
        {
            get { return _cframe; }
            set
            {
                if (value.Equals(_cframe)) return;
                _cframe = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// The amount of dampening to apply.
        /// </summary>
        [InstMember(2), EditorVisible("Data")]
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
        [InstMember(3), EditorVisible("Data")]
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

        protected override void OnStep(Part part)
        {
        }
    }
}