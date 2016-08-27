// Part.cs - dEngine
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
using System.Linq;
using BulletSharp;
using C5;
using dEngine.Data;
using dEngine.Graphics;
using dEngine.Graphics.Structs;
using dEngine.Instances.Attributes;
using dEngine.Instances.Interfaces;
using dEngine.Instances.Materials;
using dEngine.Serializer.V1;
using dEngine.Settings.Global;
using dEngine.Utility;
using SharpDX;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;
using BulletVector3 = BulletSharp.Math.Vector3;
using BulletMatrix = BulletSharp.Math.Matrix;

namespace dEngine.Instances
{
    /// <summary>
    /// A building block.
    /// </summary>
    [TypeId(5), ToolboxGroup("Bricks"), ExplorerOrder(11)]
    public class Part : PVInstance, IRenderable, ICameraSubject
    {
        private const float _minSize = 0;

        /// <summary/>
        public static implicit operator RigidBody(Part part)
        {
            return part.RigidBody;
        }

        /// <summary>
        /// Fired when <see cref="CFrame" /> is set or the part is moved in the physics engine.
        /// </summary>
        internal readonly Signal Moved;

        private volatile bool _anchored;
        private Colour _brickColour;
        private volatile bool _canCollide;
        private CFrame _cframe;
        private Mesh _childMesh;
        private bool _locked;
        private Material _material;

        private float _metallic;
        private Matrix _modelMatrix;
        private Shape _shape;
        private Vector3 _size;

        private volatile float _smoothness;
        private volatile float _transparency;
        internal volatile RigidBody RigidBody;
        internal Buffer TransparentInstanceBuffer;
        internal VertexBufferBinding TransparentInstanceBufferBinding;

        /// <inheritdoc />
        public Part()
        {
            _shape = Shape.Cube;
            _brickColour = new Colour(0.63921570777893f, 0.63529413938522f, 0.64705884456635f);
            _canCollide = true;
            _cframe = CFrame.Identity;
            _modelMatrix = Matrix.Identity;
            _size = new Vector3(4, 1, 2);
            _material = Material.Smooth;
            _smoothness = 0.16f;

            PhysicalProperties = new PhysicalProperties(0.7f, 0.3f, 0.5f);

            RebuildRigidBody();

            Touched = new Signal<Part>(this);
            TouchEnded = new Signal<Part>(this);
            Moved = new Signal(this);
        }

        /// <summary>
        /// The colour of the block.
        /// </summary>
        [InstMember(1), EditorVisible("Appearance")]
        public Colour BrickColour
        {
            get { return _brickColour; }
            set
            {
                if (_brickColour == value) return;
                _brickColour = value;
                UpdateRenderData();
                NotifyChanged();
            }
        }

        /// <summary>
        /// The shape of the part.
        /// </summary>
        [InstMember(2), EditorVisible("Part")]
        public Shape Shape
        {
            get { return _shape; }
            set
            {
                if (_shape == value) return;
                _shape = value;
                ChangeRenderObject();
                RebuildRigidBody();
                NotifyChanged();
            }
        }

        /// <summary>
        /// Determines if this block can collide with other objects.
        /// </summary>
        [InstMember(4), EditorVisible("Behaviour")]
        public bool CanCollide
        {
            get { return _canCollide; }
            set
            {
                if (_canCollide == value) return;
                _canCollide = value;
                lock (PhysicsSimulation.Locker)
                {
                    var rigidBody = RigidBody;
                    rigidBody.CollisionFlags = value ? CollisionFlags.None : CollisionFlags.NoContactResponse;
                    rigidBody.Activate(true);
                }
                NotifyChanged();
            }
        }

        /// <summary>
        /// The material to render this part with.
        /// </summary>
        [InstMember(5), EditorVisible("Appearance"), ContentId(ContentType.Material), Obsolete]
        public Material Material
        {
            get { return _material; }
            set
            {
                if (_material == value) return;
                _material = value;
                UpdateRenderData();
                NotifyChanged();
            }
        }

