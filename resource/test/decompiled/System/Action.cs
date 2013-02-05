using System.Runtime.CompilerServices;

namespace System
{
[TypeForwardedFrom("System.Core, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=b77a5c561934e089")]
public sealed class Action : MulticastDelegate
{
	public Action(object object1, IntPtr method);

	public virtual IAsyncResult BeginInvoke(AsyncCallback callback, object object1);

	public virtual void EndInvoke(IAsyncResult result);

	public virtual void Invoke();
}
}