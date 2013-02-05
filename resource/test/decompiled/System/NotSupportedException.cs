using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
namespace System
{
	[ComVisible(true)]
	[Serializable]
	public class NotSupportedException : SystemException
	{
		public NotSupportedException() : base(Environment.GetResourceString("Arg_NotSupportedException"))
		{
			base.SetErrorCode(-2146233067);
		}
		public NotSupportedException(string message) : base(message)
		{
			base.SetErrorCode(-2146233067);
		}
		public NotSupportedException(string message, Exception innerException) : base(message, innerException)
		{
			base.SetErrorCode(-2146233067);
		}
		[SecuritySafeCritical]
		protected NotSupportedException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
