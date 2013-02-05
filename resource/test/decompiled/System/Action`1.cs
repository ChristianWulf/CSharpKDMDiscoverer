namespace System
{
public sealed class Action<T> : MulticastDelegate
{
	public Action(object object, IntPtr method);

	public virtual IAsyncResult BeginInvoke(T obj, AsyncCallback callback, object object);

	public virtual void EndInvoke(IAsyncResult result);

	public virtual void Invoke(T obj);
}
}