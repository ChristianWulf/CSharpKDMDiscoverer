using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
namespace System
{
	[ComVisible(true)]
	[Serializable]
	public class NotImplementedException : SystemException
	{
		public NotImplementedException() : base(Environment.GetResourceString("Arg_NotImplementedException"))
		{
			base.SetErrorCode(-2147467263);
		}
		public NotImplementedException(string message) : base(message)
		{
			base.SetErrorCode(-2147467263);
		}
		public NotImplementedException(string message, Exception inner) : base(message, inner)
		{
			base.SetErrorCode(-2147467263);
		}
		[SecuritySafeCritical]
		protected NotImplementedException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
