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
    /// Tweens a UDim2 property.
    /// </summary>
    [TypeId(227)]
    public class UDim2Animation : GuiAnimation
    {
        private UDim2 _from;
        private UDim2 _to;

        /// <summary>
        /// The starting UDim2.
        /// </summary>
        [InstMember(1), EditorVisible]
        public UDim2 From
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
        /// The ending UDim2.
        /// </summary>
        [InstMember(2), EditorVisible]
        public UDim2 To
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

                var scaleX = Mathf.Lerp(_from.Scale.x, _to.Scale.x, delta);
                var scaleY = Mathf.Lerp(_from.Scale.y, _to.Scale.y, delta);
                var absX = Mathf.Lerp(_from.Absolute.x, _to.Absolute.x, delta);
                var absY = Mathf.Lerp(_from.Absolute.y, _to.Absolute.y, delta);

                TargetProperty.Set(TargetElement, new UDim2(scaleX, absX, scaleY, absY));

                return delta < 1;
            }
        }
    }
}