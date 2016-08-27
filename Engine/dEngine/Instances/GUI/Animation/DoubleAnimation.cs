// UDim2Animation.cs - dEngine
// Copyright (C) 2016-2016 DanDevPC (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//  
// You should have received a copy of the GNU General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System;
using dEngine.Instances.Attributes;
using dEngine.Utility;

namespace dEngine.Instances
{
    /// <summary>
    /// Tweens a double property.
    /// </summary>
    [TypeId(229)]
    public class DoubleAnimation : GuiAnimation
    {
        private double _from;
        private double _to;

        /// <summary>
        /// The starting number.
        /// </summary>
        [InstMember(1), EditorVisible]
        public double From
        {
            get { return _from; }
            set
            {
                if (value == _from)
                    return;
                _from = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// The ending number.
        /// </summary>
        [InstMember(2), EditorVisible]
        public double To
        {
            get { return _to; }
            set
            {
                if (value == _to)
                    return;
                _to = value;
                NotifyChanged();
            }
        }

        internal override Type TargetType { get; } = typeof(UDim2);

        internal override bool Update()
        {
            lock (Locker)
            {
                var delta = (float)GetDelta();
                
                TargetProperty.Set(TargetElement, Mathf.Lerp(_from, _to, delta));

                return delta < 1;
            }
        }
    }
}