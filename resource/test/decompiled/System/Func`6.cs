namespace System
{
public sealed class Func<T1, T2, T3, T4, T5, TResult> : MulticastDelegate
{
	public Func(object object, IntPtr method);

	public virtual IAsyncResult BeginInvoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, AsyncCallback callback, object object);

	public virtual TResult EndInvoke(IAsyncResult result);

	public virtual TResult Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
}
}