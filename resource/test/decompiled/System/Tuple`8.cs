using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace System
{
[Serializable]
public class Tuple<T1, T2, T3, T4, T5, T6, T7, TRest> : IStructuralEquatable, IStructuralComparable, IComparable, ITuple
{
	private readonly T1 m_Item1;

	private readonly T2 m_Item2;

	private readonly T3 m_Item3;

	private readonly T4 m_Item4;

	private readonly T5 m_Item5;

	private readonly T6 m_Item6;

	private readonly T7 m_Item7;

	private readonly TRest m_Rest;

	public T1 Item1
	{
		get
		{
			return this.m_Item1;
		}
	}

	public T2 Item2
	{
		get
		{
			return this.m_Item2;
		}
	}

	public T3 Item3
	{
		get
		{
			return this.m_Item3;
		}
	}

	public T4 Item4
	{
		get
		{
			return this.m_Item4;
		}
	}

	public T5 Item5
	{
		get
		{
			return this.m_Item5;
		}
	}

	public T6 Item6
	{
		get
		{
			return this.m_Item6;
		}
	}

	public T7 Item7
	{
		get
		{
			return this.m_Item7;
		}
	}

	public TRest Rest
	{
		get
		{
			return this.m_Rest;
		}
	}

	private int System.ITuple.Size
	{
		get
		{
			return 7 + (ITuple)this.m_Rest.Size;
		}
	}

	public Tuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, TRest rest)
	{
		if (!rest as ITuple)
		{
			throw new ArgumentException(Environment.GetResourceString("ArgumentException_TupleLastArgumentNotATuple"));
		}
		this.m_Item1 = item1;
		this.m_Item2 = item2;
		this.m_Item3 = item3;
		this.m_Item4 = item4;
		this.m_Item5 = item5;
		this.m_Item6 = item6;
		this.m_Item7 = item7;
		this.m_Rest = rest;
	}

	public override bool Equals(object obj)
	{
		return this.Equals(obj, EqualityComparer<object>.Default);
	}

	public override int GetHashCode()
	{
		return this.GetHashCode(EqualityComparer<object>.Default);
	}

	private int System.Collections.IStructuralComparable.CompareTo(object other, IComparer comparer)
	{
		object[] objArray;
		if (other == null)
		{
			return 1;
		}
		Tuple<T1, T2, T3, T4, T5, T6, T7, TRest> tuple = other as Tuple<T1, T2, T3, T4, T5, T6, T7, TRest>;
		if (tuple == null)
		{
			throw new ArgumentException(Environment.GetResourceString("ArgumentException_TupleIncorrectType", new object[] { this.GetType().ToString() }), "other");
		}
		int num = 0;
		num = comparer.Compare(this.m_Item1, tuple.m_Item1);
		if (num != 0)
		{
			return num;
		}
		num = comparer.Compare(this.m_Item2, tuple.m_Item2);
		if (num != 0)
		{
			return num;
		}
		num = comparer.Compare(this.m_Item3, tuple.m_Item3);
		if (num != 0)
		{
			return num;
		}
		num = comparer.Compare(this.m_Item4, tuple.m_Item4);
		if (num != 0)
		{
			return num;
		}
		num = comparer.Compare(this.m_Item5, tuple.m_Item5);
		if (num != 0)
		{
			return num;
		}
		num = comparer.Compare(this.m_Item6, tuple.m_Item6);
		if (num != 0)
		{
			return num;
		}
		num = comparer.Compare(this.m_Item7, tuple.m_Item7);
		if (num != 0)
		{
			return num;
		}
		return comparer.Compare(this.m_Rest, tuple.m_Rest);
	}

	private bool System.Collections.IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
	{
		if (other == null)
		{
			return false;
		}
		Tuple<T1, T2, T3, T4, T5, T6, T7, TRest> tuple = other as Tuple<T1, T2, T3, T4, T5, T6, T7, TRest>;
		if (tuple == null)
		{
			return false;
		}
		if (comparer.Equals(this.m_Item1, tuple.m_Item1) && comparer.Equals(this.m_Item2, tuple.m_Item2) && comparer.Equals(this.m_Item3, tuple.m_Item3) && comparer.Equals(this.m_Item4, tuple.m_Item4) && comparer.Equals(this.m_Item5, tuple.m_Item5) && comparer.Equals(this.m_Item6, tuple.m_Item6) && comparer.Equals(this.m_Item7, tuple.m_Item7))
		{
			return comparer.Equals(this.m_Rest, tuple.m_Rest);
		}
		return false;
	}

	private int System.Collections.IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
	{
		ITuple mRest = (ITuple)this.m_Rest;
		if (mRest.Size >= 8)
		{
			return mRest.GetHashCode(comparer);
		}
		int size = 8 - mRest.Size;
		switch (size)
		{
			case 0:
			{
				return Tuple.CombineHashCodes(comparer.GetHashCode(this.m_Item7), mRest.GetHashCode(comparer));
			}
			case 1:
			{
				return Tuple.CombineHashCodes(comparer.GetHashCode(this.m_Item6), comparer.GetHashCode(this.m_Item7), mRest.GetHashCode(comparer));
			}
			case 2:
			{
				return Tuple.CombineHashCodes(comparer.GetHashCode(this.m_Item5), comparer.GetHashCode(this.m_Item6), comparer.GetHashCode(this.m_Item7), mRest.GetHashCode(comparer));
			}
			case 3:
			{
				return Tuple.CombineHashCodes(comparer.GetHashCode(this.m_Item4), comparer.GetHashCode(this.m_Item5), comparer.GetHashCode(this.m_Item6), comparer.GetHashCode(this.m_Item7), mRest.GetHashCode(comparer));
			}
			case 4:
			{
				return Tuple.CombineHashCodes(comparer.GetHashCode(this.m_Item3), comparer.GetHashCode(this.m_Item4), comparer.GetHashCode(this.m_Item5), comparer.GetHashCode(this.m_Item6), comparer.GetHashCode(this.m_Item7), mRest.GetHashCode(comparer));
			}
			case 5:
			{
				return Tuple.CombineHashCodes(comparer.GetHashCode(this.m_Item2), comparer.GetHashCode(this.m_Item3), comparer.GetHashCode(this.m_Item4), comparer.GetHashCode(this.m_Item5), comparer.GetHashCode(this.m_Item6), comparer.GetHashCode(this.m_Item7), mRest.GetHashCode(comparer));
			}
			case 6:
			{
				return Tuple.CombineHashCodes(comparer.GetHashCode(this.m_Item1), comparer.GetHashCode(this.m_Item2), comparer.GetHashCode(this.m_Item3), comparer.GetHashCode(this.m_Item4), comparer.GetHashCode(this.m_Item5), comparer.GetHashCode(this.m_Item6), comparer.GetHashCode(this.m_Item7), mRest.GetHashCode(comparer));
			}
		}
		return -1;
	}

	private int System.IComparable.CompareTo(object obj)
	{
		return this.CompareTo(obj, Comparer<object>.Default);
	}

	private int System.ITuple.GetHashCode(IEqualityComparer comparer)
	{
		return this.GetHashCode(comparer);
	}

	private string System.ITuple.ToString(StringBuilder sb)
	{
		sb.Append(this.m_Item1);
		sb.Append(", ");
		sb.Append(this.m_Item2);
		sb.Append(", ");
		sb.Append(this.m_Item3);
		sb.Append(", ");
		sb.Append(this.m_Item4);
		sb.Append(", ");
		sb.Append(this.m_Item5);
		sb.Append(", ");
		sb.Append(this.m_Item6);
		sb.Append(", ");
		sb.Append(this.m_Item7);
		sb.Append(", ");
		return (ITuple)this.m_Rest.ToString(sb);
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("(");
		return this.ToString(stringBuilder);
	}
}
}