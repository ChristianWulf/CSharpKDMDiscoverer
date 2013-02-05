using System;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
namespace System.Collections.Generic
{
	[DebuggerDisplay("Count = {Count}"), DebuggerTypeProxy(typeof(HashSetDebugView<>))]
	[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
	[Serializable]
	public class HashSet<T> : ISerializable, IDeserializationCallback, ISet<T>, ICollection<T>, IEnumerable<T>, IEnumerable
	{
		[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
		[Serializable]
		public struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator
		{
			private HashSet<T> set;
			private int index;
			private int version;
			private T current;
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
		bool ICollection<T>.IsReadOnly
		{
			get
			{
			}
		}
		public IEqualityComparer<T> Comparer
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
			}
		}
		public HashSet()
		{
		}
		public HashSet(IEqualityComparer<T> comparer)
		{
		}
		public HashSet(IEnumerable<T> collection)
		{
		}
		public HashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer)
		{
		}
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		protected HashSet(SerializationInfo info, StreamingContext context)
		{
		}
		void ICollection<T>.Add(T item)
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
		public bool Remove(T item)
		{
		}
		public HashSet<T>.Enumerator GetEnumerator()
		{
		}
		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
		}
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
		}
		public virtual void OnDeserialization(object sender)
		{
		}
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public bool Add(T item)
		{
		}
		public void UnionWith(IEnumerable<T> other)
		{
		}
		[SecurityCritical]
		public void IntersectWith(IEnumerable<T> other)
		{
		}
		public void ExceptWith(IEnumerable<T> other)
		{
		}
		[SecurityCritical]
		public void SymmetricExceptWith(IEnumerable<T> other)
		{
		}
		[SecurityCritical]
		public bool IsSubsetOf(IEnumerable<T> other)
		{
		}
		[SecurityCritical]
		public bool IsProperSubsetOf(IEnumerable<T> other)
		{
		}
		public bool IsSupersetOf(IEnumerable<T> other)
		{
		}
		[SecurityCritical]
		public bool IsProperSupersetOf(IEnumerable<T> other)
		{
		}
		public bool Overlaps(IEnumerable<T> other)
		{
		}
		[SecurityCritical]
		public bool SetEquals(IEnumerable<T> other)
		{
		}
		public void CopyTo(T[] array)
		{
		}
		public void CopyTo(T[] array, int arrayIndex, int count)
		{
		}
		public int RemoveWhere(Predicate<T> match)
		{
		}
		public void TrimExcess()
		{
		}
		public static IEqualityComparer<HashSet<T>> CreateSetComparer()
		{
		}
	}
}
