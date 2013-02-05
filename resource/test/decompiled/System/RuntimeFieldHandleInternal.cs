using System;
using System.Security;
namespace System
{
	internal struct RuntimeFieldHandleInternal
	{
		internal IntPtr m_handle;
		internal static RuntimeFieldHandleInternal EmptyHandle
		{
			get
			{
				return default(RuntimeFieldHandleInternal);
			}
		}
		internal IntPtr Value
		{
			[SecurityCritical]
			get
			{
				return this.m_handle;
			}
		}
		internal bool IsNullHandle()
		{
			return this.m_handle.IsNull();
		}
		[SecurityCritical]
		internal RuntimeFieldHandleInternal(IntPtr value)
		{
			this.m_handle = value;
		}
	}
}
