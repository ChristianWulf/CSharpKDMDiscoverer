using System;
namespace System
{
	internal struct AppDomainHandle
	{
		private IntPtr m_appDomainHandle;
		internal AppDomainHandle(IntPtr domainHandle)
		{
			this.m_appDomainHandle = domainHandle;
		}
	}
}
