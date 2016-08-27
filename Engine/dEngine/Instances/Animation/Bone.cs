// Bone.cs - dEngine
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
	/// A bone.
	/// </summary>
	[TypeId(34)]
	public class Bone : Instance
	{
		private Bone _parent;
		private float _physicsBlendWeight;
		private bool _simulatePhysics;

		/// <inheritdoc />
		public Bone()
		{
		}
        
		/// <summary>
		/// The transform of the bone relative to <see cref="Skeleton.RootBone" />.
		/// </summary>
		[InstMember(1)]
		public CFrame CFrame { get; set; }
        
		public void SimulatePhysics(bool value)
		{
			_simulatePhysics = value;
		}

		public void SetPhysicsBlendWeight(float weight)
		{
			_physicsBlendWeight = weight;
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return Name;
		}
	}
}