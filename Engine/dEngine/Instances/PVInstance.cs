// PVInstance.cs - dEngine
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
using System.Collections.Generic;
using CSCore.XAudio2.X3DAudio;
using dEngine.Instances.Attributes;
using dEngine.Instances.Interfaces;

using SharpDX;

namespace dEngine.Instances
{
	/// <summary>
	/// A physical instance is an entity which occupies space in the 3D world.
	/// </summary>
	[TypeId(21)]
	public abstract class PVInstance : Instance, IListenable
    {
		/// <summary>
		/// The transform of the object.
		/// </summary>
		public abstract CFrame CFrame { get; set; }

		/// <summary>
		/// The size of the object.
		/// </summary>
		public abstract Vector3 Size { get; set; }

		internal bool MuteCFrameChanges { get; set; }
		internal bool MuteSizeChanges { get; set; }



	    void IListenable.UpdateListener(ref Listener listener)
	    {
	        ((IListenable)CFrame).UpdateListener(ref listener);
	    }
    }
}