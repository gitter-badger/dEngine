// Constraint.cs - dEngine
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
using BulletSharp;
using dEngine.Instances.Attributes;

namespace dEngine.Instances
{
    /// <summary>
    /// Base class for constraints.
    /// </summary>
    [TypeId(103), ToolboxGroup("Constraints")]
    public abstract class Constraint : Instance
    {
        protected Attachment _attachment0;
        protected Attachment _attachment1;
        protected TypedConstraint _constraint;
        private bool _enabled;

        /// <summary/>
        public static implicit operator TypedConstraint(Constraint constraint)
        {
            return constraint._constraint;
        }

        /// <summary>
        /// The <see cref="Attachment" /> which is connected to <see cref="Attachment1" />.
        /// </summary>
        [InstMember(1), EditorVisible("Attachments")]
        public Attachment Attachment0
        {
            get { return _attachment0; }
            set
            {
                lock (Locker)
                {
                    if (value == _attachment0)
                        return;

                    _attachment0?.Changed.Disconnect(AttachmentOnChanged);
                    _attachment0 = value;
                    value?.Changed.Connect(AttachmentOnChanged);
                    if (value?.Part != null)
                        RebuildConstraint();
                }
                NotifyChanged(nameof(Attachment0));
            }
        }

        /// <summary>
        /// The <see cref="Attachment" /> which is connected to <see cref="Attachment0" />.
        /// </summary>
        [InstMember(2), EditorVisible("Attachments")]
        public Attachment Attachment1
        {
            get { return _attachment1; }
            set
            {
                lock (Locker)
                {
                    if (value == _attachment1)
                        return;

                    _attachment1?.Changed.Disconnect(AttachmentOnChanged);
                    _attachment1 = value;
                    value?.Changed.Connect(AttachmentOnChanged);
                    if (value?.Part != null)
                        RebuildConstraint();
                }

                NotifyChanged(nameof(Attachment1));
            }
        }

        /// <summary>
        /// Toggles whether or not the constraint is enabled.
        /// </summary>
        [InstMember(3), EditorVisible("Behaviour")]
        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (value == _enabled)
                    return;

                _enabled = value;
                NotifyChanged(nameof(Enabled));
            }
        }

        /// <summary>
        /// Determines if both attachments have a valid <see cref="Part"/>.
        /// </summary>
        protected bool Validate()
        {
            var part0 = _attachment0?.Part;
            var part1 = _attachment1?.Part;
            var hisWorld = World;
            return hisWorld != null && hisWorld.Physics != null && part0 != null && part1 != null && part0.World == hisWorld && part1.World == hisWorld;
        }

        /// <summary>
        /// Called when an attachment is set or changed.
        /// </summary>
        protected abstract void RebuildConstraint();

        /// <summary>
        /// Removes the current constraint from the world.
        /// </summary>
        protected void DestroyConstraint()
        {
            var constraint = _constraint;
            if (constraint != null)
            {
                World.Physics.World.RemoveConstraint(_constraint);
                _constraint.Dispose(); // TODO: check if dispose crashes
            }
        }

        private void AttachmentOnChanged(string property)
        {
            lock (Locker)
            {
                RebuildConstraint();
            }
        }
    }
}