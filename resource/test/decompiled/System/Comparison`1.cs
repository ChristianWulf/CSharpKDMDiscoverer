namespace System
{
public sealed class Comparison<T> : MulticastDelegate
{
	public Comparison(object object, IntPtr method);

	public virtual IAsyncResult BeginInvoke(T x, T y, AsyncCallback callback, object object);

	public virtual int EndInvoke(IAsyncResult result);

	public virtual int Invoke(T x, T y);
}
}