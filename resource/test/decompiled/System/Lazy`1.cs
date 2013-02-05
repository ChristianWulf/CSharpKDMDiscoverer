using System.Security.Permissions;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Runtime.Serialization;
using System;

namespace System
{
[HostProtection(SecurityAction.LinkDemand, Synchronization=true, ExternalThreading=true)]
[DebuggerDisplay("ThreadSafetyMode={Mode}, IsValueCreated={IsValueCreated}, IsValueFaulted={IsValueFaulted}, Value={ValueForDebugDisplay}")]
[DebuggerTypeProxy(typeof(System_LazyDebugView`1))]
[ComVisible(false)]
[Serializable]
public class Lazy<T>
{
	private volatile object m_boxed;

	[NonSerialized]
	private readonly object m_threadSafeObj;

	[NonSerialized]
	private Func<T> m_valueFactory;

	private static Func<T> PUBLICATION_ONLY_OR_ALREADY_INITIALIZED;

	public bool IsValueCreated
	{
		get
		{
			if (this.m_boxed != null)
			{
				return this.m_boxed is Box<T>;
			}
			return false;
		}
	}

	internal bool IsValueFaulted
	{
		get
		{
			return this.m_boxed is LazyInternalExceptionHold<T>;
		}
	}

	internal LazyThreadSafetyMode Mode
	{
		get
		{
			if (this.m_threadSafeObj == null)
			{
				return 0;
			}
			if (this.m_threadSafeObj == Lazy<T>.PUBLICATION_ONLY_OR_ALREADY_INITIALIZED)
			{
				return 1;
			}
			return 2;
		}
	}

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public T Value
	{
		get
		{
			Box<T> mBoxed = null;
			if (this.m_boxed != null)
			{
				mBoxed = this.m_boxed as Box<T>;
				if (mBoxed != null)
				{
					return mBoxed.m_value;
				}
			}
			LazyInternalExceptionHold<T> lazyInternalExceptionHolder = this.m_boxed as LazyInternalExceptionHold<T>;
			throw lazyInternalExceptionHolder.m_exception;
			Debugger.NotifyOfCrossThreadDependency();
			return this.LazyInitValue();
		}
	}

	internal T ValueForDebugDisplay
	{
		get
		{
			if (!this.IsValueCreated)
			{
				T t = default(T);
				return t;
			}
			return (Box<T>)this.m_boxed.m_value;
		}
	}

	static Lazy()
	{
		if (Lazy<T>.CS$<>9__CachedAnonymousMethodDelegate2 == null)
		{
			Lazy<T>.CS$<>9__CachedAnonymousMethodDelegate2 = new Func<T>(null.Lazy<T>.<.cctor>b__1);
		}
		Lazy<T>.PUBLICATION_ONLY_OR_ALREADY_INITIALIZED = Lazy<T>.CS$<>9__CachedAnonymousMethodDelegate2;
	}

	public Lazy()
	{
	}

	public Lazy(Func<T> valueFactory)
	{
	}

	public Lazy(bool isThreadSafe)
	{
	}

	public Lazy(LazyThreadSafetyMode mode)
	{
		this.m_threadSafeObj = Lazy<T>.GetObjectFromMode(mode);
	}

	public Lazy(Func<T> valueFactory, bool isThreadSafe)
	{
	}

	public Lazy(Func<T> valueFactory, LazyThreadSafetyMode mode)
	{
		if (valueFactory == null)
		{
			throw new ArgumentNullException("valueFactory");
		}
		this.m_threadSafeObj = Lazy<T>.GetObjectFromMode(mode);
		this.m_valueFactory = valueFactory;
	}

