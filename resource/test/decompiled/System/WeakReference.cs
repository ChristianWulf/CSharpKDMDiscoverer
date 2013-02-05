using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using System.Threading;
namespace System
{
	[ComVisible(true)]
	[SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
	[Serializable]
	public class WeakReference : ISerializable
	{
		internal IntPtr m_handle;
		internal bool m_IsLongReference;
		public virtual bool IsAlive
		{
			[SecuritySafeCritical]
			get
			{
				IntPtr handle = this.m_handle;
				if (IntPtr.Zero == handle)
				{
					return false;
				}
				bool flag = GCHandle.InternalGet(handle) != null;
				return !(this.m_handle == IntPtr.Zero) && flag;
			}
		}
		public virtual bool TrackResurrection
		{
			get
			{
				return this.m_IsLongReference;
			}
		}
		public virtual object Target
		{
			[SecuritySafeCritical]
			get
			{
				IntPtr handle = this.m_handle;
				if (IntPtr.Zero == handle)
				{
					return null;
				}
				object result = GCHandle.InternalGet(handle);
				if (!(this.m_handle == IntPtr.Zero))
				{
					return result;
				}
				return null;
			}
			[SecuritySafeCritical]
			set
			{
				IntPtr handle = this.m_handle;
				if (handle == IntPtr.Zero)
				{
					throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_HandleIsNotInitialized"));
				}
				object oldValue = GCHandle.InternalGet(handle);
				handle = this.m_handle;
				if (handle == IntPtr.Zero)
				{
					throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_HandleIsNotInitialized"));
				}
				GCHandle.InternalCompareExchange(handle, value, oldValue, false);
				GC.KeepAlive(this);
			}
		}
		public WeakReference(object target) : this(target, false)
		{
		}
		[SecuritySafeCritical]
		public WeakReference(object target, bool trackResurrection)
		{
			this.m_IsLongReference = trackResurrection;
			this.m_handle = GCHandle.InternalAlloc(target, trackResurrection ? GCHandleType.WeakTrackResurrection : GCHandleType.Weak);
		}
		[SecuritySafeCritical]
		protected WeakReference(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			object value = info.GetValue("TrackedObject", typeof(object));
			this.m_IsLongReference = info.GetBoolean("TrackResurrection");
			this.m_handle = GCHandle.InternalAlloc(value, this.m_IsLongReference ? GCHandleType.WeakTrackResurrection : GCHandleType.Weak);
		}
		[SecuritySafeCritical]
		protected override void Finalize()
		{
			try
			{
				IntPtr handle = this.m_handle;
				if (handle != IntPtr.Zero && handle == Interlocked.CompareExchange(ref this.m_handle, IntPtr.Zero, handle))
				{
					GCHandle.InternalFree(handle);
				}
			}
			finally
			{
				base.Finalize();
			}
		}
		[SecurityCritical]
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			info.AddValue("TrackedObject", this.Target, typeof(object));
			info.AddValue("TrackResurrection", this.m_IsLongReference);
		}
	}
}
