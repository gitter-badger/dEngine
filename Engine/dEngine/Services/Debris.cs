// Debris.cs - dEngine
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
using C5;
using dEngine.Instances;
using dEngine.Instances.Attributes;


namespace dEngine.Services
{
	/// <summary>
	/// The debris service allows objects to be scheduled for removal.
	/// </summary>
	[TypeId(7), ExplorerOrder(10)]
	public class Debris : Service
	{
		internal static Debris Service;

		private readonly IComparer<DebrisToken> _tokenComparer;
		private int _maxItems;
		private SortedArray<DebrisToken> _queue;

		/// <summary />
		public Debris()
		{
			_tokenComparer = new DebrisToken.Comparer();
			MaxItems = 1000;

			RunService.Service.Stepped.Event += Update;
			Service = this;
		}

		/// <summary>
		/// The maximum number of items that can be added before items are forcefully destroyed.
		/// </summary>
		/// <remarks>
		/// Changing this property will destroy the current queue.
		/// </remarks>
		[InstMember(1), EditorVisible("Data")]
		public int MaxItems
		{
			get { return _maxItems; }
			set
			{
				if (value.Equals(_maxItems)) return;
				_maxItems = value;
				while (_queue?.IsEmpty == false)
				{
					var token = _queue.RemoveAt(_queue.Count - 1);
					token.Item.Destroy();
				}
				_queue = new SortedArray<DebrisToken>(_maxItems, _tokenComparer);
				_queue.CollectionCleared += QueueOnCollectionCleared;
				NotifyChanged();
			}
		}

		private void QueueOnCollectionCleared(object sender, ClearedEventArgs eventArgs)
		{
			//throw new NotImplementedException();
		}

		private void QueueOnItemRemovedAt(object sender, ItemAtEventArgs<DebrisToken> eventArgs)
		{
			//throw new NotImplementedException();
		}

		private void Update(double d)
		{
			var now = System.DateTime.Now.Ticks;
			if (_queue.IsEmpty)
				return;
			for (;;)
			{
				var i = _queue.Count - 1;
				var last = _queue[i];
				if (last.Ticks > now)
					break;
				_queue.RemoveAt(i);
				last.Item.Destroy();
				if (i == 0)
					break;
			}
		}

		internal static object GetExisting()
		{
			return DataModel.GetService<Debris>();
		}

		/// <summary>
		/// Schedules an item to be destroyed.
		/// </summary>
		/// <param name="item">The item to register.</param>
		/// <param name="lifetime">The time until the item is destroyed.</param>
		public void AddItem(Instance item, float lifetime = 10)
		{
			var token = new DebrisToken(item, lifetime);
			_queue.Add(token);
		}

		private class DebrisToken
		{
			public DebrisToken(Instance item, float lifetime)
			{
				Item = item;
				Ticks = System.DateTime.Now.AddSeconds(lifetime).Ticks;
			}

			public long Ticks { get; }
			public Instance Item { get; set; }

			public class Comparer : IComparer<DebrisToken>
			{
				public int Compare(DebrisToken x, DebrisToken y)
				{
					return x.Ticks.CompareTo(y.Ticks);
				}
			}
		}
	}
}