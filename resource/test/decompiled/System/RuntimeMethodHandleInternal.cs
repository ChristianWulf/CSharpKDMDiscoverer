using System;
using System.Security;
namespace System
{
	internal struct RuntimeMethodHandleInternal
	{
		internal IntPtr m_handle;
		internal static RuntimeMethodHandleInternal EmptyHandle
		{
			get
			{
				return default(RuntimeMethodHandleInternal);
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
		internal RuntimeMethodHandleInternal(IntPtr value)
		{
			this.m_handle = value;
		}
	}
}
