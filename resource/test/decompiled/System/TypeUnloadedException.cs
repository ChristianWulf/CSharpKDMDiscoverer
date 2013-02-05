using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
namespace System
{
	[ComVisible(true)]
	[Serializable]
	public class TypeUnloadedException : SystemException
	{
		public TypeUnloadedException() : base(Environment.GetResourceString("Arg_TypeUnloadedException"))
		{
			base.SetErrorCode(-2146234349);
		}
		public TypeUnloadedException(string message) : base(message)
		{
			base.SetErrorCode(-2146234349);
		}
		public TypeUnloadedException(string message, Exception innerException) : base(message, innerException)
		{
			base.SetErrorCode(-2146234349);
		}
		[SecuritySafeCritical]
		protected TypeUnloadedException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
