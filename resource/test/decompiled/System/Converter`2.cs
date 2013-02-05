namespace System
{
public sealed class Converter<TInput, TOutput> : MulticastDelegate
{
	public Converter(object object, IntPtr method);

	public virtual IAsyncResult BeginInvoke(TInput input, AsyncCallback callback, object object);

	public virtual TOutput EndInvoke(IAsyncResult result);

	public virtual TOutput Invoke(TInput input);
}
}