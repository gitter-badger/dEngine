// Forcefield.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System;
using dEngine.Instances.Attributes;


namespace dEngine.Instances
{
    /// <summary>
    /// Protects a <see cref="Character"/> from damage by <see cref="Character.TakeDamage"/>.
    /// </summary>
    [TypeId(15), ExplorerOrder(3), ToolboxGroup("Gameplay")]
    public sealed class Forcefield : Instance
	{
		private Character _character;

		/// <inheritdoc/>
		public Forcefield()
		{
			ParentChanged.Event += OnParentChanged;
		}

		private void OnParentChanged(Instance parent)
		{
			var character = parent as Character;
			var lastCharacter = _character;
            _character = character;

			if (character != null)
			{
                character.ShieldCount++;
				Logger.Trace($"Character {character} gained a Forcefield.");
			}
			else if (lastCharacter != null)
			{
				lastCharacter.ShieldCount--;
				Logger.Trace($"Character {lastCharacter} lost a Forcefield.");
			}
		}
	}
}