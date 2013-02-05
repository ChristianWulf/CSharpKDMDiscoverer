using System;
using System.Runtime.CompilerServices;
using System.Security;
namespace System.Collections.Generic
{
	[Serializable]
	internal sealed class EnumEqualityComparer<T> : EqualityComparer<T> where T : struct
	{
		[SecuritySafeCritical]
		public override bool Equals(T x, T y)
		{
			int num = JitHelpers.UnsafeEnumCast<T>(x);
			int num2 = JitHelpers.UnsafeEnumCast<T>(y);
			return num == num2;
		}
		[SecuritySafeCritical]
		public override int GetHashCode(T obj)
		{
			return JitHelpers.UnsafeEnumCast<T>(obj).GetHashCode();
		}
		public override bool Equals(object obj)
		{
			EnumEqualityComparer<T> enumEqualityComparer = obj as EnumEqualityComparer<T>;
			return enumEqualityComparer != null;
		}
		public override int GetHashCode()
		{
			return base.GetType().Name.GetHashCode();
		}
	}
}
