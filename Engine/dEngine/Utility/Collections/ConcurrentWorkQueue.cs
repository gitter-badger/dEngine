// ConcurrentWorkQueue.cs - dEngine
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
using System.Text;
using System.Threading;
using C5;

#pragma warning disable 1591

namespace dEngine.Utility
{
	/// <summary>
	/// A concurrent queue which will take all elements every frame and work on them.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class ConcurrentWorkQueue<T>
	{
		private readonly Thread _creationThread;
		private readonly Action<T> _perform;
		private readonly ConcurrentQueue<T> _queue;

		/// <summary>
		/// Creates a work queue.
		/// </summary>
		/// <param name="perform">The action to call for each queue item.</param>
		public ConcurrentWorkQueue(Action<T> perform)
		{
			_queue = new ConcurrentQueue<T>();
			_perform = perform;
			_creationThread = Thread.CurrentThread;
		}

		/// <summary>
		/// Determines if <see cref="Enqueue" /> calls on the same thread as the creation thread should skip the queue.
		/// </summary>
		public bool SameThreadSkipsQueue { get; set; }

		public bool IsEmpty => _queue.IsEmpty;

		/// <inheritdoc />
		public int Count => _queue.Count;

		public void CopyTo(T[] array, int index)
		{
			_queue.CopyTo(array, index);
		}

		public T[] ToArray()
		{
			return _queue.ToArray();
		}

		public event CollectionChangedHandler<T> CollectionChanged;
		public event CollectionClearedHandler<T> CollectionCleared;
		public event ItemsAddedHandler<T> ItemsAdded;
		public event ItemInsertedHandler<T> ItemInserted;
		public event ItemsRemovedHandler<T> ItemsRemoved;
		public event ItemRemovedAtHandler<T> ItemRemovedAt;

		/// <summary>
		/// Adds an item to the end of the queue.
		/// </summary>
		public void Enqueue(T item)
		{
			var currThread = Thread.CurrentThread;
			if (currThread == _creationThread && SameThreadSkipsQueue)
				_perform(item);
			else
				_queue.Enqueue(item);
		}

		public T Dequeue()
		{
			T result;
			_queue.TryDequeue(out result);
			return result;
		}

		public string ToString(string format, IFormatProvider formatProvider)
		{
			return ((IFormattable)_queue).ToString(format, formatProvider);
		}

		public bool Show(StringBuilder stringbuilder, ref int rest, IFormatProvider formatProvider)
		{
			return ((IShowable)_queue).Show(stringbuilder, ref rest, formatProvider);
		}

		/// <summary>
		/// Performs work on the queue.
		/// </summary>
		public void Work()
		{
			if (_queue.IsEmpty)
				return;

			for (int i = 0; i < _queue.Count; i++)
			{
				var item = Dequeue();
				_perform(item);
			}
		}
	}
}