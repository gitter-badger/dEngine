// BodyMover.cs - dEngine
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
	/// Base class for physics force-related objects.
	/// </summary>
	[TypeId(115), ExplorerOrder(14), ToolboxGroup("Body movers")]
	public abstract class BodyMover : Instance
	{
		/// <summary>
		/// The part to apply force to.
		/// </summary>
		protected Part _parentPart;

	    protected BodyMover()
	    {
	    }

        /// <summary>
        /// Called when the world this mover belongs to is stepped.
        /// </summary>
        /// <param name="part">The parent part at the time of the step.</param>
	    protected abstract void OnStep(Part part);

		/// <inheritdoc />
		protected override void OnAncestryChanged(Instance child, Instance parent)
		{
			base.OnAncestryChanged(child, parent);
		    var part = parent as Part;
		    _parentPart = part;
			OnParentPartChanged(part);
		}

        /// <inheritdoc />
        protected override void OnWorldChanged(IWorld newWorld, IWorld oldWorld)
	    {
	        base.OnWorldChanged(newWorld, oldWorld);

	        if (oldWorld != null)
	            oldWorld.Physics.Stepped -= PhysicsOnStepped;

            if (newWorld != null)
                newWorld.Physics.Stepped += PhysicsOnStepped;
        }

	    private void PhysicsOnStepped()
	    {
	        var part = _parentPart;
	        if (part != null)
	        {
	            OnStep(part);
	        }
	    }

	    /// <summary>
		/// Fired when <see cref="_parentPart" /> changes.
		/// </summary>
		/// <param name="parentPart"></param>
		protected virtual void OnParentPartChanged(Part parentPart)
		{
		}
	}
}