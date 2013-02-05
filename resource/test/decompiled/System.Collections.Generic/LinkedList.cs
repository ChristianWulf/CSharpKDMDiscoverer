using System;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;
namespace System.Collections.Generic
{
	[DebuggerTypeProxy(typeof(System_CollectionDebugView<>)), DebuggerDisplay("Count = {Count}"), ComVisible(false)]
	[Serializable]
	public class LinkedList<T> : ICollection<T>, IEnumerable<T>, ICollection, IEnumerable, ISerializable, IDeserializationCallback
	{
		[Serializable]
		public struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator, ISerializable, IDeserializationCallback
		{
			private LinkedList<T> list;
			private LinkedListNode<T> node;
			private int version;
			private T current;
			private int index;
			private SerializationInfo siInfo;
			private const string LinkedListName = "LinkedList";
			private const string CurrentValueName = "Current";
			private const string VersionName = "Version";
			private const string IndexName = "Index";
			public T Current
			{
				[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
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
			public bool MoveNext()
			{
			}
			void IEnumerator.Reset()
			{
			}
			public void Dispose()
			{
			}
			void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
			{
			}
			void IDeserializationCallback.OnDeserialization(object sender)
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
		public LinkedListNode<T> First
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
			}
		}
		public LinkedListNode<T> Last
		{
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
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public LinkedList()
		{
		}
		public LinkedList(IEnumerable<T> collection)
		{
		}
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		protected LinkedList(SerializationInfo info, StreamingContext context)
		{
		}
		void ICollection<T>.Add(T value)
		{
		}
		public LinkedListNode<T> AddAfter(LinkedListNode<T> node, T value)
		{
		}
		public void AddAfter(LinkedListNode<T> node, LinkedListNode<T> newNode)
		{
		}
		public LinkedListNode<T> AddBefore(LinkedListNode<T> node, T value)
		{
		}
		public void AddBefore(LinkedListNode<T> node, LinkedListNode<T> newNode)
		{
		}
		public LinkedListNode<T> AddFirst(T value)
		{
		}
		public void AddFirst(LinkedListNode<T> node)
		{
		}
		public LinkedListNode<T> AddLast(T value)
		{
		}
		public void AddLast(LinkedListNode<T> node)
		{
		}
		public void Clear()
		{
		}
		public bool Contains(T value)
		{
		}
		public void CopyTo(T[] array, int index)
		{
		}
		public LinkedListNode<T> Find(T value)
		{
		}
		public LinkedListNode<T> FindLast(T value)
		{
		}
		public LinkedList<T>.Enumerator GetEnumerator()
		{
		}
		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
		}
		public bool Remove(T value)
		{
		}
		public void Remove(LinkedListNode<T> node)
		{
		}
		public void RemoveFirst()
		{
		}
		public void RemoveLast()
		{
		}
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
		}
		public virtual void OnDeserialization(object sender)
		{
		}
		void ICollection.CopyTo(Array array, int index)
		{
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
		}
	}
}
