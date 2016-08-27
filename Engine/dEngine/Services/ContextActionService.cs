// ContextActionService.cs - dEngine
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using dEngine.Instances;
using dEngine.Instances.Attributes;

namespace dEngine.Services
{
    /// <summary>
    /// A service for binding context actions.
    /// </summary>
    [TypeId(237), ExplorerOrder(-1)]
    public class ContextActionService : Service
    {
        private static List<ContextBinding> _bindings = new List<ContextBinding>();
        private static ConcurrentDictionary<string, ContextAction> _contextActions = new ConcurrentDictionary<string, ContextAction>();
        private static HashSet<Key> _modifiers = new HashSet<Key>();

        private static readonly object ContextLocker = new object();

        internal static ContextActionService GetExisting()
        {
            return DataModel.GetService<ContextActionService>();
        }

        /// <summary />
        public ContextActionService()
        {
            var inputService = DataModel.GetService<InputService>();
            inputService.InputBegan.Connect(InputServiceOnInputBegan);
            inputService.InputEnded.Connect(InputServiceOnInputEnded);
        }

        internal static IDictionary<string, bool> StateVariables { get; private set; } = new ConcurrentDictionary<string, bool>();

        internal static void DefineState(string state)
        {
            StateVariables[state] = false;
        }

        internal static void SetState(string state, bool value)
        {
            StateVariables[state] = value;
        }

        private void InputServiceOnInputBegan(InputObject obj)
        {
            if (obj.InputType != InputType.Keyboard)
                return;

            lock (ContextLocker)
            {
                switch (obj.Key)
                {
                    case Key.LeftAlt:
                    case Key.RightAlt:
                        _modifiers.Add(Key.LeftAlt);
                        break;
                    case Key.LeftShift:
                    case Key.RightShift:
                        _modifiers.Add(Key.LeftShift);
                        break;
                    case Key.LeftControl:
                    case Key.RightControl:
                        _modifiers.Add(Key.LeftControl);
                        break;
                }

                foreach (var binding in _bindings)
                    if ((binding.Key == obj.Key) &&
                        ((binding.Modifier == Key.Unknown) || _modifiers.Contains(binding.Modifier)))
                        if (binding.When())
                            binding.ContextAction.Action();
            }
        }

        private void InputServiceOnInputEnded(InputObject obj)
        {
            if (obj.InputType != InputType.Keyboard)
                return;

            lock (ContextLocker)
            {
                switch (obj.Key)
                {
                    case Key.LeftAlt:
                    case Key.RightAlt:
                        _modifiers.Remove(Key.LeftAlt);
                        break;
                    case Key.LeftShift:
                    case Key.RightShift:
                        _modifiers.Remove(Key.LeftShift);
                        break;
                    case Key.LeftControl:
                    case Key.RightControl:
                        _modifiers.Remove(Key.LeftControl);
                        break;
                }

                foreach (var binding in _bindings)
                    if ((binding.Key == obj.Key) &&
                        ((binding.Modifier == Key.Unknown) || _modifiers.Contains(binding.Modifier)))
                        if (binding.When())
                            binding.ContextAction?.ActionRelease?.Invoke(false);
            }
        }

        /// <summary>
        /// Determines whether an action with the given name is currently bound.
        /// </summary>
        public bool DoesActionExist(string name)
        {
            return _contextActions.ContainsKey(name);
        }

        /// <summary>
        /// Registers an action with the service.
        /// </summary>
        /// <param name="name">The name of the action.</param>
        /// <param name="action">The function to be called.</param>
        public void RegisterAction(string name, Action action)
        {
            Register(name, action);
        }

        internal static void Register(string name, Action action)
        {
            _contextActions[name] = new ContextAction(name, action);
        }

        internal static void Register(string name, Action<bool> action)
        {
            if (!_contextActions.TryAdd(name, new ContextAction(name, action)))
                throw new Exception($"A context action with the key \"{name}\" was already added.");
        }

        /// <summary>
        /// Binds an action to a context.
        /// </summary>
        /// <param name="action">The name of the action to bind.</param>
        /// <param name="key">The primary key.</param>
        /// <param name="modifier">The modifier key(s).</param>
        /// <param name="when">An function to validate the context.</param>
        public void BindAction(string action, Key key, Key modifier = Key.Unknown, Func<bool> when = null)
        {
            Bind(action, key, modifier, when);
        }

        internal static ContextBinding Bind(string action, Key key, Key modifier = Key.Unknown, Func<bool> when = null)
        {
            lock (ContextLocker)
            {
                var binding = new ContextBinding(action, key, modifier, when);
                _bindings.Add(binding);
                return binding;
            }
        }

        internal static void Unbind(ContextBinding binding)
        {
            lock (ContextLocker)
            {
                _bindings.Remove(binding);
            }
        }

        internal class ContextBinding
        {
            private readonly string _action;
            private ContextAction _contextAction;

            public ContextBinding(string action, Key key, Key modifier, Func<bool> when)
            {
                _action = action;
                Key = key;
                Modifier = modifier;
                When = when ?? (() => true);
            }

            public ContextAction ContextAction
            {
                get
                {
                    if ((_contextAction == null) && !_contextActions.TryGetValue(_action, out _contextAction))
                        throw new Exception($"Cannot get context action \"{_action}\". (For shortcut {Modifier}+{Key})");
                    return _contextAction;
                }
            }

            public readonly Key Key;
            public readonly Key Modifier;
            public readonly Func<bool> When;
        }

        internal class ContextAction : IEquatable<ContextAction>
        {
            public ContextAction(string name, Action action)
            {
                Name = name;
                Action = action;
            }

            public ContextAction(string name, Action<bool> action)
            {
                Name = name;
                Action = () => ActionRelease(true);
                ActionRelease = action;
            }

            public bool Equals(ContextAction other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return string.Equals(Name, other.Name);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != GetType()) return false;
                return Equals((ContextAction)obj);
            }

            public override int GetHashCode()
            {
                return Name?.GetHashCode() ?? 0;
            }

            public readonly string Name;
            public readonly Action Action;
            public readonly Action<bool> ActionRelease;
        }
    }
}