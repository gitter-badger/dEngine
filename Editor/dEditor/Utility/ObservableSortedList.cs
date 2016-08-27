// ObservableSortedList.cs - dEditor
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
using System.Collections.ObjectModel;

namespace dEditor.Utility
{
	public class SortedObservableCollection<T>
		: ObservableCollection<T> where T : IComparable<T>
	{
		private const string _InsertErrorMessage
			= "Inserting and moving an item using an explicit index are not support by sorted observable collection";

		protected override void InsertItem(int index, T item)
		{
			if (Count == 0)
			{
				base.InsertItem(0, item);
				return;
			}

			index = Compare(item, 0, Count - 1);

			base.InsertItem(index, item);
		}

		private int Compare(T item, int lowIndex, int highIndex)
		{
			int compareIndex = (lowIndex + highIndex) / 2;

			if (compareIndex == 0)
			{
				return SearchIndexByIteration(lowIndex, highIndex, item);
			}

			int result = item.CompareTo(this[compareIndex]);

			if (result < 0)
			{
				//item precedes indexed obj in the sort order

				if ((lowIndex + compareIndex) < 100 || compareIndex == (lowIndex + compareIndex) / 2)
				{
					return SearchIndexByIteration(lowIndex, compareIndex, item);
				}

				return Compare(item, lowIndex, compareIndex);
			}

			if (result > 0)
			{
				//item follows indexed obj in the sort order

				if ((compareIndex + highIndex) < 100 || compareIndex == (compareIndex + highIndex) / 2)
				{
					return SearchIndexByIteration(compareIndex, highIndex, item);
				}

				return Compare(item, compareIndex, highIndex);
			}

			return compareIndex;
		}

		/// <summary>
		/// Iterates through sequence of the collection from low to high index
		/// and returns the index where to insert the new item
		/// </summary>
		private int SearchIndexByIteration(int lowIndex, int highIndex, T item)
		{
			for (int i = lowIndex; i <= highIndex; i++)
			{
				if (item.CompareTo(this[i]) < 0)
				{
					return i;
				}
			}
			return Count;
		}

		/// <summary>
		/// Adds the item to collection by ignoring the index
		/// </summary>
		protected override void SetItem(int index, T item)
		{
			InsertItem(index, item);
		}

		/// <summary>
		/// Throws an error because inserting an item using an explicit index
		/// is not support by sorted observable collection
		/// </summary>
		[Obsolete(_InsertErrorMessage)]
		public new void Insert(int index, T item)
		{
			throw new NotSupportedException(_InsertErrorMessage);
		}

		/// <summary>
		/// Throws an error because moving an item using explicit indexes
		/// is not support by sorted observable collection
		/// </summary>
		[Obsolete(_InsertErrorMessage)]
		public new void Move(int oldIndex, int newIndex)
		{
			throw new NotSupportedException(_InsertErrorMessage);
		}
	}
}