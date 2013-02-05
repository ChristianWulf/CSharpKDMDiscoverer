using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
namespace System.Collections.Generic
{
	internal sealed class Mscorlib_KeyedCollectionDebugView<K, T>
	{
		private KeyedCollection<K, T> kc;
		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public T[] Items
		{
			get
			{
				T[] array = new T[this.kc.Count];
				this.kc.CopyTo(array, 0);
				return array;
			}
		}
		public Mscorlib_KeyedCollectionDebugView(KeyedCollection<K, T> keyedCollection)
		{
			if (keyedCollection == null)
			{
				throw new ArgumentNullException("keyedCollection");
			}
			this.kc = keyedCollection;
		}
	}
}
