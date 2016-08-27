// SpringConstraint.cs - dEngine
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
	/// A spring applies a force to its <see cref="Attachment" />s based on spring and damper behaviour.
	/// </summary>
	[TypeId(111)]
	public sealed class SpringConstraint : Constraint
	{
		private float _damping;
		private float _freeLength;
		private bool _limitsEnabled;
		private float _maxForce;
		private float _maxLength;
		private float _minLength;
		private float _stiffness;

		/// <summary>
		/// The current distance between the two attachments.
		/// </summary>
		[EditorVisible("Derived")]
		public float CurrentDistance => 0;

		/// <summary>
		/// Summary
		/// </summary>
		[InstMember(1), EditorVisible("Spring")]
		public float Damping
		{
			get { return _damping; }
			set
			{
				if (value.Equals(_damping)) return;
				_damping = value;
				NotifyChanged();
			}
		}

		/// <summary>
		/// Summary
		/// </summary>
		[InstMember(2), EditorVisible("Spring")]
		public float FreeLength
		{
			get { return _freeLength; }
			set
			{
				if (value.Equals(_freeLength)) return;
				_freeLength = value;
				NotifyChanged();
			}
		}

		/// <summary>
		/// Summary
		/// </summary>
		[InstMember(3), EditorVisible("Spring")]
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

		/// <summary>
		/// Summary
		/// </summary>
		[InstMember(4), EditorVisible("Spring")]
		public float MaxForce
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
		/// Summary
		/// </summary>
		[InstMember(5), EditorVisible("Spring")]
		public float MaxLength
		{
			get { return _maxLength; }
			set
			{
				if (value.Equals(_maxLength)) return;
				_maxLength = value;
				NotifyChanged();
			}
		}

		/// <summary>
		/// Summary
		/// </summary>
		[InstMember(6), EditorVisible("Spring")]
		public float MinLength
		{
			get { return _minLength; }
			set
			{
				if (value.Equals(_minLength)) return;
				_minLength = value;
				NotifyChanged();
			}
		}

		/// <summary>
		/// The strength of the string.
		/// </summary>
		/// <remarks>
		/// The higher this value the more force will be applied when the attachments are separated a different length than the
		/// FreeLength.
		/// </remarks>
		[InstMember(7), EditorVisible("Spring")]
		public float Stiffness
		{
			get { return _stiffness; }
			set
			{
				if (value.Equals(_stiffness)) return;
				_stiffness = value;
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