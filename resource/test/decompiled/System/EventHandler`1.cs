namespace System
{
[Serializable]
public sealed class EventHandler<TEventArgs> : MulticastDelegate
{
	public EventHandler(object object, IntPtr method);

	public virtual IAsyncResult BeginInvoke(object sender, TEventArgs e, AsyncCallback callback, object object);

	public virtual void EndInvoke(IAsyncResult result);

	public virtual void Invoke(object sender, TEventArgs e);
}
}