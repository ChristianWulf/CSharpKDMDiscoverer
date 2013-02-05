using System;
using System.Collections.Generic;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
namespace System
{
	[TypeDependency("System.Collections.Generic.NullableComparer`1"), TypeDependency("System.Collections.Generic.NullableEqualityComparer`1")]
	[Serializable]
	public struct Nullable<T> where T : struct
	{
		private bool hasValue;
		internal T value;
		public bool HasValue
		{
			get
			{
				return this.hasValue;
			}
		}
		public T Value
		{
			[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
			get
			{
				if (!this.HasValue)
				{
					ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_NoValue);
				}
				return this.value;
			}
		}
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public Nullable(T value)
		{
			this.value = value;
			this.hasValue = true;
		}
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public T GetValueOrDefault()
		{
			return this.value;
		}
		public T GetValueOrDefault(T defaultValue)
		{
			if (!this.HasValue)
			{
				return defaultValue;
			}
			return this.value;
		}
		public override bool Equals(object other)
		{
			if (!this.HasValue)
			{
				return other == null;
			}
			return other != null && this.value.Equals(other);
		}
		public override int GetHashCode()
		{
			if (!this.HasValue)
			{
				return 0;
			}
			return this.value.GetHashCode();
		}
		public override string ToString()
		{
			if (!this.HasValue)
			{
				return "";
			}
			return this.value.ToString();
		}
		public static implicit operator T?(T value)
		{
			return new T?(value);
		}
		public static explicit operator T(T? value)
		{
			return value.Value;
		}
	}
	[ComVisible(true)]
	public static class Nullable
	{
		[ComVisible(true)]
		public static int Compare<T>(T? n1, T? n2) where T : struct
		{
			if (n1.HasValue)
			{
				if (n2.HasValue)
				{
					return Comparer<T>.Default.Compare(n1.value, n2.value);
				}
				return 1;
			}
			else
			{
				if (n2.HasValue)
				{
					return -1;
				}
				return 0;
			}
		}
		[ComVisible(true)]
		public static bool Equals<T>(T? n1, T? n2) where T : struct
		{
			if (n1.HasValue)
			{
				return n2.HasValue && EqualityComparer<T>.Default.Equals(n1.value, n2.value);
			}
			return !n2.HasValue;
		}
		public static Type GetUnderlyingType(Type nullableType)
		{
			if (nullableType == null)
			{
				throw new ArgumentNullException("nullableType");
			}
			Type result = null;
			if (nullableType.IsGenericType && !nullableType.IsGenericTypeDefinition)
			{
				Type genericTypeDefinition = nullableType.GetGenericTypeDefinition();
				if (object.ReferenceEquals(genericTypeDefinition, typeof(Nullable<>)))
				{
					result = nullableType.GetGenericArguments()[0];
				}
			}
			return result;
		}
	}
}
