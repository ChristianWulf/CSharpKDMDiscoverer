using System;
using System.Globalization;
using System.Runtime;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
namespace System
{
	[ComVisible(true)]
	[Serializable]
	public struct IntPtr : ISerializable
	{
		private unsafe void* m_value;
		public static readonly IntPtr Zero;
		public static int Size
		{
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
			get
			{
				return 4;
			}
		}
		[SecuritySafeCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		internal bool IsNull()
		{
			return this.m_value == null;
		}
		[ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail), TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), SecuritySafeCritical]
		public IntPtr(int value)
		{
			this.m_value = value;
		}
		[ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail), TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), SecuritySafeCritical]
		public IntPtr(long value)
		{
			this.m_value = checked((int)value);
		}
		[CLSCompliant(false), ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail), SecurityCritical]
		public unsafe IntPtr(void* value)
		{
			this.m_value = value;
		}
		[SecurityCritical]
		private IntPtr(SerializationInfo info, StreamingContext context)
		{
			long @int = info.GetInt64("value");
			if (IntPtr.Size == 4 && (@int > 2147483647L || @int < -2147483648L))
			{
				throw new ArgumentException(Environment.GetResourceString("Serialization_InvalidPtrValue"));
			}
			this.m_value = @int;
		}
		[SecurityCritical]
		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			info.AddValue("value", (long)this.m_value);
		}
		[SecuritySafeCritical]
		public override bool Equals(object obj)
		{
			return obj is IntPtr && this.m_value == ((IntPtr)obj).m_value;
		}
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), SecuritySafeCritical]
		public override int GetHashCode()
		{
			return this.m_value;
		}
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), SecuritySafeCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		public int ToInt32()
		{
			return this.m_value;
		}
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), SecuritySafeCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		public long ToInt64()
		{
			return (long)this.m_value;
		}
		[SecuritySafeCritical]
		public override string ToString()
		{
			return this.m_value.ToString(CultureInfo.InvariantCulture);
		}
		[SecuritySafeCritical]
		public string ToString(string format)
		{
			return this.m_value.ToString(format, CultureInfo.InvariantCulture);
		}
		[ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail), TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static explicit operator IntPtr(int value)
		{
			return new IntPtr(value);
		}
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
		public static explicit operator IntPtr(long value)
		{
			return new IntPtr(value);
		}
		[SecurityCritical, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), CLSCompliant(false), ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
		public unsafe static explicit operator IntPtr(void* value)
		{
			return new IntPtr(value);
		}
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), CLSCompliant(false), SecuritySafeCritical]
		public unsafe static explicit operator void*(IntPtr value)
		{
			return value.ToPointer();
		}
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), SecuritySafeCritical]
		public static explicit operator int(IntPtr value)
		{
			return value.m_value;
		}
		[SecuritySafeCritical, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static explicit operator long(IntPtr value)
		{
			return (long)value.m_value;
		}
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), SecuritySafeCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		public static bool operator ==(IntPtr value1, IntPtr value2)
		{
			return value1.m_value == value2.m_value;
		}
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), SecuritySafeCritical]
		public static bool operator !=(IntPtr value1, IntPtr value2)
		{
			return value1.m_value != value2.m_value;
		}
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), SecuritySafeCritical, ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
		public static IntPtr Add(IntPtr pointer, int offset)
		{
			return pointer + offset;
		}
		[SecuritySafeCritical, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
		public static IntPtr operator +(IntPtr pointer, int offset)
		{
			return new IntPtr(pointer.ToInt32() + offset);
		}
		[SecuritySafeCritical, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
		public static IntPtr Subtract(IntPtr pointer, int offset)
		{
			return pointer - offset;
		}
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), SecuritySafeCritical, ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
		public static IntPtr operator -(IntPtr pointer, int offset)
		{
			return new IntPtr(pointer.ToInt32() - offset);
		}
		[SecuritySafeCritical, CLSCompliant(false), ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public unsafe void* ToPointer()
		{
			return this.m_value;
		}
	}
}
