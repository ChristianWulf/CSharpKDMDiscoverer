using System.Runtime.CompilerServices;
using System.Runtime;

namespace System
{
[TypeDependency("System.Collections.Generic.NullableComparer`1")]
[TypeDependency("System.Collections.Generic.NullableEqualityComparer`1")]
[Serializable]
public struct Nullable<T>
{
	private bool hasValue;

	internal T @value;

	public bool HasValue
	{
		get
		{
			return this.hasValue;
		}
	}

	public T Value
	{
		get
		{
			if (!base.HasValue)
			{
				ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_NoValue);
			}
			return this.@value;
		}
	}

	[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
	public Nullable(T value)
	{
		this.@value = value;
		this.hasValue = true;
	}

	public override bool Equals(object other)
	{
		if (!base.HasValue)
		{
			return other == null;
		}
		if (other == null)
		{
			return false;
		}
		return &this.@value.Equals(other);
	}

	public override int GetHashCode()
	{
		if (!base.HasValue)
		{
			return 0;
		}
		return &this.@value.GetHashCode();
	}

	[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
	public T GetValueOrDefault()
	{
		return this.@value;
	}

	public T GetValueOrDefault(T defaultValue)
	{
		if (!base.HasValue)
		{
			return defaultValue;
		}
		return this.@value;
	}

	public static explicit operator T(Nullable<T> value)
	{
		return &value.Value;
	}

	public static implicit operator Nullable`1(T value)
	{
		return new Nullable<T>(value);
	}

	public override string ToString()
	{
		if (!base.HasValue)
		{
			return "";
		}
		return &this.@value.ToString();
	}
}
}