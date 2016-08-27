// GeneralDebugGui.cs - dEngine
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

#pragma warning disable 1591

namespace dEngine.Instances.Diagnostics
{
	[TypeId(197)]
	public sealed class GeneralDebugGui : DebugGui
	{
		private readonly TextLabel _gameTaskLabel;
		private readonly TextLabel _graphicsTaskLabel;
		private readonly Stack _stack;

		internal GeneralDebugGui(CoreGui coreGui) : base(coreGui)
		{
			_stack = new Stack {Parent = this, Size = new UDim2(0.3f, 0, 0.3f, 0), BackgroundColour = Colour.Transparent};

			_graphicsTaskLabel = MakeDebugTextLabel(_stack);
			_gameTaskLabel = MakeDebugTextLabel(_stack);
		}

	    protected override void OnUpdate()
	    {
	        
	    }

	    public override void Destroy()
		{
			base.Destroy();
			_stack.Destroy();
		}
	}
}