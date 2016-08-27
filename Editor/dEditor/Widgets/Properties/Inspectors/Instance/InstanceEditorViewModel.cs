// InstanceEditorViewModel.cs - dEditor
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Reflection;
using dEditor.Framework.Services;
using dEngine.Serializer.V1;

namespace dEditor.Widgets.Properties.Inspectors.Instance
{
	public class InstanceEditorViewModel : EditorBase<dEngine.Instances.Instance>, ILabelled
	{
		public InstanceEditorViewModel(object obj, Inst.CachedProperty propDesc) : base(obj, propDesc)
		{
		}

		public Uri Icon => IconProvider.GetIconUri(Value?.GetType());

		public override void NotifyOfPropertyChange(string propertyName = null)
		{
			if (propertyName == nameof(Value))
				NotifyOfPropertyChange(nameof(Icon));

			base.NotifyOfPropertyChange(propertyName);
		}

		public void OnLeftMouseButtonDown()
		{
			if (AreMultipleValuesSame)
			{
				dEngine.Game.Selection.Select(Value, true);
			}
		}
	}
}