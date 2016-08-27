// Joint.cs - dEngine
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
	/// Base class for joints.
	/// </summary>
	[TypeId(93), ToolboxGroup("Joints (Legacy)"), Obsolete("Constraints should be used instead of joints.")]
	public abstract class JointInstance : Instance
	{
		private Part _part0;
		private Part _part1;

		/// <summary>
		/// The primary part.
		/// </summary>
		[InstMember(1), EditorVisible("Data")]
		public Part Part0
		{
			get { return _part0; }
			set
			{
				if (_part0 == value) return;
				_part0 = value;
				UpdateJoint();
				NotifyChanged();
			}
		}

		/// <summary>
		/// The secondary part.
		/// </summary>
		[InstMember(2), EditorVisible("Data")]
		public Part Part1
		{
			get { return _part1; }
			set
			{
				if (_part1 == value) return;
				_part1 = value;
				UpdateJoint();
				NotifyChanged();
			}
		}

		/// <inheritdoc />
		protected abstract void UpdateJoint();
	}
}