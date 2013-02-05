using System.Runtime.CompilerServices;

namespace System
{
[TypeForwardedFrom("System.Core, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=b77a5c561934e089")]
public sealed class Action<T1, T2, T3> : MulticastDelegate
{
	public Action(object object, IntPtr method);

	public virtual IAsyncResult BeginInvoke(T1 arg1, T2 arg2, T3 arg3, AsyncCallback callback, object object);

	public virtual void EndInvoke(IAsyncResult result);

	public virtual void Invoke(T1 arg1, T2 arg2, T3 arg3);
}
}