using System;
namespace System.Collections.Generic
{
	public interface IEnumerator<out T> : IDisposable, IEnumerator
	{
		T Current
		{
			get;
		}
	}
}
