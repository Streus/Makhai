using System;
using System.Collections;
using System.Collections.Generic;

namespace Makhai.Core
{
	/// <summary>
	/// Specialized data-structure that allows for quick lookup via
	/// position or name field.
	/// </summary>
	public class Hotbar<T> : IList<T>
		where T : INamedItem
	{
		#region INSTANCE_VARS

		private T[] dataArray;
		private int[] nameArray;
		private int size;

		public T this[int index]
		{
			get
			{
				return dataArray[index];
			}
			set
			{
				dataArray[index] = value;
			}
		}

		public int Count { get { return size; } }
		public bool IsReadOnly { get { return false; } }
		#endregion

		#region INSTANCE_METHODS


		public void Add(T item)
		{
			throw new NotImplementedException ();
		}

		public void Clear()
		{
			throw new NotImplementedException ();
		}

		public bool Contains(T item)
		{
			throw new NotImplementedException ();
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			throw new NotImplementedException ();
		}

		public IEnumerator<T> GetEnumerator()
		{
			throw new NotImplementedException ();
		}

		public int IndexOf(T item)
		{
			throw new NotImplementedException ();
		}

		public void Insert(int index, T item)
		{
			throw new NotImplementedException ();
		}

		public bool Remove(T item)
		{
			throw new NotImplementedException ();
		}

		public void RemoveAt(int index)
		{
			throw new NotImplementedException ();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			throw new NotImplementedException ();
		}
		#endregion
	}
}
