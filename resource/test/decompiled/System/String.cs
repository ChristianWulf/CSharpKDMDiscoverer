using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;
namespace System
{
	[ComVisible(true)]
	[Serializable]
	public sealed class String : IComparable, ICloneable, IConvertible, IComparable<string>, IEnumerable<char>, IEnumerable, IEquatable<string>
	{
		private const int TrimHead = 0;
		private const int TrimTail = 1;
		private const int TrimBoth = 2;
		private const int charPtrAlignConst = 1;
		private const int alignConst = 3;
		[NonSerialized]
		private int m_stringLength;
		[ForceTokenStabilization]
		[NonSerialized]
		private char m_firstChar;
		public static readonly string Empty = "";
		internal char FirstChar
		{
			get
			{
				return this.m_firstChar;
			}
		}
		public extern char this[int index]
		{
			[SecuritySafeCritical]
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}
		public extern int Length
		{
			[SecuritySafeCritical]
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}
		public static string Join(string separator, params string[] value)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			return string.Join(separator, value, 0, value.Length);
		}
		[ComVisible(false)]
		public static string Join(string separator, params object[] values)
		{
			if (values == null)
			{
				throw new ArgumentNullException("values");
			}
			if (values.Length == 0 || values[0] == null)
			{
				return string.Empty;
			}
			if (separator == null)
			{
				separator = string.Empty;
			}
			StringBuilder stringBuilder = new StringBuilder();
			string text = values[0].ToString();
			if (text != null)
			{
				stringBuilder.Append(text);
			}
			for (int i = 1; i < values.Length; i++)
			{
				stringBuilder.Append(separator);
				if (values[i] != null)
				{
					text = values[i].ToString();
					if (text != null)
					{
						stringBuilder.Append(text);
					}
				}
			}
			return stringBuilder.ToString();
		}
		[ComVisible(false)]
		public static string Join<T>(string separator, IEnumerable<T> values)
		{
			if (values == null)
			{
				throw new ArgumentNullException("values");
			}
			if (separator == null)
			{
				separator = string.Empty;
			}
			string result;
			using (IEnumerator<T> enumerator = values.GetEnumerator())
			{
				if (!enumerator.MoveNext())
				{
					result = string.Empty;
				}
				else
				{
					StringBuilder stringBuilder = new StringBuilder();
					if (enumerator.Current != null)
					{
						T current = enumerator.Current;
						string text = current.ToString();
						if (text != null)
						{
							stringBuilder.Append(text);
						}
					}
					while (enumerator.MoveNext())
					{
						stringBuilder.Append(separator);
						if (enumerator.Current != null)
						{
							T current2 = enumerator.Current;
							string text2 = current2.ToString();
							if (text2 != null)
							{
								stringBuilder.Append(text2);
							}
						}
					}
					result = stringBuilder.ToString();
				}
			}
			return result;
		}
		[ComVisible(false)]
		public static string Join(string separator, IEnumerable<string> values)
		{
			if (values == null)
			{
				throw new ArgumentNullException("values");
			}
			if (separator == null)
			{
				separator = string.Empty;
			}
			string result;
			using (IEnumerator<string> enumerator = values.GetEnumerator())
			{
				if (!enumerator.MoveNext())
				{
					result = string.Empty;
				}
				else
				{
					StringBuilder stringBuilder = new StringBuilder();
					if (enumerator.Current != null)
					{
						stringBuilder.Append(enumerator.Current);
					}
					while (enumerator.MoveNext())
					{
						stringBuilder.Append(separator);
						if (enumerator.Current != null)
						{
							stringBuilder.Append(enumerator.Current);
						}
					}
					result = stringBuilder.ToString();
				}
			}
			return result;
		}
		[SecuritySafeCritical]
		public unsafe static string Join(string separator, string[] value, int startIndex, int count)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (startIndex < 0)
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_StartIndex"));
			}
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_NegativeCount"));
			}
			if (startIndex > value.Length - count)
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_IndexCountBuffer"));
			}
			if (separator == null)
			{
				separator = string.Empty;
			}
			if (count == 0)
			{
				return string.Empty;
			}
			int num = 0;
			int num2 = startIndex + count - 1;
			for (int i = startIndex; i <= num2; i++)
			{
				if (value[i] != null)
				{
					num += value[i].Length;
				}
			}
			num += (count - 1) * separator.Length;
			if (num < 0 || num + 1 < 0)
			{
				throw new OutOfMemoryException();
			}
			if (num == 0)
			{
				return string.Empty;
			}
			string text = string.FastAllocateString(num);
			fixed (char* ptr = &text.m_firstChar)
			{
				UnSafeCharBuffer unSafeCharBuffer = new UnSafeCharBuffer(ptr, num);
				unSafeCharBuffer.AppendString(value[startIndex]);
				for (int j = startIndex + 1; j <= num2; j++)
				{
					unSafeCharBuffer.AppendString(separator);
					unSafeCharBuffer.AppendString(value[j]);
				}
			}
			return text;
		}
		[SecuritySafeCritical]
		private unsafe static int CompareOrdinalIgnoreCaseHelper(string strA, string strB)
		{
			int num = Math.Min(strA.Length, strB.Length);
			char* ptr = &strA.m_firstChar;
			char* ptr2 = &strB.m_firstChar;
			int result;
			while (num != 0)
			{
				int num2 = (int)(*(ushort*)ptr);
				int num3 = (int)(*(ushort*)ptr2);
				if (num2 - 97 <= 25)
				{
					num2 -= 32;
				}
				if (num3 - 97 <= 25)
				{
					num3 -= 32;
				}
				if (num2 != num3)
				{
					result = num2 - num3;
					return result;
				}
				ptr += (IntPtr)2 / 2;
				ptr2 += (IntPtr)2 / 2;
				num--;
			}
			result = strA.Length - strB.Length;
			return result;
		}
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern int nativeCompareOrdinalEx(string strA, int indexA, string strB, int indexB, int count);
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal unsafe static extern int nativeCompareOrdinalIgnoreCaseWC(string strA, char* strBChars);
		[SecuritySafeCritical]
		internal unsafe static string SmallCharToUpper(string strIn)
		{
			int length = strIn.Length;
			string text = string.FastAllocateString(length);
			fixed (char* ptr = &strIn.m_firstChar, ptr2 = &text.m_firstChar)
			{
				for (int i = 0; i < length; i++)
				{
					int num = (int)((ushort*)ptr)[(IntPtr)i];
					if (num - 97 <= 25)
					{
						num -= 32;
					}
					ptr2[(IntPtr)i] = (char)num;
				}
			}
			return text;
		}
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), SecuritySafeCritical]
		private unsafe static bool EqualsHelper(string strA, string strB)
		{
			int i = strA.Length;
			if (i != strB.Length)
			{
				return false;
			}
			char* ptr = &strA.m_firstChar;
			char* ptr2 = &strB.m_firstChar;
			while (i >= 10)
			{
				if (*(int*)ptr != *(int*)ptr2 || *(int*)(ptr + (IntPtr)4 / 2) != *(int*)(ptr2 + (IntPtr)4 / 2) || *(int*)(ptr + (IntPtr)8 / 2) != *(int*)(ptr2 + (IntPtr)8 / 2) || *(int*)(ptr + (IntPtr)12 / 2) != *(int*)(ptr2 + (IntPtr)12 / 2) || *(int*)(ptr + (IntPtr)16 / 2) != *(int*)(ptr2 + (IntPtr)16 / 2))
				{
					IL_99:
					while (i > 0 && *(int*)ptr == *(int*)ptr2)
					{
						ptr += (IntPtr)4 / 2;
						ptr2 += (IntPtr)4 / 2;
						i -= 2;
					}
					return i <= 0;
				}
				ptr += (IntPtr)20 / 2;
				ptr2 += (IntPtr)20 / 2;
				i -= 10;
			}
			goto IL_99;
		}
		[SecuritySafeCritical]
		private unsafe static int CompareOrdinalHelper(string strA, string strB)
		{
			int i = Math.Min(strA.Length, strB.Length);
			int num = -1;
			char* ptr = &strA.m_firstChar;
			char* ptr2 = &strB.m_firstChar;
			while (i >= 10)
			{
				if (*(int*)ptr != *(int*)ptr2)
				{
					num = 0;
					break;
				}
				if (*(int*)(ptr + (IntPtr)4 / 2) != *(int*)(ptr2 + (IntPtr)4 / 2))
				{
					num = 2;
					break;
				}
				if (*(int*)(ptr + (IntPtr)8 / 2) != *(int*)(ptr2 + (IntPtr)8 / 2))
				{
					num = 4;
					break;
				}
				if (*(int*)(ptr + (IntPtr)12 / 2) != *(int*)(ptr2 + (IntPtr)12 / 2))
				{
					num = 6;
					break;
				}
				if (*(int*)(ptr + (IntPtr)16 / 2) != *(int*)(ptr2 + (IntPtr)16 / 2))
				{
					num = 8;
					break;
				}
				ptr += (IntPtr)20 / 2;
				ptr2 += (IntPtr)20 / 2;
				i -= 10;
			}
			int result;
			if (num != -1)
			{
				ptr += (IntPtr)num;
				ptr2 += (IntPtr)num;
				int num2;
				if ((num2 = (int)(*(ushort*)ptr - *(ushort*)ptr2)) != 0)
				{
					result = num2;
				}
				else
				{
					result = (int)(*(ushort*)(ptr + (IntPtr)2 / 2) - *(ushort*)(ptr2 + (IntPtr)2 / 2));
				}
			}
			else
			{
				while (i > 0 && *(int*)ptr == *(int*)ptr2)
				{
					ptr += (IntPtr)4 / 2;
					ptr2 += (IntPtr)4 / 2;
					i -= 2;
				}
				if (i > 0)
				{
					int num3;
					if ((num3 = (int)(*(ushort*)ptr - *(ushort*)ptr2)) != 0)
					{
						result = num3;
					}
					else
					{
						result = (int)(*(ushort*)(ptr + (IntPtr)2 / 2) - *(ushort*)(ptr2 + (IntPtr)2 / 2));
					}
				}
				else
				{
					result = strA.Length - strB.Length;
				}
			}
			return result;
		}
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public override bool Equals(object obj)
		{
			if (this == null)
			{
				throw new NullReferenceException();
			}
			string text = obj as string;
			return text != null && (object.ReferenceEquals(this, obj) || string.EqualsHelper(this, text));
		}
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public bool Equals(string value)
		{
			if (this == null)
			{
				throw new NullReferenceException();
			}
			return value != null && (object.ReferenceEquals(this, value) || string.EqualsHelper(this, value));
		}
		[SecuritySafeCritical]
		public bool Equals(string value, StringComparison comparisonType)
		{
			if (comparisonType < StringComparison.CurrentCulture || comparisonType > StringComparison.OrdinalIgnoreCase)
			{
				throw new ArgumentException(Environment.GetResourceString("NotSupported_StringComparison"), "comparisonType");
			}
			if (this == value)
			{
				return true;
			}
			if (value == null)
			{
				return false;
			}
			switch (comparisonType)
			{
				case StringComparison.CurrentCulture:
				{
					return CultureInfo.CurrentCulture.CompareInfo.Compare(this, value, CompareOptions.None) == 0;
				}
				case StringComparison.CurrentCultureIgnoreCase:
				{
					return CultureInfo.CurrentCulture.CompareInfo.Compare(this, value, CompareOptions.IgnoreCase) == 0;
				}
				case StringComparison.InvariantCulture:
				{
					return CultureInfo.InvariantCulture.CompareInfo.Compare(this, value, CompareOptions.None) == 0;
				}
				case StringComparison.InvariantCultureIgnoreCase:
				{
					return CultureInfo.InvariantCulture.CompareInfo.Compare(this, value, CompareOptions.IgnoreCase) == 0;
				}
				case StringComparison.Ordinal:
				{
					return string.EqualsHelper(this, value);
				}
				case StringComparison.OrdinalIgnoreCase:
				{
					if (this.Length != value.Length)
					{
						return false;
					}
					if (this.IsAscii() && value.IsAscii())
					{
						return string.CompareOrdinalIgnoreCaseHelper(this, value) == 0;
					}
					return TextInfo.CompareOrdinalIgnoreCase(this, value) == 0;
				}
				default:
				{
					throw new ArgumentException(Environment.GetResourceString("NotSupported_StringComparison"), "comparisonType");
				}
			}
		}
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static bool Equals(string a, string b)
		{
			return a == b || (a != null && b != null && string.EqualsHelper(a, b));
		}
		[SecuritySafeCritical]
		public static bool Equals(string a, string b, StringComparison comparisonType)
		{
			if (comparisonType < StringComparison.CurrentCulture || comparisonType > StringComparison.OrdinalIgnoreCase)
			{
				throw new ArgumentException(Environment.GetResourceString("NotSupported_StringComparison"), "comparisonType");
			}
			if (a == b)
			{
				return true;
			}
			if (a == null || b == null)
			{
				return false;
			}
			switch (comparisonType)
			{
				case StringComparison.CurrentCulture:
				{
					return CultureInfo.CurrentCulture.CompareInfo.Compare(a, b, CompareOptions.None) == 0;
				}
				case StringComparison.CurrentCultureIgnoreCase:
				{
					return CultureInfo.CurrentCulture.CompareInfo.Compare(a, b, CompareOptions.IgnoreCase) == 0;
				}
				case StringComparison.InvariantCulture:
				{
					return CultureInfo.InvariantCulture.CompareInfo.Compare(a, b, CompareOptions.None) == 0;
				}
				case StringComparison.InvariantCultureIgnoreCase:
				{
					return CultureInfo.InvariantCulture.CompareInfo.Compare(a, b, CompareOptions.IgnoreCase) == 0;
				}
				case StringComparison.Ordinal:
				{
					return string.EqualsHelper(a, b);
				}
				case StringComparison.OrdinalIgnoreCase:
				{
					if (a.Length != b.Length)
					{
						return false;
					}
					if (a.IsAscii() && b.IsAscii())
					{
						return string.CompareOrdinalIgnoreCaseHelper(a, b) == 0;
					}
					return TextInfo.CompareOrdinalIgnoreCase(a, b) == 0;
				}
				default:
				{
					throw new ArgumentException(Environment.GetResourceString("NotSupported_StringComparison"), "comparisonType");
				}
			}
		}
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static bool operator ==(string a, string b)
		{
			return string.Equals(a, b);
		}
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static bool operator !=(string a, string b)
		{
			return !string.Equals(a, b);
		}
		[SecuritySafeCritical]
		public unsafe void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
		{
			if (destination == null)
			{
				throw new ArgumentNullException("destination");
			}
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_NegativeCount"));
			}
			if (sourceIndex < 0)
			{
				throw new ArgumentOutOfRangeException("sourceIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
			}
			if (count > this.Length - sourceIndex)
			{
				throw new ArgumentOutOfRangeException("sourceIndex", Environment.GetResourceString("ArgumentOutOfRange_IndexCount"));
			}
			if (destinationIndex > destination.Length - count || destinationIndex < 0)
			{
				throw new ArgumentOutOfRangeException("destinationIndex", Environment.GetResourceString("ArgumentOutOfRange_IndexCount"));
			}
			if (count > 0)
			{
				fixed (char* ptr = &this.m_firstChar)
				{
					fixed (char* ptr2 = destination)
					{
						string.wstrcpy(ptr2 + (IntPtr)destinationIndex, ptr + (IntPtr)sourceIndex, count);
					}
				}
			}
		}
		[SecuritySafeCritical]
		public unsafe char[] ToCharArray()
		{
			int length = this.Length;
			char[] array = new char[length];
			if (length > 0)
			{
				fixed (char* ptr = &this.m_firstChar)
				{
					fixed (char* ptr2 = array)
					{
						string.wstrcpyPtrAligned(ptr2, ptr, length);
					}
				}
			}
			return array;
		}
		[SecuritySafeCritical]
		public unsafe char[] ToCharArray(int startIndex, int length)
		{
			if (startIndex < 0 || startIndex > this.Length || startIndex > this.Length - length)
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
			}
			if (length < 0)
			{
				throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("ArgumentOutOfRange_Index"));
			}
			char[] array = new char[length];
			if (length > 0)
			{
				fixed (char* ptr = &this.m_firstChar)
				{
					fixed (char* ptr2 = array)
					{
						string.wstrcpy(ptr2, ptr + (IntPtr)startIndex, length);
					}
				}
			}
			return array;
		}
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static bool IsNullOrEmpty(string value)
		{
			return value == null || value.Length == 0;
		}
		public static bool IsNullOrWhiteSpace(string value)
		{
			if (value == null)
			{
				return true;
			}
			for (int i = 0; i < value.Length; i++)
			{
				if (!char.IsWhiteSpace(value[i]))
				{
					return false;
				}
			}
			return true;
		}
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), SecuritySafeCritical]
		public unsafe override int GetHashCode()
		{
			IntPtr arg_0F_0;
			IntPtr expr_06 = arg_0F_0 = this;
			if (expr_06 != 0)
			{
				arg_0F_0 = (IntPtr)((int)expr_06 + RuntimeHelpers.OffsetToStringData);
			}
			char* ptr = arg_0F_0;
			int num = 352654597;
			int num2 = num;
			int* ptr2 = (int*)ptr;
			for (int i = this.Length; i > 0; i -= 4)
			{
				num = ((num << 5) + num + (num >> 27) ^ *ptr2);
				if (i <= 2)
				{
					break;
				}
				num2 = ((num2 << 5) + num2 + (num2 >> 27) ^ ptr2[(IntPtr)4 / 4]);
				ptr2 += (IntPtr)8 / 4;
			}
			return num + num2 * 1566083941;
		}
		public string[] Split(params char[] separator)
		{
			return this.SplitInternal(separator, 2147483647, StringSplitOptions.None);
		}
		public string[] Split(char[] separator, int count)
		{
			return this.SplitInternal(separator, count, StringSplitOptions.None);
		}
		[ComVisible(false)]
		public string[] Split(char[] separator, StringSplitOptions options)
		{
			return this.SplitInternal(separator, 2147483647, options);
		}
		[ComVisible(false)]
		public string[] Split(char[] separator, int count, StringSplitOptions options)
		{
			return this.SplitInternal(separator, count, options);
		}
		[ComVisible(false)]
		internal string[] SplitInternal(char[] separator, int count, StringSplitOptions options)
		{
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_NegativeCount"));
			}
			if (options < StringSplitOptions.None || options > StringSplitOptions.RemoveEmptyEntries)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_EnumIllegalVal", new object[]
				{
					options
				}));
			}
			bool flag = options == StringSplitOptions.RemoveEmptyEntries;
			if (count == 0 || (flag && this.Length == 0))
			{
				return new string[0];
			}
			int[] sepList = new int[this.Length];
			int num = this.MakeSeparatorList(separator, ref sepList);
			if (num == 0 || count == 1)
			{
				return new string[]
				{
					this
				};
			}
			if (flag)
			{
				return this.InternalSplitOmitEmptyEntries(sepList, null, num, count);
			}
			return this.InternalSplitKeepEmptyEntries(sepList, null, num, count);
		}
		[ComVisible(false)]
		public string[] Split(string[] separator, StringSplitOptions options)
		{
			return this.Split(separator, 2147483647, options);
		}
		[ComVisible(false)]
		public string[] Split(string[] separator, int count, StringSplitOptions options)
		{
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_NegativeCount"));
			}
			if (options < StringSplitOptions.None || options > StringSplitOptions.RemoveEmptyEntries)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_EnumIllegalVal", new object[]
				{
					(int)options
				}));
			}
			bool flag = options == StringSplitOptions.RemoveEmptyEntries;
			if (separator == null || separator.Length == 0)
			{
				return this.SplitInternal(null, count, options);
			}
			if (count == 0 || (flag && this.Length == 0))
			{
				return new string[0];
			}
			int[] sepList = new int[this.Length];
			int[] lengthList = new int[this.Length];
			int num = this.MakeSeparatorList(separator, ref sepList, ref lengthList);
			if (num == 0 || count == 1)
			{
				return new string[]
				{
					this
				};
			}
			if (flag)
			{
				return this.InternalSplitOmitEmptyEntries(sepList, lengthList, num, count);
			}
			return this.InternalSplitKeepEmptyEntries(sepList, lengthList, num, count);
		}
		private string[] InternalSplitKeepEmptyEntries(int[] sepList, int[] lengthList, int numReplaces, int count)
		{
			int num = 0;
			int num2 = 0;
			count--;
			int num3 = (numReplaces < count) ? numReplaces : count;
			string[] array = new string[num3 + 1];
			int num4 = 0;
			while (num4 < num3 && num < this.Length)
			{
				array[num2++] = this.Substring(num, sepList[num4] - num);
				num = sepList[num4] + ((lengthList == null) ? 1 : lengthList[num4]);
				num4++;
			}
			if (num < this.Length && num3 >= 0)
			{
				array[num2] = this.Substring(num);
			}
			else
			{
				if (num2 == num3)
				{
					array[num2] = string.Empty;
				}
			}
			return array;
		}
		private string[] InternalSplitOmitEmptyEntries(int[] sepList, int[] lengthList, int numReplaces, int count)
		{
			int num = (numReplaces < count) ? (numReplaces + 1) : count;
			string[] array = new string[num];
			int num2 = 0;
			int num3 = 0;
			int i = 0;
			while (i < numReplaces && num2 < this.Length)
			{
				if (sepList[i] - num2 > 0)
				{
					array[num3++] = this.Substring(num2, sepList[i] - num2);
				}
				num2 = sepList[i] + ((lengthList == null) ? 1 : lengthList[i]);
				if (num3 == count - 1)
				{
					while (i < numReplaces - 1)
					{
						if (num2 != sepList[++i])
						{
							break;
						}
						num2 += ((lengthList == null) ? 1 : lengthList[i]);
					}
					break;
				}
				i++;
			}
			if (num2 < this.Length)
			{
				array[num3++] = this.Substring(num2);
			}
			string[] array2 = array;
			if (num3 != num)
			{
				array2 = new string[num3];
				for (int j = 0; j < num3; j++)
				{
					array2[j] = array[j];
				}
			}
			return array2;
		}
		[SecuritySafeCritical]
		private unsafe int MakeSeparatorList(char[] separator, ref int[] sepList)
		{
			int num = 0;
			if (separator == null || separator.Length == 0)
			{
				fixed (char* ptr = &this.m_firstChar)
				{
					int num2 = 0;
					while (num2 < this.Length && num < sepList.Length)
					{
						if (char.IsWhiteSpace(ptr[(IntPtr)num2]))
						{
							sepList[num++] = num2;
						}
						num2++;
					}
				}
			}
			else
			{
				int num3 = sepList.Length;
				int num4 = separator.Length;
				fixed (char* ptr2 = &this.m_firstChar, ptr3 = separator)
				{
					int num5 = 0;
					while (num5 < this.Length && num < num3)
					{
						char* ptr4 = ptr3;
						int i = 0;
						while (i < num4)
						{
							if (((ushort*)ptr2)[(IntPtr)num5] == *(ushort*)ptr4)
							{
								sepList[num++] = num5;
								break;
							}
							i++;
							ptr4 += (IntPtr)2 / 2;
						}
						num5++;
					}
				}
			}
			return num;
		}
		[SecuritySafeCritical]
		private unsafe int MakeSeparatorList(string[] separators, ref int[] sepList, ref int[] lengthList)
		{
			int num = 0;
			int num2 = sepList.Length;
			fixed (char* ptr = &this.m_firstChar)
			{
				int num3 = 0;
				while (num3 < this.Length && num < num2)
				{
					for (int i = 0; i < separators.Length; i++)
					{
						string text = separators[i];
						if (!string.IsNullOrEmpty(text))
						{
							int length = text.Length;
							if (ptr[(IntPtr)num3] == text[0] && length <= this.Length - num3 && (length == 1 || string.CompareOrdinal(this, num3, text, 0, length) == 0))
							{
								sepList[num] = num3;
								lengthList[num] = length;
								num++;
								num3 += length - 1;
								break;
							}
						}
					}
					num3++;
				}
			}
			return num;
		}
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public string Substring(int startIndex)
		{
			return this.Substring(startIndex, this.Length - startIndex);
		}
		[SecuritySafeCritical, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public string Substring(int startIndex, int length)
		{
			return this.InternalSubStringWithChecks(startIndex, length, false);
		}
		[SecurityCritical]
		internal string InternalSubStringWithChecks(int startIndex, int length, bool fAlwaysCopy)
		{
			if (startIndex < 0)
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_StartIndex"));
			}
			if (startIndex > this.Length)
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_StartIndexLargerThanLength"));
			}
			if (length < 0)
			{
				throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("ArgumentOutOfRange_NegativeLength"));
			}
			if (startIndex > this.Length - length)
			{
				throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("ArgumentOutOfRange_IndexLength"));
			}
			if (length == 0)
			{
				return string.Empty;
			}
			return this.InternalSubString(startIndex, length, fAlwaysCopy);
		}
		[SecurityCritical]
		private unsafe string InternalSubString(int startIndex, int length, bool fAlwaysCopy)
		{
			if (startIndex == 0 && length == this.Length && !fAlwaysCopy)
			{
				return this;
			}
			string text = string.FastAllocateString(length);
			fixed (char* ptr = &text.m_firstChar)
			{
				fixed (char* ptr2 = &this.m_firstChar)
				{
					string.wstrcpy(ptr, ptr2 + (IntPtr)startIndex, length);
				}
			}
			return text;
		}
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public string Trim(params char[] trimChars)
		{
			if (trimChars == null || trimChars.Length == 0)
			{
				return this.TrimHelper(2);
			}
			return this.TrimHelper(trimChars, 2);
		}
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public string TrimStart(params char[] trimChars)
		{
			if (trimChars == null || trimChars.Length == 0)
			{
				return this.TrimHelper(0);
			}
			return this.TrimHelper(trimChars, 0);
		}
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public string TrimEnd(params char[] trimChars)
		{
			if (trimChars == null || trimChars.Length == 0)
			{
				return this.TrimHelper(1);
			}
			return this.TrimHelper(trimChars, 1);
		}
		[CLSCompliant(false), SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public unsafe extern String(char* value);
		[SecurityCritical, CLSCompliant(false)]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public unsafe extern String(char* value, int startIndex, int length);
		[CLSCompliant(false), SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public unsafe extern String(sbyte* value);
		[CLSCompliant(false), SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public unsafe extern String(sbyte* value, int startIndex, int length);
		[SecurityCritical, CLSCompliant(false)]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public unsafe extern String(sbyte* value, int startIndex, int length, Encoding enc);
		[SecurityCritical]
		private unsafe static string CreateString(sbyte* value, int startIndex, int length, Encoding enc)
		{
			if (enc == null)
			{
				return new string(value, startIndex, length);
			}
			if (length < 0)
			{
				throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			if (startIndex < 0)
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_StartIndex"));
			}
			if (value + (IntPtr)startIndex / 1 < value)
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_PartialWCHAR"));
			}
			byte[] array = new byte[length];
			try
			{
				Buffer.memcpy((byte*)value, startIndex, array, 0, length);
			}
			catch (NullReferenceException)
			{
				throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("ArgumentOutOfRange_PartialWCHAR"));
			}
			return enc.GetString(array);
		}
		[SecurityCritical]
		internal unsafe static string CreateStringFromEncoding(byte* bytes, int byteLength, Encoding encoding)
		{
			int charCount = encoding.GetCharCount(bytes, byteLength, null);
			if (charCount == 0)
			{
				return string.Empty;
			}
			string text = string.FastAllocateString(charCount);
			fixed (char* ptr = &text.m_firstChar)
			{
				encoding.GetChars(bytes, byteLength, ptr, charCount, null);
			}
			return text;
		}
		[SecuritySafeCritical]
		internal unsafe byte[] ConvertToAnsi(int iMaxDBCSCharByteSize, bool fBestFit, bool fThrowOnUnmappableChar, out int cbLength)
		{
			int num = (this.Length + 3) * iMaxDBCSCharByteSize;
			byte[] array = new byte[num];
			uint flags = fBestFit ? 0u : 1024u;
			uint num2 = 0u;
			int num3;
			fixed (byte* ptr = array)
			{
				fixed (char* ptr2 = &this.m_firstChar)
				{
					num3 = Win32Native.WideCharToMultiByte(0u, flags, ptr2, this.Length, ptr, num, IntPtr.Zero, fThrowOnUnmappableChar ? new IntPtr((void*)(&num2)) : IntPtr.Zero);
				}
			}
			if (num2 != 0u)
			{
				throw new ArgumentException(Environment.GetResourceString("Interop_Marshal_Unmappable_Char"));
			}
			cbLength = num3;
			array[num3] = 0;
			return array;
		}
		public bool IsNormalized()
		{
			return this.IsNormalized(NormalizationForm.FormC);
		}
		[SecuritySafeCritical]
		public bool IsNormalized(NormalizationForm normalizationForm)
		{
			return (this.IsFastSort() && (normalizationForm == NormalizationForm.FormC || normalizationForm == NormalizationForm.FormKC || normalizationForm == NormalizationForm.FormD || normalizationForm == NormalizationForm.FormKD)) || Normalization.IsNormalized(this, normalizationForm);
		}
		public string Normalize()
		{
			return this.Normalize(NormalizationForm.FormC);
		}
		[SecuritySafeCritical]
		public string Normalize(NormalizationForm normalizationForm)
		{
			if (this.IsAscii() && (normalizationForm == NormalizationForm.FormC || normalizationForm == NormalizationForm.FormKC || normalizationForm == NormalizationForm.FormD || normalizationForm == NormalizationForm.FormKD))
			{
				return this;
			}
			return Normalization.Normalize(this, normalizationForm);
		}
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern string FastAllocateString(int length);
		[SecuritySafeCritical]
		private unsafe static void FillStringChecked(string dest, int destPos, string src)
		{
			if (src.Length > dest.Length - destPos)
			{
				throw new IndexOutOfRangeException();
			}
			fixed (char* ptr = &dest.m_firstChar)
			{
				fixed (char* ptr2 = &src.m_firstChar)
				{
					string.wstrcpy(ptr + (IntPtr)destPos, ptr2, src.Length);
				}
			}
		}
		[SecuritySafeCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public extern String(char[] value, int startIndex, int length);
		[SecuritySafeCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public extern String(char[] value);
		[SecurityCritical]
		private unsafe static void wstrcpyPtrAligned(char* dmem, char* smem, int charCount)
		{
			while (charCount >= 8)
			{
				*(int*)dmem = *(int*)smem;
				*(int*)(dmem + (IntPtr)4 / 2) = *(int*)(smem + (IntPtr)4 / 2);
				*(int*)(dmem + (IntPtr)8 / 2) = *(int*)(smem + (IntPtr)8 / 2);
				*(int*)(dmem + (IntPtr)12 / 2) = *(int*)(smem + (IntPtr)12 / 2);
				dmem += (IntPtr)16 / 2;
				smem += (IntPtr)16 / 2;
				charCount -= 8;
			}
			if ((charCount & 4) != 0)
			{
				*(int*)dmem = *(int*)smem;
				*(int*)(dmem + (IntPtr)4 / 2) = *(int*)(smem + (IntPtr)4 / 2);
				dmem += (IntPtr)8 / 2;
				smem += (IntPtr)8 / 2;
			}
			if ((charCount & 2) != 0)
			{
				*(int*)dmem = *(int*)smem;
				dmem += (IntPtr)4 / 2;
				smem += (IntPtr)4 / 2;
			}
			if ((charCount & 1) != 0)
			{
				*dmem = *smem;
			}
		}
		[SecurityCritical]
		internal unsafe static void wstrcpy(char* dmem, char* smem, int charCount)
		{
			if (charCount > 0)
			{
				if ((dmem & 2) != 0)
				{
					*dmem = *smem;
					dmem += (IntPtr)2 / 2;
					smem += (IntPtr)2 / 2;
					charCount--;
				}
				while (charCount >= 8)
				{
					*(int*)dmem = *(int*)smem;
					*(int*)(dmem + (IntPtr)4 / 2) = *(int*)(smem + (IntPtr)4 / 2);
					*(int*)(dmem + (IntPtr)8 / 2) = *(int*)(smem + (IntPtr)8 / 2);
					*(int*)(dmem + (IntPtr)12 / 2) = *(int*)(smem + (IntPtr)12 / 2);
					dmem += (IntPtr)16 / 2;
					smem += (IntPtr)16 / 2;
					charCount -= 8;
				}
				if ((charCount & 4) != 0)
				{
					*(int*)dmem = *(int*)smem;
					*(int*)(dmem + (IntPtr)4 / 2) = *(int*)(smem + (IntPtr)4 / 2);
					dmem += (IntPtr)8 / 2;
					smem += (IntPtr)8 / 2;
				}
				if ((charCount & 2) != 0)
				{
					*(int*)dmem = *(int*)smem;
					dmem += (IntPtr)4 / 2;
					smem += (IntPtr)4 / 2;
				}
				if ((charCount & 1) != 0)
				{
					*dmem = *smem;
				}
			}
		}
		[SecuritySafeCritical]
		private unsafe string CtorCharArray(char[] value)
		{
			if (value != null && value.Length != 0)
			{
				string text = string.FastAllocateString(value.Length);
				fixed (char* dmem = text, ptr = value)
				{
					string.wstrcpyPtrAligned(dmem, ptr, value.Length);
				}
				return text;
			}
			return string.Empty;
		}
		[SecuritySafeCritical]
		private unsafe string CtorCharArrayStartLength(char[] value, int startIndex, int length)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (startIndex < 0)
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_StartIndex"));
			}
			if (length < 0)
			{
				throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("ArgumentOutOfRange_NegativeLength"));
			}
			if (startIndex > value.Length - length)
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
			}
			if (length > 0)
			{
				string text = string.FastAllocateString(length);
				fixed (char* dmem = text, ptr = value)
				{
					string.wstrcpy(dmem, ptr + (IntPtr)startIndex, length);
				}
				return text;
			}
			return string.Empty;
		}
		[SecuritySafeCritical]
		private unsafe string CtorCharCount(char c, int count)
		{
			if (count > 0)
			{
				string text = string.FastAllocateString(count);
				fixed (char* ptr = text)
				{
					char* ptr2 = ptr;
					while ((ptr2 & 3u) != 0u && count > 0)
					{
						char* expr_20 = ptr2;
						ptr2 = expr_20 + (IntPtr)2 / 2;
						*expr_20 = c;
						count--;
					}
					uint num = (uint)((uint)c << 16 | c);
					if (count >= 4)
					{
						count -= 4;
						do
						{
							*(int*)ptr2 = (int)num;
							*(int*)(ptr2 + (IntPtr)4 / 2) = (int)num;
							ptr2 += (IntPtr)8 / 2;
							count -= 4;
						}
						while (count >= 0);
					}
					if ((count & 2) != 0)
					{
						*(int*)ptr2 = (int)num;
						ptr2 += (IntPtr)4 / 2;
					}
					if ((count & 1) != 0)
					{
						*ptr2 = c;
					}
				}
				return text;
			}
			if (count == 0)
			{
				return string.Empty;
			}
			throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_MustBeNonNegNum", new object[]
			{
				"count"
			}));
		}
		[ForceTokenStabilization, SecurityCritical]
		private unsafe static int wcslen(char* ptr)
		{
			char* ptr2 = ptr;
			while ((ptr2 & 3u) != 0u && *(ushort*)ptr2 != 0)
			{
				ptr2 += (IntPtr)2 / 2;
			}
			if (*(ushort*)ptr2 != 0)
			{
				while (true)
				{
					if ((*(ushort*)ptr2 & *(ushort*)(ptr2 + (IntPtr)2 / 2)) == 0)
					{
						if (*(ushort*)ptr2 == 0)
						{
							break;
						}
						if (*(ushort*)(ptr2 + (IntPtr)2 / 2) == 0)
						{
							break;
						}
					}
					ptr2 += (IntPtr)4 / 2;
				}
			}
			while (*(ushort*)ptr2 != 0)
			{
				ptr2 += (IntPtr)2 / 2;
			}
			return (ptr2 - ptr / 2) / 2;
		}
		[SecurityCritical]
		private unsafe string CtorCharPtr(char* ptr)
		{
			if (ptr == null)
			{
				return string.Empty;
			}
			if (ptr < 64000)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeStringPtrNotAtom"));
			}
			string result;
			try
			{
				int num = string.wcslen(ptr);
				string text = string.FastAllocateString(num);
				try
				{
					fixed (char* dmem = text)
					{
						string.wstrcpy(dmem, ptr, num);
					}
				}
				finally
				{
					string text2 = null;
				}
				result = text;
			}
			catch (NullReferenceException)
			{
				throw new ArgumentOutOfRangeException("ptr", Environment.GetResourceString("ArgumentOutOfRange_PartialWCHAR"));
			}
			return result;
		}
		[ForceTokenStabilization, SecurityCritical]
		private unsafe string CtorCharPtrStartLength(char* ptr, int startIndex, int length)
		{
			if (length < 0)
			{
				throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("ArgumentOutOfRange_NegativeLength"));
			}
			if (startIndex < 0)
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_StartIndex"));
			}
			char* ptr2 = ptr + (IntPtr)startIndex;
			if (ptr2 < ptr)
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_PartialWCHAR"));
			}
			string text = string.FastAllocateString(length);
			string result;
			try
			{
				try
				{
					fixed (char* dmem = text)
					{
						string.wstrcpy(dmem, ptr2, length);
					}
				}
				finally
				{
					string text2 = null;
				}
				result = text;
			}
			catch (NullReferenceException)
			{
				throw new ArgumentOutOfRangeException("ptr", Environment.GetResourceString("ArgumentOutOfRange_PartialWCHAR"));
			}
			return result;
		}
		[SecuritySafeCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public extern String(char c, int count);
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static int Compare(string strA, string strB)
		{
			return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, strB, CompareOptions.None);
		}
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static int Compare(string strA, string strB, bool ignoreCase)
		{
			if (ignoreCase)
			{
				return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, strB, CompareOptions.IgnoreCase);
			}
			return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, strB, CompareOptions.None);
		}
		[SecuritySafeCritical]
		public static int Compare(string strA, string strB, StringComparison comparisonType)
		{
			if (comparisonType < StringComparison.CurrentCulture || comparisonType > StringComparison.OrdinalIgnoreCase)
			{
				throw new ArgumentException(Environment.GetResourceString("NotSupported_StringComparison"), "comparisonType");
			}
			if (strA == strB)
			{
				return 0;
			}
			if (strA == null)
			{
				return -1;
			}
			if (strB == null)
			{
				return 1;
			}
			switch (comparisonType)
			{
				case StringComparison.CurrentCulture:
				{
					return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, strB, CompareOptions.None);
				}
				case StringComparison.CurrentCultureIgnoreCase:
				{
					return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, strB, CompareOptions.IgnoreCase);
				}
				case StringComparison.InvariantCulture:
				{
					return CultureInfo.InvariantCulture.CompareInfo.Compare(strA, strB, CompareOptions.None);
				}
				case StringComparison.InvariantCultureIgnoreCase:
				{
					return CultureInfo.InvariantCulture.CompareInfo.Compare(strA, strB, CompareOptions.IgnoreCase);
				}
				case StringComparison.Ordinal:
				{
					return string.CompareOrdinalHelper(strA, strB);
				}
				case StringComparison.OrdinalIgnoreCase:
				{
					if (strA.IsAscii() && strB.IsAscii())
					{
						return string.CompareOrdinalIgnoreCaseHelper(strA, strB);
					}
					return TextInfo.CompareOrdinalIgnoreCase(strA, strB);
				}
				default:
				{
					throw new NotSupportedException(Environment.GetResourceString("NotSupported_StringComparison"));
				}
			}
		}
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static int Compare(string strA, string strB, CultureInfo culture, CompareOptions options)
		{
			if (culture == null)
			{
				throw new ArgumentNullException("culture");
			}
			return culture.CompareInfo.Compare(strA, strB, options);
		}
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static int Compare(string strA, string strB, bool ignoreCase, CultureInfo culture)
		{
			if (culture == null)
			{
				throw new ArgumentNullException("culture");
			}
			if (ignoreCase)
			{
				return culture.CompareInfo.Compare(strA, strB, CompareOptions.IgnoreCase);
			}
			return culture.CompareInfo.Compare(strA, strB, CompareOptions.None);
		}
		public static int Compare(string strA, int indexA, string strB, int indexB, int length)
		{
			int num = length;
			int num2 = length;
			if (strA != null && strA.Length - indexA < num)
			{
				num = strA.Length - indexA;
			}
			if (strB != null && strB.Length - indexB < num2)
			{
				num2 = strB.Length - indexB;
			}
			return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, indexA, num, strB, indexB, num2, CompareOptions.None);
		}
		public static int Compare(string strA, int indexA, string strB, int indexB, int length, bool ignoreCase)
		{
			int num = length;
			int num2 = length;
			if (strA != null && strA.Length - indexA < num)
			{
				num = strA.Length - indexA;
			}
			if (strB != null && strB.Length - indexB < num2)
			{
				num2 = strB.Length - indexB;
			}
			if (ignoreCase)
			{
				return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, indexA, num, strB, indexB, num2, CompareOptions.IgnoreCase);
			}
			return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, indexA, num, strB, indexB, num2, CompareOptions.None);
		}
		public static int Compare(string strA, int indexA, string strB, int indexB, int length, bool ignoreCase, CultureInfo culture)
		{
			if (culture == null)
			{
				throw new ArgumentNullException("culture");
			}
			int num = length;
			int num2 = length;
			if (strA != null && strA.Length - indexA < num)
			{
				num = strA.Length - indexA;
			}
			if (strB != null && strB.Length - indexB < num2)
			{
				num2 = strB.Length - indexB;
			}
			if (ignoreCase)
			{
				return culture.CompareInfo.Compare(strA, indexA, num, strB, indexB, num2, CompareOptions.IgnoreCase);
			}
			return culture.CompareInfo.Compare(strA, indexA, num, strB, indexB, num2, CompareOptions.None);
		}
		public static int Compare(string strA, int indexA, string strB, int indexB, int length, CultureInfo culture, CompareOptions options)
		{
			if (culture == null)
			{
				throw new ArgumentNullException("culture");
			}
			int num = length;
			int num2 = length;
			if (strA != null && strA.Length - indexA < num)
			{
				num = strA.Length - indexA;
			}
			if (strB != null && strB.Length - indexB < num2)
			{
				num2 = strB.Length - indexB;
			}
			return culture.CompareInfo.Compare(strA, indexA, num, strB, indexB, num2, options);
		}
		[SecuritySafeCritical]
		public static int Compare(string strA, int indexA, string strB, int indexB, int length, StringComparison comparisonType)
		{
			if (comparisonType < StringComparison.CurrentCulture || comparisonType > StringComparison.OrdinalIgnoreCase)
			{
				throw new ArgumentException(Environment.GetResourceString("NotSupported_StringComparison"), "comparisonType");
			}
			if (strA == null || strB == null)
			{
				if (strA == strB)
				{
					return 0;
				}
				if (strA != null)
				{
					return 1;
				}
				return -1;
			}
			else
			{
				if (length < 0)
				{
					throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("ArgumentOutOfRange_NegativeLength"));
				}
				if (indexA < 0)
				{
					throw new ArgumentOutOfRangeException("indexA", Environment.GetResourceString("ArgumentOutOfRange_Index"));
				}
				if (indexB < 0)
				{
					throw new ArgumentOutOfRangeException("indexB", Environment.GetResourceString("ArgumentOutOfRange_Index"));
				}
				if (strA.Length - indexA < 0)
				{
					throw new ArgumentOutOfRangeException("indexA", Environment.GetResourceString("ArgumentOutOfRange_Index"));
				}
				if (strB.Length - indexB < 0)
				{
					throw new ArgumentOutOfRangeException("indexB", Environment.GetResourceString("ArgumentOutOfRange_Index"));
				}
				if (length == 0 || (strA == strB && indexA == indexB))
				{
					return 0;
				}
				int num = length;
				int num2 = length;
				if (strA != null && strA.Length - indexA < num)
				{
					num = strA.Length - indexA;
				}
				if (strB != null && strB.Length - indexB < num2)
				{
					num2 = strB.Length - indexB;
				}
				switch (comparisonType)
				{
					case StringComparison.CurrentCulture:
					{
						return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, indexA, num, strB, indexB, num2, CompareOptions.None);
					}
					case StringComparison.CurrentCultureIgnoreCase:
					{
						return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, indexA, num, strB, indexB, num2, CompareOptions.IgnoreCase);
					}
					case StringComparison.InvariantCulture:
					{
						return CultureInfo.InvariantCulture.CompareInfo.Compare(strA, indexA, num, strB, indexB, num2, CompareOptions.None);
					}
					case StringComparison.InvariantCultureIgnoreCase:
					{
						return CultureInfo.InvariantCulture.CompareInfo.Compare(strA, indexA, num, strB, indexB, num2, CompareOptions.IgnoreCase);
					}
					case StringComparison.Ordinal:
					{
						return string.nativeCompareOrdinalEx(strA, indexA, strB, indexB, length);
					}
					case StringComparison.OrdinalIgnoreCase:
					{
						return TextInfo.CompareOrdinalIgnoreCaseEx(strA, indexA, strB, indexB, num, num2);
					}
					default:
					{
						throw new ArgumentException(Environment.GetResourceString("NotSupported_StringComparison"));
					}
				}
			}
		}
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public int CompareTo(object value)
		{
			if (value == null)
			{
				return 1;
			}
			if (!(value is string))
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeString"));
			}
			return string.Compare(this, (string)value, StringComparison.CurrentCulture);
		}
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public int CompareTo(string strB)
		{
			if (strB == null)
			{
				return 1;
			}
			return CultureInfo.CurrentCulture.CompareInfo.Compare(this, strB, CompareOptions.None);
		}
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static int CompareOrdinal(string strA, string strB)
		{
			if (strA == strB)
			{
				return 0;
			}
			if (strA == null)
			{
				return -1;
			}
			if (strB == null)
			{
				return 1;
			}
			return string.CompareOrdinalHelper(strA, strB);
		}
		[SecuritySafeCritical, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static int CompareOrdinal(string strA, int indexA, string strB, int indexB, int length)
		{
			if (strA != null && strB != null)
			{
				return string.nativeCompareOrdinalEx(strA, indexA, strB, indexB, length);
			}
			if (strA == strB)
			{
				return 0;
			}
			if (strA != null)
			{
				return 1;
			}
			return -1;
		}
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public bool Contains(string value)
		{
			return this.IndexOf(value, StringComparison.Ordinal) >= 0;
		}
		public bool EndsWith(string value)
		{
			return this.EndsWith(value, StringComparison.CurrentCulture);
		}
		[ComVisible(false), SecuritySafeCritical]
		public bool EndsWith(string value, StringComparison comparisonType)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (comparisonType < StringComparison.CurrentCulture || comparisonType > StringComparison.OrdinalIgnoreCase)
			{
				throw new ArgumentException(Environment.GetResourceString("NotSupported_StringComparison"), "comparisonType");
			}
			if (this == value)
			{
				return true;
			}
			if (value.Length == 0)
			{
				return true;
			}
			switch (comparisonType)
			{
				case StringComparison.CurrentCulture:
				{
					return CultureInfo.CurrentCulture.CompareInfo.IsSuffix(this, value, CompareOptions.None);
				}
				case StringComparison.CurrentCultureIgnoreCase:
				{
					return CultureInfo.CurrentCulture.CompareInfo.IsSuffix(this, value, CompareOptions.IgnoreCase);
				}
				case StringComparison.InvariantCulture:
				{
					return CultureInfo.InvariantCulture.CompareInfo.IsSuffix(this, value, CompareOptions.None);
				}
				case StringComparison.InvariantCultureIgnoreCase:
				{
					return CultureInfo.InvariantCulture.CompareInfo.IsSuffix(this, value, CompareOptions.IgnoreCase);
				}
				case StringComparison.Ordinal:
				{
					return this.Length >= value.Length && string.nativeCompareOrdinalEx(this, this.Length - value.Length, value, 0, value.Length) == 0;
				}
				case StringComparison.OrdinalIgnoreCase:
				{
					return this.Length >= value.Length && TextInfo.CompareOrdinalIgnoreCaseEx(this, this.Length - value.Length, value, 0, value.Length, value.Length) == 0;
				}
				default:
				{
					throw new ArgumentException(Environment.GetResourceString("NotSupported_StringComparison"), "comparisonType");
				}
			}
		}
		public bool EndsWith(string value, bool ignoreCase, CultureInfo culture)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (this == value)
			{
				return true;
			}
			CultureInfo cultureInfo;
			if (culture == null)
			{
				cultureInfo = CultureInfo.CurrentCulture;
			}
			else
			{
				cultureInfo = culture;
			}
			return cultureInfo.CompareInfo.IsSuffix(this, value, ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None);
		}
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		internal bool EndsWith(char value)
		{
			int length = this.Length;
			return length != 0 && this[length - 1] == value;
		}
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public int IndexOf(char value)
		{
			return this.IndexOf(value, 0, this.Length);
		}
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public int IndexOf(char value, int startIndex)
		{
			return this.IndexOf(value, startIndex, this.Length - startIndex);
		}
		[SecuritySafeCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public extern int IndexOf(char value, int startIndex, int count);
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public int IndexOfAny(char[] anyOf)
		{
			return this.IndexOfAny(anyOf, 0, this.Length);
		}
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public int IndexOfAny(char[] anyOf, int startIndex)
		{
			return this.IndexOfAny(anyOf, startIndex, this.Length - startIndex);
		}
		[SecuritySafeCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public extern int IndexOfAny(char[] anyOf, int startIndex, int count);
		public int IndexOf(string value)
		{
			return this.IndexOf(value, StringComparison.CurrentCulture);
		}
		public int IndexOf(string value, int startIndex)
		{
			return this.IndexOf(value, startIndex, StringComparison.CurrentCulture);
		}
		public int IndexOf(string value, int startIndex, int count)
		{
			if (startIndex < 0 || startIndex > this.Length)
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
			}
			if (count < 0 || count > this.Length - startIndex)
			{
				throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_Count"));
			}
			return this.IndexOf(value, startIndex, count, StringComparison.CurrentCulture);
		}
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), SecuritySafeCritical]
		public int IndexOf(string value, StringComparison comparisonType)
		{
			return this.IndexOf(value, 0, this.Length, comparisonType);
		}
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public int IndexOf(string value, int startIndex, StringComparison comparisonType)
		{
			return this.IndexOf(value, startIndex, this.Length - startIndex, comparisonType);
		}
		public int IndexOf(string value, int startIndex, int count, StringComparison comparisonType)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (startIndex < 0 || startIndex > this.Length)
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
			}
			if (count < 0 || startIndex > this.Length - count)
			{
				throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_Count"));
			}
			switch (comparisonType)
			{
				case StringComparison.CurrentCulture:
				{
					return CultureInfo.CurrentCulture.CompareInfo.IndexOf(this, value, startIndex, count, CompareOptions.None);
				}
				case StringComparison.CurrentCultureIgnoreCase:
				{
					return CultureInfo.CurrentCulture.CompareInfo.IndexOf(this, value, startIndex, count, CompareOptions.IgnoreCase);
				}
				case StringComparison.InvariantCulture:
				{
					return CultureInfo.InvariantCulture.CompareInfo.IndexOf(this, value, startIndex, count, CompareOptions.None);
				}
				case StringComparison.InvariantCultureIgnoreCase:
				{
					return CultureInfo.InvariantCulture.CompareInfo.IndexOf(this, value, startIndex, count, CompareOptions.IgnoreCase);
				}
				case StringComparison.Ordinal:
				{
					return CultureInfo.InvariantCulture.CompareInfo.IndexOf(this, value, startIndex, count, CompareOptions.Ordinal);
				}
				case StringComparison.OrdinalIgnoreCase:
				{
					return TextInfo.IndexOfStringOrdinalIgnoreCase(this, value, startIndex, count);
				}
				default:
				{
					throw new ArgumentException(Environment.GetResourceString("NotSupported_StringComparison"), "comparisonType");
				}
			}
		}
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public int LastIndexOf(char value)
		{
			return this.LastIndexOf(value, this.Length - 1, this.Length);
		}
		public int LastIndexOf(char value, int startIndex)
		{
			return this.LastIndexOf(value, startIndex, startIndex + 1);
		}
		[SecuritySafeCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public extern int LastIndexOf(char value, int startIndex, int count);
		public int LastIndexOfAny(char[] anyOf)
		{
			return this.LastIndexOfAny(anyOf, this.Length - 1, this.Length);
		}
		public int LastIndexOfAny(char[] anyOf, int startIndex)
		{
			return this.LastIndexOfAny(anyOf, startIndex, startIndex + 1);
		}
		[SecuritySafeCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public extern int LastIndexOfAny(char[] anyOf, int startIndex, int count);
		[SecuritySafeCritical]
		public int LastIndexOf(string value)
		{
			return this.LastIndexOf(value, this.Length - 1, this.Length, StringComparison.CurrentCulture);
		}
		public int LastIndexOf(string value, int startIndex)
		{
			return this.LastIndexOf(value, startIndex, startIndex + 1, StringComparison.CurrentCulture);
		}
		public int LastIndexOf(string value, int startIndex, int count)
		{
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_Count"));
			}
			return this.LastIndexOf(value, startIndex, count, StringComparison.CurrentCulture);
		}
		[SecuritySafeCritical, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public int LastIndexOf(string value, StringComparison comparisonType)
		{
			return this.LastIndexOf(value, this.Length - 1, this.Length, comparisonType);
		}
		public int LastIndexOf(string value, int startIndex, StringComparison comparisonType)
		{
			return this.LastIndexOf(value, startIndex, startIndex + 1, comparisonType);
		}
		public int LastIndexOf(string value, int startIndex, int count, StringComparison comparisonType)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (this.Length == 0 && (startIndex == -1 || startIndex == 0))
			{
				if (value.Length != 0)
				{
					return -1;
				}
				return 0;
			}
			else
			{
				if (startIndex < 0 || startIndex > this.Length)
				{
					throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
				}
				if (startIndex == this.Length)
				{
					startIndex--;
					if (count > 0)
					{
						count--;
					}
					if (value.Length == 0 && count >= 0 && startIndex - count + 1 >= 0)
					{
						return startIndex;
					}
				}
				if (count < 0 || startIndex - count + 1 < 0)
				{
					throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_Count"));
				}
				switch (comparisonType)
				{
					case StringComparison.CurrentCulture:
					{
						return CultureInfo.CurrentCulture.CompareInfo.LastIndexOf(this, value, startIndex, count, CompareOptions.None);
					}
					case StringComparison.CurrentCultureIgnoreCase:
					{
						return CultureInfo.CurrentCulture.CompareInfo.LastIndexOf(this, value, startIndex, count, CompareOptions.IgnoreCase);
					}
					case StringComparison.InvariantCulture:
					{
						return CultureInfo.InvariantCulture.CompareInfo.LastIndexOf(this, value, startIndex, count, CompareOptions.None);
					}
					case StringComparison.InvariantCultureIgnoreCase:
					{
						return CultureInfo.InvariantCulture.CompareInfo.LastIndexOf(this, value, startIndex, count, CompareOptions.IgnoreCase);
					}
					case StringComparison.Ordinal:
					{
						return CultureInfo.InvariantCulture.CompareInfo.LastIndexOf(this, value, startIndex, count, CompareOptions.Ordinal);
					}
					case StringComparison.OrdinalIgnoreCase:
					{
						return TextInfo.LastIndexOfStringOrdinalIgnoreCase(this, value, startIndex, count);
					}
					default:
					{
						throw new ArgumentException(Environment.GetResourceString("NotSupported_StringComparison"), "comparisonType");
					}
				}
			}
		}
		public string PadLeft(int totalWidth)
		{
			return this.PadHelper(totalWidth, ' ', false);
		}
		public string PadLeft(int totalWidth, char paddingChar)
		{
			return this.PadHelper(totalWidth, paddingChar, false);
		}
		public string PadRight(int totalWidth)
		{
			return this.PadHelper(totalWidth, ' ', true);
		}
		public string PadRight(int totalWidth, char paddingChar)
		{
			return this.PadHelper(totalWidth, paddingChar, true);
		}
		[SecuritySafeCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern string PadHelper(int totalWidth, char paddingChar, bool isRightPadded);
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public bool StartsWith(string value)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			return this.StartsWith(value, StringComparison.CurrentCulture);
		}
		[SecuritySafeCritical, ComVisible(false)]
		public bool StartsWith(string value, StringComparison comparisonType)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (comparisonType < StringComparison.CurrentCulture || comparisonType > StringComparison.OrdinalIgnoreCase)
			{
				throw new ArgumentException(Environment.GetResourceString("NotSupported_StringComparison"), "comparisonType");
			}
			if (this == value)
			{
				return true;
			}
			if (value.Length == 0)
			{
				return true;
			}
			switch (comparisonType)
			{
				case StringComparison.CurrentCulture:
				{
					return CultureInfo.CurrentCulture.CompareInfo.IsPrefix(this, value, CompareOptions.None);
				}
				case StringComparison.CurrentCultureIgnoreCase:
				{
					return CultureInfo.CurrentCulture.CompareInfo.IsPrefix(this, value, CompareOptions.IgnoreCase);
				}
				case StringComparison.InvariantCulture:
				{
					return CultureInfo.InvariantCulture.CompareInfo.IsPrefix(this, value, CompareOptions.None);
				}
				case StringComparison.InvariantCultureIgnoreCase:
				{
					return CultureInfo.InvariantCulture.CompareInfo.IsPrefix(this, value, CompareOptions.IgnoreCase);
				}
				case StringComparison.Ordinal:
				{
					return this.Length >= value.Length && string.nativeCompareOrdinalEx(this, 0, value, 0, value.Length) == 0;
				}
				case StringComparison.OrdinalIgnoreCase:
				{
					return this.Length >= value.Length && TextInfo.CompareOrdinalIgnoreCaseEx(this, 0, value, 0, value.Length, value.Length) == 0;
				}
				default:
				{
					throw new ArgumentException(Environment.GetResourceString("NotSupported_StringComparison"), "comparisonType");
				}
			}
		}
		public bool StartsWith(string value, bool ignoreCase, CultureInfo culture)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (this == value)
			{
				return true;
			}
			CultureInfo cultureInfo;
			if (culture == null)
			{
				cultureInfo = CultureInfo.CurrentCulture;
			}
			else
			{
				cultureInfo = culture;
			}
			return cultureInfo.CompareInfo.IsPrefix(this, value, ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None);
		}
		public string ToLower()
		{
			return this.ToLower(CultureInfo.CurrentCulture);
		}
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public string ToLower(CultureInfo culture)
		{
			if (culture == null)
			{
				throw new ArgumentNullException("culture");
			}
			return culture.TextInfo.ToLower(this);
		}
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public string ToLowerInvariant()
		{
			return this.ToLower(CultureInfo.InvariantCulture);
		}
		public string ToUpper()
		{
			return this.ToUpper(CultureInfo.CurrentCulture);
		}
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public string ToUpper(CultureInfo culture)
		{
			if (culture == null)
			{
				throw new ArgumentNullException("culture");
			}
			return culture.TextInfo.ToUpper(this);
		}
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public string ToUpperInvariant()
		{
			return this.ToUpper(CultureInfo.InvariantCulture);
		}
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public override string ToString()
		{
			return this;
		}
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public string ToString(IFormatProvider provider)
		{
			return this;
		}
		public object Clone()
		{
			return this;
		}
		public string Trim()
		{
			return this.TrimHelper(2);
		}
		[SecuritySafeCritical]
		private string TrimHelper(int trimType)
		{
			int num = this.Length - 1;
			int num2 = 0;
			if (trimType != 1)
			{
				num2 = 0;
				while (num2 < this.Length && char.IsWhiteSpace(this[num2]))
				{
					num2++;
				}
			}
			if (trimType != 0)
			{
				num = this.Length - 1;
				while (num >= num2 && char.IsWhiteSpace(this[num]))
				{
					num--;
				}
			}
			return this.CreateTrimmedString(num2, num);
		}
		[SecuritySafeCritical]
		private string TrimHelper(char[] trimChars, int trimType)
		{
			int i = this.Length - 1;
			int j = 0;
			if (trimType != 1)
			{
				for (j = 0; j < this.Length; j++)
				{
					int num = 0;
					char c = this[j];
					num = 0;
					while (num < trimChars.Length && trimChars[num] != c)
					{
						num++;
					}
					if (num == trimChars.Length)
					{
						break;
					}
				}
			}
			if (trimType != 0)
			{
				for (i = this.Length - 1; i >= j; i--)
				{
					int num2 = 0;
					char c2 = this[i];
					num2 = 0;
					while (num2 < trimChars.Length && trimChars[num2] != c2)
					{
						num2++;
					}
					if (num2 == trimChars.Length)
					{
						break;
					}
				}
			}
			return this.CreateTrimmedString(j, i);
		}
		[SecurityCritical]
		private string CreateTrimmedString(int start, int end)
		{
			int num = end - start + 1;
			if (num == this.Length)
			{
				return this;
			}
			if (num == 0)
			{
				return string.Empty;
			}
			return this.InternalSubString(start, num, false);
		}
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern string InsertInternal(int startIndex, string value);
		[SecuritySafeCritical]
		public string Insert(int startIndex, string value)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (startIndex < 0 || startIndex > this.Length)
			{
				throw new ArgumentOutOfRangeException("startIndex");
			}
			return this.InsertInternal(startIndex, value);
		}
		[SecuritySafeCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern string ReplaceInternal(char oldChar, char newChar);
		[SecuritySafeCritical]
		public string Replace(char oldChar, char newChar)
		{
			return this.ReplaceInternal(oldChar, newChar);
		}
		[SecuritySafeCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern string ReplaceInternal(string oldValue, string newValue);
		[SecuritySafeCritical, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public string Replace(string oldValue, string newValue)
		{
			if (oldValue == null)
			{
				throw new ArgumentNullException("oldValue");
			}
			return this.ReplaceInternal(oldValue, newValue);
		}
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern string RemoveInternal(int startIndex, int count);
		[SecuritySafeCritical]
		public string Remove(int startIndex, int count)
		{
			if (startIndex < 0)
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_StartIndex"));
			}
			return this.RemoveInternal(startIndex, count);
		}
		public string Remove(int startIndex)
		{
			if (startIndex < 0)
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_StartIndex"));
			}
			if (startIndex >= this.Length)
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_StartIndexLessThanLength"));
			}
			return this.Substring(0, startIndex);
		}
		public static string Format(string format, object arg0)
		{
			if (format == null)
			{
				throw new ArgumentNullException("format");
			}
			return string.Format(null, format, new object[]
			{
				arg0
			});
		}
		public static string Format(string format, object arg0, object arg1)
		{
			if (format == null)
			{
				throw new ArgumentNullException("format");
			}
			return string.Format(null, format, new object[]
			{
				arg0, 
				arg1
			});
		}
		public static string Format(string format, object arg0, object arg1, object arg2)
		{
			if (format == null)
			{
				throw new ArgumentNullException("format");
			}
			return string.Format(null, format, new object[]
			{
				arg0, 
				arg1, 
				arg2
			});
		}
		public static string Format(string format, params object[] args)
		{
			if (format == null || args == null)
			{
				throw new ArgumentNullException((format == null) ? "format" : "args");
			}
			return string.Format(null, format, args);
		}
		[SecuritySafeCritical]
		public static string Format(IFormatProvider provider, string format, params object[] args)
		{
			if (format == null || args == null)
			{
				throw new ArgumentNullException((format == null) ? "format" : "args");
			}
			StringBuilder stringBuilder = new StringBuilder(format.Length + args.Length * 8);
			stringBuilder.AppendFormat(provider, format, args);
			return stringBuilder.ToString();
		}
		[SecuritySafeCritical]
		public unsafe static string Copy(string str)
		{
			if (str == null)
			{
				throw new ArgumentNullException("str");
			}
			int length = str.Length;
			string text = string.FastAllocateString(length);
			fixed (char* ptr = &text.m_firstChar)
			{
				fixed (char* ptr2 = &str.m_firstChar)
				{
					string.wstrcpyPtrAligned(ptr, ptr2, length);
				}
			}
			return text;
		}
		public static string Concat(object arg0)
		{
			if (arg0 == null)
			{
				return string.Empty;
			}
			return arg0.ToString();
		}
		public static string Concat(object arg0, object arg1)
		{
			if (arg0 == null)
			{
				arg0 = string.Empty;
			}
			if (arg1 == null)
			{
				arg1 = string.Empty;
			}
			return arg0.ToString() + arg1.ToString();
		}
		public static string Concat(object arg0, object arg1, object arg2)
		{
			if (arg0 == null)
			{
				arg0 = string.Empty;
			}
			if (arg1 == null)
			{
				arg1 = string.Empty;
			}
			if (arg2 == null)
			{
				arg2 = string.Empty;
			}
			return arg0.ToString() + arg1.ToString() + arg2.ToString();
		}
		[SecuritySafeCritical, CLSCompliant(false)]
		public static string Concat(object arg0, object arg1, object arg2, object arg3, __arglist)
		{
			ArgIterator argIterator = new ArgIterator(__arglist);
			int num = argIterator.GetRemainingCount() + 4;
			object[] array = new object[num];
			array[0] = arg0;
			array[1] = arg1;
			array[2] = arg2;
			array[3] = arg3;
			for (int i = 4; i < num; i++)
			{
				array[i] = TypedReference.ToObject(argIterator.GetNextArg());
			}
			return string.Concat(array);
		}
		public static string Concat(params object[] args)
		{
			if (args == null)
			{
				throw new ArgumentNullException("args");
			}
			string[] array = new string[args.Length];
			int num = 0;
			for (int i = 0; i < args.Length; i++)
			{
				object obj = args[i];
				array[i] = ((obj == null) ? string.Empty : obj.ToString());
				if (array[i] == null)
				{
					array[i] = string.Empty;
				}
				num += array[i].Length;
				if (num < 0)
				{
					throw new OutOfMemoryException();
				}
			}
			return string.ConcatArray(array, num);
		}
		[ComVisible(false)]
		public static string Concat<T>(IEnumerable<T> values)
		{
			if (values == null)
			{
				throw new ArgumentNullException("values");
			}
			StringBuilder stringBuilder = new StringBuilder();
			using (IEnumerator<T> enumerator = values.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current != null)
					{
						T current = enumerator.Current;
						string text = current.ToString();
						if (text != null)
						{
							stringBuilder.Append(text);
						}
					}
				}
			}
			return stringBuilder.ToString();
		}
		[ComVisible(false)]
		public static string Concat(IEnumerable<string> values)
		{
			if (values == null)
			{
				throw new ArgumentNullException("values");
			}
			StringBuilder stringBuilder = new StringBuilder();
			using (IEnumerator<string> enumerator = values.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current != null)
					{
						stringBuilder.Append(enumerator.Current);
					}
				}
			}
			return stringBuilder.ToString();
		}
		[SecuritySafeCritical]
		public static string Concat(string str0, string str1)
		{
			if (string.IsNullOrEmpty(str0))
			{
				if (string.IsNullOrEmpty(str1))
				{
					return string.Empty;
				}
				return str1;
			}
			else
			{
				if (string.IsNullOrEmpty(str1))
				{
					return str0;
				}
				int length = str0.Length;
				string text = string.FastAllocateString(length + str1.Length);
				string.FillStringChecked(text, 0, str0);
				string.FillStringChecked(text, length, str1);
				return text;
			}
		}
		[SecuritySafeCritical]
		public static string Concat(string str0, string str1, string str2)
		{
			if (str0 == null && str1 == null && str2 == null)
			{
				return string.Empty;
			}
			if (str0 == null)
			{
				str0 = string.Empty;
			}
			if (str1 == null)
			{
				str1 = string.Empty;
			}
			if (str2 == null)
			{
				str2 = string.Empty;
			}
			int length = str0.Length + str1.Length + str2.Length;
			string text = string.FastAllocateString(length);
			string.FillStringChecked(text, 0, str0);
			string.FillStringChecked(text, str0.Length, str1);
			string.FillStringChecked(text, str0.Length + str1.Length, str2);
			return text;
		}
		[SecuritySafeCritical]
		public static string Concat(string str0, string str1, string str2, string str3)
		{
			if (str0 == null && str1 == null && str2 == null && str3 == null)
			{
				return string.Empty;
			}
			if (str0 == null)
			{
				str0 = string.Empty;
			}
			if (str1 == null)
			{
				str1 = string.Empty;
			}
			if (str2 == null)
			{
				str2 = string.Empty;
			}
			if (str3 == null)
			{
				str3 = string.Empty;
			}
			int length = str0.Length + str1.Length + str2.Length + str3.Length;
			string text = string.FastAllocateString(length);
			string.FillStringChecked(text, 0, str0);
			string.FillStringChecked(text, str0.Length, str1);
			string.FillStringChecked(text, str0.Length + str1.Length, str2);
			string.FillStringChecked(text, str0.Length + str1.Length + str2.Length, str3);
			return text;
		}
		[SecuritySafeCritical]
		private static string ConcatArray(string[] values, int totalLength)
		{
			string text = string.FastAllocateString(totalLength);
			int num = 0;
			for (int i = 0; i < values.Length; i++)
			{
				string.FillStringChecked(text, num, values[i]);
				num += values[i].Length;
			}
			return text;
		}
		public static string Concat(params string[] values)
		{
			if (values == null)
			{
				throw new ArgumentNullException("values");
			}
			int num = 0;
			string[] array = new string[values.Length];
			for (int i = 0; i < values.Length; i++)
			{
				string text = values[i];
				array[i] = ((text == null) ? string.Empty : text);
				num += array[i].Length;
				if (num < 0)
				{
					throw new OutOfMemoryException();
				}
			}
			return string.ConcatArray(array, num);
		}
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), SecuritySafeCritical]
		public static string Intern(string str)
		{
			if (str == null)
			{
				throw new ArgumentNullException("str");
			}
			return Thread.GetDomain().GetOrInternString(str);
		}
		[SecuritySafeCritical]
		public static string IsInterned(string str)
		{
			if (str == null)
			{
				throw new ArgumentNullException("str");
			}
			return Thread.GetDomain().IsStringInterned(str);
		}
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public TypeCode GetTypeCode()
		{
			return TypeCode.String;
		}
		bool IConvertible.ToBoolean(IFormatProvider provider)
		{
			return Convert.ToBoolean(this, provider);
		}
		char IConvertible.ToChar(IFormatProvider provider)
		{
			return Convert.ToChar(this, provider);
		}
		[SecuritySafeCritical]
		sbyte IConvertible.ToSByte(IFormatProvider provider)
		{
			return Convert.ToSByte(this, provider);
		}
		[SecuritySafeCritical]
		byte IConvertible.ToByte(IFormatProvider provider)
		{
			return Convert.ToByte(this, provider);
		}
		[SecuritySafeCritical]
		short IConvertible.ToInt16(IFormatProvider provider)
		{
			return Convert.ToInt16(this, provider);
		}
		[SecuritySafeCritical]
		ushort IConvertible.ToUInt16(IFormatProvider provider)
		{
			return Convert.ToUInt16(this, provider);
		}
		[SecuritySafeCritical]
		int IConvertible.ToInt32(IFormatProvider provider)
		{
			return Convert.ToInt32(this, provider);
		}
		[SecuritySafeCritical]
		uint IConvertible.ToUInt32(IFormatProvider provider)
		{
			return Convert.ToUInt32(this, provider);
		}
		[SecuritySafeCritical]
		long IConvertible.ToInt64(IFormatProvider provider)
		{
			return Convert.ToInt64(this, provider);
		}
		[SecuritySafeCritical]
		ulong IConvertible.ToUInt64(IFormatProvider provider)
		{
			return Convert.ToUInt64(this, provider);
		}
		[SecuritySafeCritical]
		float IConvertible.ToSingle(IFormatProvider provider)
		{
			return Convert.ToSingle(this, provider);
		}
		[SecuritySafeCritical]
		double IConvertible.ToDouble(IFormatProvider provider)
		{
			return Convert.ToDouble(this, provider);
		}
		[SecuritySafeCritical]
		decimal IConvertible.ToDecimal(IFormatProvider provider)
		{
			return Convert.ToDecimal(this, provider);
		}
		[SecuritySafeCritical]
		DateTime IConvertible.ToDateTime(IFormatProvider provider)
		{
			return Convert.ToDateTime(this, provider);
		}
		object IConvertible.ToType(Type type, IFormatProvider provider)
		{
			return Convert.DefaultToType(this, type, provider);
		}
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern bool IsFastSort();
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern bool IsAscii();
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern void SetTrailByte(byte data);
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern bool TryGetTrailByte(out byte data);
		public CharEnumerator GetEnumerator()
		{
			return new CharEnumerator(this);
		}
		IEnumerator<char> IEnumerable<char>.GetEnumerator()
		{
			return new CharEnumerator(this);
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return new CharEnumerator(this);
		}
		[ForceTokenStabilization, SecurityCritical]
		internal unsafe static void InternalCopy(string src, IntPtr dest, int len)
		{
			if (len == 0)
			{
				return;
			}
			fixed (char* ptr = &src.m_firstChar)
			{
				byte* src2 = (byte*)ptr;
				byte* dest2 = (byte*)dest.ToPointer();
				Buffer.memcpyimpl(src2, dest2, len);
			}
		}
	}
}
