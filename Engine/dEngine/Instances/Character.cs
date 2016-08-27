// Character.cs - dEngine
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
using dEngine.Instances.Interfaces;
using dEngine.Services;
using dEngine.Utility;

using SharpDX;
using Matrix = BulletSharp.Math.Matrix;

namespace dEngine.Instances
{
    /// <summary>
    /// A character is an instance which can be controlled by players or AI.
    /// </summary>
    [TypeId(22), ExplorerOrder(5), ToolboxGroup("Gameplay")]
    public sealed class Character : Part, ICameraSubject
    {
        private readonly CharacterMotionState _motionState;

        /// <summary>
        /// Fired when <see cref="Health" /> reaches zero.
        /// </summary>
        public readonly Signal Died;

        private CFrame _cframe;
        private Vector3 _headOffset;
        private Vector3 _moveDirection;
        private Player _player;
        private float _health;
        private float _walkSpeed;
        private float _capsuleHeight;
        private float _capsuleRadius;
        private CapsuleShape _capsuleShape;

        /// <inheritdoc />
        public Character()
        {
            _motionState = new CharacterMotionState(this);

            RunService.Service.Heartbeat.Connect(Update);
            InputService.Service.InputBegan.Connect(OnInputBegan);
            InputService.Service.InputEnded.Connect(OnInputEnded);

            _walkSpeed = 16;
            CapsuleRadius = 2;
            CapsuleHeight = 5;

            RebuildRigidBody();
            Died = new Signal(this);
            Changed.Connect(OnChanged);
        }

        private void OnChanged(string prop)
        {
            if (prop == nameof(Size))
            {
                var size = Size;
                _capsuleRadius = Math.Max(size.x, size.z)/2.0f;
                _capsuleHeight = size.y;
            }
        }

        internal int ShieldCount { get; set; }

        /// <summary>
        /// Determines if the character is protected by a forcefield.
        /// </summary>
        [EditorVisible("Data")]
        public bool IsShielded => ShieldCount > 0;

