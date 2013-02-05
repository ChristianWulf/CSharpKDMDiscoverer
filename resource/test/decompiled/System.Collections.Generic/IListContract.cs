using System;
namespace System.Collections.Generic
{
	internal abstract class IListContract<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable
	{
		T IList<T>.this[int index]
		{
			get
			{
				return default(T);
			}
			set
			{
			}
		}
		bool ICollection<T>.IsReadOnly
		{
			get
			{
				return false;
			}
		}
		int ICollection<T>.Count
		{
			get
			{
				return 0;
			}
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return null;
		}
		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return null;
		}
		int IList<T>.IndexOf(T value)
		{
			return 0;
		}
		void IList<T>.Insert(int index, T value)
		{
		}
		void IList<T>.RemoveAt(int index)
		{
		}
		void ICollection<T>.Add(T value)
		{
		}
		void ICollection<T>.Clear()
		{
		}
		bool ICollection<T>.Contains(T value)
		{
			return false;
		}
		void ICollection<T>.CopyTo(T[] array, int startIndex)
		{
		}
		bool ICollection<T>.Remove(T value)
		{
			return false;
		}
	}
}
