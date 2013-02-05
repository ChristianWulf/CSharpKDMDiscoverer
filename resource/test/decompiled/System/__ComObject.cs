using System;
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Threading;
namespace System
{
	internal class __ComObject : MarshalByRefObject
	{
		private Hashtable m_ObjectToDataMap;
		private __ComObject()
		{
		}
		[SecurityCritical]
		internal IntPtr GetIUnknown(out bool fIsURTAggregated)
		{
			fIsURTAggregated = !base.GetType().IsDefined(typeof(ComImportAttribute), false);
			return Marshal.GetIUnknownForObject(this);
		}
		internal object GetData(object key)
		{
			object result = null;
			bool flag = false;
			try
			{
				Monitor.Enter(this, ref flag);
				if (this.m_ObjectToDataMap != null)
				{
					result = this.m_ObjectToDataMap[key];
				}
			}
			finally
			{
				if (flag)
				{
					Monitor.Exit(this);
				}
			}
			return result;
		}
		internal bool SetData(object key, object data)
		{
			bool result = false;
			bool flag = false;
			try
			{
				Monitor.Enter(this, ref flag);
				if (this.m_ObjectToDataMap == null)
				{
					this.m_ObjectToDataMap = new Hashtable();
				}
				if (this.m_ObjectToDataMap[key] == null)
				{
					this.m_ObjectToDataMap[key] = data;
					result = true;
				}
			}
			finally
			{
				if (flag)
				{
					Monitor.Exit(this);
				}
			}
			return result;
		}
		[SecurityCritical]
		internal void ReleaseAllData()
		{
			bool flag = false;
			try
			{
				Monitor.Enter(this, ref flag);
				if (this.m_ObjectToDataMap != null)
				{
					foreach (object current in this.m_ObjectToDataMap.Values)
					{
						IDisposable disposable = current as IDisposable;
						if (disposable != null)
						{
							disposable.Dispose();
						}
						__ComObject _ComObject = current as __ComObject;
						if (_ComObject != null)
						{
							Marshal.ReleaseComObject(_ComObject);
						}
					}
					this.m_ObjectToDataMap = null;
				}
			}
			finally
			{
				if (flag)
				{
					Monitor.Exit(this);
				}
			}
		}
		[SecurityCritical]
		internal object GetEventProvider(RuntimeType t)
		{
			object obj = this.GetData(t);
			if (obj == null)
			{
				obj = this.CreateEventProvider(t);
			}
			return obj;
		}
		[SecurityCritical]
		internal int ReleaseSelf()
		{
			return Marshal.InternalReleaseComObject(this);
		}
		[SecurityCritical]
		internal void FinalReleaseSelf()
		{
			Marshal.InternalFinalReleaseComObject(this);
		}
		[SecurityCritical]
		[ReflectionPermission(SecurityAction.Assert, MemberAccess = true)]
		private object CreateEventProvider(RuntimeType t)
		{
			object obj = Activator.CreateInstance(t, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.CreateInstance, null, new object[]
			{
				this
			}, null);
			if (!this.SetData(t, obj))
			{
				IDisposable disposable = obj as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
				obj = this.GetData(t);
			}
			return obj;
		}
	}
}
