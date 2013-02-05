using System;
using System.Diagnostics;
using System.Runtime;
namespace System.Collections.Generic
{
	[DebuggerTypeProxy(typeof(System_DictionaryDebugView<, >)), DebuggerDisplay("Count = {Count}")]
	[Serializable]
	public class SortedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IDictionary, ICollection, IEnumerable
	{
		public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IDisposable, IDictionaryEnumerator, IEnumerator
		{
			private SortedSet<KeyValuePair<TKey, TValue>>.Enumerator treeEnum;
			private int getEnumeratorRetType;
			internal const int KeyValuePair = 1;
			internal const int DictEntry = 2;
			public KeyValuePair<TKey, TValue> Current
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
			object IDictionaryEnumerator.Key
			{
				get
				{
				}
			}
			object IDictionaryEnumerator.Value
			{
				get
				{
				}
			}
			DictionaryEntry IDictionaryEnumerator.Entry
			{
				get
				{
				}
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
		[DebuggerTypeProxy(typeof(System_DictionaryKeyCollectionDebugView<, >)), DebuggerDisplay("Count = {Count}")]
		[Serializable]
		public sealed class KeyCollection : ICollection<TKey>, IEnumerable<TKey>, ICollection, IEnumerable
		{
			public struct Enumerator : IEnumerator<TKey>, IDisposable, IEnumerator
			{
				private SortedDictionary<TKey, TValue>.Enumerator dictEnum;
				public TKey Current
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
				get
				{
				}
			}
			bool ICollection<TKey>.IsReadOnly
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
			public KeyCollection(SortedDictionary<TKey, TValue> dictionary)
			{
			}
			public SortedDictionary<TKey, TValue>.KeyCollection.Enumerator GetEnumerator()
			{
			}
			IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator()
			{
			}
			IEnumerator IEnumerable.GetEnumerator()
			{
			}
			public void CopyTo(TKey[] array, int index)
			{
			}
			void ICollection.CopyTo(Array array, int index)
			{
			}
			void ICollection<TKey>.Add(TKey item)
			{
			}
			void ICollection<TKey>.Clear()
			{
			}
			bool ICollection<TKey>.Contains(TKey item)
			{
			}
			bool ICollection<TKey>.Remove(TKey item)
			{
			}
		}
		[DebuggerDisplay("Count = {Count}"), DebuggerTypeProxy(typeof(System_DictionaryValueCollectionDebugView<, >))]
		[Serializable]
		public sealed class ValueCollection : ICollection<TValue>, IEnumerable<TValue>, ICollection, IEnumerable
		{
			public struct Enumerator : IEnumerator<TValue>, IDisposable, IEnumerator
			{
				private SortedDictionary<TKey, TValue>.Enumerator dictEnum;
				public TValue Current
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
				get
				{
				}
			}
			bool ICollection<TValue>.IsReadOnly
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
			public ValueCollection(SortedDictionary<TKey, TValue> dictionary)
			{
			}
			public SortedDictionary<TKey, TValue>.ValueCollection.Enumerator GetEnumerator()
			{
			}
			IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
			{
			}
			IEnumerator IEnumerable.GetEnumerator()
			{
			}
			public void CopyTo(TValue[] array, int index)
			{
			}
			void ICollection.CopyTo(Array array, int index)
			{
			}
			void ICollection<TValue>.Add(TValue item)
			{
			}
			void ICollection<TValue>.Clear()
			{
			}
			bool ICollection<TValue>.Contains(TValue item)
			{
			}
			bool ICollection<TValue>.Remove(TValue item)
			{
			}
		}
		bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
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
		public int Count
		{
			get
			{
			}
		}
		public IComparer<TKey> Comparer
		{
			get
			{
			}
		}
		public SortedDictionary<TKey, TValue>.KeyCollection Keys
		{
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
		public SortedDictionary<TKey, TValue>.ValueCollection Values
		{
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
		bool IDictionary.IsFixedSize
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
		ICollection IDictionary.Keys
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
		object IDictionary.this[object key]
		{
			get
			{
			}
			set
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
		public SortedDictionary()
		{
		}
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public SortedDictionary(IDictionary<TKey, TValue> dictionary)
		{
		}
		public SortedDictionary(IDictionary<TKey, TValue> dictionary, IComparer<TKey> comparer)
		{
		}
		public SortedDictionary(IComparer<TKey> comparer)
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
		public void Add(TKey key, TValue value)
		{
		}
		public void Clear()
		{
		}
		public bool ContainsKey(TKey key)
		{
		}
		public bool ContainsValue(TValue value)
		{
		}
		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
		{
		}
		public SortedDictionary<TKey, TValue>.Enumerator GetEnumerator()
		{
		}
		IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
		{
		}
		public bool Remove(TKey key)
		{
		}
		public bool TryGetValue(TKey key, out TValue value)
		{
		}
		void ICollection.CopyTo(Array array, int index)
		{
		}
		void IDictionary.Add(object key, object value)
		{
		}
		bool IDictionary.Contains(object key)
		{
		}
		IDictionaryEnumerator IDictionary.GetEnumerator()
		{
		}
		void IDictionary.Remove(object key)
		{
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
		}
	}
}
