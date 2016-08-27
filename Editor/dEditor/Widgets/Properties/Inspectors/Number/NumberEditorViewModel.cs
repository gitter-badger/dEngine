// NumberEditorViewModel.cs - dEditor
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
using System.Windows;
using System.Windows.Controls;
using dEditor.Framework.Converters;
using dEngine.Instances.Attributes;
using dEngine.Serializer.V1;

namespace dEditor.Widgets.Properties.Inspectors.Number
{
	public class NumberEditorViewModel<T> : EditorBase<T>, ILabelled where T : struct
	{
	    public NumberEditorViewModel(object obj, Inst.CachedProperty propDesc) : base(obj, propDesc)
		{
			var rangeAttr = propDesc.Range;

			if (rangeAttr != null)
			{
				Lower = rangeAttr.Min;
				Upper = rangeAttr.Max;
				TextBoxWidth = 50;
				IsRanged = true;
			}
		}

		public Dock Dock => IsRanged ? Dock.Left : Dock.Top;
		public double Lower { get; set; }
		public double Upper { get; set; }
		public bool IsRanged { get; }
		public double TextBoxWidth { get; } = double.NaN;

		public double Increment => (typeof(T) == typeof(float) || typeof(T) == typeof(double)) ? 0.05f : 1;

		public double DoubleValue
		{
			get { return Convert.ToDouble(Value); }
			set { Value = (T)Convert.ChangeType(value, typeof(T)); }
		}

	    public override void NotifyOfPropertyChange(string propertyName = null)
		{
			if (propertyName == nameof(Value))
			{
				NotifyOfPropertyChange(() => DoubleValue);
			}

			base.NotifyOfPropertyChange(propertyName);
		}
	}
}