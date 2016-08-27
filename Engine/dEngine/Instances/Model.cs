// Model.cs - dEngine
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
using C5;
using dEngine.Instances.Attributes;
using dEngine.Instances.Interfaces;
using JetBrains.Annotations;
using SharpDX;

namespace dEngine.Instances
{
    /// <summary>
    /// A container object for organizing 3D objects.
    /// </summary>
    /// <seealso cref="Folder" />
    [TypeId(10), ToolboxGroup("Containers"), ExplorerOrder(10)]
    public class Model : PVInstance, ICameraSubject
    {
        private readonly Vector3[] _boundingBoxPoints =
        {
            new Vector3(-1, -1, -1),
            new Vector3(1, -1, -1),
            new Vector3(-1, 1, -1),
            new Vector3(1, 1, -1),
            new Vector3(-1, -1, 1),
            new Vector3(1, -1, 1),
            new Vector3(-1, 1, 1),
            new Vector3(1, 1, 1)
        };

        private readonly ArrayList<Part> _descendantParts;
        private bool _boundsDirty;

        private OrientedBoundingBox _obb;
        private Part _primaryPart;
        private CFrame _cframe;
        private Vector3 _size;
        private CFrame _identityOrientation;

        private Part GetMainPart()
        {
            lock (Locker)
            {
                return _primaryPart ?? (_descendantParts.Count > 0 ? _descendantParts[0] : null);
            }
        }

        /// <inheritdoc />
        public Model()
        {
            _descendantParts = new ArrayList<Part>();

            DescendantAdded.Event += descendant =>
            {
                var part = descendant as Part;
                if (part == null) return;
                lock (Locker)
                {
                    _descendantParts.Add(part);
                }
                _boundsDirty = true;
                descendant.Changed.Event += OnDescendantChanged;
            };

            DescendantRemoving.Event += descendant =>
            {
                var part = descendant as Part;
                if (part == null) return;
                lock (Locker)
                {
                    _descendantParts.Remove(part);
                }
                _boundsDirty = true;
            };
        }

        /// <summary>
        /// The child part which represents the 'primary' part of this model.
        /// </summary>
        [InstMember(1), EditorVisible("Data"), CanBeNull]
        public Part PrimaryPart
        {
            get { return _primaryPart; }
            set
            {
                if (value == _primaryPart)
                    return;
                _primaryPart?.AncestryChanged.Disconnect(OnPrimaryPartAncestryChanged);
                if (value?.IsDescendantOf(this) != true)
                    value = null;
                _primaryPart = value;
                _boundsDirty = true;
                value?.AncestryChanged.Connect(OnPrimaryPartAncestryChanged);
                NotifyChanged(nameof(PrimaryPart));
            }
        }

        [InstMember(2), EditorVisible("Data")]
        internal CFrame IdentityOrientation
        {
            get { return _identityOrientation; }
            set
            {
                if (value == _identityOrientation) return;
                _identityOrientation = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// The transform of the model.
        /// </summary>
        public override CFrame CFrame
        {
            get { return GetModelCFrame(); }
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        /// The size of the model.
        /// </summary>
        public override Vector3 Size
        {
            get { return GetModelSize(); }
            set { throw new NotImplementedException("Resizing models has not been implemented."); }
        }

        Vector3 ICameraSubject.GetVelocity()
        {
            return Vector3.Zero;
        }


        private void OnPrimaryPartAncestryChanged(Instance child, Instance parent)
        {
            if (_primaryPart?.IsDescendantOf(this) != true)
            {
                PrimaryPart = null;
            }
        }

        /// <summary>
        /// Moves the centroid of the <see cref="Model" /> to the specified location, respecting all relative distances between
        /// parts in the model.
        /// </summary>
        /// <remarks>
        /// If <see cref="PrimaryPart" /> is will use the part's CFrame, otherwise the center of the model will be used.
        /// </remarks>
        /// <param name="position">The position to move to.</param>
        public void MoveTo(Vector3 position)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets the CFrame of the <see cref="PrimaryPart" />, respecting all relative distances between parts.
        /// </summary>
        /// <remarks>
        /// If <see cref="PrimaryPart" /> is not set, this method will error.
        /// </remarks>
        public void SetPrimaryPartCFrame(CFrame cframe)
        {
            var primaryPart = _primaryPart;
            if (primaryPart == null)
                throw new InvalidOperationException("Cannot call SetPrimaryPartCFrame() on a model with no PrimaryPart.");
            throw new NotImplementedException();
        }

        private void OnDescendantChanged(string prop)
        {
            if (prop == nameof(CFrame) || prop == nameof(Size))
            {
                _boundsDirty = true;
            }
        }

        /// <summary>
        /// Gets the CFrame of the Bounding Box.
        /// </summary>
        public CFrame GetModelCFrame()
        {
            if (_boundsDirty)
                UpdateBounds();
            return _cframe;
        }


        public void SetIdentityOrientation()
        {
            _identityOrientation = GetModelCFrame();
        }

        internal void UpdateBounds()
        {
        }

        /// <summary>
        /// Gets the size of the Bounding Box.
        /// </summary>
        public Vector3 GetModelSize()
        {
            if (_boundsDirty)
                UpdateBounds();

            return _size;
        }

        private void Recurse(Instance parent, Action<Part> action)
        {
            foreach (var child in parent.Children)
            {
                var p = child as Part;
                if (p != null)
                    action(p);
                Recurse(child, action);
            }
        }
    }
}