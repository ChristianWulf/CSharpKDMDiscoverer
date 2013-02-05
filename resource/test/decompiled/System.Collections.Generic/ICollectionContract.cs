using System;
namespace System.Collections.Generic
{
	internal class ICollectionContract<T> : ICollection<T>, IEnumerable<T>, IEnumerable
	{
		int ICollection<T>.Count
		{
			get
			{
				return 0;
			}
		}
		bool ICollection<T>.IsReadOnly
		{
			get
			{
				return false;
			}
		}
		void ICollection<T>.Add(T item)
		{
		}
		void ICollection<T>.Clear()
		{
		}
		bool ICollection<T>.Contains(T item)
		{
			return false;
		}
		void ICollection<T>.CopyTo(T[] array, int arrayIndex)
		{
		}
		bool ICollection<T>.Remove(T item)
		{
			return false;
		}
		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return null;
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return null;
		}
	}
}
