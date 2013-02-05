using System;
using System.Runtime;
using System.Runtime.InteropServices;
namespace System.Collections.Generic
{
	[ComVisible(false)]
	public sealed class LinkedListNode<T>
	{
		public LinkedList<T> List
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
			}
		}
		public LinkedListNode<T> Next
		{
			get
			{
			}
		}
		public LinkedListNode<T> Previous
		{
			get
			{
			}
		}
		public T Value
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
			}
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			set
			{
			}
		}
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public LinkedListNode(T value)
		{
		}
	}
}
