// BodyForce.cs - dEngine
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
	/// Applies a constant force to the parent <see cref="Part" />.
	/// </summary>
	[TypeId(40), ToolboxGroup("Body Movers"), ExplorerOrder(14)]
	public sealed class BodyForce : BodyMover
	{
		private Vector3 _force;
        private BulletVector3 _bulletForce;

        /// <summary>
        /// The force to be applied.
        /// </summary>
        [InstMember(1), EditorVisible("Data")]
		public Vector3 Force
		{
			get { return _force; }
			set
			{
				if (value == _force)
					return;

				_force = value;
                _bulletForce = (BulletVector3)value;

                NotifyChanged(nameof(Force));
			}
		}

		/// <inheritdoc />
		public override void Destroy()
		{
			base.Destroy();
		}
        
	    protected override void OnStep(Part part)
        {
            part.RigidBody.ApplyCentralForceRef(ref _bulletForce);
        }
	}
}