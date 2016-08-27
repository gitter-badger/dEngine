// StarterGui.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using dEngine.Instances;
using dEngine.Instances.Attributes;


namespace dEngine.Services
{
	/// <summary>
	/// A container for guis to be parented to players when they join.
	/// </summary>
	[TypeId(192), ExplorerOrder(6), Uncreatable]
	public class StarterGui : GuiContainerBase, ISingleton
	{
		private bool _resetGuisOnDeath;

		/// <summary>
		/// Determines whether guis will be reset when the player's character dies.
		/// </summary>
		[InstMember(1), EditorVisible("Behaviour")]
		public bool ResetGuisOnDeath
		{
			get { return _resetGuisOnDeath; }
			set
			{
				if (value == _resetGuisOnDeath)
					return;

				_resetGuisOnDeath = value;
				NotifyChanged(nameof(ResetGuisOnDeath));
			}
		}

		internal static object GetExisting()
		{
			return Game.StarterGui;
		}
	}
}