// Weld.cs - dEngine
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
using dEngine.Services;


namespace dEngine.Instances
{
	/// <summary>
	/// A weld joint.
	/// </summary>
	/// <remarks>
	/// Involded part is destroyed, or its CFrame is set, the weld will be destroyed.
	/// </remarks>
	[TypeId(94), ToolboxGroup("Joints"), ExplorerOrder(20), Obsolete("Constraints should be used instead of joints.")]
	public class Weld : JointInstance
	{
		private CFrame _c0;
		private CFrame _c1;

		/// <inheritdoc />
		public Weld()
		{
			_c0 = CFrame.Identity;
			_c1 = CFrame.Identity;

			RunService.Service.SimulationStarted.Event += OnSessionStarted;
		}

		/// <summary>
		/// Determines how the `offset point` should be attached to <see cref="JointInstance.Part0" />
		/// </summary>
		[InstMember(1), EditorVisible("Data")]
		public CFrame C0
		{
			get { return _c0; }
			set
			{
				_c0 = value;
				UpdateJoint();
				NotifyChanged(nameof(C0));
			}
		}

		/// <summary>
		/// Determines how <see cref="JointInstance.Part1" /> should be attached to the `offset point`.
		/// </summary>
		[InstMember(2), EditorVisible("Data")]
		public CFrame C1
		{
			get { return _c1; }
			set
			{
				_c1 = value;
				UpdateJoint();
				NotifyChanged(nameof(C1));
			}
		}

		private void OnSessionStarted()
		{
			UpdateJoint();
		}

		/// <inheritdoc />
		public override void Destroy()
		{
			base.Destroy();

			RunService.Service.SimulationStarted.Event -= OnSessionStarted;
		}

		/// <inheritdoc />
		protected override void UpdateJoint()
		{
			if (World == null)
				return;

			//throw new NotImplementedException();
		}
	}
}