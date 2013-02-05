using System;
using System.Threading;
namespace System
{
	internal sealed class System_LazyDebugView<T>
	{
		private readonly Lazy<T> m_lazy;
		public bool IsValueCreated
		{
			get
			{
				return this.m_lazy.IsValueCreated;
			}
		}
		public T Value
		{
			get
			{
				return this.m_lazy.ValueForDebugDisplay;
			}
		}
		public LazyThreadSafetyMode Mode
		{
			get
			{
				return this.m_lazy.Mode;
			}
		}
		public bool IsValueFaulted
		{
			get
			{
				return this.m_lazy.IsValueFaulted;
			}
		}
		public System_LazyDebugView(Lazy<T> lazy)
		{
			this.m_lazy = lazy;
		}
	}
}
