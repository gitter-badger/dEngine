// RopeConstraint.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using BulletSharp;
using BulletSharp.Math;
using BulletSharp.SoftBody;
using dEngine.Instances.Attributes;


namespace dEngine.Instances
{
	/// <summary>
	/// Summary
	/// </summary>
	[TypeId(110)]
	public sealed class RopeConstraint : Constraint
	{
		private float _length;
		private float _restitution;
	    private Generic6DofSpring2Constraint _spring;

	    /// <summary/>
		public RopeConstraint()
		{
		}

		/// <summary>
		/// The current distance between the two attachments.
		/// </summary>
		[EditorVisible("Derived")]
		public float CurrentDistance => 0;

		/// <summary>
		/// The length of the rope.
		/// </summary>
		[InstMember(1), EditorVisible("Rope")]
		public float Length
		{
			get { return _length; }
			set
			{
				if (value.Equals(_length)) return;
				_length = value;
				NotifyChanged();
			}
		}

		/// <summary>
		/// Summary
		/// </summary>
		[InstMember(2), EditorVisible("Rope")]
		public float Restitution
		{
			get { return _restitution; }
			set
			{
				if (value.Equals(_restitution)) return;
				_restitution = value;
				NotifyChanged();
			}
		}

	    protected override void RebuildConstraint()
	    {
            DestroyConstraint();
	        if (Validate())
	        {
	            _constraint = _spring = new Generic6DofSpring2Constraint(_attachment0, _attachment1, (Matrix)_attachment0.CFrame, (Matrix)_attachment1.CFrame) {Userobject = this};
                _spring.SetStiffness(0, _restitution, true);
                _spring.SetStiffness(1, _restitution, true);
	            World.Physics.AddConstraint(this);
	        }
	    }
	}
}