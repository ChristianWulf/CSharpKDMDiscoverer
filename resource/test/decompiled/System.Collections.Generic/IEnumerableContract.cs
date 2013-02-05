using System;
namespace System.Collections.Generic
{
	internal class IEnumerableContract<T> : IEnumerableContract, IEnumerable<T>, IEnumerable
	{
		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return null;
		}
	}
}
