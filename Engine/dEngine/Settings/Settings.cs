// Settings.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using dEngine.Instances;
using dEngine.Instances.Attributes;
using dEngine.Settings.User;
using dEngine.Utility;
using IniParser.Model;
using Neo.IronLua;


namespace dEngine.Settings
{
	/// <summary>
	/// Base class for settings.
	/// </summary>
	[TypeId(114), Uncreatable]
	public abstract class Settings : Instance
	{
		/// <inheritdoc />
		protected Settings()
		{
			RestoreDefaults();
		}

		/// <summary>
		/// Applies settings via a dictionary table.
		/// </summary>
		public void ApplySettings(LuaTable settings)
		{
			foreach (var kv in settings.Members)
			{
				Dynamitey.Dynamic.InvokeSet(this, kv.Key, kv.Value);
			}
		}

		/// <summary>
		/// Returns a table dictionary of the settings.
		/// </summary>
		public LuaTable ToDictionary()
		{
			return ((Dictionary<string, object>)this).ToLuaTable();
		}

		/// <summary>
		/// Returns a dictionary of settings.
		/// </summary>
		public static explicit operator Dictionary<string, object>(Settings settings)
		{
			var dictionary = settings.GetType()
				.GetProperties(BindingFlags.Static | BindingFlags.Public).Where(p => p.CanWrite)
				.ToDictionary(p => p.Name, p => p.GetValue(settings));

		    var customSettings = settings as ICustomSettings;
		    if (customSettings != null)
                foreach (var kv in customSettings.GetCustomSettings())
                    dictionary[(string)kv.Key] = kv.Value;

		    return dictionary;
		}

		internal IniData GetIniData()
		{
			var data = new IniData();
			data.Sections.AddSection(Name);
			foreach (var kv in (Dictionary<string, object>)this)
			{
				Comments.Comment comment;
				API.Comments.Get($"P:{GetType().FullName}.{kv.Key}", out comment);

				List<string> commentList = null;
				commentList = new List<string>(2);
				if (!string.IsNullOrEmpty(comment.Summary))
					commentList.Add(comment.Summary);
				if (!string.IsNullOrEmpty(comment.Remarks))
					commentList.Add(comment.Remarks);

				data.Sections[Name].AddKey(new KeyData(kv.Key)
				{
					Value = kv.Value?.ToString(),
					Comments = commentList
				});
			}
			return data;
		}

		/// <summary>
		/// Resets the settings to their default values.
		/// </summary>
		public abstract void RestoreDefaults();

		/// <summary>
		/// Sets a static setting property.
		/// </summary>
		public override bool TrySetMember(SetMemberBinder binder, object value)
		{
			var property = GetType().GetProperty(binder.Name, BindingFlags.Public | BindingFlags.Static);
			if (property != null)
			{
				property.SetValue(this, value);
				NotifyChanged(binder.Name);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Gets a static setting property.
		/// </summary>
		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			var property = GetType().GetProperty(binder.Name, BindingFlags.Public | BindingFlags.Static);
			if (property != null)
			{
				result = property.GetValue(this);
				return true;
			}
			result = null;
			return false;
		}
	}
}