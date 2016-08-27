// RodConstraint.cs - dEngine
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
	/// Summary
	/// </summary>
	[TypeId(108)]
	public sealed class RodConstraint : Constraint
	{
		private float _length;

		/// <summary/>
		public RodConstraint()
		{
		}

		/// <summary>
		/// The current distance between the two attachments.
		/// </summary>
		[EditorVisible("Derived")]
		public float CurrentDistance => 0;

		/// <summary>
		/// Summary
		/// </summary>
		[InstMember(1), EditorVisible("Rod")]
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