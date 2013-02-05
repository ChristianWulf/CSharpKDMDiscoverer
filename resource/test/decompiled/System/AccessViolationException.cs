using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
namespace System
{
	[ComVisible(true)]
	[Serializable]
	public class AccessViolationException : SystemException
	{
		private IntPtr _ip;
		private IntPtr _target;
		private int _accessType;
		public AccessViolationException() : base(Environment.GetResourceString("Arg_AccessViolationException"))
		{
			base.SetErrorCode(-2147467261);
		}
		public AccessViolationException(string message) : base(message)
		{
			base.SetErrorCode(-2147467261);
		}
		public AccessViolationException(string message, Exception innerException) : base(message, innerException)
		{
			base.SetErrorCode(-2147467261);
		}
		[SecuritySafeCritical]
		protected AccessViolationException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
