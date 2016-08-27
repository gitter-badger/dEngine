// ObjectBrowserViewModel.cs - dEditor
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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using dEditor.Framework;
using dEngine;
using dEngine.Instances;
using dEngine.Services;
using dEngine.Settings;

namespace dEditor.Widgets.ObjectBrowser
{
	public class ObjectBrowserViewModel : Document
	{
		private readonly IEnumerable<Type> _types;
		private IEnumerable<ObjectTypeEntry> _filteredObjects;
		private string _filterString;
		private IEnumerable<MemberEntry> _members;
		private ObjectTypeEntry _selectedObject;
		private bool _showAbstractTypes;
		private bool _showEnumTypes;
		private bool _showNonInstanceObjects;
		private bool _showServices;

		public ObjectBrowserViewModel()
		{
			_types = Assembly.GetAssembly(typeof(Engine)).GetExportedTypes();

			ShowServices = true;
			ShowAbstractTypes = false;
			ShowEnumTypes = true;
			ShowNonInstanceObjects = true;

			UpdateTypes();
		}

		public override string DisplayName => "Object Browser";

		public string FilterString
		{
			get { return _filterString; }
			set
			{
				if (value == _filterString) return;
				_filterString = value;
				NotifyOfPropertyChange();
				UpdateTypes();
			}
		}

		public IEnumerable<ObjectTypeEntry> FilteredObjects
		{
			get { return _filteredObjects; }
			private set
			{
				if (Equals(value, _filteredObjects)) return;
				_filteredObjects = value;
				NotifyOfPropertyChange();
			}
		}

		public IEnumerable<MemberEntry> Members
		{
			get { return _members; }
			private set
			{
				if (Equals(value, _members)) return;
				_members = value;
				NotifyOfPropertyChange();
			}
		}

		public bool ShowServices
		{
			get { return _showServices; }
			set
			{
				if (value == _showServices) return;
				_showServices = value;
				NotifyOfPropertyChange();
				UpdateTypes();
			}
		}

		public bool ShowNonInstanceObjects
		{
			get { return _showNonInstanceObjects; }
			set
			{
				if (value == _showNonInstanceObjects) return;
				_showNonInstanceObjects = value;
				NotifyOfPropertyChange();
				UpdateTypes();
			}
		}

		public bool ShowEnumTypes
		{
			get { return _showEnumTypes; }
			set
			{
				if (value == _showEnumTypes) return;
				_showEnumTypes = value;
				NotifyOfPropertyChange();
				UpdateTypes();
			}
		}

		public bool ShowAbstractTypes
		{
			get { return _showAbstractTypes; }
			set
			{
				if (value == _showAbstractTypes) return;
				_showAbstractTypes = value;
				NotifyOfPropertyChange();
				UpdateTypes();
			}
		}

		public ObjectTypeEntry SelectedObject
		{
			get { return _selectedObject; }
			set
			{
				if (Equals(value, _selectedObject)) return;
				_selectedObject = value;
				UpdateProperties();
				NotifyOfPropertyChange();
			}
		}

		public void SetFilter(string filter)
		{
			FilterString = filter;
		}

		public void UpdateProperties()
		{
			if (_selectedObject == null)
				return;

			if (_selectedObject.Type.IsEnum)
			{
				Members = Enum.GetNames(_selectedObject.Type).Select(e => new EnumItemEntry(_selectedObject.Type, e));
			}
			else
			{
				Members =
					_selectedObject.Type.GetMembers(BindingFlags.Public | BindingFlags.Instance)
						.Where(
							m =>
								m.MemberType == MemberTypes.Property ||
								m.MemberType == MemberTypes.Field)
						.OrderByDescending(m => m.MemberType)
						.ThenBy(m => m.Name)
						.Select(t => new MemberEntry(t));
			}
		}

		public void UpdateTypes()
		{
			FilteredObjects = _types.Where(t =>
			{
				if (!string.IsNullOrWhiteSpace(_filterString) && !t.Name.ToLower().Contains(_filterString.ToLower()))
					return false;

				if (typeof(Settings).IsAssignableFrom(t) || typeof(GenericSettings).IsAssignableFrom(t))
					return false;

				if (!ShowNonInstanceObjects && !typeof(Instance).IsAssignableFrom(t) && !(t.IsEnum && ShowEnumTypes))
					return false;

				if (!ShowServices && typeof(Service).IsAssignableFrom(t))
					return false;

				if (!ShowAbstractTypes && t.IsAbstract)
					return false;

				return true;
			})
				.OrderBy(t => t.IsEnum)
				.ThenBy(t => typeof(Instance).IsAssignableFrom(t))
				.ThenBy(t => t.Name)
				.Select(t => new ObjectTypeEntry(t));
		}
	}
}