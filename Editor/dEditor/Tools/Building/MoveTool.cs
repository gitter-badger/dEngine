// MoveTool.cs - dEditor
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using dEngine;
using dEngine.Instances;
using dEngine.Services;
using dEngine.Utility.Extensions;
using SharpDX;
using Xceed.Wpf.Toolkit;
using Vector3 = SharpDX.Vector3;

namespace dEditor.Tools.Building
{
	public class MoveTool : StudioBuildTool
	{
	    private readonly Part _marker; // a part which represents the bounding box of the entire selection
	    private AxisHandles _axisHandles;

	    public MoveTool()
	    {
	        _marker = new Part {Name = "MoveToolMarker", Parent = Game.CoreEnvironment};

            _axisHandles = new AxisHandles
			{
				Name = "MoveTool_AxisHandles",
				Parent = Game.CoreGui,
				Style = HandlesStyle.Movement,
				Adornee = _marker,
				Visible = false
			};
			_axisHandles.MouseButton1Down.Connect(OnMouseButton1Down);
			_axisHandles.MouseButton1Up.Connect(OnMouseButton1Up);
			_axisHandles.MouseDrag.Connect(OnMouseDrag);

            IncrementOptions = new double[] {1, 2, 3, 4};
		}

	    private void OnMouseDrag(NormalId arg1, float arg2)
	    {
	    }

	    private void OnMouseButton1Up(NormalId obj)
	    {
	    }

	    private void OnMouseButton1Down(NormalId obj)
	    {
	    }

	    protected override void OnEquipped()
	    {
	    }

	    protected override void OnUnequipped()
	    {
	    }

	    protected override void UpdateHandles()
	    {
	        base.UpdateHandles();
	        var boundingBox = new OrientedBoundingBox(Vector3.Zero, Vector3.Zero);//PVInstance.ComputeBoundingBoxFromCollection(SelectionService.Selection);
	        _marker.Size = boundingBox.GetdEngineSize();
	        _marker.CFrame = boundingBox.GetCFrame();

	    }

	    public override double[] IncrementOptions { get; }
	}
}