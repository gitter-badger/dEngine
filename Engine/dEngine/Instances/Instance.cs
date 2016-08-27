// Instance.cs - dEngine
// Copyright (C) 2015-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Runtime.CompilerServices;
using System.Threading;
using dEngine.Instances.Attributes;
using dEngine.Serializer.V1;
using dEngine.Services;
using dEngine.Utility;
using Dynamitey;
using JetBrains.Annotations;
using Neo.IronLua;

// ReSharper disable RedundantAssignment

namespace dEngine.Instances
{
    /// <summary>
    /// Base class for all entities.
    /// </summary>
    [TypeId(1)]
    public abstract class Instance : DynamicObject, IEquatable<Instance>, IDisposable
    {
        private static int _objectsCreated;
        private readonly Dictionary<string, List<LuaThread>> _waitForChildList;
        
        /// <summary>
        /// A collection of instances which are parented to this instance.
        /// </summary>
        /// <seealso cref="Parent" />
        internal readonly ChildrenCollection Children;

        internal readonly object Locker = new object();

        private bool _archivable;
        protected bool _deserializing;
        protected bool _serializing;
        private string _instanceId = "";
        private string _name;
        private Instance _parent;
        private bool _selected;
        private IWorld _world;
        internal int ObjectId = _objectsCreated++;

