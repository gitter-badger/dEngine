// HingeConstraint.cs - dEngine
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
	/// A hinge allows two <see cref="Attachment" />s to rotate about one axis.
	/// </summary>
	[TypeId(107)]
	public sealed class HingeConstraint : Constraint
	{
		private float _angle;
		private ActuatorType _actuatorType;
		private bool _limitsEnabled;

		/// <summary/>
		public HingeConstraint()
		{
			MotorMaxTorque = float.PositiveInfinity;
		}

		/// <summary>
		/// The current angle of the hinge.
		/// </summary>
		[EditorVisible("Derived")]
		public float CurrentAngle => _angle;

		/// <summary>
		/// Determines whether the rotation is actuated and, if so, what kind of actutation.
		/// </summary>
		[InstMember(1), EditorVisible("Behaviour")]
		public ActuatorType ActuatorType
		{
			get { return _actuatorType; }
			set
			{
				if (value.Equals(_actuatorType)) return;
				_actuatorType = value;
				NotifyChanged();
			}
		}

		#region Servo

		private float _angularSpeed;
		private float _servoMaxTorque;
		private float _targetAngle;

		/// <summary>
		/// 
		/// </summary>
		[InstMember(2), EditorVisible("Servo")]
		public float AngularSpeed
		{
			get { return _angularSpeed; }
			set
			{
				if (value.Equals(_angularSpeed)) return;
				_angularSpeed = value;
				NotifyChanged();
			}
		}

		/// <summary>
		/// Summary
		/// </summary>
		[InstMember(3), EditorVisible("Servo")]
		public float ServoMaxTorque
		{
			get { return _servoMaxTorque; }
			set
			{
				if (value.Equals(_servoMaxTorque)) return;
				_servoMaxTorque = value;
				NotifyChanged();
			}
		}

		/// <summary>
		/// Summary
		/// </summary>
		[InstMember(4), EditorVisible("Servo")]
		public float TargetAngle
		{
			get { return _targetAngle; }
			set
			{
				if (value.Equals(_targetAngle)) return;
				_targetAngle = value;
				NotifyChanged();
			}
		}
		
		#endregion

		#region Motor

		private float _angularVelocity;
		private float _motorMaxAcceleration;
		private float _motorMaxTorque;

		/// <summary>
		/// Summary
		/// </summary>
		[InstMember(5), EditorVisible("Motor")]
		public float AngularVelocity
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
		[InstMember(6), EditorVisible("Motor")]
		public float MotorMaxAcceleration
		{
			get { return _motorMaxAcceleration; }
			set
			{
				if (value.Equals(_motorMaxAcceleration)) return;
				_motorMaxAcceleration = value;
				NotifyChanged();
			}
		}

		/// <summary>
		/// Summary
		/// </summary>
		[InstMember(7), EditorVisible("Motor")]
		public float MotorMaxTorque
		{
			get { return _motorMaxTorque; }
			set
			{
				if (value.Equals(_motorMaxTorque)) return;
				_motorMaxTorque = value;
				NotifyChanged();
			}
		}

		#endregion

		/// <summary>
		/// Determines whether the range of rotation should be limited.
		/// </summary>
		public bool LimitsEnabled
		{
			get { return _limitsEnabled; }
			set
			{
				if (value.Equals(_limitsEnabled)) return;
				_limitsEnabled = value;
				NotifyChanged();
			}
		}

        /// <summary/>
	    protected override void RebuildConstraint()
	    {
            DestroyConstraint();
	        if (Validate())
	        {
	            
	        }
	    }
	}
}