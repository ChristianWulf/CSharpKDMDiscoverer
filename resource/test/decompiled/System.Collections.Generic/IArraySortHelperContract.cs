using System;
using System.Diagnostics.Contracts;
namespace System.Collections.Generic
{
	internal abstract class IArraySortHelperContract<TKey> : IArraySortHelper<TKey>
	{
		void IArraySortHelper<TKey>.Sort(TKey[] keys, int index, int length, IComparer<TKey> comparer)
		{
		}
		int IArraySortHelper<TKey>.BinarySearch(TKey[] keys, int index, int length, TKey value, IComparer<TKey> comparer)
		{
			return Contract.Result<int>();
		}
	}
}
