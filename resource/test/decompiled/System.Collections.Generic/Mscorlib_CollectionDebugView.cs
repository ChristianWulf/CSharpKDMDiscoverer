using System;
using System.Diagnostics;
namespace System.Collections.Generic
{
	internal sealed class Mscorlib_CollectionDebugView<T>
	{
		private ICollection<T> collection;
		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public T[] Items
		{
			get
			{
				T[] array = new T[this.collection.Count];
				this.collection.CopyTo(array, 0);
				return array;
			}
		}
		public Mscorlib_CollectionDebugView(ICollection<T> collection)
		{
			if (collection == null)
			{
				ThrowHelper.ThrowArgumentNullException(ExceptionArgument.collection);
			}
			this.collection = collection;
		}
	}
}
