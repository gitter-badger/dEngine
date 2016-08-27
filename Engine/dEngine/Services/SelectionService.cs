// SelectionService.cs - dEngine
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using C5;
using dEngine.Instances;
using dEngine.Instances.Attributes;
using dEngine.Utility;
using Neo.IronLua;


namespace dEngine.Services
{
    /// <summary>
    /// A service for managing selection in level editors.
    /// </summary>
    [LevelEditorRelated]
    [TypeId(63), ExplorerOrder(-1)]
    public partial class SelectionService : Service
    {
        /// <summary>
        /// Fired for every item removed in a Deselect() call.
        /// </summary>
        public readonly Signal<Instance> Deselected;

        /// <summary>
        /// Fired for every item added in a Select() call.
        /// </summary>
        public readonly Signal<Instance> Selected;

        /// <summary>
        /// Fired when the selection is changed.
        /// </summary>
        public readonly Signal SelectionChanged;

        /// <inheritdoc />
        public SelectionService()
        {
            Service = this;
            Selected = new Signal<Instance>(this);
            Deselected = new Signal<Instance>(this);
            SelectionChanged = new Signal(this);
        }

        /// <summary>
        /// Returns a table of selected items.
        /// </summary>
        public LuaTable GetSelection()
        {
            lock (Locker)
            {
                return Selection.ToLuaTable();
            }
        }

        /// <summary>
        /// Deselects all items.
        /// </summary>
        public void ClearSelection()
        {
            foreach (var item in Selection.ToList())
            {
                _selection.TryRemove(item);
                item.IsSelected = false;
                Deselected.Fire(item);
            }

            SelectionChanged.Fire();
        }

        /// <summary>
        /// Adds instances to the selection list.
        /// </summary>
        /// <param name="item">The item to select.</param>
        /// <param name="clearCurrentSelection">Determines if the current selection will be cleared.</param>
        /// <param name="pushToHistory">Determines if an action is pushed to the <see cref="HistoryService" /></param>
        public void Select(Instance item, bool clearCurrentSelection = false, bool pushToHistory = true)
        {
            if (clearCurrentSelection)
                ClearSelection();

            if (item == null || item.IsSelected)
                return;

            _selection.TryAdd(item);
            item.IsSelected = true;
            Selected.Fire(item);
            SelectionChanged.Fire();
            var element = item as GuiElement;
            if (element != null)
            {
                lock (Locker)
                {
                    SelectedGuiElements.Add(element);
                }
            }

            if (pushToHistory)
                HistoryService.ExecuteAction(new SelectionAction(item, false));
        }

        /// <summary>
        /// Removes instances from the selection list.
        /// </summary>
        /// <param name="item">The item to deselect.</param>
        /// <param name="pushToHistory">Determines if an action is pushed to the <see cref="HistoryService" /></param>
        public void Deselect(Instance item, bool pushToHistory = true)
        {
            if (item == null || !item.IsSelected)
                return;

            _selection.TryRemove(item);
            item.IsSelected = false;
            Deselected.Fire(item);
            SelectionChanged.Fire();

            var element = item as GuiElement;
            if (element != null)
            {
                lock (Locker)
                {
                    SelectedGuiElements.Add(element);
                }
            }

            if (pushToHistory)
                HistoryService.ExecuteAction(new SelectionAction(item, true));
        }

        private class SelectionAction : HistoryService.HistoryAction
        {
            private readonly bool _invert;
            private readonly Instance _item;

            public SelectionAction(Instance item, bool invert)
            {
                _item = item;
                _invert = invert;
            }

            public override void Undo()
            {
                if (_invert)
                    Service.Select(_item, false);
                else
                    Service.Deselect(_item, false);
            }

            public override void Execute()
            {
                if (_invert)
                    Service.Deselect(_item, false);
                else
                    Service.Select(_item, false);
            }
        }
    }

    public partial class SelectionService
    {
        internal static SelectionService Service;
        private static readonly ReaderWriterLockSlim _locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        private static readonly ConcurrentDictionary<Instance, byte> _selection = new ConcurrentDictionary<Instance, byte>();

        internal static IEnumerable<Instance> Selection => _selection.Keys;
        internal static C5.HashSet<GuiElement> SelectedGuiElements = new C5.HashSet<GuiElement>();
        internal static int SelectionCount => _selection.Count;

        internal static IEnumerable<T> Select<T>(Func<Instance, T> selector)
        {
            _locker.EnterReadLock();
            var result = Selection.Select(selector);
            _locker.ExitReadLock();
            return result;
        }

        internal static Instance First()
        {
            _locker.EnterReadLock();
            var result = Selection.FirstOrDefault();
            _locker.ExitReadLock();
            return result;
        }

        internal static Instance First(Func<Instance, bool> predicate)
        {
            _locker.EnterReadLock();
            var result = Selection.FirstOrDefault(predicate);
            _locker.ExitReadLock();
            return result;
        }

        internal static Instance Last()
        {
            _locker.EnterReadLock();
            var result = Selection.LastOrDefault();
            _locker.ExitReadLock();
            return result;
        }

        internal static bool All(Func<Instance, bool> predicate)
        {
            _locker.EnterReadLock();
            var result = Selection.All(predicate);
            _locker.ExitReadLock();
            return result;
        }

        internal static bool Any(Func<Instance, bool> predicate)
        {
            _locker.EnterReadLock();
            var result = Selection.Any(predicate);
            _locker.ExitReadLock();
            return result;
        }

        internal static bool Any()
        {
            return SelectionCount > 0;
        }

        internal static TValue Unanimous<TValue>(Func<Instance, TValue> getter) where TValue : class
        {
            TValue first = null;

            using (var enumerator = Selection.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var item = enumerator.Current;
                    if (first == null)
                        first = getter(item);
                    else if (getter(item) != first)
                        return null;
                }
            }

            return first;
        }

        /// <summary>
        /// Returns all selected items of the given type.
        /// </summary>
        public static IEnumerable<T> OfType<T>()
        {
            _locker.EnterReadLock();
            var results = _selection.OfType<T>();
            _locker.ExitReadLock();
            return results;
        }

        /// <summary>
        /// Performs the given function on each selected <see cref="PVInstance" />.
        /// </summary>
        public static void ForEachPV(Action<PVInstance> func)
        {
            using (var enumerator = Selection.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var pv = enumerator.Current as PVInstance;
                    if (pv != null)
                        func(pv);
                }
            }
        }

        /// <summary>
        /// Performs the given function on each selected item.
        /// </summary>
        public static void ForEach(Action<Instance> func)
        {
            using (var enumerator = Selection.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var item = enumerator.Current;
                    func(item);
                }
            }
        }

        /// <summary>
        /// Returns all items which patch the predicate.
        /// </summary>
        public static IEnumerable<Instance> Where(Func<Instance, bool> func)
        {
            _locker.EnterReadLock();
            var results = Selection.Where(func);
            _locker.ExitReadLock();
            return results;
        }

        internal static object GetExisting()
        {
            return DataModel.GetService<SelectionService>();
        }

        internal static List<Instance> ToList()
        {
            _locker.EnterReadLock();
            var results = Selection.ToList();
            _locker.ExitReadLock();
            return results;
        }
    }
}