        /// <summary>
        /// The direction the character is moving.
        /// </summary>
        [EditorVisible("Movement")]
        public Vector3 MoveDirection
        {
            get { return _moveDirection; }
            private set
            {
                if (value == _moveDirection) return;
                _moveDirection = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// The speed at which the character moves.
        /// </summary>
        [InstMember(1), EditorVisible("Movement")]
        public float WalkSpeed
        {
            get { return _walkSpeed; }
            set
            {
                if (value == _walkSpeed) return;
                _walkSpeed = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// The amount of health the Humanoid has.
        /// </summary>
        [InstMember(2), EditorVisible("Data")]
        public float Health
        {
            get { return _health; }
            set
            {
                if (value == _health || (value > 0 && _health == 0)) return;

                _health = value;

                if (value == 0)
                    Died.Fire();

                NotifyChanged();
            }
        }

        /// <summary>
        /// The radius of the collision capsule.
        /// </summary>
        [InstMember(3), EditorVisible("Physics")]
        public float CapsuleRadius
        {
            get { return _capsuleRadius; }
            set
            {
                if (value == _capsuleRadius) return;
                Size = new Vector3(value * 2, _capsuleHeight, value * 2);
                RebuildRigidBody();
                NotifyChanged();
            }
        }

        /// <summary>
        /// The height of the collision capsule.
        /// </summary>
        [InstMember(4), EditorVisible("Physics")]
        public float CapsuleHeight
        {
            get { return _capsuleHeight; }
            set
            {
                if (value == _capsuleHeight) return;
                Size = new Vector3(_capsuleRadius * 2, value, _capsuleRadius * 2);
                RebuildRigidBody();
                NotifyChanged();
            }
        }

        /// <summary>
        /// The camera offset from the center of the capsule.
        /// </summary>
        [InstMember(5), EditorVisible("Data")]
        public Vector3 HeadOffset
        {
            get { return _headOffset; }
            set
            {
                if (value == _headOffset) return;
                _headOffset = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// The skeleton this character is using.
        /// </summary>
        public Skeleton Skeleton { get; internal set; }

        /// <summary>
        /// The skeletal mesh this character is using.
        /// </summary>
        public SkeletalMesh SkeletalMesh { get; internal set; }

        /// <summary>
        /// The player that owns this character.
        /// </summary>
        public Player Player
        {
            get { return _player; }
            set
            {
                if (value == _player) return;
                _player = value;
                NotifyChanged();
            }
        }

        Vector3 ICameraSubject.GetVelocity()
        {
            return (Vector3)RigidBody.LinearVelocity;
        }

        /// <summary>
        /// Subtracts health if the Humanoid is not protected by a <see cref="Forcefield" />.
        /// </summary>
        /// <param name="amount">The amount of damage to take.</param>
        public void TakeDamage(float amount)
        {
            if (!IsShielded)
            {
                Health -= Math.Abs(amount);
            }
        }
        
        /// <summary>
        /// Adds health to the Humanoid.
        /// </summary>
        /// <param name="amount">The amount of health to give.</param>
        public void Heal(float amount)
        {
            Health += Math.Abs(amount);
        }

        private void Update(double dt)
        {
            if (Player?.IsLocalPlayer != true)
                return;

            float angleY = 0;

            var forward = InputService.Service.IsKeyDown(Key.W);
            var backward = InputService.Service.IsKeyDown(Key.S);
            var left = InputService.Service.IsKeyDown(Key.A);
            var right = InputService.Service.IsKeyDown(Key.D);
            var moved = true;

            if (forward && left && !right)
            {
                angleY = 45;
            }
            else if (forward && right && !left)
            {
                angleY = -45;
            }
            else if (forward)
            {
                angleY = 0;
            }
            else if (backward && left && !right)
            {
                angleY = 180 + -45;
            }
            else if (backward && right && !left)
            {
                angleY = 180 + 45;
            }
            else if (backward)
            {
                angleY = 180;
            }
            else if (left && !right)
            {
                angleY = 90;
            }
            else if (right && !left)
            {
                angleY = -90;
            }
            else
            {
                moved = false;
            }

            angleY *= Mathf.Deg2Rad;

            BulletSharp.Math.Vector3 linearVelocity;

            if (moved)
            {
                var cameraCF = Game.FocusedCamera?.CFrame ?? CFrame.Identity;
                var moveCF = cameraCF * CFrame.Angles(0, angleY, 0);

                var moveDir = (moveCF.lookVector * _walkSpeed);
                MoveDirection = new Vector3(moveDir.X, 0, moveDir.Y).unit;
                linearVelocity = new BulletSharp.Math.Vector3(moveDir.X, 0, moveDir.Z);
            }
            else
            {
                linearVelocity = BulletSharp.Math.Vector3.Zero;
                MoveDirection = Vector3.Zero;
            }

            lock (PhysicsSimulation.Locker)
            {
                RigidBody.LinearVelocity = linearVelocity;
            }
        }

        private void OnInputBegan(InputObject input)
        {
            if (input.InputType == InputType.Keyboard)
            {
            }
        }

        private void OnInputEnded(InputObject io)
        {
            if (io.InputType == InputType.Keyboard)
            {
            }
        }

        protected override void RebuildRigidBody()
        {
            lock (PhysicsSimulation.Locker)
            {
                World?.Physics.World.RemoveRigidBody(RigidBody);

                _capsuleShape?.Dispose();
                RigidBody?.Dispose();

                _capsuleShape = new CapsuleShape(_capsuleRadius, _capsuleHeight / 2);
                var mass = GetMass();
                var intertia = _capsuleShape.CalculateLocalInertia(mass);
                var constructionInfo = new RigidBodyConstructionInfo(mass, _motionState, _capsuleShape, intertia);
                RigidBody = new RigidBody(constructionInfo)
                {
                    AngularFactor = new BulletSharp.Math.Vector3(0, 0, 0),
                    CollisionFlags = CollisionFlags.CharacterObject,
                    ActivationState = ActivationState.DisableDeactivation,
                    UserObject = this
                };
                RigidBody.UpdateInertiaTensor();

                World?.Physics.World.AddRigidBody(RigidBody);
            }
        }

        /// <inheritdoc />
        public override void Destroy()
        {
            base.Destroy();
            RunService.Service.Heartbeat.Disconnect(Update);
            InputService.Service.InputBegan.Disconnect(OnInputBegan);
            InputService.Service.InputEnded.Disconnect(OnInputEnded);
        }

        /// <inheritdoc />
        protected override void OnChildAdded(Instance child)
        {
            base.OnChildAdded(child);

            SkeletalMesh skeletalMesh;
            Skeleton skeleton;

            if ((skeletalMesh = child as SkeletalMesh) != null)
            {
                SkeletalMesh = skeletalMesh;
            }
            else if ((skeleton = child as Skeleton) != null)
            {
                Skeleton = skeleton;
            }
        }

        /// <inheritdoc />
        protected override void OnChildRemoved(Instance child)
        {
            base.OnChildRemoved(child);

            if ((child as SkeletalMesh) != null)
            {
                SkeletalMesh = null;
            }
            else if ((child as Skeleton) != null)
            {
                Skeleton = null;
            }
        }

        /// <summary>
        /// Calculates the mass of this part.
        /// </summary>
        public override float GetMass()
        {
            return _capsuleRadius * _capsuleHeight * _capsuleRadius * 0.7f;
        }

        /// <summary>
        /// Teleports the character to the given position.
        /// </summary>
        /// <param name="position">The position the teleport to.</param>
        public void Teleport(Vector3 position)
        {
            var rotation = _cframe - _cframe.p;
            RigidBody.WorldTransform = (Matrix)(new CFrame(position) * rotation);
        }

        /// <summary>
        /// Teleports the character to the given position.
        /// </summary>
        /// <param name="position">The position the teleport to.</param>
        /// <param name="lookVector">The direction to face.</param>
        public void Teleport(Vector3 position, Vector3 lookVector)
        {
            RigidBody.WorldTransform = (Matrix)(new CFrame(position, position + lookVector));
        }

        internal class CharacterMotionState : MotionState
        {
            private readonly Character _character;

            public CharacterMotionState(Character character)
            {
                _character = character;
            }

            public override void GetWorldTransform(out Matrix worldTrans)
            {
                worldTrans = (Matrix)_character._cframe;
            }

            public override void SetWorldTransform(ref Matrix worldTrans)
            {
                _character._cframe = (CFrame)worldTrans;
                _character.Moved.Fire();
            }
        }
    }
}