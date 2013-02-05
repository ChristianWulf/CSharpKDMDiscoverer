using System;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
namespace System.Collections.Generic
{
	[DebuggerDisplay("Count = {Count}"), DebuggerTypeProxy(typeof(System_StackDebugView<>)), ComVisible(false)]
	[Serializable]
	public class Stack<T> : IEnumerable<T>, ICollection, IEnumerable
	{
		[Serializable]
		public struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator
		{
			private Stack<T> _stack;
			private int _index;
			private int _version;
			private T currentElement;
			public T Current
			{
				get
				{
				}
			}
			object IEnumerator.Current
			{
				get
				{
				}
			}
			public void Dispose()
			{
			}
			public bool MoveNext()
			{
			}
			void IEnumerator.Reset()
			{
			}
		}
		public int Count
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
			}
		}
		bool ICollection.IsSynchronized
		{
			get
			{
			}
		}
		object ICollection.SyncRoot
		{
			get
			{
			}
		}
		public Stack()
		{
		}
		public Stack(int capacity)
		{
		}
		public Stack(IEnumerable<T> collection)
		{
		}
		public void Clear()
		{
		}
		public bool Contains(T item)
		{
		}
		public void CopyTo(T[] array, int arrayIndex)
		{
		}
		void ICollection.CopyTo(Array array, int arrayIndex)
		{
		}
		public Stack<T>.Enumerator GetEnumerator()
		{
		}
		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
		}
		public void TrimExcess()
		{
		}
		public T Peek()
		{
		}
		public T Pop()
		{
		}
		public void Push(T item)
		{
		}
		public T[] ToArray()
		{
		}
	}
}
