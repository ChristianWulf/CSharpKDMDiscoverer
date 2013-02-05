using System;
namespace System.Collections.Generic
{
	[Serializable]
	internal class GenericArraySortHelper<T> : IArraySortHelper<T> where T : IComparable<T>
	{
		public void Sort(T[] keys, int index, int length, IComparer<T> comparer)
		{
			try
			{
				if (comparer == null || comparer == Comparer<T>.Default)
				{
					GenericArraySortHelper<T>.QuickSort(keys, index, index + (length - 1));
				}
				else
				{
					ArraySortHelper<T>.QuickSort(keys, index, index + (length - 1), comparer);
				}
			}
			catch (IndexOutOfRangeException)
			{
				string arg_5C_0 = "Arg_BogusIComparer";
				object[] array = new object[3];
				array[0] = default(T);
				array[1] = typeof(T).Name;
				throw new ArgumentException(Environment.GetResourceString(arg_5C_0, array));
			}
			catch (Exception innerException)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_IComparerFailed"), innerException);
			}
		}
		public int BinarySearch(T[] array, int index, int length, T value, IComparer<T> comparer)
		{
			int result;
			try
			{
				if (comparer == null || comparer == Comparer<T>.Default)
				{
					result = GenericArraySortHelper<T>.BinarySearch(array, index, length, value);
				}
				else
				{
					result = ArraySortHelper<T>.InternalBinarySearch(array, index, length, value, comparer);
				}
			}
			catch (Exception innerException)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_IComparerFailed"), innerException);
			}
			return result;
		}
		private static int BinarySearch(T[] array, int index, int length, T value)
		{
			int i = index;
			int num = index + length - 1;
			while (i <= num)
			{
				int num2 = i + (num - i >> 1);
				int num3;
				if (array[num2] == null)
				{
					num3 = ((value == null) ? 0 : -1);
				}
				else
				{
					num3 = array[num2].CompareTo(value);
				}
				if (num3 == 0)
				{
					return num2;
				}
				if (num3 < 0)
				{
					i = num2 + 1;
				}
				else
				{
					num = num2 - 1;
				}
			}
			return ~i;
		}
		private static void SwapIfGreaterWithItems(T[] keys, int a, int b)
		{
			if (a != b && (keys[a] == null || keys[a].CompareTo(keys[b]) > 0))
			{
				T t = keys[a];
				keys[a] = keys[b];
				keys[b] = t;
			}
		}
		private static void QuickSort(T[] keys, int left, int right)
		{
			do
			{
				int num = left;
				int num2 = right;
				int num3 = num + (num2 - num >> 1);
				GenericArraySortHelper<T>.SwapIfGreaterWithItems(keys, num, num3);
				GenericArraySortHelper<T>.SwapIfGreaterWithItems(keys, num, num2);
				GenericArraySortHelper<T>.SwapIfGreaterWithItems(keys, num3, num2);
				T t = keys[num3];
				do
				{
					if (t == null)
					{
						while (keys[num2] != null)
						{
							num2--;
						}
					}
					else
					{
						while (t.CompareTo(keys[num]) > 0)
						{
							num++;
						}
						while (t.CompareTo(keys[num2]) < 0)
						{
							num2--;
						}
					}
					if (num > num2)
					{
						break;
					}
					if (num < num2)
					{
						T t2 = keys[num];
						keys[num] = keys[num2];
						keys[num2] = t2;
					}
					num++;
					num2--;
				}
				while (num <= num2);
				if (num2 - left <= right - num)
				{
					if (left < num2)
					{
						GenericArraySortHelper<T>.QuickSort(keys, left, num2);
					}
					left = num;
				}
				else
				{
					if (num < right)
					{
						GenericArraySortHelper<T>.QuickSort(keys, num, right);
					}
					right = num2;
				}
			}
			while (left < right);
		}
	}
	internal class GenericArraySortHelper<TKey, TValue> : IArraySortHelper<TKey, TValue> where TKey : IComparable<TKey>
	{
		public void Sort(TKey[] keys, TValue[] values, int index, int length, IComparer<TKey> comparer)
		{
			try
			{
				if (comparer == null || comparer == Comparer<TKey>.Default)
				{
					GenericArraySortHelper<TKey, TValue>.QuickSort(keys, values, index, index + length - 1);
				}
				else
				{
					ArraySortHelper<TKey, TValue>.QuickSort(keys, values, index, index + length - 1, comparer);
				}
			}
			catch (IndexOutOfRangeException)
			{
				string arg_4F_0 = "Arg_BogusIComparer";
				object[] array = new object[3];
				array[1] = typeof(TKey).Name;
				throw new ArgumentException(Environment.GetResourceString(arg_4F_0, array));
			}
			catch (Exception innerException)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_IComparerFailed"), innerException);
			}
		}
		private static void SwapIfGreaterWithItems(TKey[] keys, TValue[] values, int a, int b)
		{
			if (a != b && (keys[a] == null || keys[a].CompareTo(keys[b]) > 0))
			{
				TKey tKey = keys[a];
				keys[a] = keys[b];
				keys[b] = tKey;
				if (values != null)
				{
					TValue tValue = values[a];
					values[a] = values[b];
					values[b] = tValue;
				}
			}
		}
		private static void QuickSort(TKey[] keys, TValue[] values, int left, int right)
		{
			do
			{
				int num = left;
				int num2 = right;
				int num3 = num + (num2 - num >> 1);
				GenericArraySortHelper<TKey, TValue>.SwapIfGreaterWithItems(keys, values, num, num3);
				GenericArraySortHelper<TKey, TValue>.SwapIfGreaterWithItems(keys, values, num, num2);
				GenericArraySortHelper<TKey, TValue>.SwapIfGreaterWithItems(keys, values, num3, num2);
				TKey tKey = keys[num3];
				do
				{
					if (tKey == null)
					{
						while (keys[num2] != null)
						{
							num2--;
						}
					}
					else
					{
						while (tKey.CompareTo(keys[num]) > 0)
						{
							num++;
						}
						while (tKey.CompareTo(keys[num2]) < 0)
						{
							num2--;
						}
					}
					if (num > num2)
					{
						break;
					}
					if (num < num2)
					{
						TKey tKey2 = keys[num];
						keys[num] = keys[num2];
						keys[num2] = tKey2;
						if (values != null)
						{
							TValue tValue = values[num];
							values[num] = values[num2];
							values[num2] = tValue;
						}
					}
					num++;
					num2--;
				}
				while (num <= num2);
				if (num2 - left <= right - num)
				{
					if (left < num2)
					{
						GenericArraySortHelper<TKey, TValue>.QuickSort(keys, values, left, num2);
					}
					left = num;
				}
				else
				{
					if (num < right)
					{
						GenericArraySortHelper<TKey, TValue>.QuickSort(keys, values, num, right);
					}
					right = num2;
				}
			}
			while (left < right);
		}
	}
}
