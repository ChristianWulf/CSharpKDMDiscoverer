namespace System
{
public sealed class Predicate<T> : MulticastDelegate
{
	public Predicate(object object, IntPtr method);

	public virtual IAsyncResult BeginInvoke(T obj, AsyncCallback callback, object object);

	public virtual bool EndInvoke(IAsyncResult result);

	public virtual bool Invoke(T obj);
}
}