using System;
namespace System.Collections.Generic
{
	[Serializable]
	internal class NullableEqualityComparer<T> : EqualityComparer<T?> where T : struct, IEquatable<T>
	{
		public override bool Equals(T? x, T? y)
		{
			if (x.HasValue)
			{
				return y.HasValue && x.value.Equals(y.value);
			}
			return !y.HasValue;
		}
		public override int GetHashCode(T? obj)
		{
			return obj.GetHashCode();
		}
		internal override int IndexOf(T?[] array, T? value, int startIndex, int count)
		{
			int num = startIndex + count;
			if (!value.HasValue)
			{
				for (int i = startIndex; i < num; i++)
				{
					if (!array[i].HasValue)
					{
						return i;
					}
				}
			}
			else
			{
				for (int j = startIndex; j < num; j++)
				{
					if (array[j].HasValue && array[j].value.Equals(value.value))
					{
						return j;
					}
				}
			}
			return -1;
		}
		internal override int LastIndexOf(T?[] array, T? value, int startIndex, int count)
		{
			int num = startIndex - count + 1;
			if (!value.HasValue)
			{
				for (int i = startIndex; i >= num; i--)
				{
					if (!array[i].HasValue)
					{
						return i;
					}
				}
			}
			else
			{
				for (int j = startIndex; j >= num; j--)
				{
					if (array[j].HasValue && array[j].value.Equals(value.value))
					{
						return j;
					}
				}
			}
			return -1;
		}
		public override bool Equals(object obj)
		{
			NullableEqualityComparer<T> nullableEqualityComparer = obj as NullableEqualityComparer<T>;
			return nullableEqualityComparer != null;
		}
		public override int GetHashCode()
		{
			return base.GetType().Name.GetHashCode();
		}
	}
}
