using System;
using System.Security;
namespace System
{
	public static class BitConverter
	{
		public static readonly bool IsLittleEndian = true;
		public static byte[] GetBytes(bool value)
		{
			return new byte[]
			{
				value ? 1 : 0
			};
		}
		public static byte[] GetBytes(char value)
		{
			return BitConverter.GetBytes((short)value);
		}
		[SecuritySafeCritical]
		public unsafe static byte[] GetBytes(short value)
		{
			byte[] array = new byte[2];
			fixed (byte* ptr = array)
			{
				*(short*)ptr = value;
			}
			return array;
		}
		[SecuritySafeCritical]
		public unsafe static byte[] GetBytes(int value)
		{
			byte[] array = new byte[4];
			fixed (byte* ptr = array)
			{
				*(int*)ptr = value;
			}
			return array;
		}
		[SecuritySafeCritical]
		public unsafe static byte[] GetBytes(long value)
		{
			byte[] array = new byte[8];
			fixed (byte* ptr = array)
			{
				*(long*)ptr = value;
			}
			return array;
		}
		[CLSCompliant(false)]
		public static byte[] GetBytes(ushort value)
		{
			return BitConverter.GetBytes((short)value);
		}
		[CLSCompliant(false)]
		public static byte[] GetBytes(uint value)
		{
			return BitConverter.GetBytes((int)value);
		}
		[CLSCompliant(false)]
		public static byte[] GetBytes(ulong value)
		{
			return BitConverter.GetBytes((long)value);
		}
		[SecuritySafeCritical]
		public unsafe static byte[] GetBytes(float value)
		{
			return BitConverter.GetBytes(*(int*)(&value));
		}
		[SecuritySafeCritical]
		public unsafe static byte[] GetBytes(double value)
		{
			return BitConverter.GetBytes(*(long*)(&value));
		}
		public static char ToChar(byte[] value, int startIndex)
		{
			return (char)BitConverter.ToInt16(value, startIndex);
		}
		[SecuritySafeCritical]
		public unsafe static short ToInt16(byte[] value, int startIndex)
		{
			if (value == null)
			{
				ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value);
			}
			if ((ulong)startIndex >= (ulong)((long)value.Length))
			{
				ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.startIndex, ExceptionResource.ArgumentOutOfRange_Index);
			}
			if (startIndex > value.Length - 2)
			{
				ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
			}
			short result;
			if (startIndex % 2 == 0)
			{
				result = *(short*)(&value[startIndex]);
			}
			else
			{
				if (BitConverter.IsLittleEndian)
				{
					result = (short)((int)(*(&value[startIndex])) | (int)(&value[startIndex])[(IntPtr)1 / 1] << 8);
				}
				else
				{
					result = (short)((int)(*(&value[startIndex])) << 8 | (int)(&value[startIndex])[(IntPtr)1 / 1]);
				}
			}
			return result;
		}
		[SecuritySafeCritical]
		public unsafe static int ToInt32(byte[] value, int startIndex)
		{
			if (value == null)
			{
				ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value);
			}
			if ((ulong)startIndex >= (ulong)((long)value.Length))
			{
				ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.startIndex, ExceptionResource.ArgumentOutOfRange_Index);
			}
			if (startIndex > value.Length - 4)
			{
				ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
			}
			int result;
			if (startIndex % 4 == 0)
			{
				result = *(int*)(&value[startIndex]);
			}
			else
			{
				if (BitConverter.IsLittleEndian)
				{
					result = ((int)(*(&value[startIndex])) | (int)(&value[startIndex])[(IntPtr)1 / 1] << 8 | (int)(&value[startIndex])[(IntPtr)2 / 1] << 16 | (int)(&value[startIndex])[(IntPtr)3 / 1] << 24);
				}
				else
				{
					result = ((int)(*(&value[startIndex])) << 24 | (int)(&value[startIndex])[(IntPtr)1 / 1] << 16 | (int)(&value[startIndex])[(IntPtr)2 / 1] << 8 | (int)(&value[startIndex])[(IntPtr)3 / 1]);
				}
			}
			return result;
		}
		[SecuritySafeCritical]
		public unsafe static long ToInt64(byte[] value, int startIndex)
		{
			if (value == null)
			{
				ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value);
			}
			if ((ulong)startIndex >= (ulong)((long)value.Length))
			{
				ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.startIndex, ExceptionResource.ArgumentOutOfRange_Index);
			}
			if (startIndex > value.Length - 8)
			{
				ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
			}
			long result;
			if (startIndex % 8 == 0)
			{
				result = *(long*)(&value[startIndex]);
			}
			else
			{
				if (BitConverter.IsLittleEndian)
				{
					int num = (int)(*(&value[startIndex])) | (int)(&value[startIndex])[(IntPtr)1 / 1] << 8 | (int)(&value[startIndex])[(IntPtr)2 / 1] << 16 | (int)(&value[startIndex])[(IntPtr)3 / 1] << 24;
					int num2 = (int)(&value[startIndex])[(IntPtr)4 / 1] | (int)(&value[startIndex])[(IntPtr)5 / 1] << 8 | (int)(&value[startIndex])[(IntPtr)6 / 1] << 16 | (int)(&value[startIndex])[(IntPtr)7 / 1] << 24;
					result = (long)((ulong)num | (ulong)((ulong)((long)num2) << 32));
				}
				else
				{
					int num3 = (int)(*(&value[startIndex])) << 24 | (int)(&value[startIndex])[(IntPtr)1 / 1] << 16 | (int)(&value[startIndex])[(IntPtr)2 / 1] << 8 | (int)(&value[startIndex])[(IntPtr)3 / 1];
					int num4 = (int)(&value[startIndex])[(IntPtr)4 / 1] << 24 | (int)(&value[startIndex])[(IntPtr)5 / 1] << 16 | (int)(&value[startIndex])[(IntPtr)6 / 1] << 8 | (int)(&value[startIndex])[(IntPtr)7 / 1];
					result = (long)((ulong)num4 | (ulong)((ulong)((long)num3) << 32));
				}
			}
			return result;
		}
		[CLSCompliant(false)]
		public static ushort ToUInt16(byte[] value, int startIndex)
		{
			return (ushort)BitConverter.ToInt16(value, startIndex);
		}
		[CLSCompliant(false)]
		public static uint ToUInt32(byte[] value, int startIndex)
		{
			return (uint)BitConverter.ToInt32(value, startIndex);
		}
		[CLSCompliant(false)]
		public static ulong ToUInt64(byte[] value, int startIndex)
		{
			return (ulong)BitConverter.ToInt64(value, startIndex);
		}
		[SecuritySafeCritical]
		public unsafe static float ToSingle(byte[] value, int startIndex)
		{
			int num = BitConverter.ToInt32(value, startIndex);
			return *(float*)(&num);
		}
		[SecuritySafeCritical]
		public unsafe static double ToDouble(byte[] value, int startIndex)
		{
			long num = BitConverter.ToInt64(value, startIndex);
			return *(double*)(&num);
		}
		private static char GetHexValue(int i)
		{
			if (i < 10)
			{
				return (char)(i + 48);
			}
			return (char)(i - 10 + 65);
		}
		public static string ToString(byte[] value, int startIndex, int length)
		{
			if (value == null)
			{
				throw new ArgumentNullException("byteArray");
			}
			int num = value.Length;
			if (startIndex < 0 || (startIndex >= num && startIndex > 0))
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_StartIndex"));
			}
			if (length < 0)
			{
				throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("ArgumentOutOfRange_GenericPositive"));
			}
			if (startIndex > num - length)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_ArrayPlusOffTooSmall"));
			}
			if (length == 0)
			{
				return string.Empty;
			}
			if (length > 715827882)
			{
				throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("ArgumentOutOfRange_LengthTooLarge", new object[]
				{
					715827882
				}));
			}
			int num2 = length * 3;
			char[] array = new char[num2];
			int i = 0;
			int num3 = startIndex;
			for (i = 0; i < num2; i += 3)
			{
				byte b = value[num3++];
				array[i] = BitConverter.GetHexValue((int)(b / 16));
				array[i + 1] = BitConverter.GetHexValue((int)(b % 16));
				array[i + 2] = '-';
			}
			return new string(array, 0, array.Length - 1);
		}
		public static string ToString(byte[] value)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			return BitConverter.ToString(value, 0, value.Length);
		}
		public static string ToString(byte[] value, int startIndex)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			return BitConverter.ToString(value, startIndex, value.Length - startIndex);
		}
		public static bool ToBoolean(byte[] value, int startIndex)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (startIndex < 0)
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			if (startIndex > value.Length - 1)
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
			}
			return value[startIndex] != 0;
		}
		[SecuritySafeCritical]
		public unsafe static long DoubleToInt64Bits(double value)
		{
			return *(long*)(&value);
		}
		[SecuritySafeCritical]
		public unsafe static double Int64BitsToDouble(long value)
		{
			return *(double*)(&value);
		}
	}
}
