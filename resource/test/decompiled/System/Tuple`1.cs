using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace System
{
[Serializable]
public class Tuple<T1> : IStructuralEquatable, IStructuralComparable, IComparable, ITuple
{
	private readonly T1 m_Item1;

	public T1 Item1
	{
		get
		{
			return this.m_Item1;
		}
	}

	private int System.ITuple.Size
	{
		get
		{
			return 1;
		}
	}

	public Tuple(T1 item1)
	{
		this.m_Item1 = item1;
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
		Tuple<T1> tuple = other as Tuple<T1>;
		if (tuple == null)
		{
			throw new ArgumentException(Environment.GetResourceString("ArgumentException_TupleIncorrectType", new object[] { this.GetType().ToString() }), "other");
		}
		return comparer.Compare(this.m_Item1, tuple.m_Item1);
	}

	private bool System.Collections.IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
	{
		if (other == null)
		{
			return false;
		}
		Tuple<T1> tuple = other as Tuple<T1>;
		if (tuple == null)
		{
			return false;
		}
		return comparer.Equals(this.m_Item1, tuple.m_Item1);
	}

	private int System.Collections.IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
	{
		return comparer.GetHashCode(this.m_Item1);
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
		sb.Append(")");
		return sb.ToString();
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("(");
		return this.ToString(stringBuilder);
	}
}
}