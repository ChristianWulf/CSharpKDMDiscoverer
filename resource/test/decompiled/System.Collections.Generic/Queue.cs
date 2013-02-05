using System;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
namespace System.Collections.Generic
{
	[DebuggerTypeProxy(typeof(System_QueueDebugView<>)), ComVisible(false), DebuggerDisplay("Count = {Count}")]
	[Serializable]
	public class Queue<T> : IEnumerable<T>, ICollection, IEnumerable
	{
		[Serializable]
		public struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator
		{
			private Queue<T> _q;
			private int _index;
			private int _version;
			private T _currentElement;
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
		public Queue()
		{
		}
		public Queue(int capacity)
		{
		}
		public Queue(IEnumerable<T> collection)
		{
		}
		public void Clear()
		{
		}
		public void CopyTo(T[] array, int arrayIndex)
		{
		}
		void ICollection.CopyTo(Array array, int index)
		{
		}
		public void Enqueue(T item)
		{
		}
		public Queue<T>.Enumerator GetEnumerator()
		{
		}
		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
		}
		public T Dequeue()
		{
		}
		public T Peek()
		{
		}
		public bool Contains(T item)
		{
		}
		public T[] ToArray()
		{
		}
		public void TrimExcess()
		{
		}
	}
}
