// Seat.cs - dEngine
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


namespace dEngine.Instances
{
    /// <summary>
    /// A brick which can be occupied by a <see cref="Character" />.
    /// </summary>
    [TypeId(16), ExplorerOrder(3), ToolboxGroup("Bricks")]
    public class Seat : Part
    {
        protected bool _disabled;
        private readonly Attachment _attachment;
        private bool _occupyOnTouch;
        private Character _occupant;

        /// <summary/>
        public Seat()
        {
            _attachment = new Attachment
            {
                Name = "SeatAttachment",
                Parent = this,
                Archivable = false,
                ParentLocked = false
            };
        }

        /// <inheritdoc/>
        public override void Destroy()
        {
            base.Destroy();

            Occupant = null;
            _attachment.ParentLocked = false;
        }

        /// <summary>
        /// The character that currently occupies this seat.
        /// </summary>
        /// <remarks>
        /// This property can be manually set, which will eject the current occupant if there is one,
        /// and force the new character to sit on it.
        /// </remarks>
        [InstMember(1)]
        public Character Occupant
        {
            get { return _occupant; }
            set
            {
                if (value == _occupant) return;
                _occupant = value; 
                NotifyChanged();
            }
        }

        /// <summary>
        /// Determines if this object can be occupied by touching it.
        /// </summary>
        [InstMember(2), EditorVisible("Data")]
        public bool OccupyOnTouch
        {
            get { return _occupyOnTouch; }
            set
            {
                if (value == _occupyOnTouch) return;
                _occupyOnTouch = value;
                NotifyChanged();
            }
        }
    }
}