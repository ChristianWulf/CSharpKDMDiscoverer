using System;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
namespace System
{
	[ComVisible(true)]
	public static class Buffer
	{
		[SecuritySafeCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern void BlockCopy(Array src, int srcOffset, Array dst, int dstOffset, int count);
		[SecuritySafeCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void InternalBlockCopy(Array src, int srcOffsetBytes, Array dst, int dstOffsetBytes, int byteCount);
		[SecurityCritical]
		internal unsafe static int IndexOfByte(byte* src, byte value, int index, int count)
		{
			byte* ptr = src + (IntPtr)index / 1;
			while ((ptr & 3) != 0)
			{
				if (count == 0)
				{
					return -1;
				}
				if (*ptr == value)
				{
					return (ptr - src / 1) / 1;
				}
				count--;
				ptr += (IntPtr)1 / 1;
			}
			uint num = (uint)(((int)value << 8) + (int)value);
			num = (num << 16) + num;
			while (count > 3)
			{
				uint num2 = *(uint*)ptr;
				num2 ^= num;
				uint num3 = 2130640639u + num2;
				num2 ^= 4294967295u;
				num2 ^= num3;
				num2 &= 2164326656u;
				if (num2 != 0u)
				{
					int num4 = (ptr - src / 1) / 1;
					if (*ptr == value)
					{
						return num4;
					}
					if (ptr[(IntPtr)1 / 1] == value)
					{
						return num4 + 1;
					}
					if (ptr[(IntPtr)2 / 1] == value)
					{
						return num4 + 2;
					}
					if (ptr[(IntPtr)3 / 1] == value)
					{
						return num4 + 3;
					}
				}
				count -= 4;
				ptr += (IntPtr)4 / 1;
			}
			while (count > 0)
			{
				if (*ptr == value)
				{
					return (ptr - src / 1) / 1;
				}
				count--;
				ptr += (IntPtr)1 / 1;
			}
			return -1;
		}
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern bool IsPrimitiveTypeArray(Array array);
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern byte _GetByte(Array array, int index);
		[SecuritySafeCritical]
		public static byte GetByte(Array array, int index)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (!Buffer.IsPrimitiveTypeArray(array))
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBePrimArray"), "array");
			}
			if (index < 0 || index >= Buffer._ByteLength(array))
			{
				throw new ArgumentOutOfRangeException("index");
			}
			return Buffer._GetByte(array, index);
		}
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void _SetByte(Array array, int index, byte value);
		[SecuritySafeCritical]
		public static void SetByte(Array array, int index, byte value)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (!Buffer.IsPrimitiveTypeArray(array))
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBePrimArray"), "array");
			}
			if (index < 0 || index >= Buffer._ByteLength(array))
			{
				throw new ArgumentOutOfRangeException("index");
			}
			Buffer._SetByte(array, index, value);
		}
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern int _ByteLength(Array array);
		[SecuritySafeCritical]
		public static int ByteLength(Array array)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (!Buffer.IsPrimitiveTypeArray(array))
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBePrimArray"), "array");
			}
			return Buffer._ByteLength(array);
		}
		[SecurityCritical]
		internal unsafe static void ZeroMemory(byte* src, long len)
		{
			while (true)
			{
				long expr_09 = len;
				len = expr_09 - 1L;
				if (expr_09 <= 0L)
				{
					break;
				}
				src[(IntPtr)len / 1] = 0;
			}
		}
		[SecurityCritical, ForceTokenStabilization, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		internal unsafe static void memcpy(byte* src, int srcIndex, byte[] dest, int destIndex, int len)
		{
			if (len == 0)
			{
				return;
			}
			fixed (byte* ptr = dest)
			{
				Buffer.memcpyimpl(src + (IntPtr)srcIndex / 1, ptr + (IntPtr)destIndex / 1, len);
			}
		}
		[SecuritySafeCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), ForceTokenStabilization]
		internal unsafe static void memcpy(byte[] src, int srcIndex, byte* pDest, int destIndex, int len)
		{
			if (len == 0)
			{
				return;
			}
			fixed (byte* ptr = src)
			{
				Buffer.memcpyimpl(ptr + (IntPtr)srcIndex / 1, pDest + (IntPtr)destIndex / 1, len);
			}
		}
		[SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), ForceTokenStabilization]
		internal unsafe static void memcpy(char* pSrc, int srcIndex, char* pDest, int destIndex, int len)
		{
			if (len == 0)
			{
				return;
			}
			Buffer.memcpyimpl((byte*)(pSrc + (IntPtr)srcIndex), (byte*)(pDest + (IntPtr)destIndex), len * 2);
		}
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), ForceTokenStabilization, SecurityCritical]
		internal unsafe static void memcpyimpl(byte* src, byte* dest, int len)
		{
			if (len >= 16)
			{
				do
				{
					*(int*)dest = *(int*)src;
					*(int*)(dest + (IntPtr)4 / 1) = *(int*)(src + (IntPtr)4 / 1);
					*(int*)(dest + (IntPtr)8 / 1) = *(int*)(src + (IntPtr)8 / 1);
					*(int*)(dest + (IntPtr)12 / 1) = *(int*)(src + (IntPtr)12 / 1);
					dest += (IntPtr)16 / 1;
					src += (IntPtr)16 / 1;
				}
				while ((len -= 16) >= 16);
			}
			if (len > 0)
			{
				if ((len & 8) != 0)
				{
					*(int*)dest = *(int*)src;
					*(int*)(dest + (IntPtr)4 / 1) = *(int*)(src + (IntPtr)4 / 1);
					dest += (IntPtr)8 / 1;
					src += (IntPtr)8 / 1;
				}
				if ((len & 4) != 0)
				{
					*(int*)dest = *(int*)src;
					dest += (IntPtr)4 / 1;
					src += (IntPtr)4 / 1;
				}
				if ((len & 2) != 0)
				{
					*(short*)dest = *(short*)src;
					dest += (IntPtr)2 / 1;
					src += (IntPtr)2 / 1;
				}
				if ((len & 1) != 0)
				{
					byte* expr_95 = dest;
					dest = expr_95 + (IntPtr)1 / 1;
					byte* expr_9C = src;
					src = expr_9C + (IntPtr)1 / 1;
					*expr_95 = *expr_9C;
				}
			}
		}
	}
}
