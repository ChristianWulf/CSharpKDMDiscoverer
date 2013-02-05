using System;
namespace System
{
	internal sealed class LocalDataStoreElement
	{
		private object m_value;
		private long m_cookie;
		public object Value
		{
			get
			{
				return this.m_value;
			}
			set
			{
				this.m_value = value;
			}
		}
		public long Cookie
		{
			get
			{
				return this.m_cookie;
			}
		}
		public LocalDataStoreElement(long cookie)
		{
			this.m_cookie = cookie;
		}
	}
}
