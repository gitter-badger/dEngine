﻿// ObservableDictionary.cs - dEditor
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace dEditor.Utility
{
	[Serializable]
	public class ObservableDictionary<TKey, TValue> :
		IDictionary<TKey, TValue>,
		IDictionary,
		ISerializable,
		IDeserializationCallback,
		INotifyCollectionChanged,
		INotifyPropertyChanged
	{
		#region protected classes

		#region KeyedDictionaryEntryCollection<TKey>

		protected class KeyedDictionaryEntryCollection<TKey> : KeyedCollection<TKey, DictionaryEntry>
		{
			#region methods

			#region protected

			protected override TKey GetKeyForItem(DictionaryEntry entry)
			{
				return (TKey)entry.Key;
			}

			#endregion protected

			#endregion methods

			#region constructors

			#region public

			public KeyedDictionaryEntryCollection()
			{
			}

			public KeyedDictionaryEntryCollection(IEqualityComparer<TKey> comparer) : base(comparer)
			{
			}

			#endregion public

			#endregion constructors
		}

		#endregion KeyedDictionaryEntryCollection<TKey>

		#endregion protected classes

		#region public structures

		#region Enumerator

		[Serializable, StructLayout(LayoutKind.Sequential)]
		public struct Enumerator<TKey, TValue> : IEnumerator<KeyValuePair<TKey, TValue>>, IDisposable,
			IDictionaryEnumerator, IEnumerator
		{
			#region constructors

			internal Enumerator(ObservableDictionary<TKey, TValue> dictionary, bool isDictionaryEntryEnumerator)
			{
				_dictionary = dictionary;
				_version = dictionary._version;
				_index = -1;
				_isDictionaryEntryEnumerator = isDictionaryEntryEnumerator;
				_current = new KeyValuePair<TKey, TValue>();
			}

			#endregion constructors

			#region properties

			#region public

			public KeyValuePair<TKey, TValue> Current
			{
				get
				{
					ValidateCurrent();
					return _current;
				}
			}

			#endregion public

			#endregion properties

			#region methods

			#region public

			public void Dispose()
			{
			}

			public bool MoveNext()
			{
				ValidateVersion();
				_index++;
				if (_index < _dictionary._keyedEntryCollection.Count)
				{
					_current = new KeyValuePair<TKey, TValue>((TKey)_dictionary._keyedEntryCollection[_index].Key,
						(TValue)_dictionary._keyedEntryCollection[_index].Value);
					return true;
				}
				_index = -2;
				_current = new KeyValuePair<TKey, TValue>();
				return false;
			}

			#endregion public

			#region private

			private void ValidateCurrent()
			{
				if (_index == -1)
				{
					throw new InvalidOperationException("The enumerator has not been started.");
				}
				if (_index == -2)
				{
					throw new InvalidOperationException("The enumerator has reached the end of the collection.");
				}
			}

			private void ValidateVersion()
			{
				if (_version != _dictionary._version)
				{
					throw new InvalidOperationException("The enumerator is not valid because the dictionary changed.");
				}
			}

			#endregion private

			#endregion methods

			#region IEnumerator implementation

			object IEnumerator.Current
			{
				get
				{
					ValidateCurrent();
					if (_isDictionaryEntryEnumerator)
					{
						return new DictionaryEntry(_current.Key, _current.Value);
					}
					return new KeyValuePair<TKey, TValue>(_current.Key, _current.Value);
				}
			}

			void IEnumerator.Reset()
			{
				ValidateVersion();
				_index = -1;
				_current = new KeyValuePair<TKey, TValue>();
			}

			#endregion IEnumerator implemenation

			#region IDictionaryEnumerator implemenation

			DictionaryEntry IDictionaryEnumerator.Entry
			{
				get
				{
					ValidateCurrent();
					return new DictionaryEntry(_current.Key, _current.Value);
				}
			}

			object IDictionaryEnumerator.Key
			{
				get
				{
					ValidateCurrent();
					return _current.Key;
				}
			}

			object IDictionaryEnumerator.Value
			{
				get
				{
					ValidateCurrent();
					return _current.Value;
				}
			}

			#endregion

			#region fields

			private readonly ObservableDictionary<TKey, TValue> _dictionary;
			private readonly int _version;
			private int _index;
			private KeyValuePair<TKey, TValue> _current;
			private readonly bool _isDictionaryEntryEnumerator;

			#endregion fields
		}

		#endregion Enumerator

		#endregion public structures

		#region constructors

		#region public

		public ObservableDictionary()
		{
			_keyedEntryCollection = new KeyedDictionaryEntryCollection<TKey>();
		}

		public ObservableDictionary(IDictionary<TKey, TValue> dictionary)
		{
			_keyedEntryCollection = new KeyedDictionaryEntryCollection<TKey>();

			foreach (KeyValuePair<TKey, TValue> entry in dictionary)
				DoAddEntry(entry.Key, entry.Value);
		}

		public ObservableDictionary(IEqualityComparer<TKey> comparer)
		{
			_keyedEntryCollection = new KeyedDictionaryEntryCollection<TKey>(comparer);
		}

		public ObservableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
		{
			_keyedEntryCollection = new KeyedDictionaryEntryCollection<TKey>(comparer);

			foreach (KeyValuePair<TKey, TValue> entry in dictionary)
				DoAddEntry(entry.Key, entry.Value);
		}

		#endregion public

		#region protected

		protected ObservableDictionary(SerializationInfo info, StreamingContext context)
		{
			_siInfo = info;
		}

		#endregion protected

		#endregion constructors

		#region properties

		#region public

		public IEqualityComparer<TKey> Comparer => _keyedEntryCollection.Comparer;

		public int Count => _keyedEntryCollection.Count;

		public Dictionary<TKey, TValue>.KeyCollection Keys => TrueDictionary.Keys;

		public TValue this[TKey key]
		{
			get { return (TValue)_keyedEntryCollection[key].Value; }
			set { DoSetEntry(key, value); }
		}

		public Dictionary<TKey, TValue>.ValueCollection Values => TrueDictionary.Values;

		#endregion public

		#region private

		private Dictionary<TKey, TValue> TrueDictionary
		{
			get
			{
				if (_dictionaryCacheVersion != _version)
				{
					_dictionaryCache.Clear();
					foreach (DictionaryEntry entry in _keyedEntryCollection)
						_dictionaryCache.Add((TKey)entry.Key, (TValue)entry.Value);
					_dictionaryCacheVersion = _version;
				}
				return _dictionaryCache;
			}
		}

		#endregion private

		#endregion properties

		#region methods

		#region public

		public void Add(TKey key, TValue value)
		{
			DoAddEntry(key, value);
		}

		public void Clear()
		{
			DoClearEntries();
		}

		public bool ContainsKey(TKey key)
		{
			return _keyedEntryCollection.Contains(key);
		}

		public bool ContainsValue(TValue value)
		{
			return TrueDictionary.ContainsValue(value);
		}

		public IEnumerator GetEnumerator()
		{
			return new Enumerator<TKey, TValue>(this, false);
		}

		public bool Remove(TKey key)
		{
			return DoRemoveEntry(key);
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			bool result = _keyedEntryCollection.Contains(key);
			value = result ? (TValue)_keyedEntryCollection[key].Value : default(TValue);
			return result;
		}

		#endregion public

		#region protected

		protected virtual bool AddEntry(TKey key, TValue value)
		{
			_keyedEntryCollection.Add(new DictionaryEntry(key, value));
			return true;
		}

		protected virtual bool ClearEntries()
		{
			// check whether there are entries to clear
			bool result = (Count > 0);
			if (result)
			{
				// if so, clear the dictionary
				_keyedEntryCollection.Clear();
			}
			return result;
		}

		protected int GetIndexAndEntryForKey(TKey key, out DictionaryEntry entry)
		{
			entry = new DictionaryEntry();
			int index = -1;
			if (_keyedEntryCollection.Contains(key))
			{
				entry = _keyedEntryCollection[key];
				index = _keyedEntryCollection.IndexOf(entry);
			}
			return index;
		}

		protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
		{
			if (CollectionChanged != null)
				CollectionChanged(this, args);
		}

		protected virtual void OnPropertyChanged(string name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}

		protected virtual bool RemoveEntry(TKey key)
		{
			// remove the entry
			return _keyedEntryCollection.Remove(key);
		}

		protected virtual bool SetEntry(TKey key, TValue value)
		{
			bool keyExists = _keyedEntryCollection.Contains(key);

			// if identical key/value pair already exists, nothing to do
			if (keyExists && value.Equals((TValue)_keyedEntryCollection[key].Value))
				return false;

			// otherwise, remove the existing entry
			if (keyExists)
				_keyedEntryCollection.Remove(key);

			// add the new entry
			_keyedEntryCollection.Add(new DictionaryEntry(key, value));

			return true;
		}

		#endregion protected

		#region private

		private void DoAddEntry(TKey key, TValue value)
		{
			if (AddEntry(key, value))
			{
				_version++;

				DictionaryEntry entry;
				int index = GetIndexAndEntryForKey(key, out entry);
				FireEntryAddedNotifications(entry, index);
			}
		}

		private void DoClearEntries()
		{
			if (ClearEntries())
			{
				_version++;
				FireResetNotifications();
			}
		}

		private bool DoRemoveEntry(TKey key)
		{
			DictionaryEntry entry;
			int index = GetIndexAndEntryForKey(key, out entry);

			bool result = RemoveEntry(key);
			if (result)
			{
				_version++;
				if (index > -1)
					FireEntryRemovedNotifications(entry, index);
			}

			return result;
		}

		private void DoSetEntry(TKey key, TValue value)
		{
			DictionaryEntry entry;
			int index = GetIndexAndEntryForKey(key, out entry);

			if (SetEntry(key, value))
			{
				_version++;

				// if prior entry existed for this key, fire the removed notifications
				if (index > -1)
				{
					FireEntryRemovedNotifications(entry, index);

					// force the property change notifications to fire for the modified entry
					_countCache--;
				}

				// then fire the added notifications
				index = GetIndexAndEntryForKey(key, out entry);
				FireEntryAddedNotifications(entry, index);
			}
		}

		private void FireEntryAddedNotifications(DictionaryEntry entry, int index)
		{
			// fire the relevant PropertyChanged notifications
			FirePropertyChangedNotifications();

			// fire CollectionChanged notification
			OnCollectionChanged(index > -1
				? new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,
					new KeyValuePair<TKey, TValue>((TKey)entry.Key, (TValue)entry.Value), index)
				: new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		private void FireEntryRemovedNotifications(DictionaryEntry entry, int index)
		{
			// fire the relevant PropertyChanged notifications
			FirePropertyChangedNotifications();

			// fire CollectionChanged notification
			OnCollectionChanged(index > -1
				? new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove,
					new KeyValuePair<TKey, TValue>((TKey)entry.Key, (TValue)entry.Value), index)
				: new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		private void FirePropertyChangedNotifications()
		{
			if (Count != _countCache)
			{
				_countCache = Count;
				OnPropertyChanged("Count");
				OnPropertyChanged("Item[]");
				OnPropertyChanged("Keys");
				OnPropertyChanged("Values");
			}
		}

		private void FireResetNotifications()
		{
			// fire the relevant PropertyChanged notifications
			FirePropertyChangedNotifications();

			// fire CollectionChanged notification
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		#endregion private

		#endregion methods

		#region interfaces

		#region IDictionary<TKey, TValue>

		void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
		{
			DoAddEntry(key, value);
		}

		bool IDictionary<TKey, TValue>.Remove(TKey key)
		{
			return DoRemoveEntry(key);
		}

		bool IDictionary<TKey, TValue>.ContainsKey(TKey key)
		{
			return _keyedEntryCollection.Contains(key);
		}

		bool IDictionary<TKey, TValue>.TryGetValue(TKey key, out TValue value)
		{
			return TryGetValue(key, out value);
		}

		ICollection<TKey> IDictionary<TKey, TValue>.Keys => Keys;

		ICollection<TValue> IDictionary<TKey, TValue>.Values => Values;

		TValue IDictionary<TKey, TValue>.this[TKey key]
		{
			get { return (TValue)_keyedEntryCollection[key].Value; }
			set { DoSetEntry(key, value); }
		}

		#endregion IDictionary<TKey, TValue>

		#region IDictionary

		void IDictionary.Add(object key, object value)
		{
			DoAddEntry((TKey)key, (TValue)value);
		}

		void IDictionary.Clear()
		{
			DoClearEntries();
		}

		bool IDictionary.Contains(object key)
		{
			return _keyedEntryCollection.Contains((TKey)key);
		}

		IDictionaryEnumerator IDictionary.GetEnumerator()
		{
			return new Enumerator<TKey, TValue>(this, true);
		}

		bool IDictionary.IsFixedSize => false;

		bool IDictionary.IsReadOnly => false;

		object IDictionary.this[object key]
		{
			get { return _keyedEntryCollection[(TKey)key].Value; }
			set { DoSetEntry((TKey)key, (TValue)value); }
		}

		ICollection IDictionary.Keys => Keys;

		void IDictionary.Remove(object key)
		{
			DoRemoveEntry((TKey)key);
		}

		ICollection IDictionary.Values => Values;

		#endregion IDictionary

		#region ICollection<KeyValuePair<TKey, TValue>>

		void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> kvp)
		{
			DoAddEntry(kvp.Key, kvp.Value);
		}

		void ICollection<KeyValuePair<TKey, TValue>>.Clear()
		{
			DoClearEntries();
		}

		bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> kvp)
		{
			return _keyedEntryCollection.Contains(kvp.Key);
		}

		void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
		{
			if (array == null)
			{
				throw new ArgumentNullException("CopyTo() failed:  array parameter was null");
			}
			if ((index < 0) || (index > array.Length))
			{
				throw new ArgumentOutOfRangeException(
					"CopyTo() failed:  index parameter was outside the bounds of the supplied array");
			}
			if ((array.Length - index) < _keyedEntryCollection.Count)
			{
				throw new ArgumentException("CopyTo() failed:  supplied array was too small");
			}

			foreach (DictionaryEntry entry in _keyedEntryCollection)
				array[index++] = new KeyValuePair<TKey, TValue>((TKey)entry.Key, (TValue)entry.Value);
		}

		int ICollection<KeyValuePair<TKey, TValue>>.Count => _keyedEntryCollection.Count;

		bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

		bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> kvp)
		{
			return DoRemoveEntry(kvp.Key);
		}

		#endregion ICollection<KeyValuePair<TKey, TValue>>

		#region ICollection

		void ICollection.CopyTo(Array array, int index)
		{
			((ICollection)_keyedEntryCollection).CopyTo(array, index);
		}

		int ICollection.Count => _keyedEntryCollection.Count;

		bool ICollection.IsSynchronized => ((ICollection)_keyedEntryCollection).IsSynchronized;

		object ICollection.SyncRoot => ((ICollection)_keyedEntryCollection).SyncRoot;

		#endregion ICollection

		#region IEnumerable<KeyValuePair<TKey, TValue>>

		IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
		{
			return new Enumerator<TKey, TValue>(this, false);
		}

		#endregion IEnumerable<KeyValuePair<TKey, TValue>>

		#region IEnumerable

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion IEnumerable

		#region ISerializable

		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException(nameof(info));
			}

			Collection<DictionaryEntry> entries = new Collection<DictionaryEntry>();
			foreach (DictionaryEntry entry in _keyedEntryCollection)
				entries.Add(entry);
			info.AddValue("entries", entries);
		}

		#endregion ISerializable

		#region IDeserializationCallback

		public virtual void OnDeserialization(object sender)
		{
			if (_siInfo != null)
			{
				Collection<DictionaryEntry> entries = (Collection<DictionaryEntry>)
					_siInfo.GetValue("entries", typeof(Collection<DictionaryEntry>));
				foreach (DictionaryEntry entry in entries)
					AddEntry((TKey)entry.Key, (TValue)entry.Value);
			}
		}

		#endregion IDeserializationCallback

		#region INotifyCollectionChanged

		event NotifyCollectionChangedEventHandler INotifyCollectionChanged.CollectionChanged
		{
			add { CollectionChanged += value; }
			remove { CollectionChanged -= value; }
		}

		protected virtual event NotifyCollectionChangedEventHandler CollectionChanged;

		#endregion INotifyCollectionChanged

		#region INotifyPropertyChanged

		event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
		{
			add { PropertyChanged += value; }
			remove { PropertyChanged -= value; }
		}

		protected virtual event PropertyChangedEventHandler PropertyChanged;

		#endregion INotifyPropertyChanged

		#endregion interfaces

		#region fields

		protected KeyedDictionaryEntryCollection<TKey> _keyedEntryCollection;

		private int _countCache;
		private readonly Dictionary<TKey, TValue> _dictionaryCache = new Dictionary<TKey, TValue>();
		private int _dictionaryCacheVersion;
		private int _version;

		[NonSerialized] private readonly SerializationInfo _siInfo;

		#endregion fields
	}
}