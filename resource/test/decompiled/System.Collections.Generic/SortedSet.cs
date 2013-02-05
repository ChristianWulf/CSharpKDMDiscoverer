using System;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.Serialization;
namespace System.Collections.Generic
{
	[DebuggerTypeProxy(typeof(SortedSetDebugView<>)), DebuggerDisplay("Count = {Count}")]
	[Serializable]
	public class SortedSet<T> : ISet<T>, ICollection<T>, IEnumerable<T>, ICollection, IEnumerable, ISerializable, IDeserializationCallback
	{
		internal class Node
		{
		}
		[Serializable]
		public struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator, ISerializable, IDeserializationCallback
		{
			private SortedSet<T> tree;
			private int version;
			private Stack<SortedSet<T>.Node> stack;
			private SortedSet<T>.Node current;
			private bool reverse;
			private SerializationInfo siInfo;
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
			void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
			{
			}
			private void GetObjectData(SerializationInfo info, StreamingContext context)
			{
			}
			void IDeserializationCallback.OnDeserialization(object sender)
			{
			}
			private void OnDeserialization(object sender)
			{
			}
			public bool MoveNext()
			{
			}
			public void Dispose()
			{
			}
			internal void Reset()
			{
			}
			void IEnumerator.Reset()
			{
			}
		}
		public int Count
		{
			get
			{
			}
		}
		public IComparer<T> Comparer
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
			}
		}
		bool ICollection<T>.IsReadOnly
		{
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
		public T Min
		{
			get
			{
			}
		}
		public T Max
		{
			get
			{
			}
		}
		public SortedSet()
		{
		}
		public SortedSet(IComparer<T> comparer)
		{
		}
		public SortedSet(IEnumerable<T> collection)
		{
		}
		public SortedSet(IEnumerable<T> collection, IComparer<T> comparer)
		{
		}
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		protected SortedSet(SerializationInfo info, StreamingContext context)
		{
		}
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public bool Add(T item)
		{
		}
		void ICollection<T>.Add(T item)
		{
		}
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public bool Remove(T item)
		{
		}
		public virtual void Clear()
		{
		}
		public virtual bool Contains(T item)
		{
		}
		public void CopyTo(T[] array)
		{
		}
		public void CopyTo(T[] array, int index)
		{
		}
		public void CopyTo(T[] array, int index, int count)
		{
		}
		void ICollection.CopyTo(Array array, int index)
		{
		}
		public SortedSet<T>.Enumerator GetEnumerator()
		{
		}
		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
		}
		public static IEqualityComparer<SortedSet<T>> CreateSetComparer()
		{
		}
		public static IEqualityComparer<SortedSet<T>> CreateSetComparer(IEqualityComparer<T> memberEqualityComparer)
		{
		}
		public void UnionWith(IEnumerable<T> other)
		{
		}
		public virtual void IntersectWith(IEnumerable<T> other)
		{
		}
		public void ExceptWith(IEnumerable<T> other)
		{
		}
		public void SymmetricExceptWith(IEnumerable<T> other)
		{
		}
		public bool IsSubsetOf(IEnumerable<T> other)
		{
		}
		public bool IsProperSubsetOf(IEnumerable<T> other)
		{
		}
		public bool IsSupersetOf(IEnumerable<T> other)
		{
		}
		public bool IsProperSupersetOf(IEnumerable<T> other)
		{
		}
		public bool SetEquals(IEnumerable<T> other)
		{
		}
		public bool Overlaps(IEnumerable<T> other)
		{
		}
		public int RemoveWhere(Predicate<T> match)
		{
		}
		public IEnumerable<T> Reverse()
		{
		}
		public virtual SortedSet<T> GetViewBetween(T lowerValue, T upperValue)
		{
		}
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
		}
		protected virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
		}
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		void IDeserializationCallback.OnDeserialization(object sender)
		{
		}
		protected virtual void OnDeserialization(object sender)
		{
		}
	}
}
