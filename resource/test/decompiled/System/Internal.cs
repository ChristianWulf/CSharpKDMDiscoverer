using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
namespace System
{
	internal static class Internal
	{
		private static void CommonlyUsedGenericInstantiations_HACK()
		{
			Array.Sort<double>(null);
			Array.Sort<int>(null);
			Array.Sort<IntPtr>(null);
			new ArraySegment<byte>(new byte[1], 0, 0);
			new Dictionary<char, object>();
			new Dictionary<Guid, byte>();
			new Dictionary<Guid, object>();
			new Dictionary<Guid, Guid>();
			new Dictionary<short, IntPtr>();
			new Dictionary<int, byte>();
			new Dictionary<int, int>();
			new Dictionary<int, object>();
			new Dictionary<IntPtr, bool>();
			new Dictionary<IntPtr, short>();
			new Dictionary<object, bool>();
			new Dictionary<object, char>();
			new Dictionary<object, Guid>();
			new Dictionary<object, int>();
			new Dictionary<object, long>();
			new Dictionary<uint, WeakReference>();
			new Dictionary<object, uint>();
			new Dictionary<uint, object>();
			new Dictionary<long, object>();
			new Dictionary<MemberTypes, object>();
			new EnumEqualityComparer<MemberTypes>();
			new Dictionary<object, KeyValuePair<object, object>>();
			new Dictionary<KeyValuePair<object, object>, object>();
			Internal.NullableHelper_HACK<bool>();
			Internal.NullableHelper_HACK<byte>();
			Internal.NullableHelper_HACK<char>();
			Internal.NullableHelper_HACK<DateTime>();
			Internal.NullableHelper_HACK<decimal>();
			Internal.NullableHelper_HACK<double>();
			Internal.NullableHelper_HACK<Guid>();
			Internal.NullableHelper_HACK<short>();
			Internal.NullableHelper_HACK<int>();
			Internal.NullableHelper_HACK<long>();
			Internal.NullableHelper_HACK<float>();
			Internal.NullableHelper_HACK<TimeSpan>();
			Internal.NullableHelper_HACK<DateTimeOffset>();
			new List<bool>();
			new List<byte>();
			new List<char>();
			new List<DateTime>();
			new List<decimal>();
			new List<double>();
			new List<Guid>();
			new List<short>();
			new List<int>();
			new List<long>();
			new List<TimeSpan>();
			new List<sbyte>();
			new List<float>();
			new List<ushort>();
			new List<uint>();
			new List<ulong>();
			new List<IntPtr>();
			new List<KeyValuePair<object, object>>();
			new List<GCHandle>();
			new List<DateTimeOffset>();
			RuntimeType.RuntimeTypeCache.Prejitinit_HACK();
			new CerArrayList<RuntimeMethodInfo>(0);
			new CerArrayList<RuntimeConstructorInfo>(0);
			new CerArrayList<RuntimePropertyInfo>(0);
			new CerArrayList<RuntimeEventInfo>(0);
			new CerArrayList<RuntimeFieldInfo>(0);
			new CerArrayList<RuntimeType>(0);
			new KeyValuePair<char, ushort>('\0', 0);
			new KeyValuePair<ushort, double>(0, -1.7976931348623157E+308);
			new KeyValuePair<object, int>(string.Empty, -2147483648);
			new KeyValuePair<int, int>(-2147483648, -2147483648);
			Internal.SZArrayHelper_HACK<bool>(null);
			Internal.SZArrayHelper_HACK<byte>(null);
			Internal.SZArrayHelper_HACK<DateTime>(null);
			Internal.SZArrayHelper_HACK<decimal>(null);
			Internal.SZArrayHelper_HACK<double>(null);
			Internal.SZArrayHelper_HACK<Guid>(null);
			Internal.SZArrayHelper_HACK<short>(null);
			Internal.SZArrayHelper_HACK<int>(null);
			Internal.SZArrayHelper_HACK<long>(null);
			Internal.SZArrayHelper_HACK<TimeSpan>(null);
			Internal.SZArrayHelper_HACK<sbyte>(null);
			Internal.SZArrayHelper_HACK<float>(null);
			Internal.SZArrayHelper_HACK<ushort>(null);
			Internal.SZArrayHelper_HACK<uint>(null);
			Internal.SZArrayHelper_HACK<ulong>(null);
			Internal.SZArrayHelper_HACK<DateTimeOffset>(null);
			Internal.SZArrayHelper_HACK<CustomAttributeTypedArgument>(null);
			Internal.SZArrayHelper_HACK<CustomAttributeNamedArgument>(null);
		}
		private static T NullableHelper_HACK<T>() where T : struct
		{
			Nullable.Compare<T>(null, null);
			Nullable.Equals<T>(null, null);
			return ((T?)null).GetValueOrDefault();
		}
		private static void SZArrayHelper_HACK<T>(SZArrayHelper oSZArrayHelper)
		{
			int arg_06_0 = oSZArrayHelper.get_Count<T>();
			T arg_0E_0 = oSZArrayHelper.get_Item<T>(0);
			oSZArrayHelper.GetEnumerator<T>();
		}
	}
}
