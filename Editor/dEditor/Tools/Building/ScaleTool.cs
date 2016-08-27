// ScaleTool.cs - dEditor
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Concurrent;
using dEngine;
using dEngine.Instances;

namespace dEditor.Tools.Building
{
	public class ScaleTool : StudioBuildTool
	{
	    private ConcurrentDictionary<Part, AxisHandles> _axisHandles;

		public ScaleTool()
		{
		}

		public override double[] IncrementOptions { get; } = {1, 10, 0.5, 0.25, 0.125, 0.0625, 0.03125};

		protected override void OnEquipped()
		{
		}

		protected override void OnUnequipped()
		{
		}

		protected override void OnItemSelected(Instance instance)
		{
			base.OnItemSelected(instance);

		    var part = instance as Part;
		    if (part != null)
		    {
		        var handle = new AxisHandles()
		        {
                    Name = "ResizeHandle",
                    Adornee = part,
                    Parent = Game.CoreGui,
                    Style = HandlesStyle.Resize,
		        };
		        _axisHandles[part] = handle;
		    }

			UpdateHandles();
		}

		private void UpdateHandles()
		{
			//throw new System.NotImplementedException();
		}

		protected override void OnItemDeselected(Instance instance)
		{
			base.OnItemDeselected(instance);

            var part = instance as Part;
            if (part != null)
            {
                AxisHandles handle;
                _axisHandles.TryRemove(part, out handle);
                handle.Destroy();
            }

        }
	}
}