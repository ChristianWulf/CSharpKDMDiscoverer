using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
namespace System
{
	[ComVisible(true)]
	[Serializable]
	public class MethodAccessException : MemberAccessException
	{
		public MethodAccessException() : base(Environment.GetResourceString("Arg_MethodAccessException"))
		{
			base.SetErrorCode(-2146233072);
		}
		public MethodAccessException(string message) : base(message)
		{
			base.SetErrorCode(-2146233072);
		}
		public MethodAccessException(string message, Exception inner) : base(message, inner)
		{
			base.SetErrorCode(-2146233072);
		}
		[SecuritySafeCritical]
		protected MethodAccessException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
