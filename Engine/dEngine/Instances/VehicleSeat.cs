// VehicleSeat.cs - dEngine
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
    /// A seat which allows <see cref="Character" />s to drive a vehicle.
    /// </summary>
    [TypeId(17)]
    [ExplorerOrder(3)]
    public sealed class VehicleSeat : Seat
    {
        private Vector3 _cameraOffset;

        private float _maxSpeed;

        private float _steer;

        private int _throttle;

        private float _torque;

        private float _turnSpeed;

        /// <summary>
        /// The offset from the seat of the <see cref="Seat.Occupant" />'s camera.
        /// </summary>
        [InstMember(1)]
        [EditorVisible]
        public Vector3 CameraOffset
        {
            get { return _cameraOffset; }
            set
            {
                if (value == _cameraOffset) return;
                _cameraOffset = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Determines if the seat can be sat in.
        /// </summary>
        [InstMember(2)]
        [EditorVisible("Control")]
        public bool Disabled
        {
            get { return _disabled; }
            set
            {
                if (value == _disabled)
                    return;
                _disabled = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// The maximum speed the vehicle can go at.
        /// </summary>
        [EditorVisible("Control")]
        public float MaxSpeed
        {
            get { return _maxSpeed; }
            set
            {
                if (value == _maxSpeed)
                    return;
                _maxSpeed = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Determines how far to the left or right to steer.
        /// </summary>
        [EditorVisible("Control")]
        public float Steer
        {
            get { return _steer; }
            set
            {
                if (value == _steer)
                    return;
                _steer = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Gets the speed that the vehicle is going at.
        /// </summary>
        public float Speed => Velocity.magnitude;

        /// <summary>
        /// The direction of movement.
        /// </summary>
        [EditorVisible("Control")]
        public int Throttle
        {
            get { return _throttle; }
            set
            {
                if (value == _throttle)
                    return;
                _throttle = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Determines how fast the vehicle is able to reach its <see cref="MaxSpeed"/>.
        /// </summary>
        [EditorVisible("Control")]
        public float Torque
        {
            get { return _torque; }
            set
            {
                if (value == _torque)
                    return;
                _torque = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// The speed at which the vehicle will turn.
        /// </summary>
        [EditorVisible("Control")]
        public float TurnSpeed
        {
            get { return _turnSpeed; }
            set
            {
                if (value == _turnSpeed)
                    return;
                _turnSpeed = value;
                NotifyChanged();
            }
        }
    }
}