        static Instance()
        {
            // intern common instance names
            string.Intern("Model");
            string.Intern("Part");
            string.Intern("WedgePart");
            string.Intern("Brick");
            string.Intern("Smooth Block Model");
            string.Intern("UnionOperation");
            string.Intern("StaticMesh");
            string.Intern("SkeletalMesh");
            string.Intern("Fire");
            string.Intern("Smoke");
            string.Intern("Decal");
            string.Intern("Sound");
            string.Intern("Humanoid");
            string.Intern("BlockMesh");
            string.Intern("SpecialMesh");
            string.Intern("SpawnLocation");
            string.Intern("Frame");
            string.Intern("SelectionBox");
            string.Intern("Script");
            string.Intern("LocalScript");
            string.Intern("Folder");
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        protected Instance()
        {
            var type = GetType();

            Destroyed = new Signal(this);
            // the destroyed event will be used by Signal, so it needs to be created first.
            Changed = new Signal<string>(this);
            ParentChanged = new Signal<Instance>(this);
            ChildAdded = new Signal<Instance>(this);
            ChildRemoved = new Signal<Instance>(this);
            AncestryChanged = new Signal<Instance, Instance>(this);
            DescendantAdded = new Signal<Instance>(this);
            DescendantRemoving = new Signal<Instance>(this);

            _waitForChildList = new Dictionary<string, List<LuaThread>>();
            Children = new ChildrenCollection();
            
            Name = type.Name;
            _archivable = true;
            IsDestroyed = false;

            if (this is ISingleton)
            {
                var oldId = _instanceId;
                _instanceId = type.Name.ToUpper();
                // ReSharper disable once VirtualMemberCallInConstructor
                OnInstanceIdChanged(_instanceId, oldId);
            }
            else
            {
                InstanceId = InstanceId.Generate();
            }

            DescendantAdded.Event += d => { Parent?.DescendantAdded.Fire(d); };

            DescendantRemoving.Event += d => { Parent?.DescendantAdded.Fire(d); };

            Game.InvokeInstanceAdded(this);
        }

        /// <summary>
        /// The <see cref="IWorld" /> this instance is a descendant of.
        /// </summary>
        public IWorld World
        {
            get { return _world; }
            set
            {
                lock (Locker)
                {
                    if (value == _world) return;
                    var oldWorld = _world;
                    _world = value;
                    OnWorldChanged(value, oldWorld);
                    NotifyChanged();
                }
            }
        }

        /// <summary>
        /// If true, the parent property of this instance will be locked and unchangeable.
        /// </summary>
        internal bool ParentLocked { get; set; }

        /// <summary>
        /// Access children with indexer.
        /// </summary>
        /// <param name="childName">The name of the child.</param>
        /// <returns>The first child with the given name.</returns>
        public dynamic this[string childName] => FindFirstChild(childName) ?? Dynamic.InvokeGet(this, childName);

        /// <summary>
        /// The logger for this instance.
        /// </summary>
        protected ILogger Logger { get; set; }

        /// <summary>
        /// Gets whether or not this object is currently selected by <see cref="SelectionService" />
        /// </summary>
        internal bool IsSelected
        {
            get { return _selected; }
            set
            {
                _selected = value;
                NotifyChanged(nameof(IsSelected));
            }
        }

        /// <summary>
        /// A property for custom data.
        /// </summary>
        /// <remarks>
        /// This property is not serialized.
        /// </remarks>
        [CanBeNull]
        internal object Tag { get; set; }

        /// <summary>
        /// If true, instance has been destroyed and cannot be re-parented.
        /// </summary>
        //[InspectorGroup("Behaviour")]
        internal bool IsDestroyed { get; private set; }

        /// <summary>
        /// The name of the instance type.
        /// </summary>
        [EditorVisible("Data")]
        public string ClassName => GetType().Name;

        /// <summary>
        /// A non-unique identifier for the object.
        /// </summary>
        [InstMember(1), EditorVisible("Data")]
        public string Name
        {
            get { return _name; }
            set
            {
                value = value ?? string.Empty;

                if (value == _name)
                    return;

                _name = string.IsInterned(value) ?? value;
                FulfillChildWaits();
                UpdateLogger();
                NotifyChanged(nameof(Name));
            }
        }

        /// <summary>
        /// The hierarchical parent of the object.
        /// </summary>
        [InstMember(2), EditorVisible("Data"), CanBeNull]
        public Instance Parent
        {
            get { return _parent; }
            set
            {
                if (!OnParentFilter(value))
                    return;

                // Objects cannot be parented to themselves.
                if (value == this)
                {
                    var errMsg = $"Attempt to set parent of {this} to itself.";
                    throw new ParentException(errMsg);
                }

                // Objects cannot be parented to their children.
                if (value != null && value.IsDescendantOf(this))
                {
                    var errMsg =
                        $"Attempt to set parent of {GetFullName()} to {value.GetFullName()} would result in circular reference.";
                    throw new ParentException(errMsg);
                }

                var oldParent = _parent;

                if (oldParent != null)
                {
                    oldParent.Children.Remove(this);
                    oldParent.DescendantRemoving.Fire(this);
                    oldParent.ChildRemoved.Fire(this);
                    oldParent.OnChildRemoved(this);
                }

                _parent = value;
                World = value as IWorld ?? value?.World;

                if (value != null)
                {
                    var result = value.Children.Add(this);
                    if (result)
                    {
                        value.ChildAdded.Fire(this);
                        value.DescendantAdded.Fire(this);
                        value.OnChildAdded(this);

                        FulfillChildWaits();
                    }
                }

                OnAncestryChanged(this, value);
                ParentChanged.Fire(value);

                NotifyChanged(nameof(Parent));
            }
        }

        /// <summary>
        /// Determines whether the object can be serialized.
        /// </summary>
        /// <remarks>
        /// If this is set to false, the object will not be saved by level editors and the <see cref="Clone" /> method will return
        /// null.
        /// </remarks>
        [InstMember(3), EditorVisible("Behaviour")]
        public bool Archivable
        {
            get { return _archivable; }
            set
            {
                if (value == _archivable)
                    return;

                _archivable = value;
                NotifyChanged(nameof(Archivable));
            }
        }

        /// <summary>
        /// A GUID for this instance.
        /// </summary>
        /// <remarks>
        /// InstanceId is serialized for network replication purposes.
        /// If a server and a client open the same file, they should both be able to reference it with the same identifier.
        /// </remarks>
        [InstMember(4)]
        public InstanceId InstanceId
        {
            get { return _instanceId; }
            internal set
            {
                var oldId = _instanceId;

                if (this is ISingleton)
                    return;

                if (Game.Instances.ContainsKey(value))
                {
                    value = InstanceId.Generate();
                }

                _instanceId = value;
                OnInstanceIdChanged(value, oldId);
            }
        }

        void IDisposable.Dispose()
        {
            Destroy();
        }

        /// <summary>
        /// Compares two instances based on their <see cref="InstanceId" />s.
        /// </summary>
        public bool Equals(Instance other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(_instanceId, other._instanceId);
        }

        /// <summary>
        /// Called when <see cref="InstanceId" />. This can happen twice: first when the object is created, second when the object
        /// has been deserialized.
        /// </summary>
        protected virtual void OnInstanceIdChanged(string newId, string oldId)
        {
            Game.Instances.TryRemove(oldId);
            Game.Instances[newId] = new WeakReference<Instance>(this);
        }

        /// <summary>
        /// Called when a child is parented to this instance.
        /// </summary>
        protected virtual void OnChildAdded(Instance child)
        {
        }

        /// <summary>
        /// Called when a child is unparented from this instance.
        /// </summary>
        protected virtual void OnChildRemoved(Instance child)
        {
        }

        /// <summary>
        /// Called when the world changed.
        /// </summary>
        /// <param name="newWorld">The new world.</param>
        /// <param name="oldWorld">The previous world.</param>
        protected virtual void OnWorldChanged(IWorld newWorld, IWorld oldWorld)
        {
        }

        /// <summary>
        /// Called when the ancestry of this part changes.
        /// </summary>
        /// <param name="child">The child of the change.</param>
        /// <param name="parent">The new parent of the child.</param>
        protected virtual void OnAncestryChanged(Instance child, Instance parent)
        {
            UpdateLogger();
            World = Parent as IWorld ?? parent?.World;
            AncestryChanged.Fire(child, parent);
            foreach (var c in Children)
                c.OnAncestryChanged(child, parent);
        }

        //public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Returns the first instance in the ancestry that passes the given predicate.
        /// </summary>
        protected Instance GetAncestor(Func<Instance, bool> predicate)
        {
            Instance parent = Parent;

            while (parent != null)
            {
                if (predicate(parent))
                    return parent;

                parent = parent.Parent;
            }

            return null;
        }

        /// <summary>
        /// Instance destructor.
        /// </summary>
        ~Instance()
        {
            ParentLocked = false;
            Destroy();
        }

        /// <summary>
        /// Returns the name of the instance.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name;
        }

