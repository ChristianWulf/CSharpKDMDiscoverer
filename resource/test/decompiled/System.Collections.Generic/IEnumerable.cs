using System;
using System.Runtime.CompilerServices;
namespace System.Collections.Generic
{
	[TypeDependency("System.SZArrayHelper")]
	public interface IEnumerable<out T> : IEnumerable
	{
		IEnumerator<T> GetEnumerator();
	}
}
