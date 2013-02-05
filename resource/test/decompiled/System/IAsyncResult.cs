using System;
using System.Runtime.InteropServices;
using System.Threading;
namespace System
{
	[ComVisible(true)]
	public interface IAsyncResult
	{
		bool IsCompleted
		{
			get;
		}
		WaitHandle AsyncWaitHandle
		{
			get;
		}
		object AsyncState
		{
			get;
		}
		bool CompletedSynchronously
		{
			get;
		}
	}
}
