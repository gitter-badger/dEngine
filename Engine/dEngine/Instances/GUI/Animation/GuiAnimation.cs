// GuiAnimation.cs - dEngine
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
using System.Reflection;
using dEngine.Instances.Attributes;
using dEngine.Serializer.V1;
using Neo.IronLua;

namespace dEngine.Instances
{
    /// <summary>
    /// Base class for GUI animations.
    /// </summary>
    [TypeId(226)]
    public abstract class GuiAnimation : Instance
    {
        private TimeSpan _duration;

        internal double CurrentTime;

        /// <summary>
        /// Determines if the animation is being played.
        /// </summary>
        public bool IsAnimating { get; internal set; }

        internal GuiElement TargetElement;
        internal Inst.CachedProperty TargetProperty { get; set; }
        internal abstract Type TargetType { get; }

        /// <summary>
        /// The duration of the animation.
        /// </summary>
        [InstMember(1), EditorVisible]
        public TimeSpan Duration
        {
            get { return _duration; }
            set
            {
                if (value == _duration)
                    return;
                _duration = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Gets/Sets the playback position of the animation.
        /// </summary>
        [InstMember(2), EditorVisible]
        public TimeSpan PlaybackPosition
        {
            get { return TimeSpan.FromSeconds(CurrentTime); }
            set
            {
                CurrentTime = value.totalSeconds;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Returns the target element and property.
        /// </summary>
        /// <returns></returns>
        public LuaTuple<GuiElement, string> GetTarget()
        {
            return new LuaTuple<GuiElement, string>(TargetElement, TargetProperty?.Name);
        }

        internal double GetDelta()
        {
            if (CurrentTime >= _duration.totalSeconds)
                return 1;
            return CurrentTime/_duration.totalSeconds;
        }

        /// <summary>
        /// Sets the element and property that the animation will be applied to.
        /// </summary>
        public void SetTarget(GuiElement element, string property)
        {
            lock (Locker)
            {
                if (element == null)
                    throw new ArgumentException("The target element cannot be null.");
                if (string.IsNullOrEmpty(property))
                    throw new ArgumentException("The target property cannot be null or empty.");

                var prop = element.GetType().GetProperty(property);
                var instAttr = prop?.DeclaringType?.GetCustomAttribute<TypeIdAttribute>();
                var memberAttr = prop?.GetCustomAttribute<InstMemberAttribute>();

                if (prop == null)
                    throw new ArgumentException($"No property found for \"{property}\".");
                if (prop.PropertyType != TargetType)
                    throw new ArgumentException(
                        $"The animation type ({TargetType}) does not match the property type of \"{property}\" ({prop.PropertyType}).");
                if ((memberAttr == null) || (instAttr == null))
                    throw new ArgumentException($"The property {property} of {element} cannot be animated.");

                TargetProperty =
                    Inst.TypeDictionary[element.ClassName].TaggedProperties[
                        Inst.EncodePropertyTag(instAttr.Id, memberAttr.Tag)];
                TargetElement = element;
            }
        }


        internal abstract bool Update();
    }
}