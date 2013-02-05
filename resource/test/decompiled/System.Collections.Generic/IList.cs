using System;
using System.Runtime.CompilerServices;
namespace System.Collections.Generic
{
	[TypeDependency("System.SZArrayHelper")]
	public interface IList<T> : ICollection<T>, IEnumerable<T>, IEnumerable
	{
		T this[int index]
		{
			get;
			set;
		}
		int IndexOf(T item);
		void Insert(int index, T item);
		void RemoveAt(int index);
	}
}