        /// <summary>
        /// If false, this part should not be selectable by 3D pickers.
        /// </summary>
        [InstMember(6), EditorVisible("Behaviour")]
        public bool Locked
        {
            get { return _locked; }
            set
            {
                if (_locked == value) return;
                _locked = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// The transparency of the part.
        /// </summary>
        [InstMember(7), EditorVisible("Appearance"), Range(0, 1)]
        public float Transparency
        {
            get { return _transparency; }
            set
            {
                if (_transparency == value) return;
                _transparency = Math.Max(0, Math.Min(1, value));

                ChangeRenderObject();
                UpdateRenderData();
                NotifyChanged();
            }
        }

        /// <summary>
        /// Determines how smooth the surface is.
        /// </summary>
        [InstMember(8), EditorVisible("Appearance"), Range(0, 1)]
        public float Smoothness
        {
            get { return _smoothness; }
            set
            {
                if (value == _smoothness) return;
                _smoothness = value;
                UpdateRenderData();
                NotifyChanged(nameof(Smoothness));
            }
        }

        /// <summary>
        /// Determines how metallic the surface is.
        /// </summary>
        [InstMember(9), EditorVisible("Appearance"), Range(0, 1)]
        public float Metallic
        {
            get { return _metallic; }
            set
            {
                if (value == _metallic) return;
                _metallic = value;
                UpdateRenderData();
                NotifyChanged(nameof(Metallic));
            }
        }

        /// <summary>
        /// The 3D position/rotation.
        /// </summary>
        [InstMember(10)]
        public override CFrame CFrame
        {
            get { return _cframe; }
            set
            {
                if (_cframe == value)
                    return;

                _cframe = value;
                _modelMatrix = value.GetModelMatrix();

                UpdateRenderData();

                lock (PhysicsSimulation.Locker)
                {
                    var trans = (BulletMatrix)value;
                    RigidBody.WorldTransform = trans;
                    RigidBody.Activate(true);
                    //if (RunService.SessionState != SessionState.Running)
                    //	World?.Physics?.World.UpdateSingleAabb(RigidBody);
                }

                Velocity = Vector3.Zero;
                RotVelocity = Vector3.Zero;

                if (!MuteCFrameChanges)
                {
                    NotifyChanged(nameof(CFrame));
                    NotifyChanged(nameof(Position));
                    NotifyChanged(nameof(Rotation));
                }
            }
        }

        /// <summary>
        /// The size of the instance.
        /// </summary>
        /// <remarks>
        /// When accessed by a model this may calculate the bounding box.
        /// </remarks>
        [InstMember(11), EditorVisible("Part")]
        public override Vector3 Size
        {
            get { return _size; }
            set
            {
                if (_size == value) return;
                _size = new Vector3(Math.Max(value.X, _minSize), Math.Max(value.Y, _minSize),
                    Math.Max(value.Z, _minSize));

                UpdateRenderData();

                lock (Locker)
                {
                    var rigidBody = RigidBody;
                    rigidBody.CollisionShape.LocalScaling = new BulletVector3(_size.x / 2, _size.y / 2, _size.z / 2);
                    if (rigidBody.IsInWorld)
                        World?.Physics?.World.UpdateSingleAabb(rigidBody);
                }

                if (!MuteSizeChanges)
                    NotifyChanged(nameof(Size));
            }
        }

        /// <summary>
        /// The properties of the physics engine.
        /// </summary>
        [InstMember(12), EditorVisible("Data")]
        public PhysicalProperties PhysicalProperties
        {
            get { return _physicalProperties; }
            set
            {
                if (value == _physicalProperties) return;
                _physicalProperties = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// The position of <see cref="CFrame" />.
        /// </summary>
        [EditorVisible("Data")]
        public Vector3 Position
        {
            get { return _cframe.p; }
            set
            {
                if (value == _cframe.p) return;
                var rotation = _cframe - _cframe.p;
                CFrame = new CFrame(value) * rotation;
            }
        }

        /// <summary>
        /// The rotation of <see cref="CFrame" />.
        /// </summary>
        [EditorVisible("Data")]
        public Vector3 Rotation
        {
            get { return _cframe.getEulerAngles() * Mathf.Rad2Deg; }
            set
            {
                if (value == Rotation) return;
                var v3 = value * Mathf.Deg2Rad;
                var rotation = CFrame.Angles(v3.x, v3.y, v3.z);
                CFrame = new CFrame(_cframe.p) * rotation;
            }
        }

        /// <summary>
        /// The linear velocity of this object.
        /// </summary>
        [EditorVisible("Data")]
        public Vector3 Velocity
        {
            get
            {
                var rigidBody = RigidBody;
                return (Vector3)rigidBody.LinearVelocity;
            }
            set
            {
                var rigidBody = RigidBody;
                rigidBody.LinearVelocity = (BulletVector3)value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// The angular velocity of this object.
        /// </summary>
        [EditorVisible("Data")]
        public Vector3 RotVelocity
        {
            get
            {
                var rigidBody = RigidBody;
                return (Vector3)rigidBody.AngularVelocity;
            }
            set
            {
                var rigidBody = RigidBody;
                rigidBody.AngularVelocity = (BulletVector3)value;
                NotifyChanged();
            }
        }

        internal Mesh ChildMesh
        {
            get { return _childMesh; }
            set
            {
                var oldMesh = _childMesh;
                if (oldMesh != null)
                {
                    oldMesh.GeometryChanged -= OnChildMeshGeometryChanged;
                    oldMesh.GeometryUpdated -= OnChildMeshGeometryUpdated;
                }

                _childMesh = value;

                if (value != null)
                {
                    value.GeometryChanged += OnChildMeshGeometryChanged;
                    value.GeometryUpdated += OnChildMeshGeometryUpdated;
                }

                ChangeRenderObject();
            }
        }

        /// <summary>
        /// If true, the part will be physically immovable and static.
        /// </summary>
        [InstMember(3), EditorVisible("Behaviour")]
        public bool Anchored
        {
            get { return _anchored; }
            set
            {
                if (_anchored == value) return;
                _anchored = value;

                if (value)
                {
                    //Velocity = Vector3.Zero;
                    //RotVelocity = Vector3.Zero;
                    UpdateMass();
                }
                else
                {
                    UpdateMass();
                }

                NotifyChanged();
            }
        }

        Vector3 ICameraSubject.GetVelocity()
        {
            return Velocity;
        }

        /// <inheritdoc />
        public RenderObject RenderObject { get; set; }

        /// <inheritdoc />
        public int RenderIndex { get; set; }

        /// <inheritdoc />
        public InstanceRenderData RenderData { get; set; }

        /// <inheritdoc />
        public void UpdateRenderData()
        {
            if (_deserializing)
                return;

            var renderSize = _size;

            var matrix = _modelMatrix;

            if (_childMesh != null)
            {
                //matrix *= Matrix.Translation((SharpDX.Vector3)_childMesh.Offset);
                if (_childMesh.UsePartSize)
                    renderSize *= _childMesh.Scale;
                else
                    renderSize = _childMesh.Scale;
            }

            RenderData = new InstanceRenderData
            {
                Size = renderSize,
                Colour = _brickColour,
                ModelMatrix = matrix,
                Transparency = _transparency,
                Emissive = 0,
                ShadingModel = ShadingModel.Standard,
                Metallic = _metallic,
                Smoothness = _smoothness
            };

            RenderObject?.UpdateInstance(this);
        }

        private void OnChildMeshGeometryUpdated()
        {
            UpdateRenderData();
        }

        private void OnChildMeshGeometryChanged(Geometry geometry)
        {
            ChangeRenderObject();
        }

        /// <inheritdoc />
        protected override void OnWorldChanged(IWorld newWorld, IWorld oldWorld)
        {
            base.OnWorldChanged(newWorld, oldWorld);
            ChangeRenderObject(); // handle addition/removal from renderer based on whether part is in a world.

            oldWorld?.Physics?.RemovePart(this);
            if (!_deserializing)
                newWorld?.Physics?.AddPart(this);
        }

        /// <summary>
        /// Gets the mesh representing this part. If the part has a Mesh, the child geometry will be returned.
        /// </summary>
        /// <returns></returns>
        internal Geometry GetMesh()
        {
            return Primitives.GeometryFromShape(_shape);
        }

        /// <inheritdoc />
        public override void Destroy()
        {
            base.Destroy();

            lock (PhysicsSimulation.Locker)
            {
                World?.Physics.RemovePart(this);
            }
        }
        
        /// <summary>
        /// Breaks any joint or constraint this object is a part of.
        /// </summary>
        public void BreakJoints()
        {
            lock (PhysicsSimulation.Locker)
            {
                for (int i = 0; i < RigidBody.NumConstraintRefs; i++)
                {
                    var constraint = RigidBody.GetConstraintRef(i);
                    World?.Physics?.World.RemoveConstraint(constraint);
                }
            }
        }

        /// <summary>
        /// Returns a list of parts that this part is connected to.
        /// </summary>
        /// <param name="recursive">If true, includes all parts in the assembly.</param>
        public Part[] GetConnectedParts(bool recursive = false)
        {
            var parts = new HashSet<Part>();
            lock (PhysicsSimulation.Locker)
            {
                for (int i = 0; i < RigidBody.NumConstraintRefs; i++)
                {
                    var partA = (Part)RigidBody.GetConstraintRef(i).RigidBodyA.UserObject;
                    var partB = (Part)RigidBody.GetConstraintRef(i).RigidBodyB.UserObject;
                    var addedA = parts.Add(partA);
                    var addedB = parts.Add(partB);

                    if (recursive)
                    {
                        if (addedA)
                            parts.AddAll(partA.GetConnectedParts(true));
                        if (addedB)
                            parts.AddAll(partB.GetConnectedParts(true));
                    }
                }
            }
            return parts.ToArray();
        }

        /// <summary>
        /// Returns a list of constraints this object is an attachment of.
        /// </summary>
        /// <returns></returns>
        public Constraint[] GetConstraints()
        {
            lock (PhysicsSimulation.Locker)
            {
                var constraints = new ArrayListEx<Constraint>(RigidBody.NumConstraintRefs);
                for (int i = 0; i < RigidBody.NumConstraintRefs; i++)
                {
                    var constraint = RigidBody.GetConstraintRef(i).Userobject as Constraint;
                    constraints.Add(constraint);
                }
                return constraints.GetArray();
            }
        }

        /// <summary>
        /// Calculates the mass of this part.
        /// </summary>
        public virtual float GetMass()
        {
            return _size.x * _size.y * _size.z * _physicalProperties.Density;
        }

        /// <summary>
        /// Called when a property that requires the RigidBody to be recreated is changed.
        /// </summary>
        protected virtual void RebuildRigidBody()
        {
            if (_deserializing)
                return;

            lock (PhysicsSimulation.Locker)
            {
                var world = World;

                world?.Physics?.RemovePart(this); // remove part from physics to be replaced

                var motionState = new PartMotionState(this);
                var collisionShape = MakeCollisionShape();
                var mass = _anchored ? 0 : GetMass();
                var constructionInfo = new RigidBodyConstructionInfo(mass, motionState, collisionShape,
                    collisionShape.CalculateLocalInertia(GetMass()));
                RigidBody = new RigidBody(constructionInfo)
                {
                    Restitution = _physicalProperties.Elasticity,
                    Friction = _physicalProperties.Friction,
                    CollisionFlags = _anchored ? CollisionFlags.StaticObject : CollisionFlags.None,
                    UserObject = this
                };
                //RigidBody.SetSleepingThresholds(PhysicsSettings.LinearSleepingThreshold,
                //    PhysicsSettings.AngularSleepingThreshold);

                world?.Physics?.AddPart(this);
            }
        }

        private void UpdateMass()
        {
            lock (PhysicsSimulation.Locker)
            {
                var mass = GetMass();
                var inertia = RigidBody.CollisionShape.CalculateLocalInertia(mass);
                RigidBody.SetMassProps(_anchored ? 0 : mass, inertia);
                RigidBody.CollisionFlags = _anchored ? CollisionFlags.StaticObject : CollisionFlags.None;
                RigidBody.Activate(true);
            }
        }

        /// <summary>
        /// Returns the interpolated CFrame.
        /// </summary>
        public CFrame GetRenderCFrame()
        {
            return (CFrame)RigidBody.InterpolationWorldTransform;
        }

        private CollisionShape MakeCollisionShape()
        {
            CollisionShape shape;
            switch (Shape)
            {
                case Shape.Cube:
                    shape = new BoxShape(1, 1, 1);
                    break;
                case Shape.Sphere:
                    shape = new SphereShape(1);
                    break;
                case Shape.Cylinder:
                    shape = new ConeShape(1, 1);
                    break;
                case Shape.Wedge:
                    var wedgeGeo = Primitives.WedgeGeometry;
                    shape = new ConvexTriangleMeshShape(new TriangleIndexVertexArray(wedgeGeo.Indices, wedgeGeo.Vertices.Select(v => (BulletVector3)v.Position).ToList()));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            shape.Margin *= PhysicsSimulation.Scale;
            shape.Margin = 0;
            shape.LocalScaling = new BulletVector3(_size.x / 2, _size.y / 2, _size.z / 2);
            return shape;
        }

        /// <summary>
        /// Returns true if the part can be rendered.
        /// </summary>
        protected bool CanRender()
        {
            return !(World == null || _transparency == 1 || IsDestroyed || _deserializing);
        }

        internal override void BeforeDeserialization()
        {
            base.BeforeDeserialization();
            _deserializing = true;
        }

        internal override void AfterDeserialization(Inst.Context context)
        {
            base.AfterDeserialization(context);
            _deserializing = false;
            UpdateRenderData();
            ChangeRenderObject();
            RebuildRigidBody();
        }

        /// <summary>
        /// Removes this part from the current <see cref="Graphics.RenderObject" />, and, if <see cref="CanRender" />, adds it to a
        /// new one.
        /// </summary>
        protected virtual void ChangeRenderObject()
        {
            lock (Locker)
            {
                if (RenderObject != null)
                {
                    // TODO: check why RenderIndex is -1 while RenderObject is set with ChildMesh
                    //Debug.Assert(RenderIndex != -1);
                    if (RenderIndex == -1)
                        return;
                }
                RenderObject?.Remove(this);

                if (!CanRender())
                    return;

                RenderObject newRO;

                if (_childMesh != null)
                {
                    var childGeo = _childMesh.Geometry;
                    if (childGeo == null || !childGeo.IsLoaded) return;
                    newRO = World.RenderObjectProvider[childGeo, _childMesh.Material];
                }
                else
                {
                    newRO = World.RenderObjectProvider[_shape];
                }

                if (newRO != null)
                {
                    if (_transparency > 0) // transparent instances are added to the RO by the renderer.
                    {
                        //RenderObject = newRO; // set so the renderer knows the intended RO.
                    }
                    else
                    {
                        newRO.Add(this);
                        newRO.UpdateInstance(this);
                    }
                }
            }
        }

        private class PartMotionState : MotionState
        {
            private readonly Part _part;

            public PartMotionState(Part part)
            {
                _part = part;
            }

            public override void GetWorldTransform(out BulletMatrix worldTrans)
            {
                worldTrans = (BulletMatrix)_part.CFrame;
            }

            public override void SetWorldTransform(ref BulletMatrix worldTrans)
            {
                if (worldTrans.M42 < Game.Workspace.VoidHeight)
                {
                    _part.Destroy();
                    return;
                }

                _part._cframe = (CFrame)worldTrans;
                _part._modelMatrix = DirectXUtil.BulletMatrixToSharpDX(_part.RigidBody.InterpolationWorldTransform);
                _part.UpdateRenderData();
                _part.Moved.Fire();
            }
        }

        /// <summary>
        /// Fired when another object comes into contact with this object.
        /// </summary>
        public readonly Signal<Part> Touched;

        /// <summary>
        /// Fired when another object stops touching this object.
        /// </summary>
        public readonly Signal<Part> TouchEnded;

        private PhysicalProperties _physicalProperties;
    }
}