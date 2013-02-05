using System;
using System.Runtime.InteropServices;
namespace System
{
	[ComVisible(true)]
	[Serializable]
	public delegate void AsyncCallback(IAsyncResult ar);
}
