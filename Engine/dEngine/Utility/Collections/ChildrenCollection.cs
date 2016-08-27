// ChildrenCollection.cs - dEngine
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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using C5;
using dEngine.Instances;
using dEngine.Services;

#pragma warning disable 1591

namespace dEngine.Utility
{
    [DebuggerDisplay("Count = {Count}")]
    public sealed class ChildrenCollection : IEnumerable<Instance>
	{
		private readonly HashedLinkedList<Instance> _internalCollection = new HashedLinkedList<Instance>();
		private readonly ReaderWriterLockSlim _locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
		
		public int Count => _internalCollection.Count;
		public bool Filter { get; set; }

		public Instance this[int index]
		{
			get
			{
				var wasHeld = _locker.IsReadLockHeld;
				if (!wasHeld)
					_locker.EnterReadLock();
				var result = _internalCollection[index];
				if (!wasHeld)
					_locker.ExitReadLock();
				return result;
			}
		}

		public IEnumerator<Instance> GetEnumerator()
		{
			IEnumerator<Instance> result;
			_locker.EnterReadLock();
			result = new SafeEnumerator<Instance>(_internalCollection.GetEnumerator(), _locker);
			if (Filter)
				result = new FilteredEnumerator(result, i => i.Archivable && i.GetType().IsPublic);
			_locker.ExitReadLock();
			return result;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		internal void EnterReadLock()
		{
			_locker.EnterReadLock();
		}

		internal void ExitReadLock()
		{
			_locker.ExitReadLock();
		}

		internal void EnterWriteLock()
		{
			_locker.EnterWriteLock();
		}

		internal void ExitWriteLock()
		{
			_locker.ExitWriteLock();
		}

		internal void EnterUpgradeableReadLock()
		{
			_locker.EnterUpgradeableReadLock();
		}

		internal void ExitUpgradeableReadLock()
		{
			_locker.ExitUpgradeableReadLock();
        }

        internal void Insert(Instance item, int index)
        {
            _locker.EnterWriteLock();
            if (!_internalCollection.Contains(item))
                _internalCollection.Insert(index, item);
            _locker.ExitWriteLock();
        }

        internal bool Add(Instance instance)
		{
			_locker.EnterWriteLock();
			var result = _internalCollection.Add(instance);
            _locker.ExitWriteLock();
            return result;
		}

		internal bool Remove(Instance instance)
		{
			_locker.EnterWriteLock();
			var result = _internalCollection.Remove(instance);
			_locker.ExitWriteLock();
			return result;
		}

		public class SafeEnumerator<T> : IEnumerator<T>
		{
			private readonly IEnumerator<T> _inner;
			private readonly ReaderWriterLockSlim _locker;

			public SafeEnumerator(IEnumerator<T> inner, ReaderWriterLockSlim locker)
			{
				_inner = inner;
				_locker = locker;
				_locker.EnterReadLock();
			}

			public void Dispose()
			{
				_locker.ExitReadLock();
			}

			public bool MoveNext()
			{
				return _inner.MoveNext();
			}

			public void Reset()
			{
				_inner.Reset();
			}

			public T Current => _inner.Current;

			object IEnumerator.Current => Current;
        }

        public class FilteredEnumerator : IEnumerator<Instance>
        {
            private readonly IEnumerator<Instance> _enumerator;
            private readonly Predicate<Instance> _predicate;

            public FilteredEnumerator(IEnumerator<Instance> enumerator, Predicate<Instance> predicate)
            {
                _enumerator = enumerator;
                _predicate = predicate;
            }

            public void Dispose()
            {
                _enumerator.Dispose();
            }

            public bool MoveNext()
            {
                do
                {
                    if (!_enumerator.MoveNext())
                        return false;
                } while (!_predicate(_enumerator.Current));

                return true;
            }

            public void Reset()
            {
                _enumerator.Reset();
            }

            object IEnumerator.Current => _enumerator.Current;

            Instance IEnumerator<Instance>.Current => _enumerator.Current;
        }
	}

    public class ConverterEnumerator<TSource, TTarget> : IEnumerator<TTarget> where TTarget : TSource
    {
        private readonly IEnumerator<TSource> _enumerator;

        public ConverterEnumerator(IEnumerator<TSource> enumerator)
        {
            _enumerator = enumerator;
        }

        public void Dispose()
        {
            _enumerator.Dispose();
        }

        public bool MoveNext()
        {
            do
            {
                if (!_enumerator.MoveNext())
                    return false;
            } while (!(_enumerator.Current is TTarget));

            return true;
        }

        public void Reset()
        {
            _enumerator.Reset();
        }

        object IEnumerator.Current => _enumerator.Current;

        TTarget IEnumerator<TTarget>.Current => (TTarget)_enumerator.Current;
    }
}