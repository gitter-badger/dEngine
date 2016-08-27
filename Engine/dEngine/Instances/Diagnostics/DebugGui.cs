// DebugGui.cs - dEngine
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
using dEngine.Services;

#pragma warning disable 1591

namespace dEngine.Instances.Diagnostics
{
	[Uncreatable,  TypeId(196)]
	public abstract class DebugGui : ScreenGui
	{
		protected DebugGui(CoreGui coreGui)
		{
			Parent = coreGui;
			RunService.Service.Heartbeat.Event += OnHeartbeat;
		}

		private void OnHeartbeat(double d)
		{
			OnUpdate();
		}

		protected TextLabel MakeDebugTextLabel(Instance parent)
		{
			return new TextLabel
			{
				FontSize = 14,
				Size = new UDim2(1, 0, 0, 16),
				Parent = parent,
				BackgroundColour = Colour.Transparent,
				TextColour = Colour.White,
				TextStrokeColour = Colour.Black,
				TextAlignmentX = AlignmentX.Left
			};
		}

		protected abstract void OnUpdate();

		public override void Destroy()
		{
			base.Destroy();
			RunService.Service.Heartbeat.Event -= OnHeartbeat;
		}
	}
}