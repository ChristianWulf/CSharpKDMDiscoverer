using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
namespace System
{
	[CLSCompliant(false), ComVisible(true)]
	[Serializable]
	public struct UIntPtr : ISerializable
	{
		private unsafe void* m_value;
		public static readonly UIntPtr Zero;
		public static int Size
		{
			get
			{
				return 4;
			}
		}
		[SecuritySafeCritical]
		public UIntPtr(uint value)
		{
			this.m_value = value;
		}
		[SecuritySafeCritical]
		public UIntPtr(ulong value)
		{
			this.m_value = checked((uint)value);
		}
		[SecurityCritical, CLSCompliant(false)]
		public unsafe UIntPtr(void* value)
		{
			this.m_value = value;
		}
		[SecurityCritical]
		private UIntPtr(SerializationInfo info, StreamingContext context)
		{
			ulong uInt = info.GetUInt64("value");
			if (UIntPtr.Size == 4 && uInt > (ulong)-1)
			{
				throw new ArgumentException(Environment.GetResourceString("Serialization_InvalidPtrValue"));
			}
			this.m_value = uInt;
		}
		[SecurityCritical]
		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			info.AddValue("value", this.m_value);
		}
		[SecuritySafeCritical]
		public override bool Equals(object obj)
		{
			return obj is UIntPtr && this.m_value == ((UIntPtr)obj).m_value;
		}
		[SecuritySafeCritical]
		public override int GetHashCode()
		{
			return this.m_value & 2147483647;
		}
		[SecuritySafeCritical]
		public uint ToUInt32()
		{
			return this.m_value;
		}
		[SecuritySafeCritical]
		public ulong ToUInt64()
		{
			return this.m_value;
		}
		[SecuritySafeCritical]
		public override string ToString()
		{
			return this.m_value.ToString(CultureInfo.InvariantCulture);
		}
		public static explicit operator UIntPtr(uint value)
		{
			return new UIntPtr(value);
		}
		public static explicit operator UIntPtr(ulong value)
		{
			return new UIntPtr(value);
		}
		[SecuritySafeCritical]
		public static explicit operator uint(UIntPtr value)
		{
			return value.m_value;
		}
		[SecuritySafeCritical]
		public static explicit operator ulong(UIntPtr value)
		{
			return value.m_value;
		}
		[CLSCompliant(false), SecurityCritical]
		public unsafe static explicit operator UIntPtr(void* value)
		{
			return new UIntPtr(value);
		}
		[SecurityCritical, CLSCompliant(false)]
		public unsafe static explicit operator void*(UIntPtr value)
		{
			return value.ToPointer();
		}
		[SecuritySafeCritical]
		public static bool operator ==(UIntPtr value1, UIntPtr value2)
		{
			return value1.m_value == value2.m_value;
		}
		[SecuritySafeCritical]
		public static bool operator !=(UIntPtr value1, UIntPtr value2)
		{
			return value1.m_value != value2.m_value;
		}
		[SecuritySafeCritical]
		public static UIntPtr Add(UIntPtr pointer, int offset)
		{
			return pointer + offset;
		}
		[SecuritySafeCritical]
		public static UIntPtr operator +(UIntPtr pointer, int offset)
		{
			return new UIntPtr(pointer.ToUInt32() + (uint)offset);
		}
		[SecuritySafeCritical]
		public static UIntPtr Subtract(UIntPtr pointer, int offset)
		{
			return pointer - offset;
		}
		[SecuritySafeCritical]
		public static UIntPtr operator -(UIntPtr pointer, int offset)
		{
			return new UIntPtr(pointer.ToUInt32() - (uint)offset);
		}
		[CLSCompliant(false), SecuritySafeCritical]
		public unsafe void* ToPointer()
		{
			return this.m_value;
		}
	}
}
