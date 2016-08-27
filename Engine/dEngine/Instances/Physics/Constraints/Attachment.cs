// Attachment.cs - dEngine
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
using dEngine.Utility;


namespace dEngine.Instances
{
	/// <summary>
	/// An attachment represents an offset relative to the object it is parented to.
	/// </summary>
	[TypeId(105)]
	public class Attachment : Instance
	{
		private Vector3 _axis;
		private CFrame _cframe;
		private Part _part;
		private Vector3 _secondaryAxis;
		private Vector3 _worldAxis;
		private Vector3 _worldPosition;
		private Vector3 _worldRotation;
		private Vector3 _worldSecondaryAxis;

        /// <summary/>
	    public Attachment()
	    {
	        ParentChanged.Event += ParentChangedOnEvent;
	    }

	    private void ParentChangedOnEvent(Instance parent)
	    {
            Part = parent as Part;
	    }

	    /// <summary>
		/// The offset CFrame of the attachment.
		/// </summary>
		[InstMember(1)]
		public CFrame CFrame
		{
			get { return _cframe; }
			set
			{
				if (value == _cframe) return;
				_cframe = value;
				UpdateDerivedData();
				NotifyChanged();
			}
		}

		/// <summary>
		/// The offset position of the attachment.
		/// </summary>
		[EditorVisible]
		public Vector3 Position
		{
			get { return _cframe.p; }
			set
			{
				if (value == _cframe.p)
					return;
				var rotation = _cframe - _cframe.p;
				_cframe = new CFrame(value) * rotation;
				WorldPosition = _part.CFrame.pointToWorldSpace(value);
				NotifyChanged();
				NotifyChanged(nameof(CFrame));
			}
		}

		/// <summary>
		/// The offset rotation of the attachment.
		/// </summary>
		[EditorVisible]
		public Vector3 Rotation
		{
			get { return _cframe.getEulerAngles(); }
			set
			{
				var v3 = value * Mathf.Deg2Rad;
				var rotation = CFrame.Angles(v3.x, v3.y, v3.z);
				_cframe = new CFrame(_cframe.p) * rotation;
				NotifyChanged();
				NotifyChanged(nameof(CFrame));
				UpdateDerivedData();
			}
		}

		/// <summary>
		/// The X axis of the offset.
		/// </summary>
		[EditorVisible("Derived")]
		public Vector3 Axis
		{
			get { return _axis; }
			private set
			{
				_axis = value;
				NotifyChanged();
			}
		}

		/// <summary>
		/// The Y axis of the direction.
		/// </summary>
		[EditorVisible("Derived")]
		public Vector3 SecondaryAxis
		{
			get { return _secondaryAxis; }
			private set
			{
				_secondaryAxis = value;
				NotifyChanged();
			}
		}

		/// <summary>
		/// The world-space offset position.
		/// </summary>
		[EditorVisible("Derived")]
		public Vector3 WorldPosition
		{
			get { return _worldPosition; }
			set
			{
				_worldPosition = value;
				NotifyChanged();
			}
		}

		/// <summary>
		/// The world-space offset rotation.
		/// </summary>
		[EditorVisible("Derived")]
		public Vector3 WorldRotation
		{
			get { return _worldRotation; }
			set
			{
				_worldRotation = value;
				NotifyChanged();
			}
		}

		/// <summary>
		/// The X axis of the world-space offset.
		/// </summary>
		[EditorVisible("Derived")]
		public Vector3 WorldAxis
		{
			get { return _worldAxis; }
			set
			{
				_worldAxis = value;
				NotifyChanged();
			}
		}

		/// <summary>
		/// The Y axis of the world-space offset.
		/// </summary>
		[EditorVisible("Derived")]
		public Vector3 WorldSecondaryAxis
		{
			get { return _worldSecondaryAxis; }
			set
			{
				_worldSecondaryAxis = value;
				NotifyChanged();
			}
		}

	    internal Part Part { get; private set; }

        /// <summary/>
        public static implicit operator RigidBody(Attachment attachment)
        {
            return attachment.Part.RigidBody;
        }

        private void UpdateDerivedData()
		{
			var part = _part;
			if (part == null)
			{
				Logger.Warn("Cannot update derived data: parent is not a part.");
				return;
			}

			var worldCF = part.CFrame.toWorldSpace(_cframe);

			Axis = _cframe.right;
			SecondaryAxis = _cframe.up;

			WorldRotation = worldCF.getEulerAngles();
			WorldAxis = worldCF.right;
			WorldSecondaryAxis = worldCF.up;
		}

		/// <inheritdoc />
		protected override bool OnParentFilter(Instance parent)
		{
			_part = parent as Part;
			if (_part == null && parent != null)
				throw new ParentException("Attachments must be parented to a Part.");
			return true;
		}

	    public BulletSharp.Math.Vector3 GetPivotPoint()
	    {
	        throw new NotImplementedException();
	    }
	}
}