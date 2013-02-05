using System;
using System.Runtime.InteropServices;
namespace System
{
	[ComVisible(true), AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Interface | AttributeTargets.Delegate, Inherited = false)]
	[Serializable]
	public sealed class ObsoleteAttribute : Attribute
	{
		private string _message;
		private bool _error;
		public string Message
		{
			get
			{
				return this._message;
			}
		}
		public bool IsError
		{
			get
			{
				return this._error;
			}
		}
		public ObsoleteAttribute()
		{
			this._message = null;
			this._error = false;
		}
		public ObsoleteAttribute(string message)
		{
			this._message = message;
			this._error = false;
		}
		public ObsoleteAttribute(string message, bool error)
		{
			this._message = message;
			this._error = error;
		}
	}
}