        private void UpdateLogger()
        {
            Logger = LogService.GetLogger("I:" + Name);
        }

        private void FulfillChildWaits()
        {
            List<LuaThread> threads;
            var parent = _parent;
            if (parent == null || !parent._waitForChildList.TryGetValue(_name, out threads)) return;
            foreach (var t in threads)
                ScriptService.ResumeThread(t);
            threads.Clear();
            parent._waitForChildList.Remove(_name);
        }

        /// <summary>
        /// Called by Protobuf-net before serialization.
        /// </summary>
        [InstBeforeSerialization]
        internal virtual void BeforeSerialization(Inst.Context sc)
        {
            _serializing = true;
            Children.Filter = true;
        }

        /// <summary>
        /// Called by Protobuf-net after serialization.
        /// </summary>
        [InstAfterSerialization]
        internal virtual void AfterSerialization(Inst.Context sc)
        {
            Children.Filter = false;
            _serializing = false;
        }

        [InstBeforeDeserialization]
        internal virtual void BeforeDeserialization()
        {
            _deserializing = true;
            ClearChildren();
        }

        /// <summary>
        /// Called by Protobuf-net after deserialization.
        /// </summary>
        [InstAfterDeserialization]
        internal virtual void AfterDeserialization(Inst.Context serializationContext)
        {
            _deserializing = false;
        }

