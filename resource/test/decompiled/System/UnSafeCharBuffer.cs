using System;
using System.Security;
namespace System
{
	internal struct UnSafeCharBuffer
	{
		private unsafe char* m_buffer;
		private int m_totalSize;
		private int m_length;
		public int Length
		{
			get
			{
				return this.m_length;
			}
		}
		[SecurityCritical]
		public unsafe UnSafeCharBuffer(char* buffer, int bufferSize)
		{
			this.m_buffer = buffer;
			this.m_totalSize = bufferSize;
			this.m_length = 0;
		}
		[SecuritySafeCritical]
		public unsafe void AppendString(string stringToAppend)
		{
			if (string.IsNullOrEmpty(stringToAppend))
			{
				return;
			}
			if (this.m_totalSize - this.m_length < stringToAppend.Length)
			{
				throw new IndexOutOfRangeException();
			}
			fixed (char* src = stringToAppend)
			{
				Buffer.memcpyimpl((byte*)src, (byte*)(this.m_buffer + (IntPtr)this.m_length), stringToAppend.Length * 2);
			}
			this.m_length += stringToAppend.Length;
		}
	}
}
