using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
namespace System
{
	internal struct Utf8String
	{
		private unsafe void* m_pStringHeap;
		private int m_StringHeapByteLength;
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe static extern bool EqualsCaseSensitive(void* szLhs, void* szRhs, int cSz);
		[SuppressUnmanagedCodeSecurity, SecurityCritical]
		[DllImport("QCall", CharSet = CharSet.Unicode)]
		private unsafe static extern bool EqualsCaseInsensitive(void* szLhs, void* szRhs, int cSz);
		[SecurityCritical, SuppressUnmanagedCodeSecurity]
		[DllImport("QCall", CharSet = CharSet.Unicode)]
		private unsafe static extern uint HashCaseInsensitive(void* sz, int cSz);
		[SecurityCritical]
		private unsafe static int GetUtf8StringByteLength(void* pUtf8String)
		{
			int num = 0;
			byte* ptr = (byte*)pUtf8String;
			while (*(bool*)ptr)
			{
				num++;
				ptr += (IntPtr)1 / 1;
			}
			return num;
		}
		[SecurityCritical]
		internal unsafe Utf8String(void* pStringHeap)
		{
			this.m_pStringHeap = pStringHeap;
			if (pStringHeap != null)
			{
				this.m_StringHeapByteLength = Utf8String.GetUtf8StringByteLength(pStringHeap);
				return;
			}
			this.m_StringHeapByteLength = 0;
		}
		[SecurityCritical]
		internal unsafe Utf8String(void* pUtf8String, int cUtf8String)
		{
			this.m_pStringHeap = pUtf8String;
			this.m_StringHeapByteLength = cUtf8String;
		}
		[SecuritySafeCritical]
		internal bool Equals(Utf8String s)
		{
			if (this.m_pStringHeap == null)
			{
				return s.m_StringHeapByteLength == 0;
			}
			return s.m_StringHeapByteLength == this.m_StringHeapByteLength && this.m_StringHeapByteLength != 0 && Utf8String.EqualsCaseSensitive(s.m_pStringHeap, this.m_pStringHeap, this.m_StringHeapByteLength);
		}
		[SecuritySafeCritical]
		internal bool EqualsCaseInsensitive(Utf8String s)
		{
			if (this.m_pStringHeap == null)
			{
				return s.m_StringHeapByteLength == 0;
			}
			return s.m_StringHeapByteLength == this.m_StringHeapByteLength && this.m_StringHeapByteLength != 0 && Utf8String.EqualsCaseInsensitive(s.m_pStringHeap, this.m_pStringHeap, this.m_StringHeapByteLength);
		}
		[SecuritySafeCritical]
		internal uint HashCaseInsensitive()
		{
			return Utf8String.HashCaseInsensitive(this.m_pStringHeap, this.m_StringHeapByteLength);
		}
		[SecuritySafeCritical]
		public unsafe override string ToString()
		{
			byte* ptr = stackalloc byte[(UIntPtr)this.m_StringHeapByteLength / 1];
			byte* ptr2 = (byte*)this.m_pStringHeap;
			for (int i = 0; i < this.m_StringHeapByteLength; i++)
			{
				ptr[(IntPtr)i / 1] = *ptr2;
				ptr2 += (IntPtr)1 / 1;
			}
			if (this.m_StringHeapByteLength == 0)
			{
				return "";
			}
			int charCount = Encoding.UTF8.GetCharCount(ptr, this.m_StringHeapByteLength);
			char* ptr3 = stackalloc char[(UIntPtr)charCount];
			Encoding.UTF8.GetChars(ptr, this.m_StringHeapByteLength, ptr3, charCount);
			return new string(ptr3, 0, charCount);
		}
	}
}