        /// <summary>
        /// Determines if the given parent should be valid.
        /// </summary>
        protected virtual bool OnParentFilter(Instance newParent)
        {
            if (newParent == _parent)
                return false;

            if (this is Workspace)
                return true;

            // ParentLocked objects cannot have their parents changed.
            if (ParentLocked)
            {
                //if (_deserializing)
                //return false;

                var parentPath = _parent != null ? _parent.GetFullName() : "nil";
                var newPath = newParent != null ? newParent.GetFullName() : "nil";
                Logger.Warn(
                    $"The Parent property of {GetFullName()} is locked to {parentPath} Cannot parent to {newPath}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Fires the <see cref="Changed" /> signal to notify that a property has changed.
        /// </summary>
        [NotifyPropertyChangedInvocator]
        protected void NotifyChanged([CallerMemberName] string propertyName = null)
        {
            Changed.Fire(propertyName);
            //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Gets the path to the instance as a string.
        /// </summary>
        /// <example>Workspace.Model.Part</example>
        public string GetFullName()
        {
            var fullName = Name;
            var parent = this;

            while ((parent = parent.Parent) != null && parent != Game.DataModel)
            {
                fullName = $"{parent.Name}.{fullName}";
            }

            return fullName;
        }

        /// <summary>
        /// Yields the current thread until a child with the given name is found.
        /// </summary>
        /// <remarks>The child.</remarks>
        [YieldFunction]
        public Instance WaitForChild(string name, double? timeout = null)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Name parameter cannot be empty.");

            foreach (var c in Children)
            {
                if (c.Name == name)
                    return c;
            }

            var thread = (LuaThread)LuaThread.running().Values[0];

            lock (Locker)
            {
                List<LuaThread> threads;
                if (_waitForChildList.TryGetValue(name, out threads))
                {
                    threads.Add(thread);
                }
                else
                {
                    _waitForChildList.Add(name, new List<LuaThread>(new[] {thread}));
                }
            }

            Timer timer = null;
            var timerAction = new TimerCallback(obj =>
            {
                ScriptService.ResumeThread(thread);
                timer?.Dispose();
            });
            timer = timeout == null ? new Timer(timerAction) : new Timer(timerAction, null, (int)(timeout * 1000), -1);

            ScriptService.YieldThread();

            return FindFirstChild(name);
        }

        /// <summary>
        /// Returns true if this type of instance is, or inherits from the provided type.
        /// </summary>
        /// <param name="type">The type to check/</param>
        public bool IsA(string type)
        {
            if (ClassName == type || type == "<<<ROOT>>")
            {
                return true;
            }

            Inst.CachedType t;

            if (!Inst.TypeDictionary.TryGetValue(type, out t))
            {
                return false;
            }

            return t.Type.IsAssignableFrom(GetType());
        }

        /// <summary>
        /// Returns true if this instance is an ancestor of the given descendant.
        /// </summary>
        public bool IsAncestorOf(Instance descendant)
        {
            return descendant?.IsDescendantOf(this) ?? false;
        }

        /// <summary>
        /// Returns true if this instance has an ancestor of the given type.
        /// </summary>
        internal bool IsDescendantOf<T>() where T : Instance
        {
            var parent = Parent;
            while (parent != null)
            {
                if (parent is T)
                    return true;
                parent = parent.Parent;
            }
            return false;
        }

        /// <summary>
        /// Returns true if this instance is a descendant of the given ancestor.
        /// </summary>
        public bool IsDescendantOf(Instance ancestor)
        {
            if (ancestor == null)
                return Parent == null;

            var parent = Parent;
            while (parent != null)
            {
                if (parent == ancestor)
                    return true;
                parent = parent.Parent;
            }
            return false;
        }

        /// <summary>
        /// Calls <see cref="Destroy" /> on every child.
        /// </summary>
        public void ClearChildren()
        {
            Children.EnterUpgradeableReadLock();
            var count = Children.Count;
            var children = new Instance[count];

            for (int i = 0; i < count; i++)
            {
                children[i] = Children[i];
            }
            Children.ExitUpgradeableReadLock();

            for (int i = 0; i < count; i++)
            {
                var child = children[i];

                if (child.IsDestroyed)
                    continue;

                if (child.ParentLocked)
                    child.ClearChildren();
                else
                    child.Destroy();
            }
        }

        /// <summary>
        /// Returns a list of all children.
        /// </summary>
        public LuaTable GetChildren()
        {
            var table = new LuaTable();
            var children = Children;
            children.EnterReadLock();
            var count = children.Count;
            for (int i = 0; i < count; i++)
            {
                table.SetArrayValue(i + 1, children[i]);
            }
            children.ExitReadLock();
            return table;
        }

        /// <summary>
        /// Returns a copy of the object and its descendants.
        /// </summary>
        /// <remarks>
        /// The copy's parent is initially nil.
        /// If the object cannot be serialized, returns nil.
        /// </remarks>
        /// <returns>A copy of the object, if it can be serialized, otherwise nil.</returns>
        public Instance Clone()
        {
            if (ParentLocked)
                throw new InvalidOperationException($"{GetFullName()} cannot be cloned.");

            if (!Archivable || ParentLocked || !GetType().IsPublic)
                return null;

            return Inst.Clone(this);
        }

        /// <summary>
        /// Sets the <see cref="Parent" /> property to null and locks it, disconnects all events and calls <see cref="Destroy" />
        /// on all children.
        /// </summary>
        /// <remarks>
        /// Instances which use unmanaged objects should override this.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if object is protected.</exception>
        public virtual void Destroy()
        {
            if (IsDestroyed) return;

            if (ParentLocked)
            {
                throw new ParentException(
                    $"Cannot destroy object \"{GetFullName()}\" because it is protected.");
            }

            IsDestroyed = true;
            Parent = null;
            ParentLocked = true;
            Archivable = false;

            if (IsSelected)
                Game.Selection.Deselect(this);

            ClearChildren();

            Game.Instances.TryRemove(InstanceId);
            Game.InvokeInstanceRemoved(this);

            if (IsSelected)
                SelectionService.Service.Deselect(this, false);

            Destroyed.Fire();
            Destroyed.Dispose();

            // ReSharper disable once GCSuppressFinalizeForTypeWithoutDestructor
            GC.SuppressFinalize(true);
        }

        /// <summary>
        /// Returns the first child with the given name.
        /// </summary>
        /// <param name="name">The name of the child.</param>
        /// <param name="recurse">Determines whether to recurse down the tree.</param>
        public Instance FindFirstChild(string name, bool recurse = false)
        {
            foreach (var child in Children)
            {
                if (child.Name == name)
                    return child;
                if (recurse)
                    return child.FindFirstChild(name, true);
            }

            return null;
        }

        /// <summary>
        /// Attempts to get child by name. (i.e. workspace.Part)
        /// </summary>
        /// <param name="binder">The <see cref="GetMemberBinder" /></param>
        /// <param name="result">The child part, if it exists.</param>
        /// <returns>True if a child with the given name was found.</returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var child = FindFirstChild(binder.Name);
            result = child;
            return child != null;
        }

        /// <summary>
        /// Determines if two instances are the same.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Instance)obj);
        }

        /// <summary>
        /// Gets the hashcode of the <see cref="InstanceId" />.
        /// </summary>
        public override int GetHashCode()
        {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return _instanceId.GetHashCode();
        }

        /// <summary>
        /// Returns the instance from a weak reference.
        /// </summary>
        public static implicit operator Instance(WeakReference<Instance> weakRef)
        {
            Instance inst;
            return weakRef.TryGetTarget(out inst) ? inst : null;
        }

        /// <summary>
        /// Fired when the Parent property of the instance or its ancestors changes.
        /// </summary>
        public readonly Signal<Instance, Instance> AncestryChanged;

        /// <summary>
        /// Fired by a property when its value changes.
        /// </summary>
        public readonly Signal<string> Changed;

        /// <summary>
        /// Fired when a new child is added to this instance.
        /// </summary>
        public readonly Signal<Instance> ChildAdded;

        /// <summary>
        /// Fired when a new child is removed from this instance.
        /// </summary>
        public readonly Signal<Instance> ChildRemoved;

        /// <summary>
        /// Fired when a descendant of this instance is added.
        /// </summary>
        public readonly Signal<Instance> DescendantAdded;

        /// <summary>
        /// Fired before a descendant of this instance is removed.
        /// </summary>
        public readonly Signal<Instance> DescendantRemoving;

        /// <summary>
        /// Fired when this instance is destroyed.
        /// </summary>
        public readonly Signal Destroyed;

        /// <summary>
        /// Fired when <see cref="Parent" /> is set.
        /// </summary>
        public readonly Signal<Instance> ParentChanged;
    }
}