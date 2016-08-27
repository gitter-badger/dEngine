// UDim2EditorViewModel.cs - dEditor
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System.Linq;
using System.Reflection;
using dEngine.Serializer.V1;

namespace dEditor.Widgets.Properties.Inspectors.UDim2
{
	public class UDim2EditorViewModel : EditorBase<dEngine.UDim2>, ILabelled
	{
		public UDim2EditorViewModel(object obj, Inst.CachedProperty desc) : base(obj, desc)
		{
		}

		public string StringValue
		{
			get { return Value.ToString(); }
			set
			{
				var floats = value.Split(',').Select(float.Parse).ToArray();
				Value = new dEngine.UDim2(floats[0], (int)floats[1], floats[2], (int)floats[3]);
				NotifyOfPropertyChange(() => StringValue);
			}
		}

		public override void NotifyOfPropertyChange(string propertyName)
		{
			if (propertyName == "Value")
			{
				NotifyOfPropertyChange(() => StringValue);
			}
			base.NotifyOfPropertyChange(propertyName);
		}
	}
}