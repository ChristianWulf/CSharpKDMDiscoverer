namespace System
{
[Serializable]
public struct ArraySegment<T>
{
	private T[] _array;

	private int _count;

	private int _offset;

	public T[] Array
	{
		get
		{
			return this._array;
		}
	}

	public int Count
	{
		get
		{
			return this._count;
		}
	}

	public int Offset
	{
		get
		{
			return this._offset;
		}
	}

	public ArraySegment(T[] array)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		this._array = array;
		this._offset = 0;
		this._count = (int)array.Length;
	}

	public ArraySegment(T[] array, int offset, int count)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
		}
		if ((int)array.Length - offset < count)
		{
			throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
		}
		this._array = array;
		this._offset = offset;
		this._count = count;
	}

	public override bool Equals(object obj)
	{
		if (obj as ArraySegment<T>)
		{
			return base.Equals((ArraySegment<T>)obj);
		}
		return false;
	}

	public bool Equals(ArraySegment<T> obj)
	{
		if (&obj._array == this._array && &obj._offset == this._offset)
		{
			return &obj._count == this._count;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return this._array.GetHashCode() ^ this._offset ^ this._count;
	}

	public static bool operator ==(ArraySegment<T> a, ArraySegment<T> b)
	{
		return &a.Equals(b);
	}

	public static bool operator !=(ArraySegment<T> a, ArraySegment<T> b)
	{
		return !(a == b);
	}
}
}