	private Box<T> CreateValue()
	{
		Box<T> boxed = null;
		LazyThreadSafetyMode mode = this.Mode;
		if (this.m_valueFactory != null)
		{
			if (mode != LazyThreadSafetyMode.PublicationOnly && this.m_valueFactory == Lazy<T>.PUBLICATION_ONLY_OR_ALREADY_INITIALIZED)
			{
				throw new InvalidOperationException(Environment.GetResourceString("Lazy_Value_RecursiveCallsToValue"));
			}
			Func<T> mValueFactory = this.m_valueFactory;
			if (mode != LazyThreadSafetyMode.PublicationOnly)
			{
				this.m_valueFactory = Lazy<T>.PUBLICATION_ONLY_OR_ALREADY_INITIALIZED;
			}
			return new Box<T>(mValueFactory());
			if (mode != LazyThreadSafetyMode.PublicationOnly)
			{
				this.m_boxed = new LazyInternalExceptionHold<T>(exception.PrepForRemoting());
			}
			throw;
		}
		try
		{
		}
		catch (Exception exception)
		{
		}
		try
		{
			return new Box<T>((T)Activator.CreateInstance(typeof(T)));
		}
		catch (MissingMethodException)
		{
			Exception missingMemberException = new MissingMemberException(Environment.GetResourceString("Lazy_CreateValue_NoParameterlessCtorForT"));
			if (mode != LazyThreadSafetyMode.PublicationOnly)
			{
				this.m_boxed = new LazyInternalExceptionHold<T>(missingMemberException);
			}
			throw missingMemberException;
		}
		return boxed;
	}

	private static object GetObjectFromMode(LazyThreadSafetyMode mode)
	{
		if (mode == LazyThreadSafetyMode.ExecutionAndPublication)
		{
			return new object();
		}
		if (mode == LazyThreadSafetyMode.PublicationOnly)
		{
			return Lazy<T>.PUBLICATION_ONLY_OR_ALREADY_INITIALIZED;
		}
		if (mode != LazyThreadSafetyMode.None)
		{
			throw new ArgumentOutOfRangeException("mode", Environment.GetResourceString("Lazy_ctor_ModeInvalid"));
		}
		return null;
	}

	private T LazyInitValue()
	{
		object mThreadSafeObj;
		Box<T> mBoxed = null;
		LazyThreadSafetyMode mode = this.Mode;
		if (mode == LazyThreadSafetyMode.None)
		{
			mBoxed = this.CreateValue();
			this.m_boxed = mBoxed;
		}
		else
		{
			if (mode == LazyThreadSafetyMode.PublicationOnly)
			{
				mBoxed = this.CreateValue();
				if (Interlocked.CompareExchange(ref this.m_boxed, mBoxed, null) != null)
				{
					return (Box<T>)this.m_boxed;
				}
			}
			else
			{
				try
				{
					Monitor.Enter(mThreadSafeObj = this.m_threadSafeObj, ref flag);
					if (this.m_boxed == null)
					{
						mBoxed = this.CreateValue();
						this.m_boxed = mBoxed;
					}
					else
					{
						mBoxed = this.m_boxed as Box<T>;
						if (mBoxed == null)
						{
							LazyInternalExceptionHold<T> lazyInternalExceptionHolder = this.m_boxed as LazyInternalExceptionHold<T>;
							throw lazyInternalExceptionHolder.m_exception;
						}
					}
				}
				finally
				{
					if (false)
					{
						Monitor.Exit(mThreadSafeObj);
					}
				}
			}
		}
		return mBoxed.m_value;
	}

	[OnSerializing]
	private void OnSerializing(StreamingContext context)
	{
		this.Value;
	}

	public override string ToString()
	{
		if (!this.IsValueCreated)
		{
			return Environment.GetResourceString("Lazy_ToString_ValueNotCreated");
		}
		T @value = this.Value;
		return &@value.ToString();
	}

	[Serializable]
	private class Boxed<T>
	{
		internal T m_value;

		internal Boxed(T value);
	}

	private class LazyInternalExceptionHolder<T>
	{
		internal Exception m_exception;

		internal LazyInternalExceptionHolder(Exception ex);
	}
}
}