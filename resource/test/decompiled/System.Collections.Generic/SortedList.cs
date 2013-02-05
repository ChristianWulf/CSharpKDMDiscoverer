using System;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
namespace System.Collections.Generic
{
	[DebuggerTypeProxy(typeof(System_DictionaryDebugView<, >)), DebuggerDisplay("Count = {Count}"), ComVisible(false)]
	[Serializable]
	public class SortedList<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IDictionary, ICollection, IEnumerable
	{
		public int Capacity
		{
			get
			{
			}
			set
			{
			}
		}
		public IComparer<TKey> Comparer
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
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
		public IList<TKey> Keys
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
			}
		}
		ICollection<TKey> IDictionary<TKey, TValue>.Keys
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
			}
		}
		ICollection IDictionary.Keys
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
			}
		}
		public IList<TValue> Values
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
			}
		}
		ICollection<TValue> IDictionary<TKey, TValue>.Values
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
			}
		}
		ICollection IDictionary.Values
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
			}
		}
		bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
		{
			get
			{
			}
		}
		bool IDictionary.IsReadOnly
		{
			get
			{
			}
		}
		bool IDictionary.IsFixedSize
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
		public TValue this[TKey key]
		{
			get
			{
			}
			set
			{
			}
		}
		object IDictionary.this[object key]
		{
			get
			{
			}
			set
			{
			}
		}
		public SortedList()
		{
		}
		public SortedList(int capacity)
		{
		}
		public SortedList(IComparer<TKey> comparer)
		{
		}
		public SortedList(int capacity, IComparer<TKey> comparer)
		{
		}
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public SortedList(IDictionary<TKey, TValue> dictionary)
		{
		}
		public SortedList(IDictionary<TKey, TValue> dictionary, IComparer<TKey> comparer)
		{
		}
		public void Add(TKey key, TValue value)
		{
		}
		void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> keyValuePair)
		{
		}
		bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> keyValuePair)
		{
		}
		bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> keyValuePair)
		{
		}
		void IDictionary.Add(object key, object value)
		{
		}
		public void Clear()
		{
		}
		bool IDictionary.Contains(object key)
		{
		}
		public bool ContainsKey(TKey key)
		{
		}
		public bool ContainsValue(TValue value)
		{
		}
		void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
		}
		void ICollection.CopyTo(Array array, int arrayIndex)
		{
		}
		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
		}
		IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
		{
		}
		IDictionaryEnumerator IDictionary.GetEnumerator()
		{
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
		}
		public int IndexOfKey(TKey key)
		{
		}
		public int IndexOfValue(TValue value)
		{
		}
		public bool TryGetValue(TKey key, out TValue value)
		{
		}
		public void RemoveAt(int index)
		{
		}
		public bool Remove(TKey key)
		{
		}
		void IDictionary.Remove(object key)
		{
		}
		public void TrimExcess()
		{
		}
	}
}
