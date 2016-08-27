// ParticleEmitter.cs - dEngine
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
using dEngine.Data;
using dEngine.Instances.Attributes;
using dEngine.Utility;


namespace dEngine.Instances
{
    /// <summary>
    /// A generic particle system.
    /// </summary>
    [TypeId(133), ExplorerOrder(3), ToolboxGroup("Brick equipment")]
    public class ParticleEmitter : Instance
    {
        private Vector3 _acceleration;
        private ColourSequence _colourSequence;
        private float _drag;
        private NormalId _emissionDirection;
        private bool _enabled;
        private NumberRange _lifetime;
        private float _blending;
        private bool _lockedToPart;
        private float _rate;
        private NumberRange _rotation;
        private NumberRange _rotSpeed;
        private NumberSequence _size;
        private NumberRange _speed;
        private Content<Texture> _texture;
        private NumberSequence _transparency;
        private float _velocityInheritance;
        private float _velocitySpread;
        private float _zOffset;

        /// <summary/>
        public ParticleEmitter()
        {
            Acceleration = Vector3.Zero;
            Colour = new ColourSequence(dEngine.Colour.White);
            Drag = 0;
            EmissionDirection = NormalId.Top;
            Enabled = true;
            Lifetime = new NumberRange(5, 10);
            Blending = 0f;
            LockedToPart = false;
            Rate = 20;
            RotSpeed = new NumberRange(0);
            Rotation = new NumberRange(0);
            Size = new NumberSequence(1);
            Speed = new NumberRange(5);
            Texture = "rbxassetid://242201991";
            Transparency = new NumberSequence(0);
            VelocityInheritance = 0;
            VelocitySpread = 0;
            ZOffset = 0;
        }

        /// <summary>
        /// Applies constant acceleration to all particles.
        /// </summary>
        [InstMember(1), EditorVisible("Motion")]
        public Vector3 Acceleration
        {
            get { return _acceleration; }
            set
            {
                if (value == _acceleration) return;
                _acceleration = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// A sequence of colours a particle will use over their lifetime.
        /// </summary>
        [InstMember(2), EditorVisible("Appearance")]
        public ColourSequence Colour
        {
            get { return _colourSequence; }
            set
            {
                if (value == _colourSequence) return;
                _colourSequence = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Determines how much drag the particles have.
        /// </summary>
        [InstMember(3), EditorVisible("Particles")]
        public float Drag
        {
            get { return _drag; }
            set
            {
                if (value == _drag) return;
                _drag = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// The direction that the particles will emit in.
        /// </summary>
        [InstMember(4), EditorVisible("Emission")]
        public NormalId EmissionDirection
        {
            get { return _emissionDirection; }
            set
            {
                if (value == _emissionDirection) return;
                _emissionDirection = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Determines if the emitter is enabled.
        /// </summary>
        [InstMember(5), EditorVisible("Data")]
        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (value == _enabled) return;
                _enabled = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// The lifetime of the particles.
        /// </summary>
        [InstMember(6), EditorVisible("Emission")]
        public NumberRange Lifetime
        {
            get { return _lifetime; }
            set
            {
                if (value == _lifetime) return;
                _lifetime = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Determines how much the particles blend with eachother.
        /// </summary>
        [InstMember(7), EditorVisible("Appearance"), Range(0, 1)]
        public float Blending
        {
            get { return _blending; }
            set
            {
                if (value == _blending) return;
                value = Mathf.Clamp(value, 0, 1);
                _blending = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Determines whether particles rigidly move with the part they're emitted from.
        /// </summary>
        [InstMember(8), EditorVisible("Particles")]
        public bool LockedToPart
        {
            get { return _lockedToPart; }
            set
            {
                if (value == _lockedToPart) return;
                _lockedToPart = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// The rate at which particles are emitted.
        /// </summary>
        [InstMember(9), EditorVisible("Emission")]
        public float Rate
        {
            get { return _rate; }
            set
            {
                if (value == _rate) return;
                _rate = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Defines minimum/maximum rotational speed.
        /// </summary>
        [InstMember(10), EditorVisible("Emission")]
        public NumberRange RotSpeed
        {
            get { return _rotSpeed; }
            set
            {
                if (value == _rotSpeed) return;
                _rotSpeed = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Defines minimum/maximum rotation.
        /// </summary>
        [InstMember(11), EditorVisible("Data")]
        public NumberRange Rotation
        {
            get { return _rotation; }
            set
            {
                if (value == _rotation) return;
                _rotation = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// The size of the particles over their lifeitme.
        /// </summary>
        [InstMember(12), EditorVisible("Appearance")]
        public NumberSequence Size
        {
            get { return _size; }
            set
            {
                if (value == _size) return;
                _size = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// The speed of the particles.
        /// </summary>
        [InstMember(13), EditorVisible("Emission")]
        public NumberRange Speed
        {
            get { return _speed; }
            set
            {
                if (value == _speed) return;
                _speed = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// The texture for each particle.
        /// </summary>
        [InstMember(14), EditorVisible("Appearance")]
        public Content<Texture> Texture
        {
            get { return _texture; }
            set
            {
                if (value == _texture) return;
                _texture = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// The transparency of a particle over its lifetime.
        /// </summary>
        [InstMember(15), EditorVisible("Appearance")]
        public NumberSequence Transparency
        {
            get { return _transparency; }
            set
            {
                if (value == _transparency) return;
                _transparency = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Determines how the particles move relative to the emitter part.
        /// </summary>
        [InstMember(16), EditorVisible("Particles")]
        public float VelocityInheritance
        {
            get { return _velocityInheritance; }
            set
            {
                if (value == _velocityInheritance) return;
                value = Mathf.Clamp(value, 0, 1);
                _velocityInheritance = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Determines how offset a particle can be fired from the local positive Z of the emitter part.
        /// </summary>
        [InstMember(17), EditorVisible("Data")]
        public float VelocitySpread
        {
            get { return _velocitySpread; }
            set
            {
                if (value == _velocitySpread) return;
                _velocitySpread = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Used to order the drawing of emitters.
        /// </summary>
        [InstMember(18), EditorVisible("Appearance")]
        public float ZOffset
        {
            get { return _zOffset; }
            set
            {
                if (value == _zOffset) return;
                _zOffset = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Emits the given number of particles.
        /// </summary>
        public void Emit(int particleCount = 16)
        {
            throw new NotImplementedException();
        }
    }
}