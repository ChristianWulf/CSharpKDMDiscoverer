using System;
using System.Diagnostics;
namespace System.Collections.Generic
{
	internal sealed class Mscorlib_DictionaryValueCollectionDebugView<TKey, TValue>
	{
		private ICollection<TValue> collection;
		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public TValue[] Items
		{
			get
			{
				TValue[] array = new TValue[this.collection.Count];
				this.collection.CopyTo(array, 0);
				return array;
			}
		}
		public Mscorlib_DictionaryValueCollectionDebugView(ICollection<TValue> collection)
		{
			if (collection == null)
			{
				ThrowHelper.ThrowArgumentNullException(ExceptionArgument.collection);
			}
			this.collection = collection;
		}
	}
}
