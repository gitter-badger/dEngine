// BodyThrust.cs - dEngine
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
    /// Applies a force on its parent towards a direction.
    /// </summary>
    [TypeId(119)]
    public sealed class BodyThrust : BodyMover
    {
        private BulletSharp.Math.Vector3 _force;
        private BulletSharp.Math.Vector3 _location;

        /// <summary>
        /// The amount of force applied on each axis.
        /// </summary>
        [InstMember(1), EditorVisible("Data")]
        public Vector3 Force
        {
            get { return (Vector3)_force; }
            set
            {
                _force = (BulletSharp.Math.Vector3)value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// The offset from the center of the part where force is applied.
        /// </summary>
        [InstMember(2), EditorVisible("Data")]
        public Vector3 Location
        {
            get { return (Vector3)_location; }
            set
            {
                _location = (BulletSharp.Math.Vector3)value;
                NotifyChanged();
            }
        }

        protected override void OnStep(Part part)
        {
            part.RigidBody.ApplyForceRef(ref _force, ref _location);
        }
    }
}