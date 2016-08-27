// PostEffect.cs - dEngine
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
using dEngine.Instances.Attributes;
using dEngine.Services;
using SharpDX.Direct3D11;

#pragma warning disable 1591

namespace dEngine.Instances
{
    /// <summary>
    /// Abstract class for post processing effects. PostEffects must be parented to the <see cref="Workspace.CurrentCamera" />.
    /// </summary>
    [TypeId(173)]
    public abstract class PostEffect : Instance
    {
        protected int _effectOrder;
        private bool _enabled;

        protected enum EffectPrority
        {
            AmbientOcclusion,
            MotionBlur,
            DepthOfField,
            Bloom,
            LensFlare,
            ToneMapping,
            ColourCorrection = 50000
        }

        /// <inheritdoc />
        protected PostEffect()
        {
            Enabled = true;
        }

        internal virtual void UpdateSize(Camera camera)
        {
        }

        protected Camera Camera;

        internal void SetCamera(Camera camera)
        {
            Camera = camera;
            UpdateSize(camera);
        }

        /// <summary>
        /// Determines if the effect is applied.
        /// </summary>
        [InstMember(1), EditorVisible("Data")]
        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (value == _enabled)
                    return;

                _enabled = value;
                NotifyChanged();
            }
        }

        internal abstract void Render(ref DeviceContext context);

        /// <inheritdoc />
        public override void Destroy()
        {
            base.Destroy();
            Camera?.PostEffects.Remove(this);
        }

        internal class PostEffectSorter : IComparer<PostEffect>
        {
            public int Compare(PostEffect x, PostEffect y)
            {
                var comp = x._effectOrder.CompareTo(y._effectOrder);
                return comp == 0 ? x.GetHashCode().CompareTo(y.GetHashCode()) : comp;
            }
        }
    }
}