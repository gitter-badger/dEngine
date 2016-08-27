// CollapsibleGroupViewModel.cs - dEditor
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;

namespace dEditor.Widgets.Properties.Inspectors
{
	public class CollapsibleGroupViewModel : InspectorBase
	{
		private static readonly Dictionary<string, bool> PersistedExpandCollapseStates = new Dictionary<string, bool>();

		private readonly string _name;
		private bool _isExpanded;

		public CollapsibleGroupViewModel(string name, IEnumerable<InspectorBase> children)
		{
			_name = name;
			Children = children;

			if (!PersistedExpandCollapseStates.TryGetValue(_name, out _isExpanded))
				_isExpanded = true;
		}

		public override string Name => _name;

		public override string Description => _name;

		public override bool IsReadOnly => false;

		public IEnumerable<InspectorBase> Children { get; }

		public bool IsExpanded
		{
			get { return _isExpanded; }
			set
			{
				_isExpanded = value;
				PersistedExpandCollapseStates[_name] = value;
				// TODO: Key should be full path to this group, not just the name.
				NotifyOfPropertyChange();
			}
		}
	}
}