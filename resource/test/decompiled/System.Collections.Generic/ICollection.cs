using System;
using System.Runtime.CompilerServices;
namespace System.Collections.Generic
{
	[TypeDependency("System.SZArrayHelper")]
	public interface ICollection<T> : IEnumerable<T>, IEnumerable
	{
		int Count
		{
			get;
		}
		bool IsReadOnly
		{
			get;
		}
		void Add(T item);
		void Clear();
		bool Contains(T item);
		void CopyTo(T[] array, int arrayIndex);
		bool Remove(T item);
	}
}
