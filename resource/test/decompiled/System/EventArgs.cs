using System;
using System.Runtime.InteropServices;
namespace System
{
	[ComVisible(true)]
	[Serializable]
	public class EventArgs
	{
		public static readonly EventArgs Empty = new EventArgs();
	}